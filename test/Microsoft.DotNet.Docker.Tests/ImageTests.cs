// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.Docker.Tests
{
    public class ImageTests
    {
        private static string ArchFilter => Environment.GetEnvironmentVariable("IMAGE_ARCH_FILTER");
        private static string VersionFilter => Environment.GetEnvironmentVariable("IMAGE_VERSION_FILTER");

        private DockerHelper DockerHelper { get; set; }

        public ImageTests(ITestOutputHelper output)
        {
            DockerHelper = new DockerHelper(output);
        }

        public static IEnumerable<object[]> GetVerifyImagesData()
        {
            List<VerifyImageDescriptor> testData = new List<VerifyImageDescriptor>
            {
                new VerifyImageDescriptor {DotNetCoreVersion = "1.0", SdkVersion = "1.1"},
                new VerifyImageDescriptor {DotNetCoreVersion = "1.1", RuntimeDepsVersion = "1.0"},
                new VerifyImageDescriptor {DotNetCoreVersion = "2.0"},
                new VerifyImageDescriptor {DotNetCoreVersion = "2.1", RuntimeDepsVersion = "2.0" },
            };

            if (DockerHelper.IsLinuxContainerModeEnabled)
            {
                testData.AddRange(new List<VerifyImageDescriptor>
                    {
                        new VerifyImageDescriptor {DotNetCoreVersion = "2.0", OsVariant = "jessie"},
                        new VerifyImageDescriptor {DotNetCoreVersion = "2.1", RuntimeDepsVersion = "2.0", OsVariant = "jessie", },
                    });
            }

            // Filter out test data that does not match the active architecture and version filters.
            return testData
                .Where(descriptor => ArchFilter == null
                    || string.Equals(descriptor.Architecture, ArchFilter, StringComparison.OrdinalIgnoreCase))
                .Where(descriptor => VersionFilter == null || descriptor.DotNetCoreVersion.StartsWith(VersionFilter))
                .Select(descriptor => new object[] { descriptor });
        }

        [Theory]
        [MemberData(nameof(GetVerifyImagesData))]
        public void VerifyImages(VerifyImageDescriptor descriptor)
        {
            string appSdkImage = GetIdentifier(descriptor.DotNetCoreVersion, "app-sdk");

            try
            {
                VerifySdkImage_NewRestoreRun(descriptor, appSdkImage);
                VerifyRuntimeImage_FrameworkDependentApp(descriptor, appSdkImage);

                if (DockerHelper.IsLinuxContainerModeEnabled)
                {
                    VerifyRuntimeDepsImage_SelfContainedApp(descriptor, appSdkImage);
                }
            }
            finally
            {
                DockerHelper.DeleteImage(appSdkImage);
            }
        }

        private void VerifySdkImage_NewRestoreRun(VerifyImageDescriptor descriptor, string appSdkImage)
        {
            // dotnet new, restore, build a new app using the sdk image
            List<string> args = new List<string>();
            args.Add($"netcoreapp_version={descriptor.DotNetCoreVersion}");
            if (!descriptor.SdkVersion.StartsWith("1."))
            {
                args.Add($"optional_new_args=--no-restore");
            }

            string buildArgs = GetBuildArgs(args.ToArray());
            string sdkImage = GetDotNetImage(descriptor.SdkVersion, DotNetImageType.SDK, descriptor.OsVariant);

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

        private void VerifyRuntimeImage_FrameworkDependentApp(VerifyImageDescriptor descriptor, string appSdkImage)
        {
            string frameworkDepAppId = GetIdentifier(descriptor.DotNetCoreVersion, "framework-dependent-app");

            try
            {
                // Publish the app to a Docker volume using the app's sdk image
                DockerHelper.Run(
                    image: appSdkImage,
                    command: $"dotnet publish -o {DockerHelper.ContainerWorkDir}",
                    containerName: frameworkDepAppId,
                    volumeName: frameworkDepAppId);

                // Run the app in the Docker volume to verify the runtime image
                string runtimeImage = GetDotNetImage(descriptor.DotNetCoreVersion, DotNetImageType.Runtime, descriptor.OsVariant);
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

        private void VerifyRuntimeDepsImage_SelfContainedApp(VerifyImageDescriptor descriptor, string appSdkImage)
        {
            string selfContainedAppId = GetIdentifier(descriptor.DotNetCoreVersion, "self-contained-app");
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
                    string optionalPublishArgs = descriptor.DotNetCoreVersion.StartsWith("1.") ? "" : "--no-restore";
                    string dotNetCmd = $"dotnet publish -r {rid} -o {DockerHelper.ContainerWorkDir} {optionalPublishArgs}";
                    DockerHelper.Run(
                        image: selfContainedAppId,
                        command: dotNetCmd,
                        containerName: selfContainedAppId,
                        volumeName: selfContainedAppId);

                    // Run the self-contained app in the Docker volume to verify the runtime-deps image
                    string runtimeDepsImage = GetDotNetImage(descriptor.RuntimeDepsVersion, DotNetImageType.Runtime_Deps, descriptor.OsVariant);
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
