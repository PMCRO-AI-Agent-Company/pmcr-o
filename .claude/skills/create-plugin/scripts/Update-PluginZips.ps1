<#
.SYNOPSIS
  Rebuilds every plugin's .zip under plugins/ incrementally -- only the
  ones whose source files changed since their zip was last built (or that
  have no zip yet). A thin batch wrapper around New-PluginZip.ps1, not a
  reimplementation: every actual build still goes through that script, so
  its two gotchas (backslash paths, relative -OutFile resolving against
  the wrong process CWD) stay guarded against here automatically.

.NOTES
  "Incrementally" = the newest LastWriteTimeUtc among all files under a
  plugin's own directory is compared against its zip's LastWriteTimeUtc.
  Newer source (or no zip yet) triggers a rebuild; otherwise the plugin
  is reported up-to-date and skipped. -Force bypasses the comparison and
  rebuilds every plugin regardless.

  -Validate additionally runs `claude plugin validate <plugin-dir>` after
  each rebuild (only rebuilds, not skips) and reports pass/FAIL per
  plugin -- opt-in because it requires the claude CLI and roughly doubles
  the time per rebuilt plugin.

.EXAMPLE
  .\.claude\skills\create-plugin\scripts\Update-PluginZips.ps1
  .\.claude\skills\create-plugin\scripts\Update-PluginZips.ps1 -Force
  .\.claude\skills\create-plugin\scripts\Update-PluginZips.ps1 -Validate
#>
param(
  [string]$PluginsDir = '.\plugins',
  [string]$OutDir = '.',
  [switch]$Force,
  [switch]$Validate
)

$newPluginZip = Join-Path $PSScriptRoot 'New-PluginZip.ps1'
if (-not (Test-Path $newPluginZip)) {
  Write-Error "missing-dependency: New-PluginZip.ps1 not found next to this script at $newPluginZip."
  exit 1
}

$pluginsDirFull = Resolve-Path $PluginsDir -ErrorAction SilentlyContinue
if (-not $pluginsDirFull) {
  Write-Error "plugins-dir-not-found: $PluginsDir does not exist."
  exit 1
}

$outDirFull = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutDir)
if (-not (Test-Path $outDirFull)) { New-Item -ItemType Directory -Path $outDirFull -Force | Out-Null }

$pluginDirs = Get-ChildItem -Path $pluginsDirFull -Directory | Sort-Object Name
$rebuiltCount = 0
$skippedCount = 0
$failedValidate = @()

foreach ($dir in $pluginDirs) {
  $zipPath = Join-Path $outDirFull "$($dir.Name).zip"
  $newestSource = Get-ChildItem -Path $dir.FullName -Recurse -File |
    Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1

  $needsBuild = $Force -or -not (Test-Path $zipPath)
  if (-not $needsBuild -and $newestSource) {
    $zipTimeUtc = (Get-Item $zipPath).LastWriteTimeUtc
    if ($newestSource.LastWriteTimeUtc -gt $zipTimeUtc) { $needsBuild = $true }
  }

  if (-not $needsBuild) {
    $skippedCount++
    Write-Host "up-to-date   $($dir.Name)"
    continue
  }

  & $newPluginZip -PluginDir $dir.FullName -OutFile $zipPath | Out-Null
  $zipInfo = Get-Item $zipPath
  $rebuiltCount++
  $line = "rebuilt      $($dir.Name)   ($($zipInfo.Length) bytes)"

  if ($Validate) {
    $validateOutput = & claude plugin validate $dir.FullName 2>&1 | Out-String
    if ($validateOutput -match 'Validation passed') {
      $line += "   validate: pass"
    }
    else {
      $line += "   validate: FAIL"
      $failedValidate += $dir.Name
    }
  }
  Write-Host $line
}

Write-Host "---"
Write-Host "$rebuiltCount rebuilt, $skippedCount up-to-date, $($pluginDirs.Count) total."
if ($Validate -and $failedValidate.Count -gt 0) {
  Write-Error "validate-failed: $($failedValidate -join ', ')"
  exit 1
}
