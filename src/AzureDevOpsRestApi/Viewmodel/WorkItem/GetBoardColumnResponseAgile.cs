using Newtonsoft.Json;

namespace AzureDevOpsAPI.Viewmodel.WorkItem
{
    public class GetBoardColumnResponseAgile
    {
        public class ColumnResponse : BaseViewModel
        {
            public int Count { get; set; }
            public Value[] Columns { get; set; }
            public Fields Fields { get; set; }
        }

        public class Value
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string ItemLimit { get; set; }
            public string IsSplit { get; set; }
            public string Description { get; set; }
            public string ColumnType { get; set; }
            public StateMappings StateMappings { get; set; }
        }

        public class Fields
        {
            public Field ColumnField { get; set; }
            public Field RowField { get; set; }
            public Field DoneField { get; set; }
        }

        public class Field
        {
            public string ReferenceName { get; set; }
            public string Url { get; set; }
        }

        public class StateMappings
        {
            [JsonProperty(PropertyName = "User Story")]
            public string UserStory { get; set; }
            [JsonProperty(PropertyName = "Bug")]
            public string Bug { get; set; }
            [JsonProperty(PropertyName = "Epic")]
            public string Epic { get; set; }
            [JsonProperty(PropertyName = "Feature")]
            public string Feature { get; set; }
        }
    }
}

