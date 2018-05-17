using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.ReleaseDefinition
{
    public class ReleaseDefinitionsResponse
    {
        public class Release
        {
            public int count { get; set; }
            public Value[] value { get; set; }
        }
        public class Value
        {
            public int id { get; set; }
            public string name { get; set; }
        }
    }
}
