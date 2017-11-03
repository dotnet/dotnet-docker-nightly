FROM alpine:3.6

RUN apk update \
    && apk add --no-cache \
        ca-certificates \
        \
        # .NET Core dependencies
        icu-libs \
        krb5-libs \
        libcurl \
        libintl \
        libssl1.0 \
        libunwind \
        libuuid \
        userspace-rcu \
        zlib \
    && apk -X https://dl-cdn.alpinelinux.org/alpine/edge/main add --no-cache \
        lttng-ust
