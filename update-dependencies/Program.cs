// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.DotNet.VersionTools;
using Microsoft.DotNet.VersionTools.Automation;
using Microsoft.DotNet.VersionTools.BuildManifest.Model;
using Microsoft.DotNet.VersionTools.Dependencies;
using Microsoft.DotNet.VersionTools.Dependencies.BuildOutput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dotnet.Docker.Nightly
{
    public static class Program
    {
        private static Options Options { get; set; } = new Options();
        private static string RepoRoot { get; set; } = Directory.GetCurrentDirectory();
        private const string RuntimeBuildInfoName = "Runtime";
        private const string SdkBuildInfoName = "Sdk";

        public static async Task Main(string[] args)
        {
            try
            {
                Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

                Options.Parse(args);

                DependencyUpdateResults updateResults = await UpdateFiles();
                if (!Options.UpdateOnly && updateResults.ChangesDetected())
                {
                    await CreatePullRequest(updateResults);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Failed to update dependencies:{Environment.NewLine}{e.ToString()}");
                Environment.Exit(1);
            }

            Environment.Exit(0);
        }

        private static async Task<DependencyUpdateResults> UpdateFiles()
        {
            Trace.TraceInformation($"Retrieving build info from '{Options.BuildInfoUrl}'");
            Stream stream = await (new HttpClient().GetStreamAsync(Options.BuildInfoUrl));
            XDocument buildInfoXml = XDocument.Load(stream);
            OrchestratedBuildModel buildInfo = OrchestratedBuildModel.Parse(buildInfoXml.Root);
            BuildIdentity sdkBuild = buildInfo.Builds
                .First(build => string.Equals(build.Name, "cli", StringComparison.OrdinalIgnoreCase));
            BuildIdentity coreSetupBuild = buildInfo.Builds
                .First(build => string.Equals(build.Name, "core-setup", StringComparison.OrdinalIgnoreCase));

            // TODO:  Remove once ProductVersion once produced
            EndpointModel endpointInfo = buildInfo.Endpoints
                .First(endpoint => string.Equals(endpoint.Id, "dotnetcli", StringComparison.OrdinalIgnoreCase));
            BlobArtifactModel blobInfo = endpointInfo.Artifacts.Blobs
                .First(artifact => artifact.Id.StartsWith("Runtime", StringComparison.OrdinalIgnoreCase));

            IEnumerable<IDependencyInfo> buildInfos = new[]
            {
                CreateDependencyBuildInfo(SdkBuildInfoName, sdkBuild.BuildId), // TODO replace with ProductVersion once produced
                CreateDependencyBuildInfo(RuntimeBuildInfoName, blobInfo.Id.Split('/')[1]), // TODO replace with coreSetupBuild.ProductVersion once produced
            };

            string dockerfileVersion = sdkBuild.BuildId.Substring(0, sdkBuild.BuildId.LastIndexOf('.'));
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

        private static async Task CreatePullRequest(DependencyUpdateResults updateResults)
        {
            GitHubAuth gitHubAuth = new GitHubAuth(Options.GitHubPassword, Options.GitHubUser, Options.GitHubEmail);
            PullRequestCreator prCreator = new PullRequestCreator(gitHubAuth, Options.GitHubUser);
            PullRequestOptions prOptions = new PullRequestOptions()
            {
                BranchNamingStrategy = new SingleBranchNamingStrategy($"UpdateDependencies-{Options.GitHubUpstreamBranch}")
            };

            string sdkVersion = updateResults.UsedInfos.First(bi => bi.SimpleName == SdkBuildInfoName).SimpleVersion;
            string commitMessage = $"Update {Options.GitHubUpstreamBranch} SDK to {sdkVersion}";

            await prCreator.CreateOrUpdateAsync(
                commitMessage,
                commitMessage,
                string.Empty,
                new GitHubBranch(Options.GitHubUpstreamBranch, new GitHubProject(Options.GitHubProject, Options.GitHubUpstreamOwner)),
                new GitHubProject(Options.GitHubProject, gitHubAuth.User),
                prOptions);
        }

        private static IEnumerable<IDependencyUpdater> GetUpdaters(string dockerfileVersion)
        {
            string[] dockerfiles = Directory.GetFiles(
                Path.Combine(RepoRoot, dockerfileVersion),
                "Dockerfile",
                SearchOption.AllDirectories);

            Trace.TraceInformation("Updating the following Dockerfiles:");
            Trace.TraceInformation(string.Join(Environment.NewLine, dockerfiles));

            return dockerfiles
                .Select(path => CreateDockerfileEnvUpdater(path, "DOTNET_SDK_VERSION", SdkBuildInfoName))
                .Concat(dockerfiles.Select(path => CreateDockerfileEnvUpdater(path, "DOTNET_VERSION", RuntimeBuildInfoName)))
                .Concat(dockerfiles.Select(path => new DockerfileShaUpdater(path)));
        }

        private static IDependencyUpdater CreateDockerfileEnvUpdater(string path, string envName, string buildInfoName)
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
