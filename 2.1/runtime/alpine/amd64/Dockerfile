FROM microsoft/dotnet-nightly:2.1-runtime-deps-alpine

# Install .NET Core
ENV DOTNET_VERSION 2.1.0-preview1-25907-02
ENV DOTNET_DOWNLOAD_URL https://dotnetcli.blob.core.windows.net/dotnet/Runtime/$DOTNET_VERSION/dotnet-runtime-$DOTNET_VERSION-alpine.3.6-x64.tar.gz
ENV DOTNET_DOWNLOAD_SHA e062357d057a2251abf3122ac32d95ef68043283036c34f61259358c2753eb82f6bc04660342d812fa2f8b5fafe35806ee032d04e2c00be3e1cf9c68c18d9c83

RUN apk add --no-cache --virtual .build-deps \
        openssl \
    && wget -O dotnet.tar.gz $DOTNET_DOWNLOAD_URL \
    && echo "$DOTNET_DOWNLOAD_SHA  dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -C /usr/share/dotnet -xzf dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet.tar.gz \
    && apk del .build-deps
