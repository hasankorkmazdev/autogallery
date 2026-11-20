<#
.SYNOPSIS
    Sunucudaki uygulama loglarini FTP ile okur (yalniz okur; hicbir sey yuklemez / silmez).

.DESCRIPTION
    Log klasoru bir ust klasordeki deploy.json'daki LogsPath'tir (site kokune gore; Serilog compact JSON, alt
    klasorler errors / app). Klasor yoksa uygulama henuz yazamamistir (cogu zaman uygulama havuzunun yazma izni).
    errors altindaki en yeni dosyanin son kayitlari (zaman, seviye, mesaj, hatanin ilk satirlari); hata yoksa app.
    FTP hesabi ftp-login.ps1 ile kaydedildiyse sifre sorulmaz.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts\deploy\ftp-logs.ps1
    powershell -ExecutionPolicy Bypass -File scripts\deploy\ftp-logs.ps1 -Tail 20
#>
param(
    # Bos birakilirsa kayitli hesaptan (ftp-login.ps1)
    [string]$FtpHost,
    [string]$User,
    [string]$RemotePath,
    # Gosterilecek son kayit sayisi
    [int]$Tail = 5,
    [pscredential]$Credential,
    [switch]$AllowPlainFtp,
    [switch]$Insecure
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "ftp-common.ps1")
if (-not $script:DeployConfig.LogsPath) { throw "deploy.json'da LogsPath bos." }
$connection = Resolve-FtpConnection $FtpHost $User $RemotePath $Credential $AllowPlainFtp $Insecure
$ErrorActionPreference = "Continue"
$FtpHost = $connection.FtpHost
$root = $connection.Root
$logs = "/" + $script:DeployConfig.LogsPath.Trim("/")
$netrc = New-FtpNetrc $connection
try {
    $options = Get-FtpCurlOptions $connection $netrc

    function Get-Names([string]$dir) {
        $names = & curl.exe @options --list-only "ftp://$FtpHost$root$dir/" 2>$null
        if ($LASTEXITCODE -ne 0) { return $null }
        @($names | ForEach-Object { $_.Trim() } | Where-Object { $_ -and $_ -ne "." -and $_ -ne ".." })
    }

    if ($null -eq (Get-Names "")) { Write-Host "Site klasoru listelenemedi (curl $LASTEXITCODE)." -ForegroundColor Red; return }
    $logDirs = Get-Names $logs
    if (-not $logDirs) {
        Write-Host "$logs YOK: uygulama henuz log yazmamis ya da klasore yazamiyor (uygulama havuzunun yazma izni)." -ForegroundColor Red
        return
    }
    Write-Host ("$logs/: " + ($logDirs -join ", "))

    foreach ($kind in "errors", "app") {
        $files = Get-Names "$logs/$kind"
        if (-not $files) { if ($kind -eq "errors") { Write-Host ""; Write-Host "Hata kaydi yok." -ForegroundColor Green }; continue }
        $latest = $files | Sort-Object | Select-Object -Last 1
        Write-Host ""
        Write-Host "=== $logs/$kind/$latest (son $Tail kayit; toplam $($files.Count) dosya) ===" -ForegroundColor Yellow
        $text = & curl.exe @options "ftp://$FtpHost$root$logs/$kind/$latest" 2>$null
        $lines = @($text | Where-Object { $_.Trim().StartsWith("{") }) | Select-Object -Last $Tail
        foreach ($line in $lines) {
            try { $entry = $line | ConvertFrom-Json } catch { Write-Host $line; continue }
            $level = if ($entry.'@l') { $entry.'@l' } else { "Information" }
            Write-Host ("{0}  [{1}]  {2}" -f $entry.'@t', $level, $entry.'@m') -ForegroundColor $(if ($level -in "Error", "Fatal") { "Red" } else { "Gray" })
            if ($entry.'@x') {
                # Yigin satirlarindan yalniz uygulamanin kendi kodu (System / Microsoft cerceveleri atlanir)
                ($entry.'@x' -split "`r?`n" | Where-Object { $_ -notmatch "^\s+at (System|Microsoft)\." } | Select-Object -First 14) | ForEach-Object { Write-Host "    $_" }
            }
        }
        if ($kind -eq "errors") { break }
    }
}
finally {
    Remove-Item $netrc -Force -ErrorAction SilentlyContinue
}
