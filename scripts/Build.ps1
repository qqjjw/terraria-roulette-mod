param(
    [Parameter(Mandatory = $true)][string]$TModLoaderPath,
    [string]$DotNet = 'dotnet'
)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$tmlRoot = (Resolve-Path -LiteralPath $TModLoaderPath).Path
if (-not (Test-Path -LiteralPath (Join-Path $tmlRoot 'tModLoader.dll'))) {
    throw 'TModLoaderPath must contain tModLoader.dll.'
}
$source = (Resolve-Path (Join-Path $repoRoot 'src/PotionRoulette')).Path
$data = Join-Path $repoRoot '.build-tools/build-data'
$artifacts = Join-Path $repoRoot 'artifacts'
New-Item -ItemType Directory -Force $data,$artifacts | Out-Null
Push-Location $tmlRoot
try {
    # Compiler-only path: no world, game window or play testing.
    & $DotNet './tModLoader.dll' -server -build $source -savedirectory $data
    if ($LASTEXITCODE -ne 0) { throw "tModLoader build failed ($LASTEXITCODE)." }
} finally { Pop-Location }
$package = Get-ChildItem -LiteralPath $data -Recurse -Filter 'PotionRoulette.tmod' |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($null -eq $package) { throw 'Build did not produce PotionRoulette.tmod.' }
Copy-Item -LiteralPath $package.FullName -Destination (Join-Path $artifacts 'PotionRoulette.tmod') -Force
Get-FileHash -LiteralPath (Join-Path $artifacts 'PotionRoulette.tmod') -Algorithm SHA256
