using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.Repository
{
    public class GetAllRepositoriesResponse
    {
        public class Repositories : BaseViewModel
        {
            public List<Value> value { get; set; }
            public int count { get; set; }
        }

        public class Project
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string state { get; set; }
            public int revision { get; set; }
        }

        public class Value
        {
            public string id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public Project project { get; set; }
            public string defaultBranch { get; set; }
            public string remoteUrl { get; set; }
        }
    }

}