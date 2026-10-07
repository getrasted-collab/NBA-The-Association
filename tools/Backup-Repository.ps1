[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $DestinationDirectory
)

$ErrorActionPreference = 'Stop'

function Invoke-Git {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]] $Arguments)

    & git @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Git failed with exit code ${LASTEXITCODE}: git $($Arguments -join ' ')"
    }
}

$repositoryRoot = (git -C $PSScriptRoot rev-parse --show-toplevel).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($repositoryRoot)) {
    throw 'The backup script must be run from a checked-out Git repository.'
}

$repositoryRoot = [System.IO.Path]::GetFullPath($repositoryRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
$destinationPath = [System.IO.Path]::GetFullPath($DestinationDirectory).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
$destinationRoot = [System.IO.Path]::GetPathRoot($destinationPath).TrimEnd([System.IO.Path]::DirectorySeparatorChar)

if ($destinationPath -eq $destinationRoot) {
    throw 'Refusing to write a backup directly to a filesystem root.'
}

if ($destinationPath -eq $repositoryRoot -or $destinationPath.StartsWith("$repositoryRoot$([System.IO.Path]::DirectorySeparatorChar)", [System.StringComparison]::OrdinalIgnoreCase)) {
    throw 'Refusing to place backup archives inside the source repository.'
}

$workingTreeStatus = git -C $repositoryRoot status --porcelain
if ($LASTEXITCODE -ne 0) {
    throw 'Unable to inspect the repository working tree.'
}
if ($workingTreeStatus) {
    throw 'Refusing to back up a dirty repository. Commit or intentionally remove all changes first.'
}

New-Item -ItemType Directory -Path $destinationPath -Force | Out-Null

$head = (git -C $repositoryRoot rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) {
    throw 'Unable to resolve repository HEAD.'
}

$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$shortHead = $head.Substring(0, 12)
$bundleName = "NBA-The-Association-$timestamp-$shortHead.bundle"
$bundlePath = Join-Path $destinationPath $bundleName
$checksumPath = "$bundlePath.sha256"

if ((Test-Path -LiteralPath $bundlePath -PathType Leaf) -or (Test-Path -LiteralPath $checksumPath -PathType Leaf)) {
    throw "Refusing to overwrite an existing backup: $bundlePath"
}

Write-Host "Repository: $repositoryRoot"
Write-Host "HEAD:       $head"
Write-Host "Creating:   $bundlePath"

Invoke-Git -Arguments @('-C', $repositoryRoot, 'bundle', 'create', $bundlePath, '--all')
Invoke-Git -Arguments @('bundle', 'verify', $bundlePath)

$hash = Get-FileHash -LiteralPath $bundlePath -Algorithm SHA256
"$($hash.Hash.ToLowerInvariant())  $bundleName" | Set-Content -LiteralPath $checksumPath -Encoding ascii -NoNewline

Write-Host "Checksum:   $checksumPath"
Write-Host 'Backup complete. This bundle contains committed Git refs only; it does not contain uncommitted, ignored, LFS, or external asset files.'
