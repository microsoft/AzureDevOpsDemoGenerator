using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatesGeneratorTool.ViewModel
{
    public class QueryByPathResponse
    {

        public class Child
        {
            public string id { get; set; }
            public string name { get; set; }
            public string path { get; set; }
            public IList<Child> children { get; set; }
            public bool isPublic { get; set; }
            public bool isFolder = false;
            public string wiql = "";
        }
        public class query
        {
            public string id { get; set; }
            public string name { get; set; }
            public string path { get; set; }
            public bool isFolder { get; set; }
            public bool hasChildren { get; set; }
            public IList<Child> children { get; set; }
            public bool isPublic { get; set; }
            public string wiql = "";
        }

        public class QueryWithWiql
        {
            public string name { get; set; }
            public string wiql { get; set; }
        }

    }
}
