using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.Wiki
{
    public class ProjectwikiResponse
    {
        public class Version
        {
            public string VersionValue { get; set; }
        }

        public class Projectwiki
        {
            public string Id { get; set; }
            public IList<Version> Versions { get; set; }
            public string Url { get; set; }
            public string RemoteUrl { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public string ProjectId { get; set; }
            public string RepositoryId { get; set; }
            public string MappedPath { get; set; }
        }
    }
}
