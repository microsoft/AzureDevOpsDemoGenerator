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

    public class IterationList
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


    public class Iterations
    {
        public int count { get; set; }
        public List<Iteration> Iteration { get; set; }

    }

    public class Iteration{
        public string id { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public attributes attributes { get; set; }
    }

    public class attributes
    {
        public string startdate { get; set; }
        public string finishdate { get; set; }
        public string timeFrame { get; set; }
    }

}
