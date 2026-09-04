param(
    [Parameter(Mandatory = $true)]
    [string]$Root,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath,

    [string[]]$Exclude = @(
        '.git','node_modules','bin','obj','.venv','__pycache__',
        'secrets','credentials','.env','.env.*'
    )
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Resolve root directory
$rootPath = (Resolve-Path -LiteralPath $Root).Path
if (-not (Test-Path -LiteralPath $rootPath -PathType Container)) {
    throw "Root must be a directory: $Root"
}

# Resolve output path
$destination = [IO.Path]::GetFullPath($OutputPath)

# Prevent writing to drive root
$rootDrive = Split-Path -Qualifier $destination
if ($destination -eq $rootDrive) {
    throw "OutputPath cannot be a drive root: $destination"
}

# Ensure output includes a filename
if ([string]::IsNullOrWhiteSpace([IO.Path]::GetFileName($destination))) {
    throw "OutputPath must include a filename: $destination"
}

# Ensure parent directory exists
$parent = Split-Path -Parent $destination
if ($parent -and $parent -ne $rootDrive) {
    if (-not (Test-Path -LiteralPath $parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }
}

# Build dump content
$items = Get-ChildItem -LiteralPath $rootPath -Recurse -Force |
    ForEach-Object {
        $relative = $_.FullName.Substring($rootPath.Length).TrimStart('\')
        if ($Exclude | Where-Object { $relative -like "$_*" }) {
            return $null
        }
        return $relative
    } | Where-Object { $_ } | Sort-Object

# Write dump
[IO.File]::WriteAllLines($destination, $items, [Text.Encoding]::UTF8)

Write-Output "Project source dump written to $destination"
