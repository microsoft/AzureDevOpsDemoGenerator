using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class GetVariableGroups
    {
        public class Value
        {
            public JObject variables { get; set; }
            public string id { get; set; }
            public string type { get; set; }
            public string name { get; set; }
            public bool isShared { get; set; }
        }

        public class Groups
        {
            public int count { get; set; }
            public List<Value> value { get; set; }
        }

        public class VariableGroupsCreateResponse
        {
            public JObject variables { get; set; }
            public int id { get; set; }
            public string type { get; set; }
            public string name { get; set; }
            public bool isShared { get; set; }
        }
    }
}
