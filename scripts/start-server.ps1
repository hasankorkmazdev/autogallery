<#
.SYNOPSIS
    Server'ı http://localhost:5080'de başlatır. 5080'i tutan ya da bin\Debug'ı kilitleyen eski sunucu önce kapatılır.
.EXAMPLE
    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/start-server.ps1
#>
$root = Split-Path -Parent $PSScriptRoot

& (Join-Path $PSScriptRoot 'free-port.ps1') -Port 5080 -ImagePath (Join-Path $root 'FatihTasci.Server\bin\Debug')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Set-Location $root
dotnet run --project FatihTasci.Server --launch-profile Vitrin
exit $LASTEXITCODE
