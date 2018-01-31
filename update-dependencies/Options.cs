// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Microsoft.DotNet.Cli.CommandLine;

namespace Dotnet.Docker.Nightly
{
    public class Options
    {
        public string CliBranch { get; private set; }
        public string CliVersionPrefix { get; private set; }
        public string CliVersionsUrl =>
            $"https://raw.githubusercontent.com/dotnet/versions/master/build-info/dotnet/cli/{CliBranch}";
        public string DockerVersionedFolder { get; private set; }
        public string GitHubEmail { get; private set; }
        public string GitHubPassword { get; private set; }
        public string GitHubProject => "dotnet-docker-nightly";
        public string GitHubUpstreamBranch => "master";
        public string GitHubUpstreamOwner => "dotnet";
        public string GitHubUser { get; private set; }
        public bool UpdateOnly => GitHubEmail == null || GitHubPassword == null || GitHubUser == null;

        private static Command CreateCommand()
        {
            return Create.Command(
                "update-dependencies",
                "Updates the .NET Core components of the Dockerfiles to the latest versions.",
                // Accept.ExactlyOneArgument()
                //     .With(name: "version", description: "The versioned folder of this repo to update (e.g. 2.1).")
                //     .And(Accept.ExactlyOneArgument()
                //         .With(name: "cli-prefix", description: "The CLI version prefix associated with the CliBranch.")),
                Create.Option(
                    "--version",
                    "The versioned folder of this repo to update (e.g. 2.1).",
                    Accept.ExactlyOneArgument()),
                Create.Option(
                    "--cli-branch",
                    "The CLI branch to retrieve the SDK to update the Dockerfiles with.",
                    Accept.ExactlyOneArgument()
                        .With(defaultValue: () => "master")),
                Create.Option(
                    "--cli-prefix",
                    "The CLI version prefix associated with the CliBranch.",
                    Accept.ExactlyOneArgument()),
                Create.Option(
                    "--user",
                    "GitHub user used while making PR.  If not specified PR is not made.",
                    Accept.ExactlyOneArgument()),
                Create.Option(
                    "--email",
                    "GitHub email used while making PR.  If not specified PR is not made.",
                    Accept.ExactlyOneArgument()),
                Create.Option(
                    "--password",
                    "GitHub password used while making PR.  If not specified PR is not made.",
                    Accept.ExactlyOneArgument())
                );
        }

        public static Options Parse(string[] args)
        {
            Options options = new Options();

            Command command = CreateCommand();
            ParseResult result = command.Parse(args);

            if (result.Errors.Any())
            {
                string msg = string.Join(Environment.NewLine, result.Errors);
                Console.WriteLine(msg);
                return null;
            }

            Console.WriteLine(command.HelpView());
            AppliedOption appliedCommand = result.AppliedCommand();
//appliedCommand.HasOption
            options.CliBranch = appliedCommand["cli-branch"].Value<string>();
            options.CliVersionPrefix = appliedCommand["version"].Value<string>();
            options.DockerVersionedFolder = appliedCommand[nameof(DockerVersionedFolder)].Value<string>();
            options.GitHubEmail = appliedCommand[nameof(GitHubEmail)]?.Value<string>();
            options.GitHubPassword = appliedCommand[nameof(GitHubPassword)]?.Value<string>();
            options.GitHubUser = appliedCommand[nameof(GitHubUser)]?.Value<string>();

            return options;
        }
    }
}