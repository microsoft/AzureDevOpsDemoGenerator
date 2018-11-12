using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class GetBuildDefResponse
    {
        public class Self
        {
            public string href { get; set; }
        }
        public class Web
        {
            public string href { get; set; }
        }

        public class Editor
        {
            public string href { get; set; }
        }

        public class Badge
        {
            public string href { get; set; }
        }

        public class Links
        {
            public Self self { get; set; }
            public Web web { get; set; }
            public Editor editor { get; set; }
            public Badge badge { get; set; }
        }

        public class AuthoredBy
        {
            public string displayName { get; set; }
            public string url { get; set; }
            public string id { get; set; }
            public string uniqueName { get; set; }
            public string imageUrl { get; set; }
            public string descriptor { get; set; }
        }

        public class Pool
        {
            public int id { get; set; }
            public string name { get; set; }
            public bool isHosted { get; set; }
        }

        public class Queue
        {
            public int id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public Pool pool { get; set; }
        }

        public class Project
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string state { get; set; }
            public int revision { get; set; }
            public string visibility { get; set; }
        }

        public class Value
        {
            public string quality { get; set; }
            public Queue queue { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public string uri { get; set; }
            public string path { get; set; }
            public string type { get; set; }
            public string queueStatus { get; set; }
            public int revision { get; set; }
            public DateTime createdDate { get; set; }
            public Project project { get; set; }
        }

        public class BuildDef
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }
    }
}
