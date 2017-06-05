// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.DotNet.VersionTools;
using Microsoft.DotNet.VersionTools.Dependencies;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Dotnet.Docker.Nightly
{
    /// <summary>
    /// An IDependencyUpdater that will scan a Dockerfile for the .NET Core artifacts that are installed.
    /// The updater will then retrieve and update the checksum sha used to validate the downloaded artifacts.
    /// </summary>
    public class DockerfileShaUpdater : FileRegexUpdater
    {
        public DockerfileShaUpdater(string dockerfilePath) : base()
        {
            Path = dockerfilePath;
            Regex = new Regex($"ENV (?<name>DOTNET_[^\r\n]*DOWNLOAD_SHA) (?<value>[^\r\n]*)");
            VersionGroupName = "value";
        }

        protected override string TryGetDesiredValue(
            IEnumerable<DependencyBuildInfo> dependencyBuildInfos,
            out IEnumerable<BuildInfo> usedBuildInfos)
        {
            string sha = null;
            usedBuildInfos = Enumerable.Empty<BuildInfo>();

            Trace.TraceInformation($"DockerfileShaUpdater is processing '{Path}'.");
            string dockerfile = File.ReadAllText(Path);

            Regex versionRegex = new Regex($"ENV (?<name>DOTNET_[^\r\n]*VERSION) (?<value>[^\r\n]*)");
            Match versionMatch = versionRegex.Match(dockerfile);
            if (versionMatch.Success)
            {
                string versionEnvName = versionMatch.Groups["name"].Value;
                string version = versionMatch.Groups["value"].Value;

                Regex shaRegex = new Regex($"ENV (DOTNET_[^\r\n]*DOWNLOAD_URL) (?<value>[^\r\n]*)");
                Match shaMatch = shaRegex.Match(dockerfile);
                if (shaMatch.Success)
                {
                    // TODO:  Cleanup differences in sha extensions - https://github.com/dotnet/cli/issues/6724
                    string shaExt = versionEnvName.Contains("SDK") ? ".sha" : ".sha512";
                    string shaUrl = shaMatch.Groups["value"].Value
                        .Replace("dotnetcli", "dotnetclichecksums")
                        .Replace($"${versionEnvName}", version)
                        + shaExt;

                    Trace.TraceInformation($"Downloading '{shaUrl}'.");
                    using (Stream shaStream = new HttpClient().GetStreamAsync(shaUrl).Result)
                    using (StreamReader reader = new StreamReader(shaStream))
                    {
                        sha = reader.ReadToEnd();
                    }
                }
                else
                {
                    Trace.TraceInformation($"DockerfileShaUpdater no-op - checksum url not found.");
                }
            }
            else
            {
                Trace.TraceInformation($"DockerfileShaUpdater no-op - dotnet url not found.");
            }

            return sha;
        }
    }
}
