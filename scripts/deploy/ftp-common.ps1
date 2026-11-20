# Ortak yayin betiklerinin ortak kodu (dot-source ile yuklenir). Bu klasor (scripts/deploy) projeden bagimsizdir,
# baska projeye oldugu gibi kopyalanir; projeye ozel her sey bir ust klasordeki deploy.json'dadir:
#
#   Profile           kayitli FTP hesabinin adi (proje basina farkli; ayni bilgisayarda birden cok proje)
#   ServerProject     dotnet publish edilecek proje klasoru (depo kokune gore)
#   PanelDir          (istege bagli) deploy.ps1 -Panel ile npm run build yapilacak klasor
#   PublishDir        yayin paketi klasoru (depo kokune gore)
#   SettingsFile      paketteki yayin ayar dosyasi (bos alan kontrolu icin)
#   RequiredSettings  bos olmamasi gereken ayarlar ("Bolum:Anahtar")
#   WarnIfTrue        true ise uyari verilecek ayarlar
#   LogsPath          sunucuda site kokune gore log klasoru (ftp-logs.ps1; Serilog compact JSON, app / errors)
#
# Kayitli FTP hesabi: %APPDATA%\FtpDeploy\<Profile>.xml (sunucu, kullanici, uzak klasor, secenekler, sifre). Sifre
# Windows DPAPI ile sifrelenir (Export-Clixml): yalniz bu Windows kullanicisi bu bilgisayarda cozebilir. ftp-login.ps1
# ile kaydedilir / silinir; kayit yoksa sifre her calismada sorulur.

$script:DeployRepoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$script:DeployConfigPath = Join-Path (Split-Path -Parent $PSScriptRoot) "deploy.json"
if (-not (Test-Path $script:DeployConfigPath)) {
    throw "Proje ayari yok: $script:DeployConfigPath (Profile, ServerProject, PublishDir, ... alanlari; ornek icin FatihTasci deposundaki scripts/deploy.json)."
}
$script:DeployConfig = Get-Content $script:DeployConfigPath -Raw -Encoding UTF8 | ConvertFrom-Json
if (-not $script:DeployConfig.Profile) { throw "deploy.json'da Profile bos." }
$script:FtpProfilePath = Join-Path $env:APPDATA ("FtpDeploy\" + $script:DeployConfig.Profile + ".xml")

# Depo kokune gore yol
function Get-RepoPath([string]$relative) { Join-Path $script:DeployRepoRoot $relative }

function Get-FtpProfile {
    if (-not (Test-Path $script:FtpProfilePath)) { return $null }
    try { Import-Clixml $script:FtpProfilePath }
    catch {
        Write-Host "Kayitli FTP hesabi okunamadi (baska kullanici / bilgisayar?): $($_.Exception.Message)" -ForegroundColor Yellow
        $null
    }
}

function Save-FtpProfile($connection) {
    $dir = Split-Path -Parent $script:FtpProfilePath
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory $dir | Out-Null }
    $connection | Select-Object FtpHost, User, RemotePath, AllowPlainFtp, Insecure, Credential | Export-Clixml $script:FtpProfilePath
}

# Parametrelerle kayitli hesabi birlestirir: verilen parametre kayittan onceliklidir. Sifre kayitta yoksa (ya da
# baska sunucu / kullanici verildiyse) terminalde sorulur.
function Resolve-FtpConnection([string]$FtpHost, [string]$User, [string]$RemotePath, [pscredential]$Credential,
    [bool]$AllowPlainFtp, [bool]$Insecure) {
    $saved = Get-FtpProfile
    if (-not $FtpHost -and $saved) { $FtpHost = $saved.FtpHost }
    if (-not $User -and $saved) { $User = $saved.User }
    if (-not $FtpHost -or -not $User) {
        throw ("FTP hesabi kayitli degil (" + $script:DeployConfig.Profile + "). Bir kez: powershell -ExecutionPolicy Bypass -File scripts\deploy\ftp-login.ps1 -FtpHost <sunucu> -User <kullanici> -RemotePath /")
    }
    $same = $saved -and $saved.FtpHost -eq $FtpHost -and $saved.User -eq $User
    if ($same) {
        if (-not $RemotePath) { $RemotePath = $saved.RemotePath }
        if (-not $Credential) { $Credential = $saved.Credential }
        $AllowPlainFtp = $AllowPlainFtp -or [bool]$saved.AllowPlainFtp
        $Insecure = $Insecure -or [bool]$saved.Insecure
    }
    if (-not $RemotePath) { $RemotePath = "/" }
    if (-not $Credential) {
        $secure = Read-Host -AsSecureString "FTP sifresi ($User@$FtpHost)"
        if ($secure.Length -eq 0) { throw "Sifre girilmedi." }
        $Credential = New-Object System.Management.Automation.PSCredential($User, $secure)
    }
    $root = "/" + $RemotePath.Trim("/")
    if ($root -eq "/") { $root = "" }
    [pscustomobject]@{ FtpHost = $FtpHost; User = $User; RemotePath = $RemotePath; Root = $root
        AllowPlainFtp = $AllowPlainFtp; Insecure = $Insecure; Credential = $Credential }
}

# curl'un okuyacagi gecici netrc dosyasi (sifre komut satirinda / surec listesinde gorunmesin); cagiran is bitince siler.
function New-FtpNetrc($connection) {
    $path = Join-Path ([System.IO.Path]::GetTempPath()) ("ftp-" + [guid]::NewGuid().ToString("N") + ".netrc")
    Set-Content -Path $path -Encoding ASCII -NoNewline -Value ("machine " + $connection.FtpHost.Split(":")[0] + " login " +
        $connection.Credential.UserName + " password " + $connection.Credential.GetNetworkCredential().Password)
    $path
}

# curl'un ortak secenekleri
function Get-FtpCurlOptions($connection, [string]$netrc) {
    $options = @("--netrc-file", $netrc, "--silent", "--show-error", "--connect-timeout", "30")
    if (-not $connection.AllowPlainFtp) { $options += "--ssl-reqd" }
    if ($connection.Insecure) { $options += "--insecure" }
    $options
}

# JSON nesnesinde "Bolum:Anahtar" yolundaki deger (yoksa $null)
function Get-JsonValue($object, [string]$path) {
    foreach ($part in $path.Split(":")) {
        if ($null -eq $object) { return $null }
        $property = $object.PSObject.Properties[$part]
        $object = if ($property) { $property.Value } else { $null }
    }
    $object
}
