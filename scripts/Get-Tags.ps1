param(
    [string]$Branch="master",
    [string]$ImageBuilderImageName="microsoft/dotnet-buildtools-prereqs:image-builder-jessie-20171005132855"
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Path "$PSScriptRoot" -Parent

& docker pull $ImageBuilderImageName

& docker run --rm `
    -v /var/run/docker.sock:/var/run/docker.sock `
    -v "${repoRoot}:/repo" `
    -w /repo `
    $ImageBuilderImageName `
    generateTagsReadme `
    "https://github.com/dotnet/dotnet-docker-nightly/blob/${Branch}"
