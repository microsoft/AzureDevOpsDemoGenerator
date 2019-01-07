using Newtonsoft.Json;
using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.Importer
{
    public class ImportBoardColumns
    {
        public class StateMappings
        {
            [JsonProperty("Epic")]
            public string Epic { get; set; }

            [JsonProperty("Feature")]
            public string Feature { get; set; }

            [JsonProperty("Product Backlog Item")]
            public string ProductBacklogItem { get; set; }

            [JsonProperty("Bug")]
            public string Bug { get; set; }
            [JsonProperty(PropertyName = "User Story")]
            public string UserStory { get; set; }
        }

        public class Value
        {
            public string name { get; set; }
            public int itemLimit { get; set; }
            public StateMappings stateMappings { get; set; }
            public string columnType { get; set; }
            public bool? isSplit { get; set; }
            public string description { get; set; }
        }

        public class ImportBoardCols
        {
            public string BoardName { get; set; }
            public IList<Value> value { get; set; }
        }
    }
}
