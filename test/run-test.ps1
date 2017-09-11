#
# Copyright (c) .NET Foundation and contributors. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

[cmdletbinding()]
param(
    [string]$Filter,
    [string]$Architecture
)

$DotnetInstallDir = "$PSScriptRoot/../.dotnet"

if (!(Test-Path "$DotnetInstallDir")) {
    mkdir "$DotnetInstallDir" | Out-Null
}

# Install the .NET Core SDK
$IsRunningOnUnix = $PSVersionTable.contains("Platform") -and $PSVersionTable.Platform -eq "Unix"
if ($IsRunningOnUnix) {
    $DotnetInstallScript = "dotnet-install.sh"
}
else {
    $DotnetInstallScript = "dotnet-install.ps1"
}

if (!(Test-Path $DotnetInstallScript)) {
    $DOTNET_INSTALL_SCRIPT_URL = "https://raw.githubusercontent.com/dotnet/cli/release/2.0.0/scripts/obtain/$DotnetInstallScript"
    Invoke-WebRequest $DOTNET_INSTALL_SCRIPT_URL -OutFile $DotnetInstallDir/$DotnetInstallScript
}

if ($IsRunningOnUnix) {
    & chmod +x $DotnetInstallDir/$DotnetInstallScript
    & $DotnetInstallDir/$DotnetInstallScript --channel "release-2.0.0" --version "2.0.0" --architecture x64 --install-dir $DotnetInstallDir
}
else {
    & $DotnetInstallDir/$DotnetInstallScript -Channel "release-2.0.0" -Version "2.0.0" -Architecture x64 -InstallDir $DotnetInstallDir
}

if ($LASTEXITCODE -ne 0) { throw "Failed to install the .NET Core SDK" }

Push-Location "$PSScriptRoot\Microsoft.DotNet.Docker.Tests"

Try {
    # Run Tests
    if ([string]::IsNullOrWhiteSpace($Architecture)) {
        $Architecture = "amd64"
    }

    $TestFilter = "Architecture=$Architecture"
    if (![string]::IsNullOrWhiteSpace($Filter)) {
        $TestFilter += "&Version=$filter"
    }

    & $DotnetInstallDir/dotnet test -v n --filter """$TestFilter"""

    if ($LASTEXITCODE -ne 0) { throw "Tests Failed" }
}
Finally {
    Pop-Location
}
