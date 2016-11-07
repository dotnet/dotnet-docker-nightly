![](https://avatars0.githubusercontent.com/u/9141961?v=3&amp;s=100)

.NET Core Nightly Docker Images
====================

This repository contains `Dockerfile` definitions for Docker images that include last-known-good (LKG) builds for the next release of the [.NET Core SDK](https://github.com/dotnet/cli).

See [dotnet/dotnet-docker](https://github.com/dotnet/dotnet-docker) for images with official releases of [.NET Core](https://github.com/dotnet/core).

You can find samples, documentation, and getting started instructions for .NET Core in the [.NET Core documentation](https://docs.microsoft.com/dotnet/articles/core/).

[![Downloads from Docker Hub](https://img.shields.io/docker/pulls/microsoft/dotnet-nightly.svg)](https://hub.docker.com/r/microsoft/dotnet-nightly)
[![Stars on Docker Hub](https://img.shields.io/docker/stars/microsoft/dotnet-nightly.svg)](https://hub.docker.com/r/microsoft/dotnet-nightly)


## Supported tags

-       ['1.0.1-runtime', '1.0-runtime' (*1.0/debian/runtime/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/1.0/debian/runtime/Dockerfile)
-       ['1.0.1-runtime-nanoserver', '1.0-runtime-nanoserver' (*1.0/nanoserver/runtime/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/1.0/nanoserver/runtime/Dockerfile)
-       ['1.0.1-runtime-deps', '1.0-runtime-deps' (*1.0/debian/runtime-deps/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/1.0/debian/runtime-deps/Dockerfile)
-       ['1.1.0-runtime', '1.1-runtime', '1-runtime', 'runtime' (*1.1/debian/runtime/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/1.1/debian/runtime/Dockerfile)
-       ['1.1.0-runtime-nanoserver', '1.1-runtime-nanoserver', '1-runtime-nanoserver', 'runtime-nanoserver' (*1.1/nanoserver/runtime/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/1.1/nanoserver/runtime/Dockerfile)
-       ['1.1.0-runtime-deps', '1.1-runtime-deps', '1-runtime-deps', 'runtime-deps' (*1.1/debian/runtime-deps/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/1.1/debian/runtime-deps/Dockerfile)
-       ['1.0.1-sdk-projectjson', '1.0-sdk-projectjson', 'latest' (*1.0/debian/sdk/projectjson/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/1.0/debian/sdk/projectjson/Dockerfile)
-       ['1.0.1-sdk-projectjson-nanoserver', '1.0-sdk-projectjson-nanoserver' (*1.0/nanoserver/sdk/projectjson/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/1.0/nanoserver/sdk/projectjson/Dockerfile)
-       ['1.0.1-sdk-msbuild', '1.0-sdk' (*1.0/debian/sdk/msbuild/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/1.0/debian/sdk/msbuild/Dockerfile)
-       ['1.0.1-sdk-msbuild-nanoserver', '1.0-sdk-nanoserver' (*1.0/nanoserver/sdk/msbuild/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/1.0/nanoserver/sdk/msbuild/Dockerfile)
-       ['1.1.0-sdk-projectjson', '1.1-sdk-projectjson' (*1.1/debian/sdk/projectjson/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/1.1/debian/sdk/projectjson/Dockerfile)
-       ['1.1.0-sdk-projectjson-nanoserver', '1.1-sdk-projectjson-nanoserver' (*1.1/nanoserver/sdk/projectjson/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/1.1/nanoserver/sdk/projectjson/Dockerfile)
-       ['1.1.0-sdk-msbuild', '1.1-sdk' (*1.1/debian/sdk/msbuild/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/1.1/debian/sdk/msbuild/Dockerfile)
-       ['1.1.0-sdk-msbuild-nanoserver', '1.1-sdk-nanoserver' (*1.1/nanoserver/sdk/msbuild/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/1.1/nanoserver/sdk/msbuild/Dockerfile)

## Image variants

The `microsoft/dotnet-nightly` images come in different flavors, each designed for a specific use case.

See [Building Docker Images for .NET Core Applications](https://docs.microsoft.com/dotnet/articles/core/docker/building-net-docker-images) to get an understanding of the different Docker images that are offered and when is the right use case for them.

### `microsoft/dotnet-nightly:<version>-sdk`

This image contains the .NET Core SDK which is comprised of two parts:

1. .NET Core
2. .NET Core command line tools

This image is recommended if you are trying .NET Core for the first time, as it allows both developing and running
applications. Use this image for your development process (developing, building and testing applications).

### `microsoft/dotnet-nightly:<version>-runtime`

This image contains only .NET Core (runtime and libraries) and it is optimized for running [framework-dependent .NET Core applications](https://docs.microsoft.com/dotnet/articles/core/deploying/index). If you wish to run self-contained applications, please use the `runtime-deps` image described below. 

### `microsoft/dotnet-nightly:<version>-runtime-deps`

This image contains the operating system with all of the native dependencies needed by .NET Core. Use this image to:

1. Run a [self-contained](https://docs.microsoft.com/dotnet/articles/core/deploying/index) application.
2. Build a custom copy of .NET Core by compiling [coreclr](https://github.com/dotnet/coreclr) and [corefx](https://github.com/dotnet/corefx).

## Windows Containers

Windows Containers images use the `microsoft/nanoserver` base OS image from Windows Server 2016.  For more information on Windows Containers and a getting started guide, please see: [Windows Containers Documentation](http://aka.ms/windowscontainers).
