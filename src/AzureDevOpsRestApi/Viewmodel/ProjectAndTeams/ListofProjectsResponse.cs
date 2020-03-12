using System.Collections.Generic;

namespace AzureDevOpsAPI.Viewmodel.ProjectAndTeams
{
    public class ProjectsResponse
    {
        public class ProjectResult : BaseViewModel
        {
            public int Count { get; set; }
            public List<Value> Value { get; set; }
            public string Errmsg { get; set; }
        }

        public class Value
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
            public string State { get; set; }
        }
    }
}
