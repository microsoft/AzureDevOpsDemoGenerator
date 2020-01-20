using Newtonsoft.Json;
using System.Collections.Generic;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class ExportIterations
    {
        public class Value
        {
            public string Name { get; set; }
        }

        public class Iterations
        {
            public int Count { get; set; }
            public IList<Value> Value { get; set; }
        }
    }

    public class ExportedIterations
    {
        public class Child
        {
            public string Name { get; set; }
            public string StructureType { get; set; }
            public bool HasChildren { get; set; }
        }

        public class Iterations
        {
            public List<Child> Children { get; set; }
            public string Name { get; set; }
            public string StructureType { get; set; }
            public bool HasChildren { get; set; }
        }


    }
}
