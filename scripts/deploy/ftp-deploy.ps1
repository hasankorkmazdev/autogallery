<#
.SYNOPSIS
    Yayin paketini FTP ile sunucuya yukler (IIS paylasimli barindirma). Varsayilan: yalniz degisen dosyalar.

.DESCRIPTION
    1. Yuklenecek dosyalar belirlenir: son basarili yuklemeden (yerel liste, %LOCALAPPDATA%\FtpDeploy) bu yana icerigi
       degisen ya da yeni dosyalar. -All hepsini, -Files yalniz verilenleri yukler. Degisen yoksa hicbir sey yapilmaz.
    2. app_offline.htm yuklenir: IIS uygulamayi durdurur, DLL'ler kilitli kalmaz (ziyaretci bakim sayfasi gorur).
    3. Dosyalar 20'lik gruplarla tek baglantida yuklenir (klasorler gerekirse olusturulur). Pakette olmayan dosya
       sunucudan silinmez; sunucuda uygulamanin yazdigi klasorlere (yuklenen dosyalar, loglar) dokunulmaz.
    4. app_offline.htm silinir, site acilir.

    Paket klasoru ve yayin ayari kontrolu bir ust klasordeki deploy.json'dan. FTP hesabi ftp-login.ps1 ile bir kez
    kaydedilirse sifre sorulmaz; kayit yoksa terminalde sorulur. Sifre komut satirinda gecmez: gecici bir netrc
    dosyasina yazilip is bitince silinir. Baglanti FTPS (TLS) ister.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts\deploy\ftp-deploy.ps1
    powershell -ExecutionPolicy Bypass -File scripts\deploy\ftp-deploy.ps1 -Files appsettings.Production.json,web.config
#>
param(
    # Bos birakilirsa kayitli hesaptan (ftp-login.ps1)
    [string]$FtpHost,
    [string]$User,
    # Sitenin sunucudaki klasoru (FTP girisi dogrudan site kokune aciliyorsa "/")
    [string]$RemotePath,
    # Bos birakilirsa deploy.json'daki PublishDir
    [string]$PublishDir,
    [pscredential]$Credential,
    # Sunucunun TLS sertifikasi gecersizse
    [switch]$Insecure,
    # Sunucu FTPS desteklemiyorsa duz FTP (sifre ve dosyalar sifresiz gider)
    [switch]$AllowPlainFtp,
    # Yayin ayarindaki bos alan kontrolunu atla
    [switch]$SkipSettingsCheck,
    # Yalniz bu dosyalar / klasorler (pakete gore goreli, virgulle: app.dll,web.config,wwwroot/js)
    [string[]]$Files,
    # Degisenler yerine tum paket
    [switch]$All,
    # Yuklemeden yalniz neyin yuklenecegini goster
    [switch]$WhatIf
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "ftp-common.ps1")
$config = $script:DeployConfig

if (-not $PublishDir) { $PublishDir = Get-RepoPath $config.PublishDir }
if (-not (Test-Path $PublishDir) -or -not (Get-ChildItem $PublishDir -File -ErrorAction SilentlyContinue)) {
    throw "Yayin paketi bulunamadi ya da bos: $PublishDir. Once scripts\deploy\deploy.ps1 (ya da dotnet publish)."
}
$PublishDir = (Resolve-Path $PublishDir).Path

# Yayin ayari: zorunlu alanlar bos kalirsa uygulama acilmaz ya da panele girilemez
if (-not $SkipSettingsCheck -and $config.SettingsFile) {
    $settingsPath = Join-Path $PublishDir $config.SettingsFile
    if (-not (Test-Path $settingsPath)) { throw "Pakette $($config.SettingsFile) yok." }
    $settings = Get-Content $settingsPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $missing = @($config.RequiredSettings | Where-Object { $_ -and -not (Get-JsonValue $settings $_) })
    if ($missing.Count -gt 0) {
        throw ("$($config.SettingsFile) icinde bos: " + ($missing -join ", ") + ". Doldurup tekrar calistirin (-SkipSettingsCheck ile atlanir).")
    }
    foreach ($key in @($config.WarnIfTrue | Where-Object { $_ })) {
        if ((Get-JsonValue $settings $key) -eq $true) { Write-Host "Uyari: $key acik." -ForegroundColor Yellow }
    }
}

