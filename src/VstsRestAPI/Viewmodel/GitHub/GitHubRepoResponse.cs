using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.GitHub
{
    public class GitHubRepoResponse
    {
        public class Owner
        {
            public string login { get; set; }
            public int id { get; set; }
            public string avatar_url { get; set; }
            public string gravatar_id { get; set; }
            public string url { get; set; }
            public string html_url { get; set; }
            public string followers_url { get; set; }
            public string following_url { get; set; }
            public string gists_url { get; set; }
            public string starred_url { get; set; }
            public string subscriptions_url { get; set; }
            public string organizations_url { get; set; }
            public string repos_url { get; set; }
            public string events_url { get; set; }
            public string received_events_url { get; set; }
            public string type { get; set; }
            public bool site_admin { get; set; }


        }

        public class RepoCreated
        {
            public int id { get; set; }
            public string name { get; set; }
            public string full_name { get; set; }
            public Owner owner { get; set; }
            public string html_url { get; set; }
        }
    }

}
