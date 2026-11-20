<#
.SYNOPSIS
    Projenin FTP hesabini (sunucu, kullanici, sifre) bu bilgisayara bir kez kaydeder; sonra deploy / ftp-deploy /
    ftp-logs sifre sormaz.

.DESCRIPTION
    Hesap adi bir ust klasordeki deploy.json'daki Profile'dir (proje basina ayri hesap; ayni bilgisayarda birden cok
    proje). Sifre terminalde sorulur, once sunucuya baglanip denenir, dogruysa %APPDATA%\FtpDeploy\<Profile>.xml'e
    Windows DPAPI ile sifrelenmis olarak yazilir: yalniz bu Windows kullanicisi bu bilgisayarda cozebilir; repoya girmez.
    Bu kullanici adina calisan her program (bu betikler dahil) sifreyi kullanabilir; bilgisayar paylasiliyorsa kaydetmeyin.
    Silmek icin -Remove. Sifre degisince yeniden calistirin.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts\deploy\ftp-login.ps1 -FtpHost ftp.ornek.com
    powershell -ExecutionPolicy Bypass -File scripts\deploy\ftp-login.ps1 -FtpHost ftp.ornek.com -User kullanici -RemotePath /
    powershell -ExecutionPolicy Bypass -File scripts\deploy\ftp-login.ps1 -Remove
#>
param(
    [string]$FtpHost,
    [string]$User,
    # Sitenin sunucudaki klasoru (FTP girisi dogrudan site kokune aciliyorsa "/"). Verilmezse FTP kokundeki klasorler
    # gosterilip sorulur.
    [string]$RemotePath,
    [switch]$Remove,
    # Sunucu FTPS desteklemiyorsa duz FTP (sifre acik gider)
    [switch]$AllowPlainFtp,
    # Sunucunun TLS sertifikasi gecersizse
    [switch]$Insecure
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "ftp-common.ps1")
$name = $script:DeployConfig.Profile

if ($Remove) {
    if (Test-Path $script:FtpProfilePath) { Remove-Item $script:FtpProfilePath -Force; Write-Host "Kayitli FTP hesabi silindi ($name)." }
    else { Write-Host "Kayitli FTP hesabi yok ($name)." }
    return
}

$saved = Get-FtpProfile
if (-not $FtpHost -and $saved) { $FtpHost = $saved.FtpHost }
if (-not $User -and $saved) { $User = $saved.User }
if (-not $FtpHost) { $FtpHost = Read-Host "FTP sunucusu" }
if (-not $User) { $User = Read-Host "FTP kullanicisi" }

$secure = Read-Host -AsSecureString "FTP sifresi ($User@$FtpHost)"
if ($secure.Length -eq 0) { throw "Sifre girilmedi." }
$connection = [pscustomobject]@{ FtpHost = $FtpHost; User = $User; RemotePath = "/"; Root = ""
    AllowPlainFtp = [bool]$AllowPlainFtp; Insecure = [bool]$Insecure
    Credential = New-Object System.Management.Automation.PSCredential($User, $secure) }

# Kaydetmeden once dene: yanlis sifre ya da klasor kaydedilmesin
$netrc = New-FtpNetrc $connection
try {
    $options = Get-FtpCurlOptions $connection $netrc
    function Test-Folder([string]$path) {
        # curl'un hata ciktisi PowerShell 5.1'de kendi hatasi sayilmasin; hata koduna gore anlasilir mesaj verilir
        $ErrorActionPreference = "Continue"
        $dir = "/" + $path.Trim("/"); if ($dir -eq "/") { $dir = "" }
        $names = & curl.exe @options --list-only "ftp://$FtpHost$dir/" 2>$null
        [pscustomobject]@{ Code = $LASTEXITCODE; Names = @($names | ForEach-Object { $_.Trim() } | Where-Object { $_ -and $_ -ne "." -and $_ -ne ".." }) }
    }

    $top = Test-Folder "/"
    switch ($top.Code) {
        0 { }
        67 { throw "Giris reddedildi: kullanici adi ya da sifre hatali. Kaydedilmedi." }
        64 { throw "Sunucu FTPS desteklemiyor (-AllowPlainFtp ile sifre acik gider). Kaydedilmedi." }
        default { throw "Sunucuya baglanilamadi (curl $($top.Code)). Kaydedilmedi." }
    }

    if (-not $RemotePath) {
        Write-Host ""
        Write-Host "FTP kokundekiler:" -ForegroundColor Yellow
        $top.Names | ForEach-Object { Write-Host "  $_" }
        Write-Host "Site kokteyse (web.config / dll burada ya da bos site) Enter; Plesk'te genelde httpdocs ya da alan adi klasoru."
        $RemotePath = Read-Host "Sitenin klasoru (bos = /)"
        if (-not $RemotePath) { $RemotePath = "/" }
    }
    $check = Test-Folder $RemotePath
    if ($check.Code -ne 0) { throw "Baglanildi ama $RemotePath klasorune erisilemedi (curl $($check.Code)). Kaydedilmedi." }
}
finally { Remove-Item $netrc -Force -ErrorAction SilentlyContinue }

$connection.RemotePath = $RemotePath
Save-FtpProfile $connection
Write-Host "Kaydedildi ($name): $User@$FtpHost ($RemotePath). Artik betikler sifre sormaz." -ForegroundColor Green
