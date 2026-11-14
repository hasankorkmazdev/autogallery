<#
.SYNOPSIS
    Verilen portları dinleyen süreçleri kapatır; başlatmadan önce eski sunucuyu temizlemek için kullanılır.
.PARAMETER Port
    Boşaltılacak port(lar), ör. 5080 ya da 5080,5173.
.PARAMETER ImagePath
    İsteğe bağlı: çalıştırılabilir dosyası bu klasörde olan süreçler de kapatılır
    (ör. FatihTasci.Server\bin\Debug: port dinlemese de derlemeyi kilitleyen eski sunucu).
.EXAMPLE
    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/free-port.ps1 -Port 5173
#>
param(
    # -File ile çağrılınca "5080,5173" tek metin olarak gelir; virgülle ayrılmış değerler ayrıca bölünür.
    [Parameter(Mandatory = $true)][string[]]$Port,
    [string]$ImagePath
)
$Port = @($Port -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ } | ForEach-Object { [int]$_ })

# Turkce karakterler konsolda / VS cikti penceresinde bozulmasin.
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$ids = @()
foreach ($p in $Port) {
    $ids += Get-NetTCPConnection -LocalPort $p -State Listen -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty OwningProcess
}
if ($ImagePath) {
    $full = [System.IO.Path]::GetFullPath($ImagePath)
    $ids += Get-Process -ErrorAction SilentlyContinue |
        Where-Object { $_.Path -and $_.Path.StartsWith($full, [System.StringComparison]::OrdinalIgnoreCase) } |
        Select-Object -ExpandProperty Id
}

$ids = $ids | Where-Object { $_ -and $_ -ne 0 -and $_ -ne $PID } | Sort-Object -Unique
foreach ($id in $ids) {
    $proc = Get-Process -Id $id -ErrorAction SilentlyContinue
    if ($proc) {
        Write-Host "Eski süreç kapatılıyor: $($proc.ProcessName) (PID $id)"
        Stop-Process -Id $id -Force -ErrorAction SilentlyContinue
    }
}

# Port gerçekten boşalana kadar bekle (en fazla 10 sn).
$deadline = (Get-Date).AddSeconds(10)
while ((Get-Date) -lt $deadline) {
    $busy = $Port | Where-Object { Get-NetTCPConnection -LocalPort $_ -State Listen -ErrorAction SilentlyContinue }
    if (-not $busy) { exit 0 }
    Start-Sleep -Milliseconds 250
}
Write-Error "Port boşaltılamadı: $($busy -join ', ')"
exit 1
