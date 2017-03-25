// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dotnet.Docker.Nightly
{
    public static class CliDependencyHelper
    {
        private static readonly Lazy<HttpClient> DownloadClient = new Lazy<HttpClient>();

        public static string GetSharedFrameworkVersion(string cliVersion)
        {
            Trace.TraceInformation($"Looking for the Shared Framework CLI '{cliVersion}' depends on.");

            string cliCommitHash = GetCommitHash(cliVersion);
            XDocument depVersions = DownloadDependencyVersions(cliCommitHash).Result;
            XNamespace msbuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
            string sharedFrameworkVersion = depVersions.Document.Root
                .Element(msbuildNamespace + "PropertyGroup")
                ?.Element(msbuildNamespace + "CLI_SharedFrameworkVersion")
                ?.Value;
            if (sharedFrameworkVersion == null)
            {
                throw new InvalidOperationException("Can't find CLI_SharedFrameworkVersion in DependencyVersions.props.");
            }

            Trace.TraceInformation($"Detected Shared Framework version '{sharedFrameworkVersion}'.");
            return sharedFrameworkVersion;
        }

        private static string GetCommitHash(string cliVersion)
        {
            using (ZipArchive archive = DownloadCliInstaller(cliVersion).Result)
            {
                ZipArchiveEntry versionTxtEntry = archive.GetEntry($"sdk/{cliVersion}/.version");
                if (versionTxtEntry == null)
                {
                    throw new InvalidOperationException("Can't find `.version` information in installer.");
                }

                using (Stream versionTxt = versionTxtEntry.Open())
                using (var versionTxtReader = new StreamReader(versionTxt))
                {
                    string commitHash = versionTxtReader.ReadLine();
                    Trace.TraceInformation($"Found commit hash '{commitHash}' in `.versions`.");
                    return commitHash;
                }
            }
        }

        private static async Task<XDocument> DownloadDependencyVersions(string cliHash)
        {
            string downloadUrl = $"https://raw.githubusercontent.com/dotnet-bot/cli/{cliHash}/build/DependencyVersions.props";
            Stream stream = await DownloadClient.Value.GetStreamAsync(downloadUrl);
            return XDocument.Load(stream);
        }

        private static async Task<ZipArchive> DownloadCliInstaller(string version)
        {
            string downloadUrl = $"https://dotnetcli.blob.core.windows.net/dotnet/Sdk/{version}/dotnet-dev-win-x64.{version}.zip";
            Stream nupkgStream = await DownloadClient.Value.GetStreamAsync(downloadUrl);
            return new ZipArchive(nupkgStream);
        }
    }
}
