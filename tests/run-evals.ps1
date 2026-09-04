[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$plugins = @('pmcro-orchestrator','pmcro-planner','pmcro-maker','pmcro-checker','pmcro-reflector','pmcro-trail')
$failures = [System.Collections.Generic.List[string]]::new()
function Require-File([string]$RelativePath) {
  if (-not (Test-Path -LiteralPath (Join-Path $Root $RelativePath) -PathType Leaf)) { $failures.Add("missing: $RelativePath") }
}

Require-File '.agents/skills/create-custom-agent/SKILL.md'
foreach ($p in $plugins) {
  Require-File "plugins/$p/plugin.json"
  $skillsRoot = Join-Path $Root "plugins/$p/skills"
  $skills = @(Get-ChildItem $skillsRoot -Directory)
  if ($skills.Count -eq 0) { $failures.Add("missing skill directory: $p"); continue }
  foreach ($skill in $skills) {
    foreach ($f in @('SKILL.md','assets','references','scripts')) {
      if (-not (Test-Path -LiteralPath (Join-Path $skill.FullName $f))) { $failures.Add("missing $p/$($skill.Name)/$f") }
    }
    $eval = Join-Path $Root "tests/$p/$($skill.Name)/eval.yaml"
    Require-File "tests/$p/$($skill.Name)/eval.yaml"
    if (Test-Path $eval) {
      $lines = @(Get-Content $eval)
      $trialCount = @($lines | Where-Object { $_ -match '^  - id:\s+\d+' }).Count
      if ($trialCount -ne 6) { $failures.Add("$p/$($skill.Name) trial count=$trialCount, expected 6") }
      if (-not ($lines -match '^pass_threshold:\s+')) { $failures.Add("$p/$($skill.Name) missing pass_threshold") }
    }
  }
}
foreach ($f in @(
  'AGENTS.md','CONTEXT.md','laws.md','.pmcro/manifest.yaml','.pmcro/AGENTS.md',
  '.pmcro/HANDOFF.md','.pmcro/state/STATE.md','.pmcro/queue/seed-intent.schema.json',
  '.pmcro/runtime/engine/Enqueue-SeedIntent.ps1','.pmcro/runtime/engine/Invoke-PmcroCycle.ps1',
  '.pmcro/workflows/declarative/pmcro-cycle.yaml','.pmcro/workflows/declarative/README.md',
  'env/README.md','env/.env.example','.claude-plugin/marketplace.json',
  '.cursor-plugin/marketplace.json','.codex-plugin/plugin.json','.agents/plugins/marketplace.json')) { Require-File $f }

foreach ($f in @('.claude-plugin/marketplace.json','.cursor-plugin/marketplace.json','.codex-plugin/plugin.json','.agents/plugins/marketplace.json','.pmcro/queue/seed-intent.schema.json')) {
  try { $null = Get-Content (Join-Path $Root $f) -Raw | ConvertFrom-Json } catch { $failures.Add("invalid JSON: $f") }
}

$workflow = Get-Content (Join-Path $Root '.pmcro/workflows/declarative/pmcro-cycle.yaml') -Raw
foreach ($required in @('kind: Workflow','kind: OnConversationStart','InvokeAzureAgent','InvokeMcpTool','EndWorkflow')) {
  if ($workflow -notmatch [regex]::Escape($required)) { $failures.Add("declarative workflow missing: $required") }
}
if ($workflow -notmatch 'learn\.microsoft\.com/api/mcp') { $failures.Add('declarative workflow missing governed MCP endpoint') }
if ($workflow -notmatch 'requireApproval:\s+true') { $failures.Add('declarative workflow MCP action must require approval') }

foreach ($f in @('.pmcro/runtime/engine/Enqueue-SeedIntent.ps1','.pmcro/runtime/engine/Invoke-PmcroCycle.ps1')) {
  $tokens=$null; $errors=$null
  [System.Management.Automation.Language.Parser]::ParseFile((Join-Path $Root $f),[ref]$tokens,[ref]$errors)|Out-Null
  if ($errors.Count -gt 0) { $failures.Add("PowerShell parse errors: $f") }
}

if ($failures.Count -gt 0) {
  $failures | ForEach-Object { Write-Error $_ }
  exit 1
}
Write-Output 'ALIGNMENT_SURFACE_OK plugins=6 skills=6 evals=6_trials_each manifests=4 workflow=maf-declarative+mcp env=template'
exit 0
