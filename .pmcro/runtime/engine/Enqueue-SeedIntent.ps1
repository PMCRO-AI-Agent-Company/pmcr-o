<#
.SYNOPSIS
  Deterministically validate and enqueue one PMCR-O Seed Intent.
.DESCRIPTION
  Preserves the raw human message as messy_seed, assigns a stable task id
  when one is not supplied, validates the resulting object against the
  repository seed-intent contract, and atomically creates one JSON file
  under .pmcro/queue/pending/. No LLM calls and no existing-task mutation.
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$PmcroRoot,
  [Parameter(Mandatory)][string]$MessySeed,
  [ValidateRange(1,5)][int]$Priority = 3,
  [string]$Domain = 'general',
  [string]$CreatedBy = 'human',
  [string]$Id,
  [string]$CanonicalSeed
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$queueRoot = Join-Path $PmcroRoot 'queue'
$pending = Join-Path $queueRoot 'pending'
$schemaPath = Join-Path $queueRoot 'seed-intent.schema.json'
if (-not (Test-Path -LiteralPath $schemaPath -PathType Leaf)) { throw "seed-intent-schema-missing: $schemaPath" }
New-Item -ItemType Directory -Force -Path $pending | Out-Null

function New-TaskId {
  param([Parameter(Mandatory)][string]$Seed)
  $bytes = [Text.Encoding]::UTF8.GetBytes($Seed)
  $sha = [Security.Cryptography.SHA256]::Create()
  try { $hash = $sha.ComputeHash($bytes) } finally { $sha.Dispose() }
  $hex = ([BitConverter]::ToString($hash) -replace '-', '').ToLowerInvariant()
  return "task-$($hex.Substring(0,16))"
}

function Assert-SeedIntent {
  param([Parameter(Mandatory)]$Item)
  foreach ($name in @('id','priority','domain','status','created_by','messy_seed','created_at')) {
    if (-not ($Item.PSObject.Properties.Name -contains $name)) { throw "seed-intent-invalid: missing required property '$name'" }
  }
  if ([string]$Item.id -notmatch '^task-[a-z0-9-]+$') { throw 'seed-intent-invalid: invalid id' }
  if ([int]$Item.priority -lt 1 -or [int]$Item.priority -gt 5) { throw 'seed-intent-invalid: priority must be 1..5' }
  if ([string]$Item.status -ne 'pending') { throw 'seed-intent-invalid: new items must be pending' }
  foreach ($name in @('domain','created_by','messy_seed')) {
    if ([string]::IsNullOrWhiteSpace([string]$Item.$name)) { throw "seed-intent-invalid: $name must be non-empty" }
  }
  $dt = [DateTimeOffset]::MinValue
  if (-not [DateTimeOffset]::TryParse([string]$Item.created_at, [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::RoundtripKind, [ref]$dt)) { throw 'seed-intent-invalid: created_at must be ISO date-time' }
  $allowed = @('id','priority','domain','status','created_by','messy_seed','canonical_seed','trail_id','created_at','claimed_at')
  foreach ($name in $Item.PSObject.Properties.Name) { if ($name -notin $allowed) { throw "seed-intent-invalid: additional property '$name' is not allowed" } }
}

if ([string]::IsNullOrWhiteSpace($MessySeed)) { throw 'seed-intent-invalid: MessySeed must be non-empty' }
if ([string]::IsNullOrWhiteSpace($Id)) { $Id = New-TaskId -Seed $MessySeed }
if ($Id -notmatch '^task-[a-z0-9-]+$') { throw 'seed-intent-invalid: Id must match ^task-[a-z0-9-]+$' }
$destination = Join-Path $pending "$Id.json"
if (Test-Path -LiteralPath $destination) { throw "seed-intent-duplicate: $Id already exists in pending/" }
$doneDestination = Join-Path (Join-Path $queueRoot 'done') "$Id.json"
if (Test-Path -LiteralPath $doneDestination) { throw "seed-intent-duplicate: $Id already exists in done/" }

$now = (Get-Date).ToUniversalTime().ToString('o')
$item = [ordered]@{
  id = $Id
  priority = $Priority
  domain = $Domain
  status = 'pending'
  created_by = $CreatedBy
  messy_seed = $MessySeed
  canonical_seed = if ([string]::IsNullOrWhiteSpace($CanonicalSeed)) { $null } else { $CanonicalSeed }
  trail_id = $null
  created_at = $now
  claimed_at = $null
}
$json = $item | ConvertTo-Json -Depth 10
$roundTrip = $json | ConvertFrom-Json
Assert-SeedIntent -Item $roundTrip

$temp = Join-Path $pending ('.' + $Id + '.' + [guid]::NewGuid().ToString('N') + '.tmp')
try {
  [IO.File]::WriteAllText($temp, $json, (New-Object Text.UTF8Encoding($false)))
  [IO.File]::Move($temp, $destination)
} finally {
  if (Test-Path -LiteralPath $temp) { Remove-Item -LiteralPath $temp -Force }
}

$item | ConvertTo-Json -Depth 10
