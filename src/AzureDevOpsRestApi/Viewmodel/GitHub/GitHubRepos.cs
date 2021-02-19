using System.Collections.Generic;

namespace AzureDevOpsAPI.Viewmodel.GitHub
{
    public class GitHubRepos
    {
        public class Repository
        {
            public string FullName { get; set; }
            public string EndPointName { get; set; }
            public string vcs { get; set; }
            public string vcs_url { get; set; }
        }

        public class Fork
        {
            public List<Repository> Repositories { get; set; }
        }
    }
}
