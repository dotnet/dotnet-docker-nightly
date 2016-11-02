[cmdletbinding()]
param(
   [string]$AppDirectory,
   [string]$SdkTag
)

Set-StrictMode -Version Latest
$ErrorActionPreference="Stop"

cd $AppDirectory
dotnet new
if (-NOT $?) {
    throw  "Failed to create project"
}

cp c:\test\NuGet.Config .

$cliVersion="1.0.1"

if ($SdkTag.StartsWith("1.0")) {
    $runtimeVersion="1.0.2"
}
else {
    $runtimeVersion="1.1.0"
}

(Get-Content project.json).replace($cliVersion, $runtimeVersion) | Set-Content project.json

dotnet restore
if (-NOT $?) {
    throw  "Failed to restore packages"
}

dotnet run
if (-NOT $?) {
    throw  "Failed to run app"
}

dotnet publish -o publish
if (-NOT $?) {
    throw  "Failed to publish app"
}
