[cmdletbinding()]
param(
   [string]$AppDirectory,
   [string]$SdkTag
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

cd $AppDirectory
dotnet new
if (-NOT $?) {
    throw  "Failed to create project"
}

if ($SdkTag -eq "1.1-sdk-msbuild-nanoserver") {
    $projectName = "$($pwd.path | Split-Path -Leaf).csproj"
    (Get-Content $projectName).replace("1.0.3", "1.1.0").replace("netcoreapp1.0", "netcoreapp1.1") | Set-Content $projectName
}

dotnet restore
if (-NOT $?) {
    throw  "Failed to restore packages"
}

dotnet run
if (-NOT $?) {
    throw  "Failed to run app"
}

dotnet publish -o publish/framework-dependent
if (-NOT $?) {
    throw  "Failed to publish app"
}
