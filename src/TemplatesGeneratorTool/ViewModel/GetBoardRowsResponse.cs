using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatesGeneratorTool.ViewModel
{
    public class GetBoardRowsResponse
    {
        public class Result:BaseViewModel
        {
            public Value[] value { get; set; }
        }
        public class Value
        {
            public string id { get; set; }
            public string name { get; set; }
        }
    }
}
