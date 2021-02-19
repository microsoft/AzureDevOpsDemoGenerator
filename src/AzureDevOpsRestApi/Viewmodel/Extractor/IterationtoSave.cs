using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class IterationtoSave
    {
        public class Nodes
        {
            public string Name { get; set; }
            public string StructureType { get; set; }
            public bool HasChildren { get; set; }
            public Child[] Children { get; set; }
        }

        public class Child
        {
            public string Name { get; set; }
            public string StructureType { get; set; }
            public bool HasChildren { get; set; }
            public Child1[] Children { get; set; }
        }

        public class Child1
        {
            public string Name { get; set; }
            public string StructureType { get; set; }
            public bool HasChildren { get; set; }
            public Child2[] Children { get; set; }

        }
        public class Child2
        {
            public string Name { get; set; }
            public bool HasChildren { get; set; }
            public Child3[] Children { get; set; }

        }
        public class Child3
        {
            public string Name { get; set; }
            public string StructureType { get; set; }
            public bool HasChildren { get; set; }
        }
    }

    public class ItearationList
    {
        public class Child
        {
            public string Name { get; set; }
            public string StructureType { get; set; }
            public bool HasChildren { get; set; }
        }

        public class Iterations
        {
            public string Name { get; set; }
            public string StructureType { get; set; }
            public bool HasChildren { get; set; }
            public List<Child> Children = new List<Child>();
        }

    }

}
