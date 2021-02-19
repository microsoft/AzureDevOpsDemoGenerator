using System.Collections.Generic;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class WorkItemNames
    {
        public class Value
        {
            public string Name { get; set; }
            public string ReferenceName { get; set; }
            public string Description { get; set; }
        }

        public class Names
        {
            public int Count { get; set; }
            public IList<Value> Value { get; set; }
        }
    }
}
