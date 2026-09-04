<#
.SYNOPSIS
  Deterministic append of one orchestrator frame to a trail's
  orchestrate.jsonl, claiming that trail for a cycle. Zero reasoning: file
  mechanics and precondition checks only. Implements the "log the
  orchestrator's own frame" step of ../assets/run.orchestrate.asset.md.
  The caller (an agent playing Orchestrator) still decides what task is
  being claimed and composes the narrative -- reasoning is not
  eliminated, only the mechanical file operations are.

.NOTES
  Does NOT mint or link a trail itself -- for the mint path, call
  pmcro-trail's New-Trail.ps1 first to get a fresh -TrailId, then call
  this script to claim/log it. For the link path (an existing --trail-id
  supplied to `orchestrate run`), call this script directly against that
  id: its own precondition checks (exists, not sealed, not already
  claimed) ARE the link-path verification -- there is no separate link
  script, because claiming and verifying are the same operation.

  Reject codes on Write-Error match ../assets/reject.orchestrate.asset.md
  exactly, checked in the same order (first match wins), plus
  `trail-linked` for a trail that already has an orchestrate.jsonl entry
  (already claimed by a prior cycle). Never opens, edits, or writes any
  phase file other than this trail's orchestrate.jsonl.
#>
param(
  [string]$PmcroRoot = '.\.pmcro',
  [string]$TrailPath,
  [string]$TrailId,
  [string]$Task,
  [string]$Content
)

if (-not $TrailPath -and -not $TrailId) {
  Write-Error "trail-not-found: supply -TrailId (with -PmcroRoot) or -TrailPath directly."
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

$orchestratePath = Join-Path $trailDir 'orchestrate.jsonl'
$orchestrateLines = @(Get-Content $orchestratePath -ErrorAction SilentlyContinue | Where-Object { $_.Trim() -ne '' })
if ($orchestrateLines.Count -gt 0) {
  Write-Error "trail-linked: this trail is already claimed by a prior cycle (orchestrate.jsonl already has an entry)."
  exit 1
}

if (-not $Task) {
  Write-Error "missing-task: -Task is required."
  exit 1
}

if (-not $Content) { $Content = $Task }

$frame = [ordered]@{
  ts      = (Get-Date).ToUniversalTime().ToString('o')
  role    = 'orchestrator'
  seq     = 1
  type    = 'MessySeedIntent'
  content = $Content
}

Add-Content -Path $orchestratePath -Value ($frame | ConvertTo-Json -Depth 10 -Compress) -Encoding utf8

[pscustomobject]@{
  status        = 'ok'
  trail_id      = $TrailId
  task          = $Task
  handed_off_to = 'planner'
} | ConvertTo-Json -Depth 5
