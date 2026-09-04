<#
.SYNOPSIS
  Deterministic link of an EXISTING Class B PMCR-O trail. Zero reasoning:
  file mechanics only. Implements the link-path steps documented in
  ../assets/run.initialize.asset.md ("Steps (link path -- --trail-id
  <guid> supplied and the folder exists)").

.NOTES
  New-Trail.ps1 mints a brand-new trail and refuses (trail-already-exists)
  if the folder is already there. This script is the other half of the
  initialize contract: given a --trail-id that already exists on disk,
  verify it is open and unclaimed, then return the same success shape
  New-Trail.ps1 would -- without creating or modifying anything. It never
  writes to disposition.json or any *.jsonl file; it is read-only.

  Exactly one of -PmcroRoot / -TrailPath must be usable to derive the
  trail's folder, same convention as the other phase scripts -- -TrailId
  is only required when deriving the folder from -PmcroRoot; -TrailPath
  alone is sufficient (fixed 2026-09-04: the original version of this
  script wrongly required -TrailId even when -TrailPath was supplied
  directly, contradicting this paragraph and every sibling phase script's
  actual behavior).
#>
param(
  [string]$PmcroRoot,
  [string]$TrailPath,
  [string]$TrailId
)

if (-not $TrailPath -and -not $TrailId) {
  Write-Error "missing-target: supply -TrailId (with -PmcroRoot) or -TrailPath directly (New-Trail.ps1 is for minting a new one)."
  exit 1
}

if (-not $TrailPath -and -not $PmcroRoot) {
  Write-Error "missing-target: supply either -PmcroRoot (default trails/<guid> layout) or -TrailPath (exact folder override)."
  exit 1
}

$trailDir = if ($TrailPath) { $TrailPath } else { Join-Path (Join-Path $PmcroRoot 'trails') $TrailId }

if (-not (Test-Path $trailDir)) {
  Write-Error "trail-not-found: $trailDir does not exist. Use New-Trail.ps1 to mint a new trail instead."
  exit 1
}

$dispositionPath = Join-Path $trailDir 'disposition.json'
if (-not (Test-Path $dispositionPath)) {
  Write-Error "trail-not-found: $trailDir exists but has no disposition.json; it is not a valid trail folder."
  exit 1
}

$disposition = Get-Content $dispositionPath -Raw | ConvertFrom-Json

if ($disposition.trail_class -ne 'B') {
  Write-Error "class-a-unsupported: $trailDir is not a Class B trail. Test-TrailLink only links Class B (guid-folder) trails."
  exit 1
}

if ($disposition.sealed -eq $true) {
  Write-Error "trail-sealed: $($disposition.trail_id) is already sealed (disposition: $($disposition.disposition)). A sealed trail cannot be linked into a new cycle."
  exit 1
}

# "Linked" (claimed by an active cycle) has no dedicated disposition.json
# field -- by convention (matching New-OrchestrateFrame.ps1's own
# precondition check) a trail counts as linked once orchestrate.jsonl has
# at least one non-empty frame. Checked here too so pmcro-trail:initialize
# can be called standalone, without going through orchestrate first, and
# still give a correct answer.
$orchestratePath = Join-Path $trailDir 'orchestrate.jsonl'
$orchestrateLines = @()
if (Test-Path $orchestratePath) {
  $orchestrateLines = @(Get-Content $orchestratePath | Where-Object { $_.Trim() -ne '' })
}

if ($orchestrateLines.Count -gt 0) {
  Write-Error "trail-linked: $($disposition.trail_id) is already bound to an active cycle (orchestrate.jsonl has $($orchestrateLines.Count) frame(s))."
  exit 1
}

$TrailId = $disposition.trail_id
$reportedPath = if ($TrailPath) { $TrailPath } else { "trails/$TrailId/" }
$isScratch = if ($null -ne $disposition.scratch) { [bool]$disposition.scratch } else { [bool]$TrailPath }

[pscustomobject]@{
  status      = 'ok'
  trail_id    = $TrailId
  task_id     = if ($disposition.PSObject.Properties.Name -contains 'task_id') { $disposition.task_id } else { $null }
  trail_class = 'B'
  path        = $reportedPath
  scratch     = $isScratch
  files       = @('orchestrate.jsonl', 'plan.jsonl', 'make.jsonl', 'check.jsonl', 'reflect.jsonl', 'disposition.json')
} | ConvertTo-Json -Depth 5
