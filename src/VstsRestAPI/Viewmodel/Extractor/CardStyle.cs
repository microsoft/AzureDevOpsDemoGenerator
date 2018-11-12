using Newtonsoft.Json;
using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class CardStyle
    {
        public class Claus
        {
            public string fieldName { get; set; }
            public int index { get; set; }
            public string logicalOperator { get; set; }
            [JsonProperty(PropertyName = "operator")]
            public string Operator { get; set; }
            public string value { get; set; }
        }

        public class Settings
        {
            [JsonProperty(PropertyName = "title-color")]
            public string titlecolor { get; set; }
            [JsonProperty(PropertyName = "background-color")]
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
            public Rules rules { get; set; }
        }
    }
}
