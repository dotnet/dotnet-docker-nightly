# escape=`

# Installer image
FROM microsoft/windowsservercore-insider AS installer-env

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# Install .NET Core Runtime
ENV DOTNET_VERSION 2.0.0-preview3-25518-01
ENV DOTNET_DOWNLOAD_URL https://dotnetcli.blob.core.windows.net/dotnet/Runtime/$DOTNET_VERSION/dotnet-runtime-$DOTNET_VERSION-win-x64.zip
ENV DOTNET_DOWNLOAD_SHA 84445918ADA89E4C835961AC41B86CFC5FFD0E7E5F01D999A2D8D68D9B69EC6FA09727C769F95DCC67AFE9DED23687C686C5CCCC2F86D521F986523CF2064AE9

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
