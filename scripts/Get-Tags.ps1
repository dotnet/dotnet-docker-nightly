param(
    [string]$Branch='master',
    [string]$ImageBuilderImageName='microsoft/dotnet-buildtools-prereqs:image-builder-jessie-20171005132855',
    [string]$RepoName
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Path "$PSScriptRoot" -Parent

if ([String]::IsNullOrWhiteSpace($RepoName))
{
    [Uri]$remoteUrl = New-Object -TypeName System.Uri -ArgumentList "https://github.com/dotnet/dotnet-docker-nightly.git"
    if ([Uri]::TryCreate(((git config --get remote.origin.url) | Out-String), [UriKind]::Absolute, [ref]$remoteUrl))
    {
        $RepoName = [System.IO.Path]::GetFileNameWithoutExtension($remoteUrl.ToString())
    }
}

if ([String]::IsNullOrWhiteSpace($RepoName))
{
    Write-Warning 'Could not automatically determine repository name. Falling back to "dotnet-docker-nightly". Add -RepoName <REPO> to override.'
    $RepoName = "dotnet-docker-nightly"
}

& docker pull $ImageBuilderImageName

& docker run --rm `
    -v /var/run/docker.sock:/var/run/docker.sock `
    -v "${repoRoot}:/repo" `
    -w /repo `
    $ImageBuilderImageName `
    generateTagsReadme "https://github.com/dotnet/${RepoName}/blob/${Branch}"
