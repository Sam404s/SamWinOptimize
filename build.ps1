# SamWinOptimize build and packaging script
[CmdletBinding()]
param(
    [ValidateSet('win-x64', 'win-arm64', 'both')]
    [string]$Runtime = 'win-x64',
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [switch]$SkipInstaller
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$Project = Join-Path $Root 'src\SamWinOptimize\SamWinOptimize.csproj'
$PublishRoot = Join-Path $Root 'publish'
$DistRoot = Join-Path $Root 'dist'
$InnoCompiler = 'D:\ProgramFiles\Inno Setup 7\ISCC.exe'

[xml]$projectXml = Get-Content $Project
$Version = [string]$projectXml.Project.PropertyGroup.Version
if ([string]::IsNullOrWhiteSpace($Version)) {
    throw "Project version was not found in $Project"
}

$runtimes = if ($Runtime -eq 'both') { @('win-x64', 'win-arm64') } else { @($Runtime) }

Write-Host "Building SamWinOptimize $Version ($Configuration)" -ForegroundColor Cyan
dotnet restore (Join-Path $Root 'SamWinOptimize.slnx')
if ($LASTEXITCODE -ne 0) { throw 'dotnet restore failed.' }

dotnet build $Project --configuration $Configuration --no-restore
if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed.' }

New-Item -ItemType Directory -Path $PublishRoot, $DistRoot -Force | Out-Null
if (Test-Path $DistRoot) {
    Get-ChildItem -Path $DistRoot -Force | Remove-Item -Recurse -Force
}
foreach ($rid in $runtimes) {
    $publishDir = Join-Path $PublishRoot $rid
    if (Test-Path $publishDir) {
        Remove-Item $publishDir -Recurse -Force
    }

    Write-Host "Publishing $rid ..." -ForegroundColor Yellow
    dotnet publish $Project --configuration $Configuration --runtime $rid --self-contained true --no-restore `
        -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true `
        --output $publishDir
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $rid." }

    $zipPath = Join-Path $DistRoot "SamWinOptimize-$Version-$rid.zip"
    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
    Compress-Archive -Path (Join-Path $publishDir '*') -DestinationPath $zipPath -CompressionLevel Optimal
    Write-Host "  $zipPath" -ForegroundColor Green
}

if (-not $SkipInstaller -and $runtimes -contains 'win-x64' -and (Test-Path $InnoCompiler)) {
    Write-Host 'Building Inno Setup installer ...' -ForegroundColor Yellow
    & $InnoCompiler "/DMyAppVersion=$Version" (Join-Path $Root 'SamWinOptimize.iss')
    if ($LASTEXITCODE -ne 0) { throw 'Inno Setup compilation failed.' }
}
elseif (-not $SkipInstaller) {
    Write-Warning "Inno Setup compiler was not found at $InnoCompiler; ZIP packages were still created."
}

Write-Host "Packaging complete: $DistRoot" -ForegroundColor Green

