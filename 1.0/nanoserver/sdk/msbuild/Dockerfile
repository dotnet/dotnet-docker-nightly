FROM microsoft/nanoserver:10.0.14393.447

# Install .NET Core
ENV DOTNET_VERSION 1.0.3
ENV DOTNET_DOWNLOAD_URL https://dotnetcli.blob.core.windows.net/dotnet/preview/Binaries/$DOTNET_VERSION/dotnet-win-x64.$DOTNET_VERSION.zip

RUN powershell -NoProfile -Command \
        $ErrorActionPreference = 'Stop'; \
        Invoke-WebRequest %DOTNET_DOWNLOAD_URL% -OutFile dotnet.zip; \
        Expand-Archive dotnet.zip -DestinationPath '%ProgramFiles%\dotnet'; \
        Remove-Item -Force dotnet.zip

RUN setx /M PATH "%PATH%;%ProgramFiles%\dotnet"

# Install .NET Core SDK
ENV DOTNET_SDK_VERSION 1.0.0-preview3-003830
ENV DOTNET_SDK_DOWNLOAD_URL https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$DOTNET_SDK_VERSION/dotnet-dev-win-x64.$DOTNET_SDK_VERSION.zip

RUN powershell -NoProfile -Command \
        $ErrorActionPreference = 'Stop'; \
        Invoke-WebRequest %DOTNET_SDK_DOWNLOAD_URL% -OutFile dotnet.zip; \
        Expand-Archive dotnet.zip -DestinationPath '%ProgramFiles%\dotnet' -Force; \
        Remove-Item -Force dotnet.zip

# Trigger the population of the local package cache
ENV NUGET_XMLDOC_MODE skip
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE true
RUN mkdir warmup \
    && cd warmup \
    && dotnet new \
    # Projects created with .NET Core SDK preview3 target 1.0.1, replace with 1.0.3 to populate cache
    && powershell -NoProfile -Command "(Get-Content project.json).replace('1.0.1', '1.0.3') | Set-Content project.json" \
    # Need to use myget cache because 1.0.3 hasn't been released
    && dotnet restore -s https://dotnet.myget.org/F/dotnet-core/api/v3/index.json -s https://api.nuget.org/v3/index.json \
    && cd .. \
    && rmdir /q/s warmup
