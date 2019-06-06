using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.GitHub
{
    public class ForkRepos
    {
        public class Repository
        {
            public string fullName { get; set; }
            public string endPointName { get; set; }
        }

        public class Fork
        {
            public List<Repository> repositories { get; set; }
        }
    }
}
