using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.WorkItem
{
    public class SetEpicSettings
    {
        public class Epiclist 
        {
            [JsonProperty("backlogVisibilities")]
            public BacklogVisibilities BacklogVisibilities { get; set; }
        }


        public class BacklogVisibilities : BaseViewModel
        {
            [JsonProperty("Microsoft.EpicCategory")]
            public string Epi { get; set; }
            [JsonProperty("Microsoft.FeatureCategory")]
            public string Pbi { get; set; }
            [JsonProperty("Microsoft.RequirementCategory")]
            public string Req { get; set; }
        }



    }
}
