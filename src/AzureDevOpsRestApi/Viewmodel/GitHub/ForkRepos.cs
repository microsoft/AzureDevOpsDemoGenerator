using System.Collections.Generic;

namespace AzureDevOpsAPI.Viewmodel.GitHub
{
    public class ForkRepos
    {
        public class Repository
        {
            public string FullName { get; set; }
            public string EndPointName { get; set; }
        }

        public class Fork
        {
            public List<Repository> Repositories { get; set; }
        }
    }
}
