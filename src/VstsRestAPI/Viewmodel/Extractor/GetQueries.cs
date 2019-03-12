using System;
using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class GetQueries
    {
        public class Child1
        {
            public string id { get; set; }
            public string name { get; set; }
            public string path { get; set; }
            public string wiql { get; set; }
            public bool isPublic { get; set; }
        }

        public class Child
        {
            public string id { get; set; }
            public string name { get; set; }
            public string path { get; set; }
            public string wiql { get; set; }
            public string queryType { get; set; }
            public bool hasChildren { get; set; }
            public List<Child1> children { get; set; }
        }

        public class Value
        {
            public string id { get; set; }
            public string name { get; set; }
            public string path { get; set; }
            public bool isFolder { get; set; }
            public bool hasChildren { get; set; }
            public List<Child> children { get; set; }
            public bool isPublic { get; set; }
        }

        public class Queries
        {
            public int count { get; set; }
            public List<Value> value { get; set; }
        }
    }
}
