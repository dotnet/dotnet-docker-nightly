This repository contains Docker images that include last-known-good (LKG) builds for the next release of [.NET Core](https://github.com/dotnet/core).

See [dotnet/dotnet-docker](https://github.com/dotnet/dotnet-docker) for images with official releases of [.NET Core](https://github.com/dotnet/core).

Please be sure to use a build from either the [Windows Server Insider Preview](https://www.microsoft.com/en-us/software-download/windowsinsiderpreviewserver) or the [Windows Insider](https://insider.windows.com/GettingStarted) programs as your Container host before trying to pull these images. Otherwise, pulling these images will **fail**.

Read more about the changes coming to Nano Server in future releases of Windows Server [here](https://docs.microsoft.com/en-us/windows-server/get-started/nano-in-semi-annual-channel).

# Supported Windows tags
- [`2.0.0-runtime-nanoserver`, `2.0.0-runtime`, `2.0-runtime`, `2-runtime`, `runtime` (*2.0/runtime/nanoserver-insider/amd64/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/2.0/runtime/nanoserver-insider/amd64/Dockerfile)
- [`2.0.0-sdk-nanoserver`, `2.0.0-sdk`, `2.0-sdk`, `2-sdk`, `sdk`, `latest` (*2.0/sdk/nanoserver-insider/amd64/Dockerfile*)](https://github.com/dotnet/dotnet-docker-nightly/blob/master/2.0/sdk/nanoserver-insider/amd64/Dockerfile)

# What is .NET Core?

.NET Core is a general purpose development platform maintained by Microsoft and the .NET community on [GitHub](https://github.com/dotnet/core). It is cross-platform, supporting Windows, macOS and Linux, and can be used in device, cloud, and embedded/IoT scenarios.

.NET has several capabilities that make development easier, including automatic memory management, (runtime) generic types, reflection, asynchrony, concurrency, and native interop. Millions of developers take advantage of these capabilities to efficiently build high-quality applications.

You can use C# to write .NET Core apps. C# is simple, powerful, type-safe, and object-oriented while retaining the expressiveness and elegance of C-style languages. Anyone familiar with C and similar languages will find it straightforward to write in C#.

[.NET Core](https://github.com/dotnet/core) is open source (MIT and Apache 2 licenses) and was contributed to the [.NET Foundation](http://dotnetfoundation.org) by Microsoft in 2014. It can be freely adopted by individuals and companies, including for personal, academic or commercial purposes. Multiple companies use .NET Core as part of apps, tools, new platforms and hosting services.

> https://docs.microsoft.com/dotnet/articles/core/

![logo](https://avatars0.githubusercontent.com/u/9141961?v=3&amp;s=100)

# How to use these Images

## Run a .NET Core application with the .NET Core Runtime image

For production scenarios, you will want to deploy and run an application with a .NET Core Runtime image. This results in smaller Docker images compared to the SDK image. You can try the instructions below which make use of [multi-stage build](https://docs.docker.com/engine/userguide/eng-image/multistage-build/) Dockerfiles to build your application with the SDK image and then copy only the published artifacts into a runtime image.

You need to create a `Dockerfile` with the following:

```dockerfile
# escape=`
FROM microsoft/nanoserver-insider-dotnet:sdk AS build-env
WORKDIR /dotnetapp

# copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore -s https://dotnet.myget.org/F/dotnet-core/api/v3/index.json -s https://api.nuget.org/v3/index.json

# copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# build runtime image
FROM microsoft/nanoserver-insider-dotnet:runtime 
WORKDIR /dotnetapp
COPY --from=build-env /dotnetapp/out ./
ENTRYPOINT ["C:\\Program Files\\dotnet\\dotnet.exe", "dotnetapp.dll"]
```

Build and run the Docker image:

```console
docker build -t dotnetapp .
docker run --rm dotnetapp
```

The `Dockerfile` and the Docker commands assumes that your application is called `dotnetapp`. You can change the `Dockerfile` and the commands, as needed.

## Interactively build and run a simple .NET Core application

You can interactively try out .NET Core by taking advantage of the convenience of a container. Try the following set of commands to create and run a .NET Core application in a minute (depending on your internet speed).

```console
docker run -it --rm microsoft/nanoserver-insider-dotnet
[now in the container]
mkdir app
cd app
dotnet new console
dir
dotnet restore -s https://dotnet.myget.org/F/dotnet-core/api/v3/index.json -s https://api.nuget.org/v3/index.json
dotnet run
dotnet bin/Debug/netcoreapp2.0/app.dll
dotnet publish -c Release -o out
dotnet out/app.dll
exit
 ```

The steps above are intended to show the basic functions of .NET Core tools. Try running `dotnet run` twice. You'll see that the second invocation skips compilation. The subsequent command after `dotnet run` demonstrates that you can run an application directly out of the bin folder, without the additional build logic that `dotnet run` adds. The last two commands demonstrate the publishing scenario, which prepares an app to be deployed on the same or other machine, with a requirement on only the .NET Core Runtime, not the larger SDK. Naturally, you don't have to exit immediately, but can continue to try out the product as long as you want.

## Image variants

The `microsoft/nanoserver-insider-dotnet` images come in different flavors, each designed for a specific use case.

### `microsoft/nanoserver-insider-dotnet:<version>-sdk`

This is the defacto image. If you are unsure about what your needs are, you probably want to use this one. It is designed to be used both as a throw away container (mount your source code and start the container to start your app), as well as the base to build other images off of.

It contains the .NET Core SDK which is comprised of two parts:

1. .NET Core
2. .NET Core command line tools

Use this image for your development process (developing, building and testing applications).

### `microsoft/nanoserver-insider-dotnet:<version>-runtime`

This image contains the .NET Core (runtime and libraries) and is optimized for running .NET Core apps in production.

## More Examples using these Images

You can learn more about using .NET Core with Docker with [.NET Docker samples](https://github.com/dotnet/dotnet-docker-samples):

- [Development](https://github.com/dotnet/dotnet-docker-samples/tree/master/dotnetapp-dev) sample using the `sdk` .NET Core SDK image.
- [Production](https://github.com/dotnet/dotnet-docker-samples/tree/master/dotnetapp-prod) sample using the `runtime` .NET Core image.
- [Self-contained](https://github.com/dotnet/dotnet-docker-samples/tree/master/dotnetapp-selfcontained) sample using the `runtime-deps` base OS image (with native dependencies added).

You can directly run a .NET Core Docker image from the [microsoft/dotnet-samples](https://hub.docker.com/r/microsoft/dotnet-samples/) repo.

See [Building Docker Images for .NET Core Applications](https://docs.microsoft.com/dotnet/articles/core/docker/building-net-docker-images) to learn more about the various Docker images and when to use each for them.

## Related Repos

See the following related repos for other application types:

- [microsoft/dotnet](https://hub.docker.com/r/microsoft/dotnet/) for the released .NET Core images.
- [microsoft/aspnetcore](https://hub.docker.com/r/microsoft/aspnetcore/) for ASP.NET Core images.
- [microsoft/aspnet](https://hub.docker.com/r/microsoft/aspnet/) for ASP.NET Web Forms and MVC images.
- [microsoft/dotnet-framework](https://hub.docker.com/r/microsoft/dotnet-framework/) for .NET Framework images (for web applications, see microsoft/aspnet).
- [microsoft/dotnet-nightly](https://hub.docker.com/r/microsoft/dotnet-nightly/) for pre-release .NET Core images (used to experiment with the latest builds).

# This is prerelease software
Windows Server Insider Preview builds may be substantially modified before they are commercially released. Microsoft makes no warranties, express or implied, with respect to the information provided here. Some product features and functionality may require additional hardware or software. These builds are for testing purposes only. Microsoft is not obligated to provide any support services for this preview software.   

----------------------------------------------------------------------------------------------------------------------------------------------------------

License:  By requesting and using this Container OS Image for Windows containers, you acknowledge, understand, and consent to the following Supplemental License Terms:

MICROSOFT SOFTWARE SUPPLEMENTAL LICENSE TERMS
CONTAINER OS IMAGE 
Microsoft Corporation (or based on where you live, one of its affiliates) (referenced as “us,” “we,” or “Microsoft”) licenses this Container OS Image supplement to you (“Supplement”). You are licensed to use this Supplement in conjunction with the underlying host operating system software (“Host Software”) solely to assist running the containers feature in the Host Software.  The Host Software license terms apply to your use of the Supplement. You may not use it if you do not have a license for the Host Software. You may use this Supplement with each validly licensed copy of the Host Software.

ADDITIONAL LICENSING REQUIREMENTS AND/OR USE RIGHTS 
Your use of the Supplement as specified in the preceding paragraph may result in the creation or modification of a container image (“Container Image”) that includes certain Supplement components. For clarity, a Container Image is separate and distinct from a virtual machine or virtual appliance image.  Pursuant to these license terms, we grant you a restricted right to redistribute such Supplement components under the following conditions:
	(i) you may use the Supplement components only as used in, and as a part of your Container Image,
	(ii) you may use such Supplement components in your Container Image as long as you have significant primary functionality in your Container Image that is materially separate and distinct from the Supplement; and 
	(iii) you agree to include these license terms (or similar terms required by us or a hoster) with your Container Image to properly license the possible use of the Supplement components by your end-users.
We reserve all other rights not expressly granted herein.

By using this Supplement, you accept these terms. If you do not accept them, do not use this Supplement.

As part of the Supplemental License Terms for this Container OS Image for Windows containers, you are also subject to the underlying Windows Server host software license terms, which are located at: https://www.microsoft.com/en-us/useterms.

# User Feedback

## Issues

If you have any problems with or questions about this image, please contact us through a [GitHub issue](https://github.com/dotnet/dotnet-docker-nightly/issues).

## Contributing

You are invited to contribute new features, fixes, or updates, large or small; we are always thrilled to receive pull requests, and do our best to process them as fast as we can.

Before you start to code, please read the [.NET Core contribution guidelines](https://github.com/dotnet/coreclr/blob/master/CONTRIBUTING.md).

## Documentation

You can read documentation for .NET Core, including Docker usage in the [.NET Core docs](https://docs.microsoft.com/en-us/dotnet/articles/core/). The docs are [open source on GitHub](https://github.com/dotnet/core-docs). Contributions are welcome!

[win-containers]: http://aka.ms/windowscontainers