<#
.SYNOPSIS
  Deterministic regression check for Complete-ReflectAndSeed.ps1's
  QueueRoot path derivation. Exercises the two scenarios that motivated the
  fix landed in trail fdd09c8b-6517-4858-a6d0-be15659aa649 (an absolute
  -PmcroRoot silently wrote queue items outside the repo) and exits
  non-zero with a clear message if either regresses.

.NOTES
  Self-contained: mints its own disposable scratch trails under the system
  temp folder (never touches the repo's real .pmcro/trails or
  .pmcro/queue), and always cleans them up in a finally block, even on
  failure. Follows this repo's existing deterministic, zero-reasoning
  Test-*.ps1 convention (see pmcro-trail's Test-TrailLink.ps1) rather than
  adopting a testing framework such as Pester.

  -ReflectorScriptPath lets a caller point this check at a *different*
  copy of Complete-ReflectAndSeed.ps1 (e.g. a deliberately-reverted one)
  to prove the check actually catches the regression it targets, rather
  than always passing regardless of what it is checking. Defaults to the
  real script next to this one.
#>
param(
  [string]$ReflectorScriptPath,
  [string]$ScratchRoot
)

$ErrorActionPreference = 'Stop'

$pluginsRoot = Split-Path (Split-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) -Parent) -Parent
$TrailScript = Join-Path $pluginsRoot 'pmcro-trail\skills\initialize\scripts\New-Trail.ps1'
$OrchScript  = Join-Path $pluginsRoot 'pmcro-orchestrator\skills\orchestrate\scripts\New-OrchestrateFrame.ps1'
$PlanScript  = Join-Path $pluginsRoot 'pmcro-planner\skills\plan\scripts\New-PlanFrame.ps1'
$MakeScript  = Join-Path $pluginsRoot 'pmcro-maker\skills\make\scripts\New-MakeStep.ps1'
$CheckScript = Join-Path $pluginsRoot 'pmcro-checker\skills\check\scripts\New-CheckFrame.ps1'
if (-not $ReflectorScriptPath) { $ReflectorScriptPath = Join-Path $PSScriptRoot 'Complete-ReflectAndSeed.ps1' }

foreach ($p in @($TrailScript, $OrchScript, $PlanScript, $MakeScript, $CheckScript, $ReflectorScriptPath)) {
  if (-not (Test-Path $p)) {
    Write-Error "setup-error: required script not found: $p"
    exit 2
  }
}

if (-not $ScratchRoot) {
  $ScratchRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("pmcro-queueroot-test-" + [guid]::NewGuid().ToString('N'))
}

function Drive-MinimalCycle {
  param([string]$PmcroRootArg, [string]$SeedId, [switch]$UseDefaultPmcroRoot)

  $trailArgs = if ($UseDefaultPmcroRoot) { @{ PmcroRoot = '.\.pmcro' } } else { @{ PmcroRoot = $PmcroRootArg } }
  $trailId = (& $TrailScript @trailArgs -Class B | ConvertFrom-Json).trail_id

  $phaseArgs = if ($UseDefaultPmcroRoot) { @{} } else { @{ PmcroRoot = $PmcroRootArg } }
  & $OrchScript @phaseArgs -TrailId $trailId -Task $SeedId | Out-Null
  & $PlanScript @phaseArgs -TrailId $trailId -Goal 'v' -StepsJson '[{"index":1,"action":"noop","subject_agent":"maker"}]' -SuccessCriteriaJson '["x"]' | Out-Null
  & $MakeScript @phaseArgs -TrailId $trailId -StepIndex 1 -Action 'noop' -Result ok | Out-Null
  & $CheckScript @phaseArgs -TrailId $trailId -CriteriaJson '[{"check":"x","result":"PASS","evidence":"noop"}]' -Verdict PASS | Out-Null

  & $ReflectorScriptPath @phaseArgs -TrailId $trailId -Content 'v' -Disposition done `
    -NextSeedJson '{"summary":"s","proposed_role":"maker"}' -NextSeedId $SeedId | Out-Null

  return $trailId
}

$failures = @()

# Case A: absolute custom -PmcroRoot, distinct from CWD.
$rootA = Join-Path $ScratchRoot 'rootA'
try {
  Drive-MinimalCycle -PmcroRootArg $rootA -SeedId 'queueroot-check-a' | Out-Null
  $expectedA = Join-Path $rootA 'queue\queueroot-check-a.json'
  if (-not (Test-Path $expectedA)) {
    $failures += "Case A (absolute -PmcroRoot): queue item did not land at expected path $expectedA."
  }
} catch {
  $failures += "Case A (absolute -PmcroRoot): threw an exception -- $($_.Exception.Message)"
}

# Case B: default relative -PmcroRoot ('.\.pmcro'), CWD pushed to a scratch fake repo root.
$fakeRepo = Join-Path $ScratchRoot 'fakerepo'
New-Item -ItemType Directory -Path $fakeRepo -Force | Out-Null
Push-Location $fakeRepo
try {
  Drive-MinimalCycle -SeedId 'queueroot-check-b' -UseDefaultPmcroRoot | Out-Null
  $expectedB = Join-Path $fakeRepo '.pmcro\queue\queueroot-check-b.json'
  if (-not (Test-Path $expectedB)) {
    $failures += "Case B (default relative -PmcroRoot): queue item did not land at expected path $expectedB -- regression vs. original relative-path behavior."
  }
} catch {
  $failures += "Case B (default relative -PmcroRoot): threw an exception -- $($_.Exception.Message)"
} finally {
  Pop-Location
}

Remove-Item -Recurse -Force $ScratchRoot -ErrorAction SilentlyContinue

if ($failures.Count -gt 0) {
  Write-Error ("FAIL: QueueRoot derivation regression check failed against '$ReflectorScriptPath':`n - " + ($failures -join "`n - "))
  exit 1
}

Write-Output "PASS: QueueRoot derivation in '$ReflectorScriptPath' correctly resolves for both an absolute -PmcroRoot and the default relative -PmcroRoot."
exit 0
