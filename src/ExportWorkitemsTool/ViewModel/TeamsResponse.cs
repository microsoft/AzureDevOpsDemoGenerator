using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatesGeneratorTool.ViewModel
{
    public class TeamsResponse
    {
        public class Value
        {
            public string name { get; set; }
            public string description { get; set; }
        }
        public class Team
        {
            public IList<Value> value { get; set; }
            public int count { get; set; }
        }
    }
}
