FROM alpine:3.6

RUN apk add --no-cache \
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
        tzdata \
        userspace-rcu \
        zlib \
    && apk -X https://dl-cdn.alpinelinux.org/alpine/edge/main add --no-cache \
        lttng-ust
