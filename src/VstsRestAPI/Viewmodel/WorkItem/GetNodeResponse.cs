using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.WorkItem
{
    public class GetNodeResponse
    {
        public class Node : BaseViewModel
        {
            public int id { get; set; }
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public Attributes attributes { get; set; }
            public _Links _links { get; set; }
            public string url { get; set; }
        }

        public class Attributes
        {
            public DateTime startDate { get; set; }
            public DateTime finishDate { get; set; }
        }

        public class _Links
        {
            public Self self { get; set; }
            public Parent parent { get; set; }
        }

        public class Self
        {
            public string href { get; set; }
        }

        public class Parent
        {
            public string href { get; set; }
        }
    }
}
