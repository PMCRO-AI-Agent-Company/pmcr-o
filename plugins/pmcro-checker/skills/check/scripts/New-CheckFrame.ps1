<#
.SYNOPSIS
  Deterministic append of one CheckFrame to a trail's check.jsonl.
  Zero reasoning: file mechanics and precondition checks only. Implements
  the accept path documented in ../assets/run.check.asset.md. The caller
  (an agent playing Checker) still evaluates each criterion against real
  evidence -- CriteriaJson, Verdict -- reasoning is not eliminated, only
  the mechanical file operations are.

.NOTES
  Reject codes on Write-Error match ../assets/reject.check.asset.md exactly,
  checked in the same order (first match wins). Never opens, edits, or
  writes any phase file other than this trail's check.jsonl.
#>
param(
  [string]$PmcroRoot = '.\.pmcro',
  [string]$TrailPath,
  [string]$TrailId,
  [string]$CriteriaJson,
  [ValidateSet('PASS', 'FAIL')][string]$Verdict
)

if (-not $TrailPath -and -not $TrailId) {
  Write-Error "missing-trail-id: supply -TrailId (with -PmcroRoot) or -TrailPath directly."
  exit 1
}

$trailDir = if ($TrailPath) { $TrailPath } else { Join-Path (Join-Path $PmcroRoot 'trails') $TrailId }

if (-not (Test-Path $trailDir)) {
  Write-Error "trail-not-found: $trailDir does not exist."
  exit 1
}

$disposition = Get-Content (Join-Path $trailDir 'disposition.json') -Raw | ConvertFrom-Json
if ($disposition.sealed) {
  Write-Error "trail-sealed: this trail is already sealed."
  exit 1
}
$TrailId = $disposition.trail_id

$planLines = @(Get-Content (Join-Path $trailDir 'plan.jsonl') -ErrorAction SilentlyContinue | Where-Object { $_.Trim() -ne '' })
if ($planLines.Count -eq 0) {
  Write-Error "no-plan-frame: plan.jsonl has no PlanFrame with success_criteria to check against."
  exit 1
}

$makeLines = @(Get-Content (Join-Path $trailDir 'make.jsonl') -ErrorAction SilentlyContinue | Where-Object { $_.Trim() -ne '' })
if ($makeLines.Count -eq 0) {
  Write-Error "no-make-steps: make.jsonl has no MakeStep entries yet."
  exit 1
}

if (-not $CriteriaJson -or -not $Verdict) {
  Write-Error "missing-criteria: -CriteriaJson (array of {check, result, evidence}) and -Verdict are both required."
  exit 1
}
try { $criteria = $CriteriaJson | ConvertFrom-Json } catch { $criteria = $null }
if (-not $criteria -or @($criteria).Count -eq 0) {
  Write-Error "missing-criteria: -CriteriaJson must be a non-empty JSON array."
  exit 1
}

$checkPath = Join-Path $trailDir 'check.jsonl'
$checkLines = @(Get-Content $checkPath -ErrorAction SilentlyContinue | Where-Object { $_.Trim() -ne '' })

$frame = [ordered]@{
  ts       = (Get-Date).ToUniversalTime().ToString('o')
  role     = 'checker'
  seq      = $checkLines.Count + 1
  type     = 'CheckFrame'
  criteria = @($criteria)
  verdict  = $Verdict
}

Add-Content -Path $checkPath -Value ($frame | ConvertTo-Json -Depth 10 -Compress) -Encoding utf8

[pscustomobject]@{
  status   = 'ok'
  trail_id = $TrailId
  phase    = 'check'
  seq      = $frame.seq
  verdict  = $Verdict
} | ConvertTo-Json -Depth 5
