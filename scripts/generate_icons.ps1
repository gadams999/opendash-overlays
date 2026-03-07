# Generate Windows icon PNGs from SVG source files
# Usage: .\scripts\generate_icons.ps1
# Requires: .NET 10 SDK

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir
$projectDir = Join-Path $scriptDir "IconGenerator"

Write-Host "Restoring packages..." -ForegroundColor Cyan
dotnet restore $projectDir --verbosity quiet

Write-Host "Running icon generator..." -ForegroundColor Cyan
dotnet run --project $projectDir -- $repoRoot

if ($LASTEXITCODE -ne 0) {
    Write-Host "Icon generation failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Icon generation complete." -ForegroundColor Green
Write-Host "Output: assets/icons/generated/" -ForegroundColor Green
