using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatesGeneratorTool.ViewModel
{
    public class CardStyleResponse
    {
        public class Settings
        {
            [JsonProperty(PropertyName = "background-color")]
            public string backgroundcolor { get; set; }
            [JsonProperty(PropertyName = "title-color")]
            public string titlecolor = "";
        }

        public class Fill
        {
            public string name { get; set; }
            public string isEnabled { get; set; }
            public string filter { get; set; }
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

        public class CardStyles
        {
            public Rules rules { get; set; }
        }
    }
}
