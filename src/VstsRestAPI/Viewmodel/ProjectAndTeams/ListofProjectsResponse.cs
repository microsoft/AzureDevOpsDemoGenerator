using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.ProjectAndTeams
{
    public class ProjectsResponse
    {
        public class ProjectResult : BaseViewModel
        {
            public int count { get; set; }
            public List<Value> value { get; set; }
            public string errmsg { get; set; }
        }

        public class Value
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string state { get; set; }
        }
    }
}
