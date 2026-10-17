param(
  [ValidateSet('preflight','inspect','check')][string]$Mode='inspect',
  [string]$Repo='',
  [string]$Symptom=''
)
$ErrorActionPreference='Stop'
function Has([string]$n){ return $null -ne (Get-Command $n -ErrorAction SilentlyContinue) }
$gh=Has 'gh'; $git=Has 'git'
$login=$null; $apiOk=$false; $ghVersion=$null; $gitVersion=$null
if($gh){ $ghVersion=((gh --version | Select-Object -First 1) -join '').Trim(); try { $login=(gh api user --jq '.login' 2>$null).Trim(); $apiOk=[bool]$login } catch {} }
if($git){ $gitVersion=((git --version) -join '').Trim() }
$helper=$null
if($git){ try { $helper=(git config --global credential.helper 2>$null | Select-Object -First 1) } catch {} }
$hostsFile=Join-Path $env:APPDATA 'GitHub CLI\hosts.yml'
$remoteHosts=@()
if($Repo -and (Test-Path -LiteralPath $Repo) -and $git){
  try {
    foreach($u in @(git -C $Repo remote get-url --all origin 2>$null)){
      if($u -match '^git@([^:]+):'){ $remoteHosts += $Matches[1] }
      else { try { $remoteHosts += ([uri]$u).Host } catch {} }
    }
  } catch {}
}
$state=if($apiOk){'cli-authenticated-browser-recovery-pending'}else{'cli-not-authenticated'}
$result=[ordered]@{
  apiVersion='pmcro.github-account-recovery/v1'; kind='GitHubRecoveryInspection'; mode=$Mode
  readOnly=$true; ghInstalled=$gh; gitInstalled=$git; ghVersion=$ghVersion; gitVersion=$gitVersion
  ghApiAuthenticated=$apiOk; githubLogin=$login; ghHostsFileExists=(Test-Path -LiteralPath $hostsFile)
  credentialHelper=$helper; remoteHosts=@($remoteHosts | Sort-Object -Unique); symptom=$Symptom; state=$state
  officialRecovery=@(
    'https://github.com/login',
    'https://docs.github.com/en/authentication/securing-your-account-with-two-factor-authentication-2fa/troubleshooting-two-factor-authentication-issues',
    'https://docs.github.com/en/site-policy/other-site-policies/github-account-recovery-policy'
  )
  prohibited=@('credential-bypass','gh auth token','gh auth logout','token-revocation','disable-2fa','secret-export')
}
if($Mode -eq 'check'){
  $result['ok']=($gh -and $git)
  $result['safeOutput']=($null -eq (($result | ConvertTo-Json -Depth 8) | Select-String -Pattern 'gho_[A-Za-z0-9]|github_pat_[A-Za-z0-9]' -Quiet))
}
$result | ConvertTo-Json -Depth 8
