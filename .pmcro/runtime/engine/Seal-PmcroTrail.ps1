<#
.SYNOPSIS
  Seal one Class-B PMCR-O trail after the Reflector has recorded its Reflection.
.DESCRIPTION
  This is the single durable mutation path for disposition.json sealed=true.
  It performs deterministic governance checks only: the trail must be open,
  contain a Checker PASS with evidence, and contain a final Reflector frame.
  It never writes phase files and never composes reasoning.

  Only the Reflector lifecycle adapter is permitted to call this script.
  The script intentionally exposes no generic "force" or "actor" bypass.
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$PmcroRoot,
  [Parameter(Mandatory)][string]$TrailId
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$trailDir = Join-Path (Join-Path $PmcroRoot 'trails') $TrailId
if (-not (Test-Path -LiteralPath $trailDir -PathType Container)) {
  throw "trail-not-found: $trailDir"
}

$dispositionPath = Join-Path $trailDir 'disposition.json'
$checkPath = Join-Path $trailDir 'check.jsonl'
$reflectPath = Join-Path $trailDir 'reflect.jsonl'
foreach ($path in @($dispositionPath, $checkPath, $reflectPath)) {
  if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
    throw "trail-incomplete: missing $path"
  }
}

$disposition = Get-Content -LiteralPath $dispositionPath -Raw | ConvertFrom-Json
if ([bool]$disposition.sealed) {
  throw 'trail-already-sealed: nothing to close.'
}
if ([string]$disposition.trail_class -ne 'B') {
  throw "class-unsupported: only Class B trails may be sealed by this runtime."
}
if ([string]$disposition.trail_id -ne $TrailId) {
  throw 'trail-id-mismatch: disposition.trail_id does not match requested TrailId.'
}

$checkLines = @(Get-Content -LiteralPath $checkPath -ErrorAction SilentlyContinue | Where-Object { $_.Trim() -ne '' })
if ($checkLines.Count -eq 0) {
  throw 'no-check-frame: check.jsonl has no CheckFrame verdict.'
}
$check = $checkLines[-1] | ConvertFrom-Json
if ([string]$check.role -ne 'checker' -or [string]$check.type -ne 'CheckFrame') {
  throw 'invalid-check-frame: final check entry is not a Checker CheckFrame.'
}
if ([string]$check.verdict -ne 'PASS') {
  throw 'checker-gate-failed: trail cannot be sealed unless the final CheckFrame verdict is PASS.'
}
$criteria = @($check.criteria)
if ($criteria.Count -eq 0) {
  throw 'evidence-required: CheckFrame has no criteria evidence.'
}
foreach ($criterion in $criteria) {
  if ([string]$criterion.result -ne 'PASS') {
    throw 'checker-gate-failed: all recorded criteria must PASS before seal.'
  }
  if ([string]::IsNullOrWhiteSpace([string]$criterion.evidence)) {
    throw 'evidence-required: every passing criterion must carry non-empty evidence.'
  }
}

$reflectLines = @(Get-Content -LiteralPath $reflectPath -ErrorAction SilentlyContinue | Where-Object { $_.Trim() -ne '' })
if ($reflectLines.Count -eq 0) {
  throw 'no-reflection: reflect.jsonl has no Reflection to authorize seal.'
}
$reflection = $reflectLines[-1] | ConvertFrom-Json
if ([string]$reflection.role -ne 'reflector' -or [string]$reflection.type -ne 'Reflection') {
  throw 'invalid-reflection: final reflect entry is not a Reflector Reflection.'
}
if ([string]::IsNullOrWhiteSpace([string]$reflection.disposition)) {
  throw 'missing-disposition: Reflection must declare a disposition before seal.'
}

$sealedAt = (Get-Date).ToUniversalTime().ToString('o')
$disposition.sealed = $true
$disposition.disposition = [string]$reflection.disposition
$disposition.sealed_at = $sealedAt

$temp = Join-Path $trailDir ('.disposition.' + [guid]::NewGuid().ToString('N') + '.tmp')
try {
  $json = $disposition | ConvertTo-Json -Depth 10
  [IO.File]::WriteAllText($temp, $json, (New-Object Text.UTF8Encoding($false)))
  [IO.File]::Replace($temp, $dispositionPath, $null, $true)
} catch [IO.FileNotFoundException] {
  [IO.File]::Move($temp, $dispositionPath)
} finally {
  if (Test-Path -LiteralPath $temp) { Remove-Item -LiteralPath $temp -Force }
}

[pscustomobject]@{
  status = 'ok'
  trail_id = $TrailId
  phase = 'reflect'
  disposition = [string]$reflection.disposition
  sealed = $true
  sealed_at = $sealedAt
  checker_verdict = [string]$check.verdict
  evidence_count = $criteria.Count
} | ConvertTo-Json -Depth 10
