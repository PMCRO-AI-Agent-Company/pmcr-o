<#
.SYNOPSIS
  Zip a single plugin folder for Cowork's "Upload a plugin" flow.

.NOTES
  Two gotchas this script works around, both discovered by actually running
  the naive version rather than just reading it:

  1. Do NOT use Compress-Archive for this: on Windows it writes internal zip
     entry paths with backslashes (e.g. ".claude-plugin\plugin.json"), which
     is not a valid path separator per the zip spec. Cowork's upload validator
     then reports "Zip file contains path with invalid characters" even
     though the manifest itself is correct.

  2. Do NOT pass a relative -OutFile straight into
     [System.IO.Compression.ZipFile]::Open(). PowerShell's Set-Location only
     moves PowerShell's own notion of "current location" — it does not move
     the underlying process's working directory that plain .NET file APIs
     resolve relative paths against. A relative path there silently resolves
     against the process CWD (e.g. C:\WINDOWS\system32) instead of wherever
     the script appears to be "in", and fails with an access-denied or
     wrong-location error. Fix: resolve every path through PowerShell's own
     path provider (GetUnresolvedProviderPathFromPSPath) before handing it
     to .NET.

.EXAMPLE
  .\scripts\New-PluginZip.ps1 -PluginDir .\plugins\pmcro-trail -OutFile .\pmcro-trail.zip
#>
param(
  [Parameter(Mandatory)][string]$PluginDir,
  [Parameter(Mandatory)][string]$OutFile
)

Add-Type -AssemblyName System.IO.Compression.FileSystem

$srcDir = (Resolve-Path $PluginDir).Path.TrimEnd('\', '/')

# Resolve OutFile against PowerShell's current location, not the process CWD,
# and make it absolute so ZipFile.Open never has to guess.
$outFileFull = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutFile)

if (Test-Path $outFileFull) { Remove-Item $outFileFull -Force }

$zip = [System.IO.Compression.ZipFile]::Open($outFileFull, 'Create')
try {
  $files = Get-ChildItem -Path $srcDir -Recurse -File
  foreach ($f in $files) {
    $rel = $f.FullName.Substring($srcDir.Length + 1) -replace '\\', '/'
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $f.FullName, $rel) | Out-Null
  }
}
finally {
  $zip.Dispose()
}

Write-Host "Wrote $outFileFull ($((Get-Item $outFileFull).Length) bytes, $($files.Count) files)"
