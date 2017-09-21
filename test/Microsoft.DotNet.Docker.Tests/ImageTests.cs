// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions;

namespace Microsoft.DotNet.Docker.Tests
{
    public class ImageTests
    {
        private DockerHelper DockerHelper { get; set; }

        public ImageTests(ITestOutputHelper output)
        {
            DockerHelper = new DockerHelper(output);
        }

        [Fact]
        [Trait("Version", "1.0")]
        [Trait("Architecture", "amd64")]
        public void VerifyImages_1_0()
        {
            VerifyImages(dotNetCoreVersion: "1.0", sdkVersion: "1.1");
        }

        [Fact]
        [Trait("Version", "1.1")]
        [Trait("Architecture", "amd64")]
        public void VerifyImages_1_1()
        {
            VerifyImages(dotNetCoreVersion: "1.1", runtimeDepsVersion: "1.0");
        }

        [Theory]
        [MemberData(nameof(GetImageOsVariants_2))]
        [Trait("Version", "2.0")]
        [Trait("Architecture", "amd64")]
        public void VerifyImages_2_0(string osVariant)
        {
            VerifyImages(dotNetCoreVersion: "2.0", osVariant: osVariant);
        }

        [Theory]
        [MemberData(nameof(GetImageOsVariants_2))]
        [Trait("Version", "2.1")]
        [Trait("Architecture", "amd64")]
        public void VerifyImages_2_1(string osVariant)
        {
            VerifyImages(dotNetCoreVersion: "2.1", osVariant: osVariant, runtimeDepsVersion: "2.0");
        }

        public static IEnumerable<object[]> GetImageOsVariants_2()
        {
            // null represents the default os variant - e.g. debian:stretch/nanoserver
            yield return new object[] { null };

            if (DockerHelper.DockerOS == "linux")
            {
                yield return new object[] { "jessie" };
            }
        }

        private void VerifyImages(
            string dotNetCoreVersion,
            string osVariant = null,
            string netcoreappVersion = null,
            string sdkVersion = null,
            string runtimeDepsVersion = null)
        {
            if (netcoreappVersion == null)
            {
                netcoreappVersion = dotNetCoreVersion;
            }

            if (sdkVersion == null)
            {
                sdkVersion = dotNetCoreVersion;
            }

            if (runtimeDepsVersion == null)
            {
                runtimeDepsVersion = dotNetCoreVersion;
            }

            string appSdkImage = GetIdentifier(dotNetCoreVersion, "app-sdk");

            try
            {
                VerifySdkImage_NewRestoreRun(appSdkImage, sdkVersion, osVariant, netcoreappVersion);
                VerifyRuntimeImage_FrameworkDependentApp(dotNetCoreVersion, osVariant, appSdkImage);

                if (DockerHelper.IsLinuxContainerModeEnabled)
                {
                    VerifyRuntimeDepsImage_SelfContainedApp(dotNetCoreVersion, runtimeDepsVersion, osVariant, appSdkImage);
                }
            }
            finally
            {
                DockerHelper.DeleteImage(appSdkImage);
            }
        }

        private void VerifySdkImage_NewRestoreRun(
            string appSdkImage, string sdkImageVersion, string osVariant, string netcoreappVersion)
        {
            // dotnet new, restore, build a new app using the sdk image
            List<string> args = new List<string>();
            args.Add($"netcoreapp_version={netcoreappVersion}");
            if (!sdkImageVersion.StartsWith("1."))
            {
                args.Add($"optional_new_args=--no-restore");
            }

            string buildArgs = GetBuildArgs(args.ToArray());
            string sdkImage = GetDotNetImage(sdkImageVersion, DotNetImageType.SDK, osVariant);

            DockerHelper.Build(
                dockerfile: $"Dockerfile.{DockerHelper.DockerOS.ToLower()}.testapp",
                fromImage: sdkImage,
                tag: appSdkImage,
                buildArgs: buildArgs);

            // dotnet run the new app using the sdk image
            DockerHelper.Run(
                image: appSdkImage,
                command: "dotnet run",
                containerName: appSdkImage);
        }

