using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class GetVariableGroups
    {
        public class Value
        {
            public JObject Variables { get; set; }
            public string Id { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public bool IsShared { get; set; }
        }

        public class Groups
        {
            public int Count { get; set; }
            public List<Value> Value { get; set; }
        }

        public class VariableGroupsCreateResponse
        {
            public JObject Variables { get; set; }
            public int Id { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public bool IsShared { get; set; }
        }
    }
}
