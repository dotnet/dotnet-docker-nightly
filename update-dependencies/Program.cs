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
            try
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
            catch (Exception e)
            {
                Console.Error.WriteLine($"Failed to update dependencies:{Environment.NewLine}{e.ToString()}");
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
            // Adjust the LatestReleaseVersion since it is not the full version and all consumers here need it to be.
            cliBuildInfo.LatestReleaseVersion = $"{s_config.RuntimeReleasePrefix}-{cliBuildInfo.LatestReleaseVersion}";
            string sharedFrameworkVersion = CliDependencyHelper.GetSharedFrameworkVersion(cliBuildInfo.LatestReleaseVersion);

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
            string commitMessage = $"Update {s_config.BranchTagPrefix} SDK to {cliVersion}";

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
            string majorMinorVersion = s_config.CliReleasePrefix.Substring(0, s_config.CliReleasePrefix.LastIndexOf('.'));
            string searchFolder = Path.Combine(s_repoRoot, majorMinorVersion);
            string[] dockerfiles = Directory.GetFiles(searchFolder, "Dockerfile", SearchOption.AllDirectories);
            Trace.TraceInformation("Updating the following Dockerfiles:");
            Trace.TraceInformation($"{string.Join(Environment.NewLine, dockerfiles)}");
            return dockerfiles.Select(path => CreateDockerfileEnvUpdater(path, "DOTNET_SDK_VERSION", CliBuildInfoName))
                .Concat(dockerfiles.Select(path => CreateDockerfileEnvUpdater(path, "DOTNET_VERSION", SharedFrameworkBuildInfoName)))
                .Concat(dockerfiles.Select(path => new DockerfileShaUpdater(path)));
        }

        private static IDependencyUpdater CreateDockerfileEnvUpdater(
            string path, string envName, string buildInfoName)
        {
            return new FileRegexReleaseUpdater()
            {
                Path = path,
                BuildInfoName = buildInfoName,
                Regex = new Regex($"ENV {envName} (?<envValue>[^\r\n]*)"),
                VersionGroupName = "envValue"
            };
        }
    }
}
