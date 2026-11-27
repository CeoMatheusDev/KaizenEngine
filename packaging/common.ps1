#
# Kaizen Engine — shared helpers for packaging scripts (Windows).
# Dot-source this file: . "$PSScriptRoot\..\common.ps1"
#

$script:KzPackagingDir = Split-Path -Parent $PSCommandPath
$script:KzRepoRoot = Split-Path -Parent $KzPackagingDir

$script:KzPlatform = "windows"
$script:KzOsDir = "Windows"
$script:KzArch = "x86_64"
$script:KzDefaultDist = Join-Path $KzRepoRoot "dist\Kaizen"

function Write-KzInfo([string]$Message) { Write-Host "[kz] $Message" }
function Write-KzOk([string]$Message) { Write-Host "[ ok ] $Message" -ForegroundColor Green }
function Write-KzWarn([string]$Message) { Write-Warning $Message }
function Write-KzErr([string]$Message) { Write-Host "[fail] $Message" -ForegroundColor Red }

function Stop-Kz([string]$Message) {
    Write-KzErr $Message
    exit 1
}

function Test-KzCommand([string]$Name) {
    return [bool](Get-Command $Name -ErrorAction SilentlyContinue)
}

function Get-KzJobs {
    if ($env:NUMBER_OF_PROCESSORS) { return [int]$env:NUMBER_OF_PROCESSORS }
    return 4
}

# Mirrors the SCons naming: kaizen.<platform>.<target>.<arch>.exe
function Get-KzBinaryName([string]$Target) {
    return "kaizen.$KzPlatform.$Target.$KzArch.exe"
}

function Get-KzBinaryPath([string]$Target, [string]$BinDir) {
    if (-not $BinDir) { $BinDir = Join-Path $KzRepoRoot "bin" }
    return Join-Path $BinDir (Get-KzBinaryName $Target)
}

function Get-KzVersionFullBuild([string]$Binary) {
    if (-not (Test-Path $Binary)) { return $null }
    $out = & $Binary --version 2>$null | Select-Object -First 1
    if (-not $out) { return $null }
    return $out.Trim()
}

function Get-KzVersionFullConfig([string]$FullBuild) {
    foreach ($tag in @(".custom_build", ".official", ".dev")) {
        if ($FullBuild.EndsWith($tag)) {
            return $FullBuild.Substring(0, $FullBuild.Length - $tag.Length)
        }
    }
    return $FullBuild
}

function Assert-KzRepo {
    if (-not (Test-Path (Join-Path $KzRepoRoot "SConstruct"))) {
        Stop-Kz "Not inside the Kaizen source repository (SConstruct not found): $KzRepoRoot"
    }
}

function Assert-KzOutputWritable([string]$Path, [bool]$Force) {
    if ((Test-Path $Path) -and (Get-ChildItem -Force $Path | Select-Object -First 1)) {
        if (-not $Force) {
            Stop-Kz "Output directory is not empty: $Path. Use -Force to replace it."
        }
        Write-KzWarn "Replacing existing output: $Path"
    }
}
