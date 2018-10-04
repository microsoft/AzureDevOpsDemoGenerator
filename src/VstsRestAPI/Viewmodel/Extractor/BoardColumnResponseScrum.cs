using Newtonsoft.Json;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class BoardColumnResponseScrum
    {
        public Column[] value { get; set; }

        public class StateMappings
        {
            [JsonProperty(PropertyName = "Product Backlog Item")]
            public string ProductBacklogItem { get; set; }
            public string Bug { get; set; }
        }

        public class Column
        {
            public string name { get; set; }
            public int itemLimit { get; set; }
            public StateMappings stateMappings { get; set; }
            public string columnType { get; set; }
            public bool? isSplit { get; set; }
            public string description = "";
        }
    }
}
