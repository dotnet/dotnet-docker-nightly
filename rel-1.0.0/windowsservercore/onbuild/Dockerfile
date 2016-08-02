FROM microsoft/dotnet-nightly:rel-1.0.0-windowsservercore-sdk

RUN mkdir dotnetapp
WORKDIR dotnetapp

ENTRYPOINT ["dotnet", "run"]

ONBUILD COPY . .
ONBUILD RUN dotnet restore
