<#
.SYNOPSIS
  Deterministic append of one MakeStep to a trail's make.jsonl.
  Zero reasoning: file mechanics and precondition checks only. Implements
  the accept path documented in ../assets/run.make.asset.md. The caller
  (an agent playing Maker) still performs the actual step and composes
  what really happened -- Action, Result -- reasoning is not eliminated,
  only the mechanical file operations are.

.NOTES
  Reject codes on Write-Error match ../assets/reject.make.asset.md exactly,
  checked in the same order (first match wins). Never opens, edits, or
  writes any phase file other than this trail's make.jsonl.
#>
param(
  [string]$PmcroRoot = '.\.pmcro',
  [string]$TrailPath,
  [string]$TrailId,
  [Nullable[int]]$StepIndex,
  [string]$Action,
  [ValidateSet('ok', 'failed', 'skipped')][string]$Result = 'ok'
)

if (-not $TrailPath -and -not $TrailId) {
  Write-Error "missing-trail-id: supply -TrailId (with -PmcroRoot) or -TrailPath directly."
  exit 1
}
if ($null -eq $StepIndex) {
  Write-Error "missing-step-index: -StepIndex is required."
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

$planPath = Join-Path $trailDir 'plan.jsonl'
$planLines = @(Get-Content $planPath -ErrorAction SilentlyContinue | Where-Object { $_.Trim() -ne '' })
if ($planLines.Count -eq 0) {
  Write-Error "no-plan-frame: plan.jsonl has no PlanFrame to execute against."
  exit 1
}
$planFrame = $planLines[-1] | ConvertFrom-Json
$matchingStep = @($planFrame.steps) | Where-Object { $_.index -eq $StepIndex }
if (-not $matchingStep) {
  Write-Error "unknown-step-index: $StepIndex doesn't match any steps[].index in the current PlanFrame."
  exit 1
}

if (-not $Action) {
  Write-Error "missing-action: -Action is required (what actually happened)."
  exit 1
}

$makePath = Join-Path $trailDir 'make.jsonl'
$makeLines = @(Get-Content $makePath -ErrorAction SilentlyContinue | Where-Object { $_.Trim() -ne '' })

$frame = [ordered]@{
  ts         = (Get-Date).ToUniversalTime().ToString('o')
  role       = 'maker'
  seq        = $makeLines.Count + 1
  type       = 'MakeStep'
  step_index = $StepIndex
  action     = $Action
  result     = $Result
}

Add-Content -Path $makePath -Value ($frame | ConvertTo-Json -Depth 10 -Compress) -Encoding utf8

# Determine hand-off: any step index ASSIGNED TO MAKER (subject_agent ==
# 'maker') with no MakeStep entry yet (including the one just written)
# means more work remains for Maker. Steps assigned to other roles (e.g. a
# checker-owned verification step) are not Maker's to attempt and must not
# block hand-off to Checker -- found by actually planning a cycle whose
# last step belonged to Checker, not by reading this logic in isolation.
$allMakeLines = @($makeLines) + @(($frame | ConvertTo-Json -Depth 10 -Compress))
$attemptedIndices = $allMakeLines | ForEach-Object { ($_ | ConvertFrom-Json).step_index } | Select-Object -Unique
$makerPlanIndices = @($planFrame.steps) | Where-Object { $_.subject_agent -eq 'maker' } | ForEach-Object { $_.index }
$remaining = $makerPlanIndices | Where-Object { $attemptedIndices -notcontains $_ }
$handedOffTo = if (@($remaining).Count -gt 0) { 'maker' } else { 'checker' }

[pscustomobject]@{
  status        = 'ok'
  trail_id      = $TrailId
  phase         = 'make'
  seq           = $frame.seq
  step_index    = $StepIndex
  result        = $Result
  handed_off_to = $handedOffTo
} | ConvertTo-Json -Depth 5
