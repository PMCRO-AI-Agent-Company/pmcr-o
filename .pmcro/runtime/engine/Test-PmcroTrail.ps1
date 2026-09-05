<#
.SYNOPSIS
  Deterministic structural and governance validator for a Class-B trail.
.DESCRIPTION
  Read-only. Validates phase ownership, JSONL shape, disposition state,
  Checker gate, and Reflector seal authority. It never mutates the trail.
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$PmcroRoot,
  [Parameter(Mandatory)][string]$TrailId
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$trailDir = Join-Path (Join-Path $PmcroRoot 'trails') $TrailId
if (-not (Test-Path -LiteralPath $trailDir -PathType Container)) { throw "trail-not-found: $trailDir" }
$errors = [System.Collections.Generic.List[string]]::new()
$phaseRoles = [ordered]@{ orchestrate='orchestrator'; plan='planner'; make='maker'; check='checker'; reflect='reflector' }
foreach ($phase in $phaseRoles.Keys) {
  $path = Join-Path $trailDir "$phase.jsonl"
  if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { $errors.Add("missing-phase-file: $phase.jsonl"); continue }
  $seq = 0
  foreach ($line in @(Get-Content -LiteralPath $path | Where-Object { $_.Trim() -ne '' })) {
    try { $frame = $line | ConvertFrom-Json } catch { $errors.Add("invalid-json: $phase.jsonl"); continue }
    $seq++
    if ([string]$frame.role -ne $phaseRoles[$phase]) { $errors.Add("wrong-owner: $phase.jsonl expected $($phaseRoles[$phase])") }
    if ([int]$frame.seq -ne $seq) { $errors.Add("bad-seq: $phase.jsonl expected $seq") }
    if ([string]$frame.ts -notmatch 'Z$') { $errors.Add("non-utc-timestamp: $phase.jsonl seq $seq") }
  }
}
$dispPath = Join-Path $trailDir 'disposition.json'
if (-not (Test-Path -LiteralPath $dispPath -PathType Leaf)) { $errors.Add('missing-disposition') }
else {
  $disp = Get-Content $dispPath -Raw | ConvertFrom-Json
  if ([string]$disp.trail_id -ne $TrailId) { $errors.Add('trail-id-mismatch') }
  if ([string]$disp.trail_class -ne 'B') { $errors.Add('unsupported-trail-class') }
  $checkLines = @(Get-Content (Join-Path $trailDir 'check.jsonl') | Where-Object { $_.Trim() -ne '' })
  $reflectLines = @(Get-Content (Join-Path $trailDir 'reflect.jsonl') | Where-Object { $_.Trim() -ne '' })
  if ([bool]$disp.sealed) {
    if ($checkLines.Count -eq 0 -or [string](($checkLines[-1] | ConvertFrom-Json).verdict) -ne 'PASS') { $errors.Add('sealed-without-checker-pass') }
    if ($reflectLines.Count -eq 0) { $errors.Add('sealed-without-reflection') }
    elseif ([string](($reflectLines[-1] | ConvertFrom-Json).role) -ne 'reflector') { $errors.Add('sealed-without-reflector') }
  }
}
if ($errors.Count) {
  $errors | ForEach-Object { Write-Output "CHECKER: FAIL — $_" }
  exit 1
}
[pscustomobject]@{ status='ok'; trail_id=$TrailId; valid=$true; sealed=[bool]$disp.sealed } | ConvertTo-Json -Depth 5
