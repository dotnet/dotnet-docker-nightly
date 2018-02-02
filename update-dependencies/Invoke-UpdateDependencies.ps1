#
# Copyright (c) .NET Foundation and contributors. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

[cmdletbinding()]
param(
    [string]$UpdateDependenciesParams,
    [switch]$CleanupDocker,
    [switch]$UseImageCache
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$imageName = "update-dependencies"

try {
    $image = & docker image ls $imageName -q
    if (!$UseImageCache -or [string]::IsNullOrEmpty($image))
    {
        & docker build -t $imageName -f $PSScriptRoot\Dockerfile --pull $PSScriptRoot\..
        if ($LastExitCode -ne 0) {
            throw "Failed to build the update dependencies tool"
        }
    }

    Invoke-Expression "docker run --rm $imageName $UpdateDependenciesParams"
    if ($LastExitCode -ne 0) {
        throw "Failed to update dependencies"
    }
}
finally {
    if ($CleanupDocker){
        & docker rmi -f $imageName
        & docker system prune -f
    }
}
