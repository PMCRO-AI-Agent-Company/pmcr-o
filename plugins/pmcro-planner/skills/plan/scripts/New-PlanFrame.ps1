<#
.SYNOPSIS
  Deterministic append of one PlanFrame to a trail's plan.jsonl.
  Zero reasoning: file mechanics and precondition checks only. Implements
  the accept path documented in ../assets/run.plan.asset.md. The caller
  (an agent playing Planner) still composes the frame's *substantive*
  content -- Goal, StepsJson, SuccessCriteriaJson -- reasoning is not
  eliminated, only the mechanical file operations are.

.NOTES
  Reject codes on Write-Error match ../assets/reject.plan.asset.md exactly,
  checked in the same order (first match wins). Never opens, edits, or
  writes any phase file other than this trail's plan.jsonl.
#>
param(
  [string]$PmcroRoot = '.\.pmcro',
  [string]$TrailPath,
  [string]$TrailId,
  [string]$Goal,
  [string]$StepsJson,
  [string]$SuccessCriteriaJson = '[]',
  [string]$OutOfScopeJson = '[]',
  [string]$ChosenName,
  [string]$NameRationale
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

$dispositionPath = Join-Path $trailDir 'disposition.json'
$disposition = Get-Content $dispositionPath -Raw | ConvertFrom-Json
if ($disposition.sealed) {
  Write-Error "trail-sealed: this trail is already sealed."
  exit 1
}
$TrailId = $disposition.trail_id

$orchestratePath = Join-Path $trailDir 'orchestrate.jsonl'
$orchestrateLines = @(Get-Content $orchestratePath -ErrorAction SilentlyContinue | Where-Object { $_.Trim() -ne '' })
if ($orchestrateLines.Count -eq 0) {
  Write-Error "no-orchestrate-frame: orchestrate.jsonl has no entry -- no cycle was opened to plan for."
  exit 1
}

$planPath = Join-Path $trailDir 'plan.jsonl'
$planLines = @(Get-Content $planPath -ErrorAction SilentlyContinue | Where-Object { $_.Trim() -ne '' })
if ($planLines.Count -gt 0) {
  Write-Error "already-planned: plan.jsonl already has a PlanFrame for this cycle's still-open task."
  exit 1
}

if (-not $Goal) {
  Write-Error "invalid-step-shape: -Goal is required."
  exit 1
}

try { $steps = $StepsJson | ConvertFrom-Json } catch { $steps = $null }
if (-not $steps -or @($steps).Count -eq 0) {
  Write-Error "invalid-step-shape: -StepsJson must be a non-empty JSON array of {index, action, subject_agent}."
  exit 1
}
foreach ($step in @($steps)) {
  if (-not ($step.PSObject.Properties.Name -contains 'index') -or
      -not ($step.PSObject.Properties.Name -contains 'action') -or
      -not ($step.PSObject.Properties.Name -contains 'subject_agent')) {
    Write-Error "invalid-step-shape: every step needs index, action, and subject_agent."
    exit 1
  }
}

$successCriteria = $SuccessCriteriaJson | ConvertFrom-Json
$outOfScope = $OutOfScopeJson | ConvertFrom-Json

# NOTE: arrays returned by ConvertFrom-Json must be re-wrapped with @(...)
# before being assigned into an object that will itself go through
# ConvertTo-Json -- otherwise Windows PowerShell 5.1 serializes them as
# {"value": [...], "Count": N} instead of a plain JSON array. Confirmed by
# direct repro; re-wrapping is the fix, not a workaround-of-convenience.
$frame = [ordered]@{
  ts    = (Get-Date).ToUniversalTime().ToString('o')
  role  = 'planner'
  seq   = $planLines.Count + 1
  type  = 'PlanFrame'
  goal  = $Goal
  steps = @($steps)
  success_criteria = @($successCriteria)
  out_of_scope     = @($outOfScope)
}
if ($ChosenName)    { $frame['chosen_name']    = $ChosenName }
if ($NameRationale) { $frame['name_rationale'] = $NameRationale }

Add-Content -Path $planPath -Value ($frame | ConvertTo-Json -Depth 10 -Compress) -Encoding utf8

[pscustomobject]@{
  status         = 'ok'
  trail_id       = $TrailId
  phase          = 'plan'
  seq            = $frame.seq
  handed_off_to  = 'maker'
} | ConvertTo-Json -Depth 5
