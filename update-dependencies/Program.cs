// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.DotNet.VersionTools;
using Microsoft.DotNet.VersionTools.Automation;
using Microsoft.DotNet.VersionTools.Dependencies;
using Microsoft.DotNet.VersionTools.Dependencies.BuildOutput;
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
        private static Options Options { get; set; } = new Options();
        private static string RepoRoot { get; set; } = Directory.GetCurrentDirectory();
        private const string RuntimeBuildInfoName = "SharedFramework";
        private const string SdkBuildInfoName = "Cli";

        public static void Main(string[] args)
        {
            try
            {
                Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

                Options.Parse(args);

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

            Environment.Exit(0);
        }

        private static DependencyUpdateResults UpdateFiles()
        {
            // TODO:  Read from build-info
            // https://github.com/dotnet/buildtools/blob/master/src/Microsoft.DotNet.VersionTools/BuildManifest/Model/OrchestratedBuildModel.cs
            // https://github.com/dotnet/versions/blob/master/build-info/dotnet/product/cli/release/2.1/build.xml
            string sdkVersion = "2.1.300-preview1-008009";
            string runtimeVersion = "2.1.0-preview1-26126-02";
            string dockerfileVersion = sdkVersion.Substring(0, sdkVersion.LastIndexOf('.'));

            IEnumerable<IDependencyInfo> buildInfos = new[]
            {
                CreateDependencyBuildInfo(SdkBuildInfoName, sdkVersion),
                CreateDependencyBuildInfo(RuntimeBuildInfoName, runtimeVersion),
            };
            IEnumerable<IDependencyUpdater> updaters = GetUpdaters(dockerfileVersion);

            return DependencyUpdateUtils.Update(updaters, buildInfos);
        }

        private static IDependencyInfo CreateDependencyBuildInfo(string name, string latestReleaseVersion)
        {
            return new BuildDependencyInfo(
                new BuildInfo()
                {
                    Name = name,
                    LatestReleaseVersion = latestReleaseVersion,
                    LatestPackages = new Dictionary<string, string>()
                },
                false,
                Enumerable.Empty<string>());
        }

        private static Task CreatePullRequest(DependencyUpdateResults updateResults)
        {
            GitHubAuth gitHubAuth = new GitHubAuth(Options.GitHubPassword, Options.GitHubUser, Options.GitHubEmail);
            PullRequestCreator prCreator = new PullRequestCreator(gitHubAuth, Options.GitHubUser);
            PullRequestOptions prOptions = new PullRequestOptions()
            {
                BranchNamingStrategy = new SingleBranchNamingStrategy($"UpdateDependencies-{Options.GitHubUpstreamBranch}")
            };

            string cliVersion = updateResults.UsedInfos.First(bi => bi.SimpleName == SdkBuildInfoName).SimpleVersion;
            string commitMessage = $"Update {Options.GitHubUpstreamBranch} SDK to {cliVersion}";

            return prCreator.CreateOrUpdateAsync(
                commitMessage,
                commitMessage,
                string.Empty,
                new GitHubBranch(Options.GitHubUpstreamBranch, new GitHubProject(Options.GitHubProject, Options.GitHubUpstreamOwner)),
                new GitHubProject(Options.GitHubProject, gitHubAuth.User),
                prOptions);
        }

        private static IEnumerable<IDependencyUpdater> GetUpdaters(string dockerfileVersion)
        {
            string searchFolder = Path.Combine(RepoRoot, dockerfileVersion);
            string[] dockerfiles = Directory.GetFiles(searchFolder, "Dockerfile", SearchOption.AllDirectories);

            Trace.TraceInformation("Updating the following Dockerfiles:");
            Trace.TraceInformation($"{string.Join(Environment.NewLine, dockerfiles)}");

            return dockerfiles
                .Select(path => CreateDockerfileEnvUpdater(path, "DOTNET_SDK_VERSION", SdkBuildInfoName))
                .Concat(dockerfiles.Select(path => CreateDockerfileEnvUpdater(path, "DOTNET_VERSION", RuntimeBuildInfoName)))
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
