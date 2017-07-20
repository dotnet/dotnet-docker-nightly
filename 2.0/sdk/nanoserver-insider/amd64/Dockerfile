# escape=`

# Installer image
FROM microsoft/windowsservercore-insider AS installer-env

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# Install .NET Core SDK
ENV DOTNET_SDK_VERSION 2.0.0-preview3-006811
ENV DOTNET_SDK_DOWNLOAD_URL https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$DOTNET_SDK_VERSION/dotnet-sdk-$DOTNET_SDK_VERSION-win-x64.zip
ENV DOTNET_SDK_DOWNLOAD_SHA E0370FBCDD4E489B4A7BA48DB35E89D93FE4481F1437ACB0838F49F0FFC2D84A615742689419CBE39EDF4EC0A303F4E3982BE961A92601C9049F088B5CE62BAA

RUN Invoke-WebRequest $Env:DOTNET_SDK_DOWNLOAD_URL -OutFile dotnet.zip; `
    if ((Get-FileHash dotnet.zip -Algorithm sha512).Hash -ne $Env:DOTNET_SDK_DOWNLOAD_SHA) { `
        Write-Host 'CHECKSUM VERIFICATION FAILED!'; `
        exit 1; `
    }; `
    `
    Expand-Archive dotnet.zip -DestinationPath dotnet; `
    Remove-Item -Force dotnet.zip

# SDK image
FROM microsoft/nanoserver-insider

COPY --from=installer-env ["dotnet", "C:\\Program Files\\dotnet"]

RUN setx PATH "%PATH%;C:\Program Files\dotnet"

# Trigger the population of the local package cache
ENV NUGET_XMLDOC_MODE skip
RUN mkdir warmup `
    && cd warmup `
    && dotnet new `
    && cd .. `
    && rmdir /s /q warmup
