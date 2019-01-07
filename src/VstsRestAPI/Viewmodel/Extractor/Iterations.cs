using Newtonsoft.Json;
using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class ExportIterations
    {
        public class Value
        {
            public string name { get; set; }
        }

        public class Iterations
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }
    }

    public class ExportedIterations
    {
        public class Child
        {
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
        }

        public class Iterations
        {
            public List<Child> children { get; set; }
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
        }


    }
}
