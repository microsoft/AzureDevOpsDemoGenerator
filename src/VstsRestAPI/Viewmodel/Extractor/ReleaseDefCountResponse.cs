using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class ReleaseDefCountResponse
    {
        public class ReleaseDefinition
        {
            public int id { get; set; }
        }

        public class Value
        {
            public string source { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public object variableGroups { get; set; }
        }

        public class Release
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }
    }
}
