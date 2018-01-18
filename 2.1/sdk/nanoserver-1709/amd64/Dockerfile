# escape=`

# Installer image
FROM microsoft/windowsservercore:1709 AS installer-env

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# Retrieve .NET Core SDK
ENV DOTNET_SDK_VERSION 2.2.0-preview1-007987

RUN Invoke-WebRequest -OutFile dotnet.zip https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$Env:DOTNET_SDK_VERSION/dotnet-sdk-$Env:DOTNET_SDK_VERSION-win-x64.zip; `
    $dotnet_sha512 = '3fa3c7c8bfdd6a8c78affaea133e7b89810aaf55abc3445b8d553071b0c81fc0a9fc490d596b60077c766a2551d542dde53680ea9a089bf450c06829ef184077'; `
    if ((Get-FileHash dotnet.zip -Algorithm sha512).Hash -ne $dotnet_sha512) { `
        Write-Host 'CHECKSUM VERIFICATION FAILED!'; `
        exit 1; `
    }; `
    `
    Expand-Archive dotnet.zip -DestinationPath dotnet; `
    Remove-Item -Force dotnet.zip


# SDK image
FROM microsoft/nanoserver:1709

COPY --from=installer-env ["dotnet", "C:\\Program Files\\dotnet"]

# In order to set system PATH, ContainerAdministrator must be used
USER ContainerAdministrator 
RUN setx /M PATH "%PATH%;C:\Program Files\dotnet"
USER ContainerUser

# Trigger the population of the local package cache
ENV NUGET_XMLDOC_MODE skip
RUN mkdir warmup `
    && cd warmup `
    && dotnet new `
    && cd .. `
    && rmdir /s /q warmup
