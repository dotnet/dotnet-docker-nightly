FROM microsoft/dotnet-nightly:2.1-runtime-deps-alpine

# Install .NET Core
ENV DOTNET_VERSION 2.1.0-preview1-25907-01
ENV DOTNET_DOWNLOAD_URL https://dotnetcli.blob.core.windows.net/dotnet/Runtime/$DOTNET_VERSION/dotnet-runtime-$DOTNET_VERSION-alpine.3.6-x64.tar.gz
ENV DOTNET_DOWNLOAD_SHA 52aa6a93ba7371b4e0c1309d8b79614fc13c670caebb9f1d90c7cae66c4fd6b9c142e723280ce43a748c6450e1ce41c9d47c62d2d1fb0d21855f4916745b9fe9

RUN apk add --no-cache --virtual .build-deps \
        openssl \
    && wget -O dotnet.tar.gz $DOTNET_DOWNLOAD_URL \
    && echo "$DOTNET_DOWNLOAD_SHA  dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -C /usr/share/dotnet -xzf dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet.tar.gz \
    && apk del .build-deps
