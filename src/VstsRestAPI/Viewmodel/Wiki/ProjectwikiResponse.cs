using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.Wiki
{
    public class ProjectwikiResponse
    {
        public class Version
        {
            public string version { get; set; }
        }

        public class Projectwiki
        {
            public string id { get; set; }
            public IList<Version> versions { get; set; }
            public string url { get; set; }
            public string remoteUrl { get; set; }
            public string type { get; set; }
            public string name { get; set; }
            public string projectId { get; set; }
            public string repositoryId { get; set; }
            public string mappedPath { get; set; }
        }
    }
}
