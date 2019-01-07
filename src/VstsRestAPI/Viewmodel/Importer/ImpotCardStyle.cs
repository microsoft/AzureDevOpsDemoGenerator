using Newtonsoft.Json;
using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.Importer
{
    public class CardStyle
    {
        public class Claus
        {
            public string fieldName { get; set; }
            public int index { get; set; }
            public string logicalOperator { get; set; }
            [JsonProperty("operator")]
            public string Operator { get; set; }
            public string value { get; set; }
        }

        public class Settings
        {
            [JsonProperty("title-color")]
            public string titlecolor { get; set; }

            [JsonProperty("background-color")]
            public string backgroundcolor { get; set; }
        }

        public class Fill
        {
            public string name { get; set; }
            public string isEnabled { get; set; }
            public string filter { get; set; }
            public IList<Claus> clauses { get; set; }
            public Settings settings { get; set; }
        }

        public class TagStyle
        {
            public string name { get; set; }
            public string isEnabled { get; set; }
            public Settings settings { get; set; }
        }

        public class Rules
        {
            public IList<Fill> fill { get; set; }
            public IList<TagStyle> tagStyle { get; set; }
        }

        public class Style
        {
            public string url { get; set; }
            public Rules rules { get; set; }
            public string _links { get; set; }
            public string BoardName { get; set; }
        }
    }
}
