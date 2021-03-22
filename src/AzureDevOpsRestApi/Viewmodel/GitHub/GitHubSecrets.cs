using System;
using System.Collections.Generic;
using System.Text;

namespace AzureDevOpsRestApi.Viewmodel.GitHub
{
    public class GitHubSecrets
    {
        public class Secrets
        {
            public string secretName { get; set; }
            public string secretValue { get; set; }
        }

        public class GitHubSecret
        {
            public List<Secrets> secrets { get; set; }
        }

    }
}
