using System;

namespace AzureDevOpsAPI.Viewmodel.WorkItem
{
    public class GetNodeResponse
    {
        public class Node : BaseViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string StructureType { get; set; }
            public bool HasChildren { get; set; }
            public Attributes Attributes { get; set; }
            public Links Links { get; set; }
            public string Url { get; set; }
        }

        public class Attributes
        {
            public DateTime StartDate { get; set; }
            public DateTime FinishDate { get; set; }
        }

        public class Links
        {
            public Self Self { get; set; }
            public Parent Parent { get; set; }
        }

        public class Self
        {
            public string Href { get; set; }
        }

        public class Parent
        {
            public string Href { get; set; }
        }
    }
}
