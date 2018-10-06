using Newtonsoft.Json;
using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.WorkItem
{
    public class Scrum
    {
        public class Columns
        {
            public string id { get; set; }
            public string name { get; set; }
            public string itemLimit { get; set; }
            public string isSplit { get; set; }
            public string description { get; set; }
            public string columnType { get; set; }
            public StateMappings stateMappings { get; set; }
        }

        public class StateMappings
        {
            [JsonProperty(PropertyName = "Product Backlog Item")]
            public string PBI { get; set; }

            [JsonProperty(PropertyName = "Bug")]
            public string bug { get; set; }
        }
    }
    public class Agile
    {
        public class StateMappings
        {
            [JsonProperty(PropertyName = "User Stories")]
            public string UserStories { get; set; }
            [JsonProperty(PropertyName = "Bug")]
            public string Bug { get; set; }
        }

        public class Columns
        {
            public string id { get; set; }

            public string name { get; set; }
            public int itemLimit { get; set; }
            public StateMappings stateMappings { get; set; }
            public string columnType { get; set; }
            public bool? isSplit { get; set; }
            public string description { get; set; }
        }
    }
}
