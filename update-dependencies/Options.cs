// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CommandLine;
using System.Linq;

namespace Dotnet.Docker.Nightly
{
    public class Options
    {
        public string CliBranch { get; private set; }
        public string CliVersionPrefix { get; private set; }
        public string CliVersionsUrl  =>
            $"https://raw.githubusercontent.com/dotnet/versions/master/build-info/dotnet/cli/{CliBranch}";
        public string DockerVersionFolder { get; private set; }
        public string GitHubEmail { get; private set; }
        public string GitHubPassword { get; private set; }
        public string GitHubProject => "dotnet-docker-nightly";
        public string GitHubUpstreamBranch => "master";
        public string GitHubUpstreamOwner => "dotnet";
        public string GitHubUser { get; private set; }
        public bool UpdateOnly => GitHubEmail == null || GitHubPassword == null || GitHubUser == null;

        public void Parse(string[] args)
        {
            bool result = true;

            ArgumentSyntax argSyntax = ArgumentSyntax.Parse(args, syntax =>
            {
                string cliBranch = "master";
                syntax.DefineOption(
                    "cli-branch",
                    ref cliBranch,
                    "CLI branch to retrieve the SDK from to update the Dockerfiles with (default is master)");
                CliBranch = cliBranch;

                string gitHubEmail = null;
                syntax.DefineOption(
                    "email",
                    ref gitHubEmail,
                    "GitHub email used to make PR (if not specified, a PR will not be created)");
                GitHubEmail = gitHubEmail;

                string gitHubPassword = null;
                syntax.DefineOption(
                    "password",
                    ref gitHubPassword,
                    "GitHub password used to make PR (if not specified, a PR will not be created)");
                GitHubEmail = gitHubPassword;

                string gitHubUser = null;
                syntax.DefineOption(
                    "user",
                    ref gitHubUser,
                    "GitHub user used to make PR (if not specified, a PR will not be created)");
                GitHubUser = gitHubUser;

                string cliVersionPrefix = null;
                syntax.DefineParameter(
                    "cli-prefix",
                    ref cliVersionPrefix,
                    "CLI version prefix associated with the cli-branch");
                CliVersionPrefix = cliVersionPrefix;

                string dockerVersionFolder = null;
                syntax.DefineParameter(
                    "version",
                    ref dockerVersionFolder,
                    "Version folder of this repo to update (e.g. 2.1)");
                DockerVersionFolder = dockerVersionFolder;
            });

            // Workaround for https://github.com/dotnet/corefxlab/issues/1689
            foreach (Argument arg in argSyntax.GetActiveArguments())
            {
                if (arg.IsParameter && !arg.IsSpecified)
                {
                    Console.Error.WriteLine($"error: `{arg.Name}` must be specified.");
                    Environment.Exit(1);
                }
            }
        }
    }
}
