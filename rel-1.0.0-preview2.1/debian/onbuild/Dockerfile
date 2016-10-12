FROM microsoft/dotnet-nightly:rel-1.0.0-preview2.1-sdk

RUN mkdir -p /dotnetapp
WORKDIR /dotnetapp

ENTRYPOINT ["dotnet", "run"]

ONBUILD COPY . /dotnetapp
ONBUILD RUN dotnet restore
