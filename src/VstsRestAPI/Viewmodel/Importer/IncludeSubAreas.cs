using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.Importer
{
    public class IncludeSubAreas
    {
        public class Root
        {
            public string defaultValue { get; set; }
            public List<Value> values { get; set; }
        }

        public class Value
        {
            public string value { get; set; }
            public bool includeChildren { get; set; }
        }
    }


}
