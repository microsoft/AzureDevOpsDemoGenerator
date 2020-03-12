using Newtonsoft.Json;
using System.Collections.Generic;

namespace AzureDevOpsAPI.Viewmodel.Importer
{
    public class CardStyle
    {
        public class Claus
        {
            public string FieldName { get; set; }
            public int Index { get; set; }
            public string LogicalOperator { get; set; }
            [JsonProperty("operator")]
            public string Operator { get; set; }
            public string Value { get; set; }
        }

        public class Settings
        {
            [JsonProperty("title-color")]
            public string Titlecolor { get; set; }

            [JsonProperty("background-color")]
            public string Backgroundcolor { get; set; }
        }

        public class Fill
        {
            public string Name { get; set; }
            public string IsEnabled { get; set; }
            public string Filter { get; set; }
            public IList<Claus> Clauses { get; set; }
            public Settings Settings { get; set; }
        }

        public class TagStyle
        {
            public string Name { get; set; }
            public string IsEnabled { get; set; }
            public Settings Settings { get; set; }
        }

        public class Rules
        {
            public IList<Fill> Fill { get; set; }
            public IList<TagStyle> TagStyle { get; set; }
        }

        public class Style
        {
            public string Url { get; set; }
            public Rules Rules { get; set; }
            public string Links { get; set; }
            public string BoardName { get; set; }
        }
    }
}
