FROM microsoft/dotnet-nightly:2.1-runtime-deps-alpine

# Install .NET Core
ENV DOTNET_VERSION 2.1.0-preview1-25901-02
ENV DOTNET_DOWNLOAD_URL https://dotnetcli.blob.core.windows.net/dotnet/Runtime/$DOTNET_VERSION/dotnet-runtime-$DOTNET_VERSION-alpine.3.6-x64.tar.gz
ENV DOTNET_DOWNLOAD_SHA 2c7ff8a63c03354b6c3f563f662e8820108be7995b2d72db7d5446ad579977a37db46bae7c5de918b4423a1c37bcfd505df514248f253be1ed2d9a84a982aa98

RUN apk add --no-cache --virtual .build-deps \
        openssl \
    && wget -O dotnet.tar.gz $DOTNET_DOWNLOAD_URL \
    && echo "$DOTNET_DOWNLOAD_SHA  dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -C /usr/share/dotnet -xzf dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet.tar.gz \
    && apk del .build-deps
