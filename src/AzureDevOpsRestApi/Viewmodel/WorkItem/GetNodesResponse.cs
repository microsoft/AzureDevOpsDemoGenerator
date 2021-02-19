
using System;

namespace AzureDevOpsAPI.Viewmodel.WorkItem
{
    public class GetNodesResponse
    {
        public class Nodes : BaseViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string StructureType { get; set; }
            public bool HasChildren { get; set; }
            public Child[] Children { get; set; }
            public Links Links { get; set; }
            public string Url { get; set; }
        }

        public class Links
        {
            public Self Self { get; set; }
        }

        public class Self
        {
            public string Href { get; set; }
        }

        public class Child
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string StructureType { get; set; }
            public bool HasChildren { get; set; }
            public string Url { get; set; }
            public Child[] Children { get; set; }
        }

        public class Child1
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string StructureType { get; set; }
            public bool HasChildren { get; set; }
            public string Url { get; set; }
            public Attributes Attributes { get; set; }
        }

        public class Attributes
        {
            public DateTime? StartDate { get; set; }
            public DateTime? FinishDate { get; set; }
        }
    }
}