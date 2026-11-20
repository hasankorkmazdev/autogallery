<#
.SYNOPSIS
    Tek komutla yayin: (istege bagli) paneli derler, yayin paketini uretir, yalniz degisen dosyalari FTP ile yukler.

.DESCRIPTION
    Proje klasorleri bir ust klasordeki deploy.json'dan (ServerProject, PanelDir, PublishDir).
    1. -Panel: PanelDir'de npm run build.
    2. dotnet publish <ServerProject> -c Release -o <PublishDir> (-Property ile ek MSBuild ozellikleri).
    3. ftp-deploy.ps1: son yuklemeden bu yana degisen dosyalar (-All: hepsi, -WhatIf: yalniz liste).

    FTP hesabi bir kez ftp-login.ps1 ile kaydedilir; sonra sifre sorulmaz.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts\deploy\deploy.ps1
    powershell -ExecutionPolicy Bypass -File scripts\deploy\deploy.ps1 -Panel
    powershell -ExecutionPolicy Bypass -File scripts\deploy\deploy.ps1 -Property IncludeDemoData=true
    powershell -ExecutionPolicy Bypass -File scripts\deploy\deploy.ps1 -RemotePath /test.ornek.com
#>
param(
    # Once paneli derle (deploy.json'da PanelDir olmali)
    [switch]$Panel,
    # dotnet publish'e -p: ile gecen ozellikler (orn. IncludeDemoData=true)
    [string[]]$Property,
    # Degisenler yerine tum paketi yukle
    [switch]$All,
    # Derle, ama yuklemeden yalniz neyin gidecegini goster
    [switch]$WhatIf,
    # Kayitli klasor yerine baska bir klasore (orn. test sitesi: /test.ornek.com); degisen listesi klasor basina ayri
    [string]$RemotePath
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "ftp-common.ps1")
$config = $script:DeployConfig

if ($Panel) {
    if (-not $config.PanelDir) { throw "deploy.json'da PanelDir yok." }
    Write-Host "== Panel derleniyor ==" -ForegroundColor Cyan
    & npm --prefix (Get-RepoPath $config.PanelDir) run build
    if ($LASTEXITCODE -ne 0) { throw "Panel derlenemedi (npm run build)." }
}

Write-Host "== Yayin paketi ==" -ForegroundColor Cyan
$publishArgs = @("publish", (Get-RepoPath $config.ServerProject), "-c", "Release", "-o", (Get-RepoPath $config.PublishDir), "--nologo", "-v", "quiet")
foreach ($item in @($Property | ForEach-Object { $_ -split "," } | Where-Object { $_ })) { $publishArgs += "-p:" + $item.Trim() }
& dotnet @publishArgs
if ($LASTEXITCODE -ne 0) { throw "dotnet publish basarisiz." }

Write-Host "== Yukleme ==" -ForegroundColor Cyan
$deployArgs = @{}
if ($All) { $deployArgs.All = $true }
if ($WhatIf) { $deployArgs.WhatIf = $true }
if ($RemotePath) { $deployArgs.RemotePath = $RemotePath }
& (Join-Path $PSScriptRoot "ftp-deploy.ps1") @deployArgs
