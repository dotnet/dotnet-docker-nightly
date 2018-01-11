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
    public class CliDependencyHelper
    {
        private static readonly Lazy<HttpClient> DownloadClient = new Lazy<HttpClient>();

        public string CliVersion { get; }
        public int CliMajorVersion { get; }

        public CliDependencyHelper(string cliVersion)
        {
            if (string.IsNullOrWhiteSpace(cliVersion))
            {
                throw new ArgumentNullException(nameof(cliVersion));
            }

            CliVersion = cliVersion;
            CliMajorVersion = Int32.Parse(cliVersion.Substring(0, cliVersion.IndexOf('.')));
        }

        public string GetSharedFrameworkVersion()
        {
            Trace.TraceInformation($"Looking for the Shared Framework CLI '{CliVersion}' depends on.");

            string cliCommitHash = GetCommitHash();
            XDocument depVersions = DownloadDependencyVersions(cliCommitHash).Result;
            XNamespace msbuildNamespace = depVersions.Document.Root.GetDefaultNamespace();
            const string sharedFrameworkProperty = "MicrosoftNETCoreAppPackageVersion";
            string sharedFrameworkVersion = depVersions.Document.Root
                .Element(msbuildNamespace + "PropertyGroup")
                ?.Element(msbuildNamespace + sharedFrameworkProperty)
                ?.Value;
            if (sharedFrameworkVersion == null)
            {
                throw new InvalidOperationException($"Can't find '{sharedFrameworkProperty}' in DependencyVersions.props.");
            }

            Trace.TraceInformation($"Detected Shared Framework version '{sharedFrameworkVersion}'.");
            return sharedFrameworkVersion;
        }

        private string GetCommitHash()
        {
            using (ZipArchive archive = DownloadCliInstaller().Result)
            {
                string zipEntryName = CliMajorVersion == 1 ? $"sdk/{CliVersion}/.version" : $"sdk\\{CliVersion}\\.version";
                ZipArchiveEntry versionTxtEntry = archive.GetEntry(zipEntryName);
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

        private async Task<XDocument> DownloadDependencyVersions(string cliHash)
        {
            string dependenciesFileName = CliMajorVersion == 1 ? $"Microsoft.DotNet.Cli.DependencyVersions" : $"DependencyVersions";
            string downloadUrl = $"https://raw.githubusercontent.com/dotnet-bot/cli/{cliHash}/build/{dependenciesFileName}.props";
            Trace.TraceInformation($"Downloading `{downloadUrl}`");
            Stream stream = await DownloadClient.Value.GetStreamAsync(downloadUrl);
            return XDocument.Load(stream);
        }

        private async Task<ZipArchive> DownloadCliInstaller()
        {
            string installerName = CliMajorVersion == 1 ? $"dotnet-dev-win-x64.{CliVersion}" : $"dotnet-sdk-{CliVersion}-win-x64";
            string downloadUrl = $"https://dotnetcli.blob.core.windows.net/dotnet/Sdk/{CliVersion}/{installerName}.zip";
            Trace.TraceInformation($"Downloading `{downloadUrl}`");
            Stream nupkgStream = await DownloadClient.Value.GetStreamAsync(downloadUrl);
            return new ZipArchive(nupkgStream);
        }
    }
}
