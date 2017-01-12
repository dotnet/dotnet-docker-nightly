FROM microsoft/nanoserver:10.0.14393.693

# Install .NET Core SDK
ENV DOTNET_SDK_VERSION 1.0.0-rc3-004488
ENV DOTNET_SDK_DOWNLOAD_URL https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$DOTNET_SDK_VERSION/dotnet-dev-win-x64.$DOTNET_SDK_VERSION.zip

RUN powershell -NoProfile -Command \
        $ErrorActionPreference = 'Stop'; \
        Invoke-WebRequest %DOTNET_SDK_DOWNLOAD_URL% -OutFile dotnet.zip; \
        Expand-Archive dotnet.zip -DestinationPath '%ProgramFiles%\dotnet' -Force; \
        Remove-Item -Force dotnet.zip

RUN setx /M PATH "%PATH%;%ProgramFiles%\dotnet"

# Trigger the population of the local package cache
ENV NUGET_XMLDOC_MODE skip
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE true
RUN mkdir warmup \
    && cd warmup \
    && dotnet new \
    # Projects created with .NET Core SDK rc3 target 1.0, replace with 1.1 to populate cache
    && powershell -NoProfile -Command "(Get-Content warmup.csproj).replace('1.0.3', '1.1.0').replace('netcoreapp1.0', 'netcoreapp1.1') | Set-Content warmup.csproj" \
    && dotnet restore \
    && cd .. \
    && rmdir /q/s warmup
