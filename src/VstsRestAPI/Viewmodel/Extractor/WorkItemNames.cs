using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class WorkItemNames
    {
        public class Value
        {
            public string name { get; set; }
            public string referenceName { get; set; }
            public string description { get; set; }
        }

        public class Names
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }
    }
}
