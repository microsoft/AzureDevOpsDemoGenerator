using Newtonsoft.Json;
using System.Collections.Generic;

namespace AzureDevOpsAPI.Viewmodel.Importer
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
            [JsonProperty(PropertyName = "Issue")]
            public string Issue { get; set; }
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

        public class ImportBoardCols
        {
            public string BoardName { get; set; }
            public IList<Value> Value { get; set; }
        }
    }
}
