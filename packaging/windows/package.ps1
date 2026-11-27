<#
.SYNOPSIS
    Kaizen Engine — Windows packaging driver.

.DESCRIPTION
    Assembles a self-contained Windows distribution from artifacts produced by
    the SCons build. Does not include build intermediates, caches or source
    (unless -WithSource).

    NOTE: provided for the Windows workflow but NOT validated in this
    repository (no Windows toolchain available). Validate on Windows before
    distributing.

.EXAMPLE
    .\packaging\windows\package.ps1 -Output dist\Kaizen -Force
#>
[CmdletBinding()]
param(
    [string]$Output = "",
    [switch]$Force,
    [switch]$WithSource,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "..\common.ps1")

Assert-KzRepo
if (-not $Output) { $Output = $KzDefaultDist }

Write-KzInfo "Packaging Kaizen"

$editor = Get-KzBinaryPath "editor"
$release = Get-KzBinaryPath "template_release"
$debug = Get-KzBinaryPath "template_debug"
foreach ($f in @($editor, $release, $debug)) {
    if (-not (Test-Path $f)) { Stop-Kz "Missing build artifact: $f. Run build.ps1 first." }
}

$fullBuild = Get-KzVersionFullBuild $editor
if (-not $fullBuild) { Stop-Kz "Could not determine the engine version from $editor." }
$fullConfig = Get-KzVersionFullConfig $fullBuild


Write-KzInfo "  version  : $fullBuild"
Write-KzInfo "  templates: $fullConfig"
Write-KzInfo "  output   : $Output"

if ($DryRun) {
    Write-KzInfo "Dry run — no files written."
    exit 0
}

Assert-KzOutputWritable $Output $Force.IsPresent

$stage = "$Output.staging.$PID"
if (Test-Path $stage) { Remove-Item -Recurse -Force $stage }

$binDir = Join-Path $stage "Engine\Binaries\$KzOsDir"
$tplDir = Join-Path $stage "Engine\ExportTemplates\$KzOsDir"
$cfgDir = Join-Path $stage "Engine\Config\windows"
$buildCfg = Join-Path $stage "Engine\Build\Config"
$buildTools = Join-Path $stage "Engine\Build\Toolchains"

foreach ($d in @($binDir, (Join-Path $tplDir "Release"), (Join-Path $tplDir "Debug"), $cfgDir, $buildCfg, $buildTools, (Join-Path $stage "Setup"), (Join-Path $stage "Samples"))) {
    New-Item -ItemType Directory -Force -Path $d | Out-Null
}

# Engine binaries.
$editorName = Split-Path -Leaf $editor
Copy-Item $editor (Join-Path $binDir $editorName) -Force
Copy-Item (Join-Path $binDir $editorName) (Join-Path $binDir "kaizen.exe") -Force
Set-Content -Path (Join-Path $binDir "_sc_") -Value "" -NoNewline

# Export templates.
Copy-Item $release (Join-Path $tplDir "Release\windows_release_$KzArch.exe") -Force
Copy-Item $debug (Join-Path $tplDir "Debug\windows_debug_$KzArch.exe") -Force

# Portable mode: duplicate templates where the editor looks for them.
$portableTpl = Join-Path $binDir "editor_data\export_templates\$fullConfig"
New-Item -ItemType Directory -Force -Path $portableTpl | Out-Null
Copy-Item $release (Join-Path $portableTpl "windows_release_$KzArch.exe") -Force
Copy-Item $debug (Join-Path $portableTpl "windows_debug_$KzArch.exe") -Force


# Licenses and sample project.
$legal = Join-Path $stage "Engine\Config\legal"
New-Item -ItemType Directory -Force -Path $legal | Out-Null
foreach ($f in @("LICENSE.txt", "COPYRIGHT.txt", "AUTHORS.md")) {
    $src = Join-Path $KzRepoRoot $f
    if (Test-Path $src) { Copy-Item $src $legal -Force }
}
$sampleSrc = Join-Path $KzRepoRoot "packaging\samples\hello-kaizen"
if (Test-Path $sampleSrc) {
    Copy-Item $sampleSrc (Join-Path $stage "Samples\hello-kaizen") -Recurse -Force
    Remove-Item (Join-Path $stage "Samples\hello-kaizen\.kaizen") -Recurse -Force -ErrorAction SilentlyContinue
}

# Setup notes.
$setupNote = @"
# Kaizen Engine — Setup (Windows)

- Portable use: run ``Engine\Binaries\Windows\kaizen.exe`` directly. The ``_sc_``
  marker keeps editor data inside ``Engine\Binaries\Windows\editor_data\``.
- Export templates are also mirrored in that folder for portable use.
- No installer is provided for Windows; extract the archive anywhere.
"@
Set-Content -Path (Join-Path $stage "Setup\README.md") -Value $setupNote

# Manifest.
$files = Get-ChildItem -Recurse -File $stage | ForEach-Object {
    [pscustomobject]@{
        path = $_.FullName.Substring($stage.Length + 1).Replace("\", "/")
        size = $_.Length
        sha256 = (Get-FileHash $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    }
}
$manifest = [pscustomobject]@{
    schema = 1
    engine = [pscustomobject]@{
        name = "Kaizen Engine"
        short_name = "kaizen"
        version_full_build = $fullBuild
        version_full_config = $fullConfig
    }
    build = [pscustomobject]@{
        platform = $KzPlatform
        os_dir = $KzOsDir
        arch = $KzArch
        host = [System.Environment]::OSVersion.VersionString
        cc = "msvc/mingw"
        cxx = "msvc/mingw"
        source_included = $WithSource.IsPresent
        generated_at = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    }
    files = $files
    symlinks = @()
}
$manifest | ConvertTo-Json -Depth 6 | Set-Content -Path (Join-Path $buildCfg "build-info.json") -Encoding UTF8

if ($WithSource) {
    $src = Join-Path $stage "Engine\Source"
    New-Item -ItemType Directory -Force -Path $src | Out-Null
    $exclude = @("bin", "dist", ".sconsign5.dblite", ".scons_env.json", "__pycache__", "obj")
    Get-ChildItem $KzRepoRoot -Force | Where-Object { $_.Name -notin $exclude } | ForEach-Object {
        Copy-Item $_.FullName $src -Recurse -Force
    }
    Copy-Item (Join-Path $KzRepoRoot "packaging") (Join-Path $stage "Engine\Build\Scripts") -Recurse -Force
}

if (Test-Path $Output) { Remove-Item -Recurse -Force $Output }
Move-Item $stage $Output

Write-KzOk "Distribution ready: $Output"
Write-KzInfo "Validate with: .\packaging\windows\validate.ps1 -Dist $Output"
