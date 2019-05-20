using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.GitHub
{
    public class ForkRepos
    {
        public class Repository
        {
            public string fullName { get; set; }
        }

        public class Fork
        {
            public IList<Repository> repositories { get; set; }
        }
    }
}
