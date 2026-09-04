<#
.SYNOPSIS
  Deterministic append of one Reflection, optional queue-item filing, and
  trail sealing. Zero reasoning: file mechanics and precondition checks
  only. Implements the accept path documented in
  ../assets/run.reflect-and-seed.asset.md. The caller (an agent playing
  Reflector) still decides the disposition and composes the narrative
  content and any next_seed -- reasoning is not eliminated, only the
  mechanical file operations (append, queue-item write, seal) are.

.NOTES
  Reject codes on Write-Error match ../assets/reject.reflect-and-seed.asset.md
  exactly, checked in the same order (first match wins). This is the only
  script in the colony permitted to flip a trail's disposition.json to
  sealed. -NextSeedJson and -NextSeedId must both be supplied, or both
  omitted -- a next_seed with nowhere to file it (or a filed item with no
  next_seed) is a caller error, not a partial success.
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
$dispositionDoc = Get-Content $dispositionPath -Raw | ConvertFrom-Json
if ($dispositionDoc.sealed) {
  Write-Error "trail-already-sealed: nothing to close."
  exit 1
}
$TrailId = $dispositionDoc.trail_id

$checkLines = @(Get-Content (Join-Path $trailDir 'check.jsonl') -ErrorAction SilentlyContinue | Where-Object { $_.Trim() -ne '' })
if ($checkLines.Count -eq 0) {
  Write-Error "no-check-frame: check.jsonl has no CheckFrame verdict to reflect on."
  exit 1
}

if (-not $Disposition) {
  Write-Error "missing-disposition: -Disposition is required (done|blocked|superseded|informational)."
  exit 1
}
if ([bool]$NextSeedJson -ne [bool]$NextSeedId) {
  Write-Error "invalid-next-seed: -NextSeedJson and -NextSeedId must both be supplied together, or both omitted."
  exit 1
}

$reflectPath = Join-Path $trailDir 'reflect.jsonl'
$reflectLines = @(Get-Content $reflectPath -ErrorAction SilentlyContinue | Where-Object { $_.Trim() -ne '' })

$nextSeedObj = $null
if ($NextSeedJson) { $nextSeedObj = $NextSeedJson | ConvertFrom-Json }

$frame = [ordered]@{
  ts          = (Get-Date).ToUniversalTime().ToString('o')
  role        = 'reflector'
  seq         = $reflectLines.Count + 1
  type        = 'Reflection'
  content     = $Content
  disposition = $Disposition
  next_seed   = $nextSeedObj
}

Add-Content -Path $reflectPath -Value ($frame | ConvertTo-Json -Depth 10 -Compress) -Encoding utf8

$seeded = $false
if ($nextSeedObj) {
  if (-not $QueueRoot) { $QueueRoot = Join-Path (Split-Path (Split-Path $PmcroRoot -Parent) -Parent) '.pmcro\queue' }
  if (-not (Test-Path $QueueRoot)) { New-Item -ItemType Directory -Path $QueueRoot -Force | Out-Null }
  $queueItem = [ordered]@{
    id               = $NextSeedId
    summary          = $nextSeedObj.summary
    proposed_role    = $nextSeedObj.proposed_role
    source_trail_id  = $TrailId
    created_at       = (Get-Date).ToUniversalTime().ToString('o')
    status           = 'open'
  }
  $queueItem | ConvertTo-Json -Depth 5 |
    Out-File -FilePath (Join-Path $QueueRoot "$NextSeedId.json") -Encoding utf8
  $seeded = $true
}

$dispositionDoc.sealed = $true
$dispositionDoc.disposition = $Disposition
$dispositionDoc | Add-Member -NotePropertyName sealed_at -NotePropertyValue ((Get-Date).ToUniversalTime().ToString('o')) -Force
$dispositionDoc | ConvertTo-Json -Depth 5 | Out-File -FilePath $dispositionPath -Encoding utf8

[pscustomobject]@{
  status              = 'ok'
  trail_id            = $TrailId
  phase               = 'reflect'
  seq                 = $frame.seq
  disposition         = $Disposition
  sealed              = $true
  next_seed_enqueued  = $seeded
} | ConvertTo-Json -Depth 5
