FROM microsoft/dotnet-nightly:2.1-runtime-deps-alpine

# Install .NET Core
ENV DOTNET_VERSION 2.1.0-preview1-26201-01

RUN apk add --no-cache --virtual .build-deps \
        openssl \
    && wget -O dotnet.tar.gz https://dotnetcli.blob.core.windows.net/dotnet/Runtime/$DOTNET_VERSION/dotnet-runtime-$DOTNET_VERSION-alpine.3.6-x64.tar.gz \
    && dotnet_sha512='c2c4eafb855104421ef6f4aab42b0225094615642eb75d4d983cf3c788a102e636a309ecade7e7beb8cd7f0ceeec09a2ea01b26dac4c4b65d190bd019436f2f5' \
    && echo "$dotnet_sha512  dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -C /usr/share/dotnet -xzf dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet.tar.gz \
    && apk del .build-deps
