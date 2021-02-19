using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.WorkItem
{
    public class GetCardFieldResponse
    {
        public class ListofCards
        {
            [JsonProperty("cards")]
            public Cards Cards { get; set; }
        }
        public class Cards : BaseViewModel
        {
            [JsonProperty("Bug")]
            public Dictionary<string, string>[] Bugs { get; set; }
            [JsonProperty("Product Backlog Item")]
            public Dictionary<string, string>[] Pbis { get; set; }
        }
    }
    public class GetCardFieldResponseAgile
    {
        public class ListofCards
        {
            [JsonProperty("cards")]
            public Cards Cards { get; set; }
        }
        public class Cards : BaseViewModel
        {
            [JsonProperty("Bug")]
            public Dictionary<string, string>[] Bugs { get; set; }
            [JsonProperty("User Story")]
            public Dictionary<string, string>[] UserStory { get; set; }
        }
    }
}
