using System;
using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class ExportIterations
    {
        public class Value
        {
            public string id { get; set; }
            public string name { get; set; }
            public string path { get; set; }
        }

        public class Iterations
        {
            public IList<Value> value { get; set; }
        }
    }
}
