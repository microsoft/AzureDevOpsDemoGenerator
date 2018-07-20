using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.WorkItem
{
    public class CardStylesPatch
    {
        public class ListofCardStyles
        {
            [JsonProperty("rules")]
            public Rules rules { get; set; }
        }

        public class Rules : BaseViewModel
        {
            [JsonProperty("fill")]
            public Fill[] fill { get; set; }
            [JsonProperty("tagStyle")]
            public TagStyle[] tagstyle { get; set; }
        }

        public class Fill
        {
            public string name { get; set; }
            public string isEnabled { get; set; }
            public string filter { get; set; }

            public Dictionary<string, string> settings { get; set; }

        }
        public class TagStyle
        {
            public string name { get; set; }
            public string isEnabled { get; set; }


            public Dictionary<string, string> settings { get; set; }

        }
    }
}
