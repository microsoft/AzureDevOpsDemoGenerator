using Newtonsoft.Json;
using System.Collections.Generic;
namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class BoardColumnResponseAgile
    {
        public class StateMappings
        {
            [JsonProperty(PropertyName = "User Story")]
            public string UserStory { get; set; }
            [JsonProperty(PropertyName = "Bug")]
            public string Bug { get; set; }
            [JsonProperty(PropertyName = "Feature")]
            public string Feature { get; set; }
            [JsonProperty(PropertyName = "Epic")]
            public string Epic { get; set; }
        }

        public class Value
        {
            public string Name { get; set; }
            public int ItemLimit { get; set; }
            public StateMappings StateMappings { get; set; }
            public string ColumnType { get; set; }
            public bool? IsSplit { get; set; }
            public string Description { get; set; }
        }

        public class ColumnResponse
        {
            public string BoardName { get; set; }
            public List<Value> Value { get; set; }
        }
    }
}
