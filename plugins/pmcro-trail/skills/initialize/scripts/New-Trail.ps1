<#
.SYNOPSIS
  Deterministic mint of a Class B PMCR-O trail. Zero reasoning: file
  mechanics only. Implements the accept path documented in
  ../assets/run.initialize.asset.md.

.NOTES
  Default location is <PmcroRoot>/trails/<TrailId> (production cycles).
  -OutputPath overrides this entirely, for scratch/testing runs that must
  not touch the real .pmcro/trails/ (e.g. exercising this script without
  polluting live evidence). Exactly one of -PmcroRoot / -OutputPath must be
  usable to derive a target folder.
#>
param(
  [string]$PmcroRoot,
  [string]$OutputPath,
  [string]$TrailId,
  [ValidateSet('A', 'B')][string]$Class = 'B'
)

if ($Class -eq 'A') {
  Write-Error "class-a-unsupported: New-Trail only mints Class B (guid-folder) trails."
  exit 1
}

if (-not $PmcroRoot -and -not $OutputPath) {
  Write-Error "missing-target: supply either -PmcroRoot (default trails/<guid> layout) or -OutputPath (exact folder override)."
  exit 1
}

if (-not $TrailId) {
  $TrailId = [guid]::NewGuid().ToString()
}

$trailPath = if ($OutputPath) { $OutputPath } else { Join-Path (Join-Path $PmcroRoot 'trails') $TrailId }

if (Test-Path $trailPath) {
  Write-Error "trail-already-exists: $trailPath already exists; New-Trail only mints new trails. Use the link path for an existing id."
  exit 1
}

New-Item -ItemType Directory -Path $trailPath -Force | Out-Null

foreach ($phase in @('plan', 'make', 'check', 'reflect')) {
  New-Item -ItemType File -Path (Join-Path $trailPath "$phase.jsonl") -Force | Out-Null
}

$disposition = [ordered]@{
  trail_id         = $TrailId
  trail_class      = 'B'
  engine_generated = $true
  opened_at        = (Get-Date).ToUniversalTime().ToString('o')
  disposition      = 'open'
  sealed           = $false
  frame_schema     = 'plugins/pmcro-trail/skills/initialize/assets/schema.trail-frame.asset.json'
  scratch          = [bool]$OutputPath
}

$disposition | ConvertTo-Json -Depth 5 |
  Out-File -FilePath (Join-Path $trailPath 'disposition.json') -Encoding utf8

$reportedPath = if ($OutputPath) { $OutputPath } else { "trails/$TrailId/" }

[pscustomobject]@{
  status      = 'ok'
  trail_id    = $TrailId
  trail_class = 'B'
  path        = $reportedPath
  scratch     = [bool]$OutputPath
  files       = @('plan.jsonl', 'make.jsonl', 'check.jsonl', 'reflect.jsonl', 'disposition.json')
} | ConvertTo-Json -Depth 5