        private void VerifyRuntimeImage_FrameworkDependentApp(
            string runtimeImageVersion, string osVariant, string appSdkImage)
        {
            string frameworkDepAppId = GetIdentifier(runtimeImageVersion, "framework-dependent-app");

            try
            {
                // Publish the app to a Docker volume using the app's sdk image
                DockerHelper.Run(
                    image: appSdkImage,
                    command: $"dotnet publish -o {DockerHelper.ContainerWorkDir}",
                    containerName: frameworkDepAppId,
                    volumeName: frameworkDepAppId);

                // Run the app in the Docker volume to verify the runtime image
                string runtimeImage = GetDotNetImage(runtimeImageVersion, DotNetImageType.Runtime, osVariant);
                string appDllPath = DockerHelper.GetContainerWorkPath("testApp.dll");
                DockerHelper.Run(
                    image: runtimeImage,
                    command: $"dotnet {appDllPath}",
                    containerName: frameworkDepAppId,
                    volumeName: frameworkDepAppId);
            }
            finally
            {
                DockerHelper.DeleteVolume(frameworkDepAppId);
            }
        }

        private void VerifyRuntimeDepsImage_SelfContainedApp(
            string dotNetCoreVersion, string runtimeDepsImageVersion, string osVariant, string appSdkImage)
        {
            string selfContainedAppId = GetIdentifier(dotNetCoreVersion, "self-contained-app");
            string rid = "debian.8-x64";

            try
            {
                // Build a self-contained app
                string buildArgs = GetBuildArgs($"rid={rid}");
                DockerHelper.Build(
                    dockerfile: "Dockerfile.linux.testapp.selfcontained",
                    fromImage: appSdkImage,
                    tag: selfContainedAppId,
                    buildArgs: buildArgs);

                try
                {
                    // Publish the self-contained app to a Docker volume using the app's sdk image
                    string optionalPublishArgs = dotNetCoreVersion.StartsWith("1.") ? "" : "--no-restore";
                    string dotNetCmd = $"dotnet publish -r {rid} -o {DockerHelper.ContainerWorkDir} {optionalPublishArgs}";
                    DockerHelper.Run(
                        image: selfContainedAppId,
                        command: dotNetCmd,
                        containerName: selfContainedAppId,
                        volumeName: selfContainedAppId);

                    // Run the self-contained app in the Docker volume to verify the runtime-deps image
                    string runtimeDepsImage = GetDotNetImage(runtimeDepsImageVersion, DotNetImageType.Runtime_Deps, osVariant);
                    string appExePath = DockerHelper.GetContainerWorkPath("testApp");
                    DockerHelper.Run(
                        image: runtimeDepsImage,
                        command: appExePath,
                        containerName: selfContainedAppId,
                        volumeName: selfContainedAppId);
                }
                finally
                {
                    DockerHelper.DeleteVolume(selfContainedAppId);
                }
            }
            finally
            {
                DockerHelper.DeleteImage(selfContainedAppId);
            }
        }

        private static string GetBuildArgs(params string[] args)
        {
            string buildArgs = string.Empty;

            if (args != null && args.Any())
            {
                foreach (string arg in args)
                {
                    buildArgs += $" --build-arg {arg}";
                }
            }

            return buildArgs;
        }

        public static string GetDotNetImage(string imageVersion, DotNetImageType imageType, string osVariant)
        {
            string variantName = Enum.GetName(typeof(DotNetImageType), imageType).ToLowerInvariant().Replace('_', '-');
            string imageName = $"microsoft/dotnet-nightly:{imageVersion}-{variantName}";
            if (!string.IsNullOrEmpty(osVariant))
            {
                imageName += $"-{osVariant}";
            }

            return imageName;
        }

        private static string GetIdentifier(string version, string type)
        {
            return $"{version}-{type}-{DateTime.Now.ToFileTime()}";
        }
    }
}
