FROM microsoft/dotnet-nightly:2.1-runtime-deps-alpine

# Install .NET Core
ENV DOTNET_VERSION 2.1.0-preview1-26112-04

RUN apk add --no-cache --virtual .build-deps \
        openssl \
    && wget -O dotnet.tar.gz https://dotnetcli.blob.core.windows.net/dotnet/Runtime/$DOTNET_VERSION/dotnet-runtime-$DOTNET_VERSION-alpine.3.6-x64.tar.gz \
    && dotnet_sha512='83c31ffe752888f93250fa9e69a8b11f7705a8d853730f01493cf2f935799704f6ad338b5d1748cdb4ecabfaec75751d6fa3b18d9d996c2f3fce0bc179f99ba0' \
    && echo "$dotnet_sha512  dotnet.tar.gz" | sha512sum -c - \    
    && mkdir -p /usr/share/dotnet \
    && tar -C /usr/share/dotnet -xzf dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet.tar.gz \
    && apk del .build-deps
