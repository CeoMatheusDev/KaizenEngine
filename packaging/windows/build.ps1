<#
.SYNOPSIS
    Kaizen Engine — Windows build driver.

.DESCRIPTION
    Wraps the engine's SCons build for Windows (MSVC or MinGW-w64). Builds the
    editor and/or export templates.

    NOTE: this script is provided for the Windows workflow but has NOT been
    validated in this repository (no Windows toolchain was available). Run it
    on Windows with Visual Studio Build Tools 2022+ or MinGW-w64 installed.

.EXAMPLE
    .\packaging\windows\build.ps1 -Target all -Jobs 16
#>
[CmdletBinding()]
param(
    [string[]]$Target = @("all"),
    [switch]$Dev,
    [switch]$Production,
    [int]$Jobs = 0,
    [string]$Arch = "x86_64",
    [switch]$DryRun,
    [switch]$VerboseBuild
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "..\common.ps1")

if ($Jobs -le 0) { $Jobs = Get-KzJobs }
Assert-KzRepo

if ($Dev -and $Production) { Stop-Kz "-Dev and -Production are mutually exclusive." }
if ($Arch -ne "x86_64") { Stop-Kz "Only x86_64 is supported by this packaging script." }

$expanded = @()
foreach ($t in $Target) {
    switch ($t) {
        "all" { $expanded += @("editor", "template_release", "template_debug") }
        { $_ -in @("editor", "template_debug", "template_release") } { $expanded += $t }
        default { Stop-Kz "Unknown target '$t'. Use editor, template_debug, template_release or all." }
    }
}
$expanded = $expanded | Select-Object -Unique

try {
    $scons = Get-Command scons -ErrorAction Stop
} catch {
    Stop-Kz "scons not found in PATH. Install SCons: python -m pip install scons"
}

$sconsArgs = @("platform=$KzPlatform", "arch=$Arch", "-j$Jobs")
if ($Dev) { $sconsArgs += "dev_build=yes" }
if ($Production) { $sconsArgs += "production=yes" }
if ($VerboseBuild) { $sconsArgs += "verbose=yes" }

Write-KzInfo "Kaizen Windows build"
Write-KzInfo "  targets    : $($expanded -join ', ')"
Write-KzInfo "  jobs       : $Jobs"

function Invoke-KzStep([string]$Description, [scriptblock]$Action) {
    Write-KzInfo $Description
    if ($DryRun) {
        Write-Host "  + $Description"
        return
    }
    & $Action
    if ($LASTEXITCODE -ne 0) { Stop-Kz "Step failed: $Description" }
}

foreach ($target in $expanded) {
    $argsForTarget = $sconsArgs + "target=$target"
    Invoke-KzStep "scons $($argsForTarget -join ' ')" {
        & $scons.Source @argsForTarget
    }
    if (-not $DryRun) {
        $binary = Get-KzBinaryPath $target
        if (-not (Test-Path $binary)) { Stop-Kz "Build finished but binary not found: $binary" }
        Write-KzOk "Built: $binary"
    }
}


Write-KzInfo "Build complete."
