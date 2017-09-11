// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using Xunit.Abstractions;

namespace Microsoft.DotNet.Docker.Tests
{
    public class DockerHelper
    {
        private string DockerOS => GetDockerOS();
        public string ContainerWorkDir => IsLinuxContainerModeEnabled ? "/sandbox" : "c:\\sandbox";
        public bool IsLinuxContainerModeEnabled => string.Equals(DockerOS, "linux", StringComparison.OrdinalIgnoreCase);
        private ITestOutputHelper Output { get; set; }

        public DockerHelper(ITestOutputHelper output)
        {
            Output = output;
        }

        public void Build(string dockerfilePath, string fromImage, string tag, string buildArgs)
        {
            string dockerfileContents = File.ReadAllText(dockerfilePath);
            dockerfileContents = dockerfileContents.Replace("$base_image", fromImage);
            string tempDockerfilePath = dockerfilePath + ".temp";
            File.WriteAllText(tempDockerfilePath, dockerfileContents);

            try
            {
                Execute($"build -t {tag} {buildArgs} -f {dockerfilePath} .");
            }
            finally
            {
                File.Delete(tempDockerfilePath);
            }
        }

        public void DeleteImage(string name)
        {
            if (ResourceExists("image", name))
            {
                Execute($"image rm -f {name}");
            }
        }

        public void DeleteVolume(string name)
        {
            if (ResourceExists("volume", name))
            {
                Execute($"volume rm -f {name}");
            }
        }

        private void Execute(string args)
        {
            Output.WriteLine($"Executing : docker {args}");
            ProcessStartInfo info = new ProcessStartInfo("docker", args);
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            Process process = Process.Start(info);
            process.WaitForExit();
            Output.WriteLine(process.StandardOutput.ReadToEnd());

            if (process.ExitCode != 0)
            {
                string stdErr = process.StandardError.ReadToEnd();
                string msg = $"Failed to execute {info.FileName} {info.Arguments}{Environment.NewLine}{stdErr}";
                throw new InvalidOperationException(msg);
            }
        }

        private static string GetDockerOS()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("docker", "version -f \"{{ .Server.Os }}\"");
            startInfo.RedirectStandardOutput = true;
            Process process = Process.Start(startInfo);
            process.WaitForExit();
            return process.StandardOutput.ReadToEnd().Trim();
        }

        public string GetContainerWorkPath(string relativePath)
        {
            string separator = IsLinuxContainerModeEnabled ? "/" : "\\";
            return $"{ContainerWorkDir}{separator}{relativePath}";
        }

        private static bool ResourceExists(string type, string id)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("docker", $"{type} ls -q {id}");
            startInfo.RedirectStandardOutput = true;
            Process process = Process.Start(startInfo);
            process.WaitForExit();
            return process.ExitCode == 0 && process.StandardOutput.ReadToEnd().Trim() != "";
        }

        public void Run(string image, string command, string containerName, string volumeName = null)
        {
            string volumeArg = volumeName == null ? string.Empty : $" -v {volumeName}:{ContainerWorkDir}";
            Execute($"run --rm --name {containerName}{volumeArg} {image} {command}");
        }
    }
}
