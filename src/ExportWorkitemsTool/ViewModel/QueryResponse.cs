using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatesGeneratorTool.ViewModel
{
    public class QueryResponse
    {

        public class Wiql
        {
            public string href { get; set; }
        }
        public class Links
        {
            public Wiql wiql { get; set; }
        }
        public class Child
        {
            public string id { get; set; }
            public string name { get; set; }
            public string path { get; set; }
            public bool isPublic { get; set; }
            public Links _links { get; set; }
            public string url { get; set; }
            public bool isFolder = false;
            public bool hasChildren = false;
            public IList<Child> children { get; set; }
            public string wiql = "";
        }
        public class Value
        {
            public string id { get; set; }
            public string name { get; set; }
            public string path { get; set; }
            public bool isFolder { get; set; }
            public bool hasChildren { get; set; }
            public IList<Child> children { get; set; }
            public bool isPublic { get; set; }
            public Links _links { get; set; }
            public string url { get; set; }
            public string wiql = "";
        }

        public class Query
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }
    }
}