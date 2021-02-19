using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.WorkItem
{
    public class CardStylesPatch
    {
        public class ListofCardStyles
        {
            [JsonProperty("rules")]
            public Rules Rules { get; set; }
        }

        public class Rules : BaseViewModel
        {
            [JsonProperty("fill")]
            public Fill[] Fill { get; set; }
            [JsonProperty("tagStyle")]
            public TagStyle[] Tagstyle { get; set; }
        }

        public class Fill
        {
            public string Name { get; set; }
            public string IsEnabled { get; set; }
            public string Filter { get; set; }

            public Dictionary<string, string> Settings { get; set; }

        }
        public class TagStyle
        {
            public string Name { get; set; }
            public string IsEnabled { get; set; }


            public Dictionary<string, string> Settings { get; set; }

        }
    }
}
