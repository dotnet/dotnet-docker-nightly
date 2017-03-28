// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.DotNet.VersionTools;
using Microsoft.DotNet.VersionTools.Automation;
using Microsoft.DotNet.VersionTools.Dependencies;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dotnet.Docker.Nightly
{
    public static class Program
    {
        private const string CliBuildInfoName = "Cli";
        private const string SharedFrameworkBuildInfoName = "SharedFramework";
        private static readonly string s_repoRoot = Directory.GetCurrentDirectory();
        private static readonly Config s_config = Config.s_Instance;
        private static bool s_updateOnly = false;

        public static void Main(string[] args)
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

            if (ParseArgs(args))
            {
                DependencyUpdateResults updateResults = UpdateFiles();

                if (!s_updateOnly && updateResults.ChangesDetected())
                {
                    CreatePullRequest(updateResults).Wait();
                }
            }
        }

        private static bool ParseArgs(string[] args)
        {
            foreach (string arg in args)
            {
                if (string.Equals(arg, "--Update", StringComparison.OrdinalIgnoreCase))
                {
                    s_updateOnly = true;
                }
                else if (arg.Contains('='))
                {
                    int delimiterIndex = arg.IndexOf('=');
                    string name = arg.Substring(0, delimiterIndex);
                    string value = arg.Substring(delimiterIndex + 1);

                    Environment.SetEnvironmentVariable(name, value);
                }
                else
                {
                    Console.Error.WriteLine($"Unrecognized argument '{arg}'");
                    return false;
                }
            }

            return true;
        }

        private static DependencyUpdateResults UpdateFiles()
        {
            // Ideally this logic would depend on the CLI produces and consumes metadata.  Since it doesn't
            // exist various version information is inspected to obtain the latest CLI version along with
            // the runtime (e.g. shared framework) it depends on.

            BuildInfo cliBuildInfo = BuildInfo.Get(CliBuildInfoName, s_config.CliVersionUrl, fetchLatestReleaseFile: false);
            string sharedFrameworkVersion = CliDependencyHelper.GetSharedFrameworkVersion(
                $"{s_config.RuntimeReleasePrefix}-{cliBuildInfo.LatestReleaseVersion}");

            IEnumerable<DependencyBuildInfo> buildInfos = new[]
            {
                new DependencyBuildInfo(cliBuildInfo, false, Enumerable.Empty<string>()),
                new DependencyBuildInfo(
                    new BuildInfo() 
                    {
                        Name = SharedFrameworkBuildInfoName,
                        LatestReleaseVersion = sharedFrameworkVersion,
                        LatestPackages = new Dictionary<string, string>()
                    },
                    false,
                    Enumerable.Empty<string>()),
            };
            IEnumerable<IDependencyUpdater> updaters = GetUpdaters();

            return DependencyUpdateUtils.Update(updaters, buildInfos);
        }

        private static Task CreatePullRequest(DependencyUpdateResults updateResults)
        {
            string cliVersion = updateResults.UsedBuildInfos.First(bi => bi.Name == CliBuildInfoName).LatestReleaseVersion;
            string commitMessage = $"Update {s_config.BranchTagPrefix} SDK to {s_config.CliReleasePrefix}-{cliVersion}";

            GitHubAuth gitHubAuth = new GitHubAuth(s_config.Password, s_config.UserName, s_config.Email);

            PullRequestCreator prCreator = new PullRequestCreator(
                gitHubAuth,
                new GitHubProject(s_config.GitHubProject, gitHubAuth.User),
                new GitHubBranch(s_config.GitHubUpstreamBranch, new GitHubProject(s_config.GitHubProject, s_config.GitHubUpstreamOwner)),
                s_config.UserName,
                new SingleBranchNamingStrategy($"UpdateDependencies-{s_config.BranchTagPrefix}")
            );

            return prCreator.CreateOrUpdateAsync(commitMessage, commitMessage, string.Empty);
        }

        private static IEnumerable<IDependencyUpdater> GetUpdaters()
        {
            string[] dockerfiles = Directory.GetFiles(s_repoRoot, "Dockerfile", SearchOption.AllDirectories);
            return dockerfiles.Select(path => CreateSdkUpdater(path))
                .Concat(dockerfiles.Select(path => CreateRuntimeUpdater(path)));
        }

        private static IDependencyUpdater CreateSdkUpdater(string path)
        {
            string versionRegex;
            if (string.IsNullOrEmpty(s_config.CliReleaseMoniker))
            {
                versionRegex = $@"{s_config.CliReleasePrefix}-(?<version>[^\r\n]*)";
            }
            else
            {
                versionRegex = $@"[\d\.]*-(?<version>{s_config.CliReleaseMoniker}-\d+)\r\n";
            }

            return new FileRegexReleaseUpdater()
            {
                Path = path,
                BuildInfoName = CliBuildInfoName,
                Regex = new Regex($"ENV DOTNET_SDK_VERSION {versionRegex}"),
                VersionGroupName = "version"
            };
        }

        private static IDependencyUpdater CreateRuntimeUpdater(string path)
        {
            return new FileRegexReleaseUpdater()
            {
                Path = path,
                BuildInfoName = SharedFrameworkBuildInfoName,
                Regex = new Regex($@"ENV DOTNET_VERSION (?<version>{s_config.RuntimeReleasePrefix}-[^\r\n]*)"),
                VersionGroupName = "version"
            };
        }
    }
}
