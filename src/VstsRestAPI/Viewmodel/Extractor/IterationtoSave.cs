using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class IterationtoSave
    {
        public class Nodes
        {
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public Child[] children { get; set; }
        }

        public class Child
        {
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public Child1[] children { get; set; }
        }

        public class Child1
        {
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public Child2[] children { get; set; }

        }
        public class Child2
        {
            public string name { get; set; }
            public bool hasChildren { get; set; }
            public Child3[] children { get; set; }

        }
        public class Child3
        {
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
        }
    }

    public class ItearationList
    {
        public class Child
        {
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
        }

        public class Iterations
        {
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public List<Child> children = new List<Child>();
        }

    }

}
