<#
.SYNOPSIS
  Deterministic PMCR-O queue claim and trail-link driver. No LLM calls.
.DESCRIPTION
  Claims exactly one pending Seed Intent using priority/FIFO ordering,
  allocates a Class-B trail through the shipped pmcro-trail initializer,
  records the Orchestrator frame through its existing deterministic script,
  and leaves Planner/Maker/Checker/Reflector reasoning to their roles.
  Queue state is pending -> claimed -> in_progress; completion belongs to
  the sealed-trail lifecycle and is never inferred by this script.
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$PmcroRoot,
  [string]$TaskId
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$queueRoot = Join-Path $PmcroRoot 'queue'
$pending = Join-Path $queueRoot 'pending'
$done = Join-Path $queueRoot 'done'
New-Item -ItemType Directory -Force -Path $pending,$done | Out-Null
$workspaceRoot = Split-Path -Parent $PmcroRoot
$newTrail = Join-Path $workspaceRoot 'plugins\pmcro-trail\skills\initialize\scripts\New-Trail.ps1'
$orchestrate = Join-Path $workspaceRoot 'plugins\pmcro-orchestrator\skills\orchestrate\scripts\New-OrchestrateFrame.ps1'
foreach ($script in @($newTrail,$orchestrate)) {
  if (-not (Test-Path -LiteralPath $script -PathType Leaf)) { throw "runtime-dependency-missing: $script" }
}

$lockPath = Join-Path $queueRoot '.cycle-run.lock'
$lock = $null
try {
  try {
    $lock = [IO.File]::Open($lockPath, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
    $bytes = [Text.Encoding]::UTF8.GetBytes("pid=$PID`nstarted=$((Get-Date).ToUniversalTime().ToString('o'))`n")
    $lock.Write($bytes,0,$bytes.Length); $lock.Flush()
  } catch [IO.IOException] {
    throw 'cycle-locked: another deterministic cycle invocation owns the queue lock'
  }

  function Read-Task([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $null }
    return (Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json)
  }

  function Write-AtomicJson([string]$Path, $Object) {
    $dir = Split-Path -Parent $Path
    $name = Split-Path -Leaf $Path
    $temp = Join-Path $dir ('.' + $name + '.' + [guid]::NewGuid().ToString('N') + '.tmp')
    try {
      $json = $Object | ConvertTo-Json -Depth 20
      [IO.File]::WriteAllText($temp, $json, (New-Object Text.UTF8Encoding($false)))
      if (Test-Path -LiteralPath $Path) { $backup = $temp + '.bak'; [IO.File]::Replace($temp, $Path, $backup, $true); if (Test-Path -LiteralPath $backup) { Remove-Item -LiteralPath $backup -Force } } else { [IO.File]::Move($temp, $Path) }
    } catch [IO.FileNotFoundException] {
      [IO.File]::Move($temp, $Path)
    } finally {
      if (Test-Path -LiteralPath $temp) { Remove-Item -LiteralPath $temp -Force }
    }
  }

  $files = @(Get-ChildItem -LiteralPath $pending -Filter '*.json' -File | Sort-Object Name)
  $candidates = @()
  foreach ($file in $files) {
    $task = Read-Task $file.FullName
    if ($null -ne $task -and $task.status -eq 'pending') {
      $candidates += [pscustomobject]@{ File=$file; Task=$task }
    }
  }
  if ($TaskId) {
    $candidates = @($candidates | Where-Object { $_.Task.id -eq $TaskId })
  } else {
    $candidates = @($candidates | Sort-Object @{Expression={[int]$_.Task.priority}}, @{Expression={$_.Task.created_at}}, @{Expression={$_.Task.id}})
  }
  if ($candidates.Count -eq 0) {
    [pscustomobject]@{ status='idle'; reason='no-pending-seed-intent' } | ConvertTo-Json -Depth 10
    exit 0
  }

  $selected = $candidates[0]
  $task = $selected.Task
  $taskPath = $selected.File.FullName
  $trailId = [guid]::NewGuid().ToString()
  $now = (Get-Date).ToUniversalTime().ToString('o')
  $task.status = 'claimed'
  $task.claimed_at = $now
  $task.trail_id = $trailId
  Write-AtomicJson -Path $taskPath -Object $task

  try {
    & $newTrail -PmcroRoot $PmcroRoot -TrailId $trailId -Class B | Out-String | Write-Verbose
    $trailPath = Join-Path (Join-Path $PmcroRoot 'trails') $trailId
    $frameText = if ($task.canonical_seed) { [string]$task.canonical_seed } else { [string]$task.messy_seed }
    & $orchestrate -PmcroRoot $PmcroRoot -TrailId $trailId -Task $task.id -Content $frameText | Out-String | Write-Verbose
  } catch {
    Write-Error "cycle-start-failed: $($_.Exception.Message); task remains claimed for deterministic recovery: $taskPath"
    exit 1
  }

  $task.status = 'in_progress'
  Write-AtomicJson -Path $taskPath -Object $task
  $dispositionPath = Join-Path (Join-Path $PmcroRoot 'trails') "$trailId\disposition.json"
  if (Test-Path -LiteralPath $dispositionPath) {
    $d = Get-Content -LiteralPath $dispositionPath -Raw | ConvertFrom-Json
    $d | Add-Member -NotePropertyName task_id -NotePropertyValue $task.id -Force
    Write-AtomicJson -Path $dispositionPath -Object $d
  }

  [pscustomobject]@{
    status='started'
    task_id=$task.id
    priority=$task.priority
    trail_id=$trailId
    queue_state=$task.status
    handed_off_to='planner'
    trail_path="trails/$trailId/"
  } | ConvertTo-Json -Depth 10
} finally {
  if ($null -ne $lock) { $lock.Dispose() }
  if (Test-Path -LiteralPath $lockPath) { Remove-Item -LiteralPath $lockPath -Force -ErrorAction SilentlyContinue }
}
