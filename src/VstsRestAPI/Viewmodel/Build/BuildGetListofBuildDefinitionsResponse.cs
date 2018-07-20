using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.Build
{
    public class BuildGetListofBuildDefinitionsResponse
    {

        public class Definitions : BaseViewModel
        {
            public int count { get; set; }
            public Value[] value { get; set; }
        }

        public class Value
        {
            public string quality { get; set; }
            public Authoredby authoredBy { get; set; }
            public Queue queue { get; set; }
            public string uri { get; set; }
            public string type { get; set; }
            public int revision { get; set; }
            public DateTime createdDate { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public Project project { get; set; }
        }

        public class Authoredby
        {
            public string id { get; set; }
            public string displayName { get; set; }
            public string uniqueName { get; set; }
            public string url { get; set; }
            public string imageUrl { get; set; }
        }

        public class Queue
        {
            public Pool pool { get; set; }
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Pool
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Project
        {
            public string id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public string state { get; set; }
            public int revision { get; set; }
        }
    }
}