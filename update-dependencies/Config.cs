// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Dotnet.Docker.Nightly
{
    public class Config
    {
        public static Config s_Instance { get; } = new Config();

        private Lazy<string> _userName = new Lazy<string>(() => GetEnvironmentVariable("GITHUB_USER"));
        private Lazy<string> _email = new Lazy<string>(() => GetEnvironmentVariable("GITHUB_EMAIL"));
        private Lazy<string> _password = new Lazy<string>(() => GetEnvironmentVariable("GITHUB_PASSWORD"));

        private Lazy<string> _cliBranch = new Lazy<string>(() => GetEnvironmentVariable("CLI_BRANCH"));
        private Lazy<string> _cliReleaseMoniker = new Lazy<string>(() => GetEnvironmentVariable("CLI_RELEASE_MONIKER", ""));
        private Lazy<string> _cliReleasePrefix = new Lazy<string>(() => GetEnvironmentVariable("CLI_RELEASE_PREFIX"));
        private Lazy<string> _gitHubUpstreamOwner = new Lazy<string>(() => GetEnvironmentVariable("GITHUB_UPSTREAM_OWNER", "dotnet"));
        private Lazy<string> _gitHubProject = new Lazy<string>(() => GetEnvironmentVariable("GITHUB_PROJECT", "dotnet-docker-nightly"));
        private Lazy<string> _gitHubUpstreamBranch = new Lazy<string>(() => GetEnvironmentVariable("GITHUB_UPSTREAM_BRANCH", "master"));
        private Lazy<string[]> _gitHubPullRequestNotifications = new Lazy<string[]>(() =>
                                                GetEnvironmentVariable("GITHUB_PULL_REQUEST_NOTIFICATIONS", "")
                                                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
        private Lazy<string> _runtimeReleasePrefix = new Lazy<string>(() => Environment.GetEnvironmentVariable("RUNTIME_RELEASE_PREFIX"));

        private Config()
        {
        }

        public string UserName => _userName.Value;
        public string Email => _email.Value;
        public string Password => _password.Value;
        public string CliBranch => _cliBranch.Value;
        public string CliReleaseMoniker => _cliReleaseMoniker.Value;
        public string CliReleasePrefix => _cliReleasePrefix.Value;
        public string CliVersionUrl => $"https://raw.githubusercontent.com/dotnet/versions/master/build-info/dotnet/cli/{CliBranch}";
        public string BranchTagPrefix => CliBranch.Replace('/', '-');
        public string GitHubUpstreamOwner => _gitHubUpstreamOwner.Value;
        public string GitHubProject => _gitHubProject.Value;
        public string GitHubUpstreamBranch => _gitHubUpstreamBranch.Value;
        public string[] GitHubPullRequestNotifications => _gitHubPullRequestNotifications.Value;
        public string RuntimeReleasePrefix => _runtimeReleasePrefix.Value ?? CliReleasePrefix;

        private static string GetEnvironmentVariable(string name, string defaultValue = null)
        {
            string value = Environment.GetEnvironmentVariable(name);
            if (value == null)
            {
                value = defaultValue;
            }

            if (value == null)
            {
                throw new InvalidOperationException($"Can't find environment variable '{name}'.");
            }

            return value;
        }
    }
}
