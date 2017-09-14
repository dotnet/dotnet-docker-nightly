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
            VerifyImages("1.1");
        }

        [Fact]
        [Trait("Version", "2.0")]
        [Trait("Architecture", "amd64")]
        public void VerifyImages_2_0()
        {
            VerifyImages("2.0");
        }

        [Fact]
        [Trait("Version", "2.1")]
        [Trait("Architecture", "amd64")]
        public void VerifyImages_2_1()
        {
            VerifyImages(dotNetCoreVersion: "2.1", netcoreappVersion: "2.0", runtimeDepsVersion: "2.0");
        }

        private void VerifyImages(
            string dotNetCoreVersion,
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
                VerifySdkImage_NewRestoreRun(appSdkImage, sdkVersion, netcoreappVersion);
                VerifyRuntimeImage_FrameworkDependentApp(dotNetCoreVersion, appSdkImage);

                if (DockerHelper.IsLinuxContainerModeEnabled)
                {
                    VerifyRuntimeDepsImage_SelfContainedApp(dotNetCoreVersion, runtimeDepsVersion, appSdkImage);
                }
            }
            finally
            {
                DockerHelper.DeleteImage(appSdkImage);
            }
        }

        private void VerifySdkImage_NewRestoreRun(
            string appSdkImage, string sdkImageVersion, string netcoreappVersion)
        {
            string sdkImage = GetDotNetImage(sdkImageVersion, DotNetImageType.SDK);
            string buildArgs = GetBuildArgs($"netcoreapp_version={netcoreappVersion}");
            DockerHelper.Build($"Dockerfile.{DockerHelper.DockerOS.ToLower()}.test", sdkImage, appSdkImage, buildArgs);

            DockerHelper.Run(appSdkImage, "dotnet run", appSdkImage);
        }

        private void VerifyRuntimeImage_FrameworkDependentApp(string runtimeImageVersion, string appSdkImage)
        {
            string frameworkDepAppId = GetIdentifier(runtimeImageVersion, "framework-dependent-app");

            try
            {
                string dotNetCmd = $"dotnet publish -o {DockerHelper.ContainerWorkDir}";
                DockerHelper.Run(appSdkImage, dotNetCmd, frameworkDepAppId, frameworkDepAppId);

                string runtimeImage = GetDotNetImage(runtimeImageVersion, DotNetImageType.Runtime);
                string appDllPath = DockerHelper.GetContainerWorkPath("test.dll");
                DockerHelper.Run(runtimeImage, $"dotnet {appDllPath}", frameworkDepAppId, frameworkDepAppId);
            }
            finally
            {
                DockerHelper.DeleteVolume(frameworkDepAppId);
            }
        }

        private void VerifyRuntimeDepsImage_SelfContainedApp(string dotNetCoreVersion, string runtimeDepsImageVersion, string appSdkImage)
        {
            string selfContainedAppId = GetIdentifier(dotNetCoreVersion, "self-contained-app");
            string rid = "debian.8-x64";

            try
            {
                List<string> args = new List<string>();
                args.Add($"rid={rid}");
                if (dotNetCoreVersion == "2.0")
                {
                    args.Add($"optional_restore_args=/p:runtimeidentifier={rid}");
                }

                string buildArgs = GetBuildArgs(args.ToArray());
                DockerHelper.Build("Dockerfile.linux.publish", appSdkImage, selfContainedAppId, buildArgs);

                try
                {
                    string optionalPublishArgs = dotNetCoreVersion.StartsWith("1.") ? "" : "--no-restore";
                    string dotNetCmd = $"dotnet publish -r {rid} -o {DockerHelper.ContainerWorkDir} {optionalPublishArgs}";
                    DockerHelper.Run(selfContainedAppId, dotNetCmd, selfContainedAppId, selfContainedAppId);

                    string runtimeDepsImage = GetDotNetImage(runtimeDepsImageVersion, DotNetImageType.Runtime_Deps);
                    string appExePath = DockerHelper.GetContainerWorkPath("test");
                    DockerHelper.Run(runtimeDepsImage, appExePath, selfContainedAppId, selfContainedAppId);
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

        public static string GetDotNetImage(string imageVersion, DotNetImageType imageType)
        {
            string variantName = Enum.GetName(typeof(DotNetImageType), imageType).ToLowerInvariant().Replace('_', '-');
            return $"microsoft/dotnet-nightly:{imageVersion}-{variantName}";
        }

        private static string GetIdentifier(string version, string type)
        {
            return $"{version}-{type}-{DateTime.Now.ToFileTime()}";
        }
    }
}
