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
        private static Options Options { get; set; }
        private static string RepoRoot => Directory.GetCurrentDirectory();
        private const string SharedFrameworkBuildInfoName = "SharedFramework";

        public static int Main(string[] args)
        {
            try
            {
                Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

                Options = Options.Parse(args);

                if (Options == null)
                {
                    Environment.Exit(1);
                }

                DependencyUpdateResults updateResults = UpdateFiles();

                if (!Options.UpdateOnly && updateResults.ChangesDetected())
                {
                    CreatePullRequest(updateResults).Wait();
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Failed to update dependencies:{Environment.NewLine}{e.ToString()}");
                Environment.Exit(1);
            }

            return 0;
        }

        private static DependencyUpdateResults UpdateFiles()
        {
            // Ideally this logic would depend on the CLI produces and consumes metadata.  Since it doesn't
            // exist various version information is inspected to obtain the latest CLI version along with
            // the runtime (e.g. shared framework) it depends on.

            BuildInfo cliBuildInfo = BuildInfo.Get(CliBuildInfoName, Options.CliVersionsUrl, fetchLatestReleaseFile: false);
            // Adjust the LatestReleaseVersion since it is not the full version and all consumers here need it to be.
            cliBuildInfo.LatestReleaseVersion = $"{Options.CliVersionPrefix}-{cliBuildInfo.LatestReleaseVersion}";
            CliDependencyHelper cliDependencyHelper = new CliDependencyHelper(cliBuildInfo.LatestReleaseVersion);
            string sharedFrameworkVersion = cliDependencyHelper.GetSharedFrameworkVersion();

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
            IEnumerable<IDependencyUpdater> updaters = GetUpdaters(cliDependencyHelper);

            return DependencyUpdateUtils.Update(updaters, buildInfos);
        }

        private static Task CreatePullRequest(DependencyUpdateResults updateResults)
        {
            string cliVersion = updateResults.UsedBuildInfos.First(bi => bi.Name == CliBuildInfoName).LatestReleaseVersion;
            string commitMessage = $"Update {Options.GitHubUpstreamBranch} SDK to {cliVersion}";

            GitHubAuth gitHubAuth = new GitHubAuth(Options.GitHubPassword, Options.GitHubUser, Options.GitHubEmail);

            PullRequestCreator prCreator = new PullRequestCreator(
                gitHubAuth,
                new GitHubProject(Options.GitHubProject, gitHubAuth.User),
                new GitHubBranch(Options.GitHubUpstreamBranch, new GitHubProject(Options.GitHubProject, Options.GitHubUpstreamOwner)),
                Options.GitHubUser,
                new SingleBranchNamingStrategy($"UpdateDependencies-{Options.GitHubUpstreamBranch}")
            );

            return prCreator.CreateOrUpdateAsync(commitMessage, commitMessage, string.Empty);
        }

        private static IEnumerable<IDependencyUpdater> GetUpdaters(CliDependencyHelper cliDependencyHelper)
        {
            string majorMinorVersion = Options.DockerVersionedFolder.Substring(0, Options.DockerVersionedFolder.LastIndexOf('.'));
            string[] dockerfiles = GetDockerfiles(majorMinorVersion);
            Trace.TraceInformation("Updating the following Dockerfiles:");
            Trace.TraceInformation($"{string.Join(Environment.NewLine, dockerfiles)}");
            IEnumerable<IDependencyUpdater> updaters = dockerfiles
                .Select(path => CreateSDKDockerfileEnvUpdater(path, CliBuildInfoName))
                .Concat(dockerfiles.Select(path => CreateDockerfileEnvUpdater(path, "DOTNET_VERSION", SharedFrameworkBuildInfoName)));

            if (cliDependencyHelper.CliMajorVersion > 1)
            {
                updaters = updaters.Concat(dockerfiles.Select(path => new DockerfileShaUpdater(path)));
            }
            else if (Options.DockerVersionedFolder.StartsWith("1.1"))
            {
                dockerfiles = GetDockerfiles("1.0");
                updaters = updaters.Concat(dockerfiles.Select(path => CreateSDKDockerfileEnvUpdater(path, CliBuildInfoName)));
            }

            return updaters;
        }

        private static string[] GetDockerfiles(string version)
        {
            string searchFolder = Path.Combine(RepoRoot, version);
            return Directory.GetFiles(searchFolder, "Dockerfile", SearchOption.AllDirectories);
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

        private static IDependencyUpdater CreateSDKDockerfileEnvUpdater(string path, string buildInfoName)
        {
            return CreateDockerfileEnvUpdater(path, "DOTNET_SDK_VERSION", CliBuildInfoName);
        }
    }
}
