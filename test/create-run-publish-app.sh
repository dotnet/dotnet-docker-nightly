#!/usr/bin/env bash
set -e 	# Exit immediately upon failure

: ${1?"Need to pass sandbox directory as argument"}
: ${2?"Need to pass sdk image version as argument"}

cd $1

echo "Testing framework-dependent deployment"
dotnet new

if [ "rel-1.0.0-preview2.1" == "${2}" ]; then
    dotnet restore
    dotnet run
    dotnet publish -o publish/framework-dependent
else
    dotnet restore3
    dotnet run3
    dotnet publish3 -o publish/framework-dependent
fi


echo "Testing self-contained deployment"

if [ "rel-1.0.0-preview2.1" == "${2}" ]; then
    cp /test/NuGet.Config .

    runtimes_section="  },\n  \"runtimes\": {\n    \"debian.8-x64\": {}\n  }"
    sed -i '/"type": "platform"/d' ./project.json
    sed -i "s/^  }$/${runtimes_section}/" ./project.json

    dotnet restore
    dotnet run
    dotnet publish -o publish/self-contained
else
    dotnet publish3 -r debian.8-x64 -o publish/self-contained
fi
