#
# Copyright (c) .NET Foundation and contributors. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

[cmdletbinding()]
param(
    [Parameter(Mandatory=$true, HelpMessage="The versioned folder of this repo to update (e.g. 2.1).")]
    [string]$DockerVersionedFolder,
    [Parameter(Mandatory=$true, HelpMessage="The CLI branch to retrieve the SDK to update the Dockerfiles with.")]
    [string]$CliBranch,
    [Parameter(Mandatory=$true, HelpMessage="The CLI version prefix associated with the CliBranch.")]
    [string]$CliVersionPrefix,
    [Parameter(HelpMessage="GitHub user used while making PR.  If not specified PR is not made.")]
    [string]$GitHubUser,
    [Parameter(HelpMessage="GitHub email used while making make PR.  If not specified PR is not made.")]
    [string]$GitHubEmail,
    [Parameter(HelpMessage="GitHub password used while making make PR.  If not specified PR is not made.")]
    [string]$GitHubPassword,
    [Parameter(HelpMessage="Cleanup the Docker assets created by this script.")]
    [switch]$CleanupDocker
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

try {
    & docker build -t update-dependencies -f $PSScriptRoot\Dockerfile $PSScriptRoot\..
    if ($LastExitCode -ne 0) {
        throw "Failed building update-dependencies"
    }

    & docker run --rm update-dependencies $DockerVersionedFolder $CliVersionPrefix $GitHubUser $GitHubEmail $GitHubPassword
    if ($LastExitCode -ne 0) {
        throw "Failed to update dependencies"
    }
}
finally {
    if ($CleanupDocker){
        & docker rmi -f update-dependencies
        & docker system prune -f
    }
}
