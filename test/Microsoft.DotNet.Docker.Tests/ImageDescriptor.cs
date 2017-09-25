// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.DotNet.Docker.Tests
{
    public class ImageDescriptor
    {
        private string runtimeDepsVersion;
        private string sdkVersion;

        public string Architecture { get; set; } = "amd64";
        public string DotNetCoreVersion { get; set; }
        public string OsVariant { get; set; }

        public string RuntimeDepsVersion
        {
            get { return runtimeDepsVersion ?? DotNetCoreVersion; }
            set { runtimeDepsVersion = value; }
        }

        public string SdkVersion
        {
            get { return sdkVersion ?? DotNetCoreVersion; }
            set { sdkVersion = value; }
        }
    }
}
