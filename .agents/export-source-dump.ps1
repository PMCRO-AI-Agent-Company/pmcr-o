[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Root,

    [Parameter(Mandatory = $true)]
    [string[]]$Exclude,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$rootPath = (Resolve-Path -LiteralPath $Root).Path.TrimEnd('\')
if (-not (Test-Path -LiteralPath $rootPath -PathType Container)) {
    throw "Root must be a directory: $Root"
}

if ([IO.Path]::IsPathRooted($OutputPath)) {
    $destination = [IO.Path]::GetFullPath($OutputPath)
} else {
    $destination = [IO.Path]::GetFullPath((Join-Path $rootPath $OutputPath))
}

$rootDrive = Split-Path -Qualifier $destination
if ($destination -eq $rootDrive) {
    throw "OutputPath cannot be a drive root: $destination. Provide a full file path."
}

if ([string]::IsNullOrWhiteSpace([IO.Path]::GetFileName($destination))) {
    throw "OutputPath must include a filename: $destination"
}

$destinationParent = Split-Path -Parent $destination
if (-not (Test-Path -LiteralPath $destinationParent -PathType Container)) {
    $null = New-Item -ItemType Directory -Path $destinationParent -Force
}

$excludePatterns = @($Exclude | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
$items = Get-ChildItem -LiteralPath $rootPath -Recurse -Force -File |
    Where-Object {
        $relative = $_.FullName.Substring($rootPath.Length).TrimStart('\')
        $isDestination = [StringComparer]::OrdinalIgnoreCase.Equals($_.FullName, $destination)
        $isExcluded = $false
        foreach ($pattern in $excludePatterns) {
            if ($relative -like "$pattern" -or $relative -like "$pattern\*") {
                $isExcluded = $true
                break
            }
        }
        -not $isDestination -and -not $isExcluded
    }

$content = $items |
    ForEach-Object { $_.FullName.Substring($rootPath.Length).TrimStart('\') } |
    Sort-Object |
    Out-String

[IO.File]::WriteAllText($destination, $content, [Text.UTF8Encoding]::new($false))
Write-Output "Source dump written to $destination"
