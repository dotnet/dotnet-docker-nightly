#!/usr/bin/env bash
set -e  # Exit immediately upon failure

: ${1?"Need to pass sandbox directory as argument"}
: ${2?"Need to pass sdk image tag as argument"}

cd $1

echo "Testing framework-dependent deployment"
dotnet new

if [[ $2 == "1.0-sdk-msbuild" ]]; then
    sed -i "s/1.0.1/1.0.3/" ./${PWD##*/}.csproj
elif [[ $2 == "1.1-sdk-msbuild" ]]; then
    sed -i "s/1.0.1/1.1.0/;s/netcoreapp1.0/netcoreapp1.1/" ./${PWD##*/}.csproj
fi

dotnet restore
dotnet run
dotnet publish -o publish/framework-dependent

if [[ $2 == "1.0"* ]]; then
    # Need to use myget cache because 1.0.3 hasn't been released
    nuget_sources="-s https://dotnet.myget.org/F/dotnet-core/api/v3/index.json -s https://api.nuget.org/v3/index.json"
fi

echo "Testing self-contained deployment"
if [[ $2 == *"projectjson"* ]]; then
    runtimes_section="  },\n  \"runtimes\": {\n    \"debian.8-x64\": {}\n  }"
    sed -i '/"type": "platform"/d' ./project.json
    sed -i "s/^  }$/${runtimes_section}/" ./project.json

    dotnet restore $nuget_sources
    dotnet run
    dotnet publish -o publish/self-contained
else
    sed -i '/<PropertyGroup>/a \    <RuntimeIdentifiers>debian.8-x64<\/RuntimeIdentifiers>' ./${PWD##*/}.csproj

    dotnet restore $nuget_sources
    dotnet publish -r debian.8-x64 -o publish/self-contained
fi
