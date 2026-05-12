#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [string]$OutputPath = "./openapi/swagger.json",
    [string]$ProjectPath = "../CpmServer/CpmServer.csproj",
    [string]$StartupAssembly = "../CpmServer/bin/Debug/net8.0/CpmServer.dll",
    [string]$DocumentName = "v1"
)

$ErrorActionPreference = "Stop"

$scriptsDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$rootDir = Split-Path -Parent $scriptsDir

$OutputPath = Join-Path $rootDir $OutputPath
$ProjectPath = Join-Path $rootDir $ProjectPath

# Ensure output directory exists
$outputDir = Split-Path -Parent $OutputPath
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

# Restore tools
Write-Host "Restoring dotnet tools..."
dotnet tool restore

# Build the project first to ensure the assembly exists
Write-Host "Building CpmServer..."
dotnet build $ProjectPath --configuration Debug

$StartupAssembly = Join-Path $rootDir $StartupAssembly

if (-not (Test-Path $StartupAssembly)) {
    Write-Error "Startup assembly not found at $StartupAssembly. Build may have failed."
    exit 1
}

# Generate OpenAPI document
Write-Host "Generating OpenAPI document to $OutputPath ..."
dotnet swagger tofile --output "$OutputPath" "$StartupAssembly" $DocumentName

Write-Host "Done. OpenAPI spec written to $OutputPath"
