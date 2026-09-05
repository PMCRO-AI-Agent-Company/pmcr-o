<#
.SYNOPSIS
  Append one Reflection, optionally file one next seed, then seal a trail.
.DESCRIPTION
  Deterministic mechanics only. This is the sole lifecycle script allowed
  to seal a trail. Sealing is delegated to the central runtime seal gate,
  which requires Checker PASS and non-empty criterion evidence.
#>
param(
  [string]$PmcroRoot = '.\.pmcro',
  [string]$TrailPath,
  [string]$TrailId,
  [string]$Content,
  [ValidateSet('done', 'blocked', 'superseded', 'informational')][string]$Disposition,
  [string]$NextSeedJson,
  [string]$NextSeedId,
  [string]$QueueRoot
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $TrailPath -and -not $TrailId) { throw 'missing-trail-id: supply -TrailId (with -PmcroRoot) or -TrailPath directly.' }
$trailDir = if ($TrailPath) { $TrailPath } else { Join-Path (Join-Path $PmcroRoot 'trails') $TrailId }
if (-not (Test-Path -LiteralPath $trailDir -PathType Container)) { throw "trail-not-found: $trailDir" }
$dispositionPath = Join-Path $trailDir 'disposition.json'
$dispositionDoc = Get-Content -LiteralPath $dispositionPath -Raw | ConvertFrom-Json
if ([bool]$dispositionDoc.sealed) { throw 'trail-already-sealed: nothing to close.' }
$TrailId = [string]$dispositionDoc.trail_id

$checkPath = Join-Path $trailDir 'check.jsonl'
$checkLines = @(Get-Content -LiteralPath $checkPath -ErrorAction SilentlyContinue | Where-Object { $_.Trim() -ne '' })
if ($checkLines.Count -eq 0) { throw 'no-check-frame: check.jsonl has no CheckFrame verdict to reflect on.' }
$check = $checkLines[-1] | ConvertFrom-Json
if ([string]$check.role -ne 'checker' -or [string]$check.type -ne 'CheckFrame') { throw 'invalid-check-frame: final check entry is not a Checker CheckFrame.' }
if (-not $Disposition) { throw 'missing-disposition: -Disposition is required.' }
if ([bool]$NextSeedJson -ne [bool]$NextSeedId) { throw 'invalid-next-seed: -NextSeedJson and -NextSeedId must both be supplied together, or both omitted.' }

$reflectPath = Join-Path $trailDir 'reflect.jsonl'
$reflectLines = @(Get-Content -LiteralPath $reflectPath -ErrorAction SilentlyContinue | Where-Object { $_.Trim() -ne '' })
$nextSeedObj = $null
if ($NextSeedJson) { $nextSeedObj = $NextSeedJson | ConvertFrom-Json }
$frame = [ordered]@{
  ts = (Get-Date).ToUniversalTime().ToString('o')
  role = 'reflector'
  seq = $reflectLines.Count + 1
  type = 'Reflection'
  content = $Content
  disposition = $Disposition
  next_seed = $nextSeedObj
}
Add-Content -LiteralPath $reflectPath -Value ($frame | ConvertTo-Json -Depth 10 -Compress) -Encoding utf8

$seeded = $false
if ($nextSeedObj) {
  if (-not $QueueRoot) { $QueueRoot = Join-Path (Split-Path (Split-Path $trailDir -Parent) -Parent) 'queue' }
  New-Item -ItemType Directory -Force -Path $QueueRoot | Out-Null
  $queueItem = [ordered]@{
    id = $NextSeedId
    summary = $nextSeedObj.summary
    proposed_role = $nextSeedObj.proposed_role
    source_trail_id = $TrailId
    created_at = (Get-Date).ToUniversalTime().ToString('o')
    status = 'open'
  }
  $queueItem | ConvertTo-Json -Depth 5 | Out-File -LiteralPath (Join-Path $QueueRoot "$NextSeedId.json") -Encoding utf8
  $seeded = $true
}

# Reflector-only seal boundary. The central gate owns the final mutation.
$sealScript = Join-Path $PmcroRoot 'runtime\engine\Seal-PmcroTrail.ps1'
if (-not (Test-Path -LiteralPath $sealScript -PathType Leaf)) { throw "runtime-dependency-missing: $sealScript" }
& $sealScript -PmcroRoot $PmcroRoot -TrailId $TrailId | Out-String | Write-Verbose

[pscustomobject]@{
  status = 'ok'
  trail_id = $TrailId
  phase = 'reflect'
  seq = $frame.seq
  disposition = $Disposition
  sealed = $true
  next_seed_enqueued = $seeded
} | ConvertTo-Json -Depth 5
