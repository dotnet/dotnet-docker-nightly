[cmdletbinding()]
param(
   [string]$OS="windowsservercore"
)

Set-StrictMode -Version Latest
$ErrorActionPreference="Stop"

$dockerRepo="microsoft/dotnet-nightly"

pushd $PSScriptRoot

docker --version
docker ps -a
docker images

docker rm -f $(docker ps -a -q)
docker rmi -f $(docker images -q)

Stop-Service Docker
del -Recurse -Force 'C:\ProgramData\Docker'
Start-Service Docker

Get-ChildItem -Recurse -Filter Dockerfile | where DirectoryName -like "*\$OS*" | sort DirectoryName | foreach {
    $tag = "$($dockerRepo):" + $_.DirectoryName.Replace($PSScriptRoot, '').TrimStart('\').Replace('\', '-') -replace "${OS}$", "${OS}-sdk"
    Write-Host "----- Building $tag -----"
    docker build -t $tag --no-cache $_.DirectoryName
    if (-NOT $?) {
        throw "Failed building $tag"
    }
}

./test/run-test.ps1 -OS $OS

popd