$connection = Resolve-FtpConnection $FtpHost $User $RemotePath $Credential $AllowPlainFtp $Insecure
$remoteRoot = $connection.Root

function Get-Relative($file) { $file.FullName.Substring($PublishDir.Length).TrimStart("\") -replace "\\", "/" }

# Son basarili yuklemenin listesi: goreli yol -> "boyut:sha256". Hesap + sunucu + kullanici + klasor basina ayri.
$stateDir = Join-Path $env:LOCALAPPDATA "FtpDeploy"
$stateName = ($config.Profile + "-" + $connection.FtpHost + "-" + $connection.User + "-" + $connection.RemotePath) -replace "[^A-Za-z0-9.-]", "_"
$statePath = Join-Path $stateDir ($stateName + ".json")
$state = @{}
if (Test-Path $statePath) {
    (Get-Content $statePath -Raw -Encoding UTF8 | ConvertFrom-Json).PSObject.Properties | ForEach-Object { $state[$_.Name] = $_.Value }
}
function Get-Signature($file) { "{0}:{1}" -f $file.Length, (Get-FileHash $file.FullName -Algorithm SHA256).Hash }
function Save-State {
    if (-not (Test-Path $stateDir)) { New-Item -ItemType Directory $stateDir | Out-Null }
    $ordered = [ordered]@{}
    foreach ($key in ($state.Keys | Sort-Object)) { $ordered[$key] = $state[$key] }
    [System.IO.File]::WriteAllText($statePath, ($ordered | ConvertTo-Json), (New-Object System.Text.UTF8Encoding($false)))
}

# Yuklenecek dosyalar bakim sayfasindan once belirlenir: listede hata varsa ya da degisen yoksa site hic kapanmaz.
# (PowerShell degisken adlarinda buyuk / kucuk harf ayirmaz: $Files parametresiyle karismasin diye $uploadFiles)
if ($Files) {
    # powershell -File ile verilen "a,b,c" tek metin gelir; virgulden ayrilir. Klasor verilirse altindaki tum dosyalar.
    $names = @($Files | ForEach-Object { $_ -split "," } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    $candidates = @($names | ForEach-Object {
            $path = Join-Path $PublishDir $_
            if (Test-Path $path -PathType Container) { Get-ChildItem $path -Recurse -File }
            elseif (Test-Path $path -PathType Leaf) { Get-Item $path }
            else { throw "Pakette yok: $_" }
        })
}
else {
    $candidates = @(Get-ChildItem $PublishDir -Recurse -File | Where-Object { $_.Name -ne "app_offline.htm" })
}

$signatures = @{}
foreach ($file in $candidates) { $signatures[(Get-Relative $file)] = Get-Signature $file }
if ($Files -or $All) { $uploadFiles = $candidates }
else {
    if ($state.Count -eq 0) { Write-Host "Bu sunucuya onceki yukleme kaydi yok: tum paket yuklenecek." -ForegroundColor Yellow }
    $uploadFiles = @($candidates | Where-Object { $state[(Get-Relative $_)] -ne $signatures[(Get-Relative $_)] })
}

$total = $uploadFiles.Count
if ($total -eq 0) {
    Write-Host "Degisen dosya yok; sunucu guncel." -ForegroundColor Green
    return
}
$sizeMb = [Math]::Round((($uploadFiles | Measure-Object Length -Sum).Sum) / 1MB, 1)
Write-Host "$total dosya ($sizeMb MB) -> $($connection.User)@$($connection.FtpHost)$(if ($remoteRoot) { $remoteRoot } else { '/' })"
if ($total -le 15) { $uploadFiles | ForEach-Object { Write-Host ("  " + (Get-Relative $_)) } }
if ($WhatIf) { return }

$curl = (Get-Command curl.exe -ErrorAction Stop).Source
$host0 = $connection.FtpHost

function Invoke-Curl([string[]]$arguments) {
    # curl'un hata ciktisi PowerShell 5.1'de kendi hatasi sayilmasin; hata koduna gore anlasilir mesaj verilir
    $ErrorActionPreference = "Continue"
    & $curl @common @arguments 2>$null
    $code = $LASTEXITCODE
    if ($code -eq 0) { return }
    if ($code -eq 64) { throw "Sunucu FTPS (TLS) desteklemiyor. Sifre acik gidecek olsa da devam etmek icin -AllowPlainFtp ekleyin." }
    if ($code -eq 35 -or $code -eq 60) { throw "TLS sertifikasi dogrulanamadi (curl $code). Sunucu adini kontrol edin ya da -Insecure ekleyin." }
    if ($code -eq 67) { throw "FTP girisi reddedildi: kullanici adi ya da sifre hatali (kayitliysa ftp-login.ps1 ile yeniden kaydedin)." }
    if ($code -eq 9) { throw "Uzak klasore erisilemedi (curl 9). -RemotePath'i kontrol edin." }
    throw "curl hata kodu $code"
}

function Get-Url([string]$relative) {
    $parts = $relative.Split("/") | ForEach-Object { [uri]::EscapeDataString($_) }
    "ftp://$host0$remoteRoot/" + ($parts -join "/")
}

$netrc = New-FtpNetrc $connection
$common = @(Get-FtpCurlOptions $connection $netrc) + @("--retry", "3")
$offline = Join-Path ([System.IO.Path]::GetTempPath()) "app_offline.htm"
$offlineUploaded = $false
try {
    Set-Content -Path $offline -Encoding UTF8 -Value @'
<!doctype html><html lang="tr"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>K&#305;sa bir bak&#305;m</title></head>
<body style="margin:0;min-height:100vh;display:flex;align-items:center;justify-content:center;background:#0B0B0C;color:#F5F3EF;font-family:Georgia,serif;text-align:center">
<div><p style="color:#9A9A9A;letter-spacing:.2em;font-size:12px;font-family:sans-serif">BAKIM</p><h1 style="font-weight:400">Sitemiz birka&#231; dakika i&#231;inde yeniden a&#231;&#305;lacak.</h1></div></body></html>
'@

    # Uzak klasor var olmali: yanlis yolda --ftp-create-dirs yeni klasor acip dosyalari oraya yukler
    Write-Host "0/3 Baglanti ve uzak klasor kontrol ediliyor..."
    Invoke-Curl @("--list-only", "ftp://$host0$remoteRoot/") | Out-Null

    Write-Host "1/3 Bakim sayfasi yukleniyor (uygulama durur)..."
    Invoke-Curl @("--ftp-create-dirs", "-T", $offline, (Get-Url "app_offline.htm"))
    $offlineUploaded = $true
    Start-Sleep -Seconds 3

    Write-Host "2/3 $total dosya yukleniyor..."
    # Dosyalar gruplar halinde tek curl cagrisiyla gider: baglanti (TLS el sikismasi + giris) her dosyada yeniden
    # kurulmaz. --fail-early ilk hatada durur. Her basarili grup listeye yazilir: yarida kalirsa kalanlar yuklenir.
    $batchSize = 20
    for ($start = 0; $start -lt $total; $start += $batchSize) {
        $batch = $uploadFiles[$start..([Math]::Min($start + $batchSize, $total) - 1)]
        $arguments = @("--ftp-create-dirs", "--fail-early")
        foreach ($file in $batch) { $arguments += @("-T", $file.FullName, (Get-Url (Get-Relative $file))) }
        $end = $start + $batch.Count
        Write-Progress -Activity "FTP yukleme" -Status "$end / $total  $(Get-Relative $batch[-1])" -PercentComplete ($end * 100 / $total)
        Invoke-Curl $arguments
        foreach ($file in $batch) { $state[(Get-Relative $file)] = $signatures[(Get-Relative $file)] }
        Save-State
    }
    Write-Progress -Activity "FTP yukleme" -Completed

    Write-Host "3/3 Bakim sayfasi kaldiriliyor (site acilir)..."
    Invoke-Curl @("--list-only", "--quote", "DELE $remoteRoot/app_offline.htm", "ftp://$host0$remoteRoot/") | Out-Null
    $offlineUploaded = $false
    Write-Host "Tamam: $total dosya yuklendi." -ForegroundColor Green
}
finally {
    Remove-Item $netrc -Force -ErrorAction SilentlyContinue
    if ($offlineUploaded) {
        Write-Host "Yukleme yarida kaldi; site bakim sayfasinda. Tekrar calistirin (yalniz kalan dosyalar gider)." -ForegroundColor Red
    }
}
