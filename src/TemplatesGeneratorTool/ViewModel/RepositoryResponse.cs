using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatesGeneratorTool.ViewModel
{
    public class RepositoryResponse
    {
        public class Value
        {
            public string id { get; set; }

            public string name { get; set; }
        }

        public class Repository
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }
    }
}
