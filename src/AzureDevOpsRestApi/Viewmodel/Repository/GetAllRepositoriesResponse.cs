using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.Repository
{
    public class GetAllRepositoriesResponse
    {
        public class Repositories : BaseViewModel
        {
            public List<Value> Value { get; set; }
            public int Count { get; set; }
        }

        public class Project
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
            public string State { get; set; }
            public int Revision { get; set; }
        }

        public class Value
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
            public Project Project { get; set; }
            public string DefaultBranch { get; set; }
            public string RemoteUrl { get; set; }
        }
    }

}