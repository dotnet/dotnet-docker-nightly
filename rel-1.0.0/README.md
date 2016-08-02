![](https://avatars0.githubusercontent.com/u/9141961?v=3&amp;s=100)

.NET Core Nightly Docker Images
====================

This repository contains `Dockerfile` definitions for Docker images that include last-known-good (LKG) builds of the next release of the [.NET Core command-line (CLI) tools](https://github.com/dotnet/cli).

See [dotnet/dotnet-docker](https://github.com/dotnet/dotnet-docker) for images with official releases of [.NET Core](http://dotnet.github.io).

This project is part of the .NET Core CLI tools. You can find samples, documentation, and getting started instructions at the [dotnet/cli] repo.

[![Downloads from Docker Hub](https://img.shields.io/docker/pulls/microsoft/dotnet-nightly.svg)](https://registry.hub.docker.com/u/microsoft/dotnet-nightly)
[![Stars on Docker Hub](https://img.shields.io/docker/stars/microsoft/dotnet-nightly.svg)](https://registry.hub.docker.com/u/microsoft/dotnet-nightly)


## Supported tags

### Development images
-       [`rel-1.0.0-sdk`, `latest` (*rel-1.0.0/debian/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/rel-1.0.0/debian/Dockerfile)
-       [`rel-1.0.0-windowsservercore-sdk`, `windowsservercore` (*rel-1.0.0/windowsservercore/Dockerfile*)](https://github.com/dotnet/dotnet-docker/blob/master/rel-1.0.0/windowsservercore/Dockerfile)
-       [`rel-1.0.0-nanoserver-sdk`, `nanoserver` (*rel-1.0.0/nanoserver/Dockerfile*)](https://github.com/dotnet/dotnet-docker/blob/master/rel-1.0.0/nanoserver/Dockerfile)
-       [`rel-1.0.0-onbuild`, `onbuild` (*rel-1.0.0/onbuild/Dockerfile*)](https://github.com/dotnet/dotnet-docker/blob/master/rel-1.0.0/debian/onbuild/Dockerfile)
-       [`rel-1.0.0-windowsservercore-onbuild`, `windowsservercore-onbuild` (*rel-1.0.0/windowsservercore/onbuild/Dockerfile*)](https://github.com/dotnet/dotnet-docker/blob/master/rel-1.0.0/windowsservercore/onbuild/Dockerfile)
-       [`rel-1.0.0-nanoserver-onbuild`, `nanoserver-onbuild` (*rel-1.0.0/nanoserver/onbuild/Dockerfile*)](https://github.com/dotnet/dotnet-docker/blob/master/rel-1.0.0/nanoserver/onbuild/Dockerfile)

### Runtime images
-       [`rel-1.0.0-core`, `core` (*rel-1.0.0/core/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/rel-1.0.0/debian/core/Dockerfile)
-       [`rel-1.0.0-windowsservercore-core`, `windowsservercore-core` (*rel-1.0.0/windowsservercore/core/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/rel-1.0.0/windowsservercore/core/Dockerfile)
-       [`rel-1.0.0-nanoserver-core`, `nanoserver-core` (*rel-1.0.0/nanoserver/core/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/rel-1.0.0/nanoserver/core/Dockerfile)
-       [`rel-1.0.0-core-deps`, `core-deps` (*rel-1.0.0/core-deps/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/rel-1.0.0/debian/core-deps/Dockerfile)

## Image variants

The `microsoft/dotnet-nightly` images come in different flavors, each designed for a specific use case.

### `microsoft/dotnet-nightly:<version>-sdk`

This image contains the .NET Core SDK which is comprised of two parts: 

1. .NET Core
2. .NET Core command line tools

This image is recommended if you are trying .NET Core for the first time, as it allows both developing and running 
applications. Use this image for your development process (developing, building and testing applications). 

### `microsoft/dotnet-nightly:<version>-onbuild`

The most straightforward way to use this image is to use a Docker container as both the build and runtime environment for your application. Creating a simple `Dockerfile` with the following content in the same directory as your project files will compile and run your project:

```dockerfile
FROM microsoft/dotnet-nightly:onbuild
```

This image includes multiple `ONBUILD` triggers which should cover most applications. The build will `COPY . /dotnetapp` and `RUN dotnet restore`.

This image also includes the `ENTRYPOINT dotnet run` instruction which will run your application when the Docker image is run.

You can then build and run the Docker image:

```console
$ docker build -t my-dotnet-app .
$ docker run -it --rm --name my-running-app my-dotnet-app
```

### `microsoft/dotnet-nightly:<version>-core`

This image contains only .NET Core (runtime and libraries) and it is optimized for running [portable .NET Core applications](https://docs.microsoft.com/en-us/dotnet/articles/core/app-types). If you wish to run self-contained applications, please use the `core-deps` image described below. 

### `microsoft/dotnet-nightly:<version>-core-deps`

This image contains the operating system with all of the native dependencies needed by .NET Core. Use this image to:

1. Run a [self-contained](https://docs.microsoft.com/en-us/dotnet/articles/core/app-types) application.
2. Build a custom copy of .NET Core by compiling [coreclr](https://github.com/dotnet/coreclr) and [corefx](https://github.com/dotnet/corefx).

## Windows Containers

  Windows Containers images use the `microsoft/windowsservercore` and `microsoft/nanoserver` base OS images from Windows Server 2016 Technical Preview 5.  For more information on Windows Containers and a getting started guide, please see: [Windows Containers Documentation](http://aka.ms/windowscontainers).
  
-       `rel-1.0.0-windowsservercore-sdk`
-       `rel-1.0.0-nanoserver-sdk`
-       `rel-1.0.0-windowsservercore-onbuild`
-       `rel-1.0.0-nanoserver-onbuild`
-       `rel-1.0.0-windowsservercore-core`
-       `rel-1.0.0-nanoserver-core`


[dotnet/cli]: https://github.com/dotnet/cli
