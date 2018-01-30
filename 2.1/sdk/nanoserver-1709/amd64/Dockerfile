# escape=`

# Installer image
FROM microsoft/windowsservercore:1709 AS installer-env

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# Retrieve .NET Core SDK
ENV DOTNET_SDK_VERSION 2.1.300-preview2-008024

RUN Invoke-WebRequest -OutFile dotnet.zip https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$Env:DOTNET_SDK_VERSION/dotnet-sdk-$Env:DOTNET_SDK_VERSION-win-x64.zip; `
    $dotnet_sha512 = '30b95462fceb791a1b36f01f539636cef08db7e3dfbe3dfe3308c2cb92d967db94cb22d05960d37975f2482fa571fbf461905a4c34cdf66d8937fe9b56f071c9'; `
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
