using Newtonsoft.Json;

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

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string Bug { get; set; }
            [JsonProperty(PropertyName = "Epic")]
            public string epic { get; set; }
            [JsonProperty(PropertyName = "Feature")]
            public string feature { get; set; }
            [JsonProperty(PropertyName = "User Story")]
            public string UserStory { get; set; }
        }
    }
    public class Agile
    {
        public class StateMappings
        {
            [JsonProperty(PropertyName = "User Story")]
            public string UserStory { get; set; }
            [JsonProperty(PropertyName ="Bug")] 
            public string bug { get; set; }
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
