using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class GetBuildDefResponse
    {
        public class Project
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string state { get; set; }
            public int revision { get; set; }
            public string visibility { get; set; }
        }

        public class Value
        {
            public Project project { get; set; }
        }

        public class BuildDef
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }
    }
}
