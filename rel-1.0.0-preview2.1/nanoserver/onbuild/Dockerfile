FROM microsoft/dotnet-nightly:rel-1.0.0-preview2.1-nanoserver-sdk

RUN mkdir dotnetapp
WORKDIR dotnetapp

ENTRYPOINT ["dotnet", "run"]

ONBUILD COPY . .
ONBUILD RUN dotnet restore
