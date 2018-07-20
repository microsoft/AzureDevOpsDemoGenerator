using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatesGeneratorTool.ViewModel
{
    public class BuildDefinitionResponse
    {
        public class Build
        {
            public int count { get; set; }
            public Value[] value { get; set; }
        }
        public class Value
        {
            public int id { get; set; }
        }
    }
}
