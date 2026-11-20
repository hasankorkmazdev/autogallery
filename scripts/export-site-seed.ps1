<#
.SYNOPSIS
    Yerel veritabanindaki site ayarlarini (logo, icerik yazilari, calisma saatleri...) ve showroom fotograflarini
    FatihTasci.Server/Assets/Seed/Site altina disa aktarir; canli sitede DbSeeder bunlari bir kez ice alir.

.DESCRIPTION
    site.json: ayarlar (bos olmayan, "__" ile baslamayan anahtarlar) ve fotografli showroom sahneleri. Dosyalar
    Storage'daki goreli yollariyla public/ ve private/ altina kopyalanir. Canlida yalniz bos ya da hic olmayan ayar
    doldurulur, yalniz fotografsiz showroom sahnesine fotograf konur (panelde degistirilen ezilmez).
    Paket: dotnet publish ... -p:IncludeDemoData=true (Assets/Seed yalniz bu bayrakla pakete girer).
    Yeniden disa aktarinca Version artar; canlida yalniz yeni surum ice alinir.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts\export-site-seed.ps1
#>
param(
    [string]$ConnectionString = "Server=(localdb)\MSSQLLocalDB;Database=FatihTasci;Integrated Security=True;TrustServerCertificate=True",
    # Bos birakilirsa FatihTasci.Server/Storage
    [string]$StorageRoot
)

$ErrorActionPreference = "Stop"
$server = Join-Path (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)) "FatihTasci.Server"
if (-not $StorageRoot) { $StorageRoot = Join-Path $server "Storage" }
$target = Join-Path $server "Assets\Seed\Site"

$conn = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
$conn.Open()
function Get-Rows([string]$sql) {
    $command = $conn.CreateCommand()
    $command.CommandText = $sql
    $table = New-Object System.Data.DataTable
    $table.Load($command.ExecuteReader())
    , $table
}
try {
    $settings = Get-Rows "SELECT [Key], [Value] FROM Common.SiteSettings WHERE [Key] NOT LIKE '[_][_]%' AND [Value] IS NOT NULL AND LEN([Value]) > 0 ORDER BY [Key]"
    $scenes = Get-Rows "SELECT Title, Tag, OriginalPath, MediumPath, LargePath FROM Common.ShowroomScenes WHERE IsDelete = 0 AND LargePath IS NOT NULL ORDER BY SortOrder"
}
finally { $conn.Close() }

$previous = 0
$json = Join-Path $target "site.json"
if (Test-Path $json) { $previous = [int]((Get-Content $json -Raw -Encoding UTF8 | ConvertFrom-Json).Version) }
if (Test-Path $target) { Remove-Item $target -Recurse -Force }
New-Item -ItemType Directory $target | Out-Null

function Copy-StorageFile([string]$area, [string]$relative) {
    if (-not $relative) { return }
    $source = Join-Path (Join-Path $StorageRoot $area) ($relative -replace "/", "\")
    if (-not (Test-Path $source)) { throw "Dosya yok: $source" }
    $destination = Join-Path (Join-Path $target $area) ($relative -replace "/", "\")
    New-Item -ItemType Directory (Split-Path -Parent $destination) -Force | Out-Null
    Copy-Item $source $destination
}

$settingList = @(foreach ($row in $settings.Rows) { [ordered]@{ Key = $row.Key; Value = $row.Value } })
$logo = $settings.Rows | Where-Object { $_.Key -eq "LogoPath" } | Select-Object -First 1
if ($logo) { Copy-StorageFile "public" $logo.Value }

$sceneList = @(foreach ($row in $scenes.Rows) {
        Copy-StorageFile "private" $row.OriginalPath
        Copy-StorageFile "public" $row.MediumPath
        Copy-StorageFile "public" $row.LargePath
        [ordered]@{ Title = $row.Title; Tag = $row.Tag; OriginalPath = $row.OriginalPath; MediumPath = $row.MediumPath; LargePath = $row.LargePath }
    })

$document = [ordered]@{ Version = $previous + 1; Settings = $settingList; Showroom = $sceneList }
[System.IO.File]::WriteAllText($json, ($document | ConvertTo-Json -Depth 5), (New-Object System.Text.UTF8Encoding($false)))
Write-Host ("Surum {0}: {1} ayar, {2} showroom sahnesi, {3} dosya -> {4}" -f $document.Version, $settingList.Count, $sceneList.Count,
    (Get-ChildItem $target -Recurse -File).Count, $target)
