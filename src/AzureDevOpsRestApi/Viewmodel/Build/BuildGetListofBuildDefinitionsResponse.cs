using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureDevOpsAPI.Viewmodel;

namespace AzureDevOpsRestApi.Viewmodel.Build
{
    public class BuildGetListofBuildDefinitionsResponse
    {

        public class Definitions : BaseViewModel
        {
            public int Count { get; set; }
            public Value[] Value { get; set; }
        }

        public class Value
        {
            public string Quality { get; set; }
            public Authoredby AuthoredBy { get; set; }
            public Queue Queue { get; set; }
            public string Uri { get; set; }
            public string Type { get; set; }
            public int Revision { get; set; }
            public DateTime CreatedDate { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
            public Project Project { get; set; }
        }

        public class Authoredby
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string UniqueName { get; set; }
            public string Url { get; set; }
            public string ImageUrl { get; set; }
        }

        public class Queue
        {
            public Pool Pool { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Pool
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Project
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
            public string State { get; set; }
            public int Revision { get; set; }
        }
    }
}