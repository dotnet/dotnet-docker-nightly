# escape=`

# Installer image
FROM microsoft/windowsservercore-insider AS installer-env

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# Install .NET Core Runtime
ENV DOTNET_VERSION 2.0.0-preview3-25511-03
ENV DOTNET_DOWNLOAD_URL https://dotnetcli.blob.core.windows.net/dotnet/Runtime/$DOTNET_VERSION/dotnet-runtime-$DOTNET_VERSION-win-x64.zip
ENV DOTNET_DOWNLOAD_SHA 694FF77BD839A530785BF89DDAF34E5D9E40631B682B97BFCE1CAA25CD92BEBD40603CE18CB49A1C4357934377C12AB7B536E7DB87967AB73A8FF3F48E535108

RUN Invoke-WebRequest $Env:DOTNET_DOWNLOAD_URL -OutFile dotnet.zip; `
    if ((Get-FileHash dotnet.zip -Algorithm sha512).Hash -ne $Env:DOTNET_DOWNLOAD_SHA) { `
        Write-Host 'CHECKSUM VERIFICATION FAILED!'; `
        exit 1; `
    }; `
    `
    Expand-Archive dotnet.zip -DestinationPath dotnet; `
    Remove-Item -Force dotnet.zip

# Runtime image
FROM microsoft/nanoserver-insider

COPY --from=installer-env ["dotnet", "C:\\Program Files\\dotnet"]

RUN setx PATH "%PATH%;C:\Program Files\dotnet"
