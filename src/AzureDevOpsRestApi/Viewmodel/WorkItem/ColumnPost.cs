using Newtonsoft.Json;

namespace AzureDevOpsAPI.Viewmodel.WorkItem
{
    public class Scrum
    {
        public class Columns
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string ItemLimit { get; set; }
            public string IsSplit { get; set; }
            public string Description { get; set; }
            public string ColumnType { get; set; }
            public StateMappings StateMappings { get; set; }
        }

        public class StateMappings
        {
            [JsonProperty(PropertyName = "Product Backlog Item")]
            public string Pbi { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string Bug { get; set; }
            [JsonProperty(PropertyName = "Epic")]
            public string Epic { get; set; }
            [JsonProperty(PropertyName = "Feature")]
            public string Feature { get; set; }
            [JsonProperty(PropertyName = "User Story")]
            public string UserStory { get; set; }
            [JsonProperty(PropertyName = "Issue")]
            public string Issue { get; set; }
        }
    }
    public class Agile
    {
        public class StateMappings
        {
            [JsonProperty(PropertyName = "User Story")]
            public string UserStory { get; set; }
            [JsonProperty(PropertyName ="Bug")] 
            public string Bug { get; set; }
        }

        public class Columns
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int ItemLimit { get; set; }
            public StateMappings StateMappings { get; set; }
            public string ColumnType { get; set; }
            public bool? IsSplit { get; set; }
            public string Description { get; set; }
        }


    }
}
