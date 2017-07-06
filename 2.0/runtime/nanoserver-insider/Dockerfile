# escape=`

# Installer image
FROM microsoft/windowsservercore-insider AS installer-env

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

ENV DOTNET_VERSION 2.0.0-preview3-25510-01
ENV DOTNET_DOWNLOAD_URL https://dotnetcli.blob.core.windows.net/dotnet/Runtime/$DOTNET_VERSION/dotnet-runtime-$DOTNET_VERSION-win-x64.zip
ENV DOTNET_DOWNLOAD_SHA D7026FA81A12A0C02CE2F9D6092D971B57FA84F1B2B232A67E10C5E36E7D992A767BDEA488DFF7C604372BADAD7F3657CC9A45BBE3767FD1F98EC6D29EB5E772

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
