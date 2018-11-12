using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.WorkItem
{
    public class GetCardFieldResponse
    {
        public class ListofCards
        {
            [JsonProperty("cards")]
            public Cards cards { get; set; }
        }
        public class Cards : BaseViewModel
        {
            [JsonProperty("Bug")]
            public Dictionary<string, string>[] bugs { get; set; }
            [JsonProperty("Product Backlog Item")]
            public Dictionary<string, string>[] pbis { get; set; }
        }
    }
    public class GetCardFieldResponseAgile
    {
        public class ListofCards
        {
            [JsonProperty("cards")]
            public Cards cards { get; set; }
        }
        public class Cards : BaseViewModel
        {
            [JsonProperty("Bug")]
            public Dictionary<string, string>[] bugs { get; set; }
            [JsonProperty("User Story")]
            public Dictionary<string, string>[] UserStory { get; set; }
        }
    }
}
