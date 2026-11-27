<#
.SYNOPSIS
    Kaizen Engine — Windows distribution validator.

.DESCRIPTION
    Verifies structure, binaries, export templates, managed (.NET) files and
    manifest integrity of a packaged Windows distribution.

    NOTE: provided for the Windows workflow but NOT validated in this
    repository (no Windows toolchain available).

.EXAMPLE
    .\packaging\windows\validate.ps1 -Dist dist\Kaizen -Mono yes
#>
[CmdletBinding()]
param(
    [string]$Dist = "",
    [ValidateSet("auto", "yes", "no")]
    [string]$Mono = "auto",
    [switch]$SkipRun,
    [switch]$Gui
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "..\common.ps1")

if (-not $Dist) { $Dist = $KzDefaultDist }
if (-not (Test-Path $Dist)) { Stop-Kz "Distribution directory not found: $Dist" }

$pass = 0
$fail = 0
$warn = 0

function Check-Ok([string]$Message) { $script:pass++; Write-Host "  PASS $Message" -ForegroundColor Green }
function Check-Warn([string]$Message) { $script:warn++; Write-Host "  WARN $Message" -ForegroundColor Yellow }
function Check-Fail([string]$Message) { $script:fail++; Write-Host "  FAIL $Message" -ForegroundColor Red }

function Require-File([string]$Path, [string]$Description) {
    if (Test-Path $Path) { Check-Ok $Description } else { Check-Fail "$Description (missing: $Path)" }
}

$binDir = Join-Path $Dist "Engine\Binaries\$KzOsDir"
$tplDir = Join-Path $Dist "Engine\ExportTemplates\$KzOsDir"
$managed = Join-Path $Dist "Engine\Managed\KaizenSharp"
$manifestPath = Join-Path $Dist "Engine\Build\Config\build-info.json"

$canonical = Get-ChildItem $binDir -Filter "kaizen.windows.*.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
if ($Mono -eq "auto") {
    $Mono = if ($canonical -and $canonical.Name -like "*.mono.exe") { "yes" } else { "no" }
}
$isMono = ($Mono -eq "yes")

Write-KzInfo "Validating Kaizen distribution"
Write-KzInfo "  dist : $Dist"
Write-KzInfo "  mono : $isMono"

# Structure.
Require-File (Join-Path $Dist "Setup\README.md") "setup notes"
Require-File (Join-Path $Dist "Samples\hello-kaizen\project.kaizen") "sample project"
Require-File (Join-Path $tplDir "Release\windows_release_$KzArch.exe") "release template"
Require-File (Join-Path $tplDir "Debug\windows_debug_$KzArch.exe") "debug template"

# Binary.
if (-not $canonical) {
    Check-Fail "canonical engine binary (kaizen.windows.*.exe) not found"
} else {
    Check-Ok "canonical binary: $($canonical.Name)"
    $version = Get-KzVersionFullBuild $canonical.FullName
    if ($version) { Check-Ok "engine version: $version" } else { Check-Fail "could not run the engine binary" }
    if (-not $SkipRun) {
        $tmp = Join-Path $env:TEMP ("kaizen-validate-" + [guid]::NewGuid())
        New-Item -ItemType Directory -Force -Path $tmp | Out-Null
        $oldHome = $env:USERPROFILE
        try {
            & $canonical.FullName --headless --quit | Out-Null
            if ($LASTEXITCODE -eq 0) { Check-Ok "headless startup completed" } else { Check-Fail "headless startup failed ($LASTEXITCODE)" }
        } finally {
            $env:USERPROFILE = $oldHome
            Remove-Item -Recurse -Force $tmp -ErrorAction SilentlyContinue
        }
    }
}

# Managed assemblies.
if ($isMono) {
    Require-File (Join-Path $managed "Api\Release\KaizenSharp.dll") "KaizenSharp API (Release)"
    Require-File (Join-Path $managed "Api\Debug\KaizenSharp.dll") "KaizenSharp API (Debug)"
    Require-File (Join-Path $managed "Tools\KaizenTools.dll") "KaizenTools"
    Require-File (Join-Path $managed "Tools\KaizenTools.deps.json") "KaizenTools deps manifest"
    Require-File (Join-Path $Dist "Samples\hello-kaizen-csharp\project.kaizen") "C# sample project"
    if (Get-ChildItem (Join-Path $managed "Tools\nupkgs") -Filter "Kaizen.NET.Sdk.*.nupkg" -ErrorAction SilentlyContinue) {
        Check-Ok "Kaizen.NET.Sdk NuGet package present"
    } else {
        Check-Fail "Kaizen.NET.Sdk NuGet package missing"
    }
    if (Test-Path (Join-Path $binDir "KaizenSharp")) { Check-Ok "KaizenSharp next to executable" } else { Check-Fail "KaizenSharp missing next to executable" }
}

# Manifest.
if (Test-Path $manifestPath) {
    Check-Ok "build manifest exists"
    $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
    $bad = 0
    foreach ($entry in $manifest.files) {
        $target = Join-Path $Dist ($entry.path -replace "/", "\")
        if (-not (Test-Path $target)) { Check-Fail "missing file: $($entry.path)"; $bad++ ; continue }
        $hash = (Get-FileHash $target -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($hash -ne $entry.sha256) { Check-Fail "checksum mismatch: $($entry.path)"; $bad++ }
    }
    if ($bad -eq 0) { Check-Ok "manifest verified ($($manifest.files.Count) files)" }
    else { $fail++ }
} else {
    Check-Fail "build manifest missing"
}

# Artifact hygiene.
foreach ($pattern in @("*.o", "*.obj", "*.a", "*.lib", ".sconsign*", "__pycache__")) {
    $found = Get-ChildItem -Recurse -Force -Filter $pattern $Dist -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) { Check-Fail "forbidden build artifact found: $pattern" } else { Check-Ok "no $pattern artifacts" }
}

Write-Host ""
Write-Host "Validation summary: $pass passed, $fail failed, $warn warnings"
if ($fail -gt 0) { exit 1 }
Write-KzOk "Distribution is valid."
