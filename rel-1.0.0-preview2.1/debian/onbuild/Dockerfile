FROM microsoft/dotnet-nightly:rel-1.0.0-preview2.1-sdk

WORKDIR /dotnetapp

ENTRYPOINT ["dotnet", "out/dotnetapp.dll"]

ONBUILD COPY project.json ./
ONBUILD RUN dotnet restore
ONBUILD COPY . ./
ONBUILD RUN dotnet publish -c Release -o out
