// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Microsoft.DotNet.Cli.CommandLine;

namespace Dotnet.Docker.Nightly
{
    public class Options
    {
        private Option CliBranchOption { get; set; }
        private Option CliVersionPrefixOption { get; set; }
        private Option DockerVersionFolderOption { get; set; }
        private Option GitHubEmailOption { get; set; }
        private Option GitHubPasswordOption { get; set; }
        private Option GitHubUserOption { get; set; }

        public string CliBranch { get; private set; }
        public string CliVersionPrefix { get; private set; }
        public string CliVersionsUrl =>
            $"https://raw.githubusercontent.com/dotnet/versions/master/build-info/dotnet/cli/{CliBranch}";
        public string DockerVersionFolder { get; private set; }
        public string GitHubEmail { get; private set; }
        public string GitHubPassword { get; private set; }
        public string GitHubProject => "dotnet-docker-nightly";
        public string GitHubUpstreamBranch => "master";
        public string GitHubUpstreamOwner => "dotnet";
        public string GitHubUser { get; private set; }
        public bool UpdateOnly => GitHubEmail == null || GitHubPassword == null || GitHubUser == null;

        public Options()
        {
            CliBranchOption = Create.Option(
                "--cli-branch",
                "The CLI branch to retrieve the SDK to update the Dockerfiles with (Default is master).",
                Accept.ExactlyOneArgument()
                    .With(defaultValue: () => "master"));
            CliVersionPrefixOption = Create.Option(
                "--cli-prefix",
                "The CLI version prefix associated with the CliBranch.",
                Accept.ExactlyOneArgument());
            DockerVersionFolderOption = Create.Option(
                "--version",
                "The version folder of this repo to update (e.g. 2.1).",
                Accept.ExactlyOneArgument());
            GitHubEmailOption = Create.Option(
                "--email",
                "GitHub email to use while making the PR.  If not specified, a PR is not made.",
                Accept.ExactlyOneArgument());
            GitHubPasswordOption = Create.Option(
                "--password",
                "GitHub password to use while making the PR.  If not specified, a PR is not made.",
                Accept.ExactlyOneArgument());
            GitHubUserOption = Create.Option(
                "--user",
                "GitHub user to use while making the PR.  If not specified, a PR is not made.",
                Accept.ExactlyOneArgument());
        }

        public bool Parse(string[] args)
        {
            bool result;

            Command command = Create.Command(
                "update-dependencies",
                "Updates the .NET Core components of the Dockerfiles to the latest versions.",
                DockerVersionFolderOption,
                CliBranchOption,
                CliVersionPrefixOption,
                GitHubUserOption,
                GitHubEmailOption,
                GitHubPasswordOption);
            ParseResult parseResult = command.Parse(args);

            if (parseResult.Errors.Any())
            {
                string msg = string.Join(Environment.NewLine, parseResult.Errors);
                Console.WriteLine(msg);
                Console.WriteLine(command.HelpView());
                result = false;
            }
            else
            {
                AppliedOption appliedCommand = parseResult.AppliedCommand();

                //appliedCommand.HasOption
                CliBranch = GetOption(CliBranchOption, appliedCommand, true);
                CliVersionPrefix = GetOption(CliVersionPrefixOption, appliedCommand, true);
                DockerVersionFolder = GetOption(DockerVersionFolderOption, appliedCommand, true);
                GitHubEmail = GetOption(GitHubEmailOption, appliedCommand);
                GitHubPassword = GetOption(GitHubPasswordOption, appliedCommand);
                GitHubUser = GetOption(GitHubUserOption, appliedCommand);
                result = true;
            }

            return result;
        }

        private string GetOption(Option option, AppliedOption appliedCommand, bool isRequired = false)
        {
            if (!appliedCommand.HasOption(option.Name) && isRequired)
            {
                Console.WriteLine($"Required option `{option.Name}` was not specified");
                throw new Exception();
            }
            else
            {
                return appliedCommand[option.Name].Value<string>();
            }
        }
    }
}