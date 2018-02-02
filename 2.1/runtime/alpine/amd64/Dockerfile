FROM microsoft/dotnet-nightly:2.1-runtime-deps-alpine

# Install .NET Core
ENV DOTNET_VERSION 2.1.0-preview1-26126-02

RUN apk add --no-cache --virtual .build-deps \
        openssl \
    && wget -O dotnet.tar.gz https://dotnetcli.blob.core.windows.net/dotnet/Runtime/$DOTNET_VERSION/dotnet-runtime-$DOTNET_VERSION-alpine.3.6-x64.tar.gz \
    && dotnet_sha512='d1d40708a403d873d785a7fe08935161b6e69c686dbc44c5ecb062644aad7ec4da41f2d31c75c7b807654253df25fedf56ce9a62744d76c984be778f467dbaa1' \
    && echo "$dotnet_sha512  dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -C /usr/share/dotnet -xzf dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet.tar.gz \
    && apk del .build-deps
