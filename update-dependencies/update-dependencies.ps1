#
# Copyright (c) .NET Foundation and contributors. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

param(
    [string]$CliBranch="rel/1.0.0",
    [string]$DotnetInstallDir,
    [string[]]$EnvVars=@(),
    [switch]$Help)

if($Help)
{
    Write-Host "Usage: .\update-dependencies.ps1 [Options]"
    Write-Host ""
    Write-Host "Summary: Installs the .NET Core SDK and then compiles and runs update-dependencies.exe."
    Write-Host ""
    Write-Host "Options:"
    Write-Host "  -CliBranch <branch_name>           The dotnet/cli branch to use for installing the .NET SDK. Defaults to 'rel/1.0.0'."
    Write-Host "  -DotnetInstallDir <path>           The directory in which to install the .NET SDK. Defaults to '`$RepoRoot\.dotnet\Windows\`$Architecture'."
    Write-Host "  -EnvVars <'V1=val1','V2=val2'...>  Comma separated list of environment variable name-value pairs"
    Write-Host "  -Help                              Display this help message"
    exit 0
}

$Architecture='x64'

$RepoRoot = "$PSScriptRoot\.."
$AppPath = "$PSScriptRoot"

if ([string]::IsNullOrWhiteSpace($DotnetInstallDir))
{
    $DotnetInstallDir = "$RepoRoot\.dotnet\Windows\$Architecture"
}

if (!(Test-Path "$RepoRoot\artifacts"))
{
    mkdir "$RepoRoot\artifacts" | Out-Null
}

# Install the .NET Core SDK
$DOTNET_INSTALL_SCRIPT_URL="https://raw.githubusercontent.com/dotnet/cli/$CliBranch/scripts/obtain/dotnet-install.ps1"
Invoke-WebRequest $DOTNET_INSTALL_SCRIPT_URL -OutFile "$RepoRoot\artifacts\dotnet-install.ps1"

& "$RepoRoot\artifacts\dotnet-install.ps1" -Channel "rel-1.0.0" -Version "1.0.0-preview3-003981" -Architecture $Architecture -InstallDir $DotnetInstallDir
if($LASTEXITCODE -ne 0) { throw "Failed to install the .NET Core SDK" }

pushd "$AppPath"

 # Restore the app
Write-Host "Restoring app $AppPath..."
dotnet restore
if($LASTEXITCODE -ne 0) { throw "Failed to restore" }

# Publish the app
Write-Host "Compiling app..."
dotnet publish -o "$AppPath\bin" -c Release
if($LASTEXITCODE -ne 0) { throw "Failed to compile" }

popd

# Run the app
Write-Host "Invoking app: dotnet $AppPath\bin\update-dependencies.dll $EnvVars"
pushd $RepoRoot
& dotnet "$AppPath\bin\update-dependencies.dll" @EnvVars
if($LASTEXITCODE -ne 0) { throw "App execution failed" }
popd
