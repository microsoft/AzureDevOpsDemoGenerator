using System;
using System.Collections.Generic;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class GetQueries
    {
        public class Child1
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Path { get; set; }
            public string Wiql { get; set; }
            public bool IsPublic { get; set; }
        }

        public class Child
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Path { get; set; }
            public string Wiql { get; set; }
            public string QueryType { get; set; }
            public bool HasChildren { get; set; }
            public List<Child1> Children { get; set; }
        }

        public class Value
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Path { get; set; }
            public bool IsFolder { get; set; }
            public bool HasChildren { get; set; }
            public List<Child> Children { get; set; }
            public bool IsPublic { get; set; }
        }

        public class Queries
        {
            public int Count { get; set; }
            public List<Value> Value { get; set; }
        }
    }
}
