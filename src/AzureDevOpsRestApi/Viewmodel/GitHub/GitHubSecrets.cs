using System;
using System.Collections.Generic;
using System.Text;

namespace AzureDevOpsRestApi.Viewmodel.GitHub
{
    public class GitHubSecrets
    {
        public class Secrets
        {
            public string org { get; set; }
            public string secretName { get; set; }
            public string secretValue { get; set; }
            public string visibility { get; set; }
            public int selected_repository_ids { get; set; }
        }

        public class GitHubSecret
        {
            public List<Secrets> secrets { get; set; }
        }

    }
}
