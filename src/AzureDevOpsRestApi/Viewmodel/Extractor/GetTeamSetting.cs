using Newtonsoft.Json;
using System.Collections.Generic;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class ExportTeamSetting
    {
        public class BacklogVisibilities
        {
            [JsonProperty(PropertyName = "Microsoft.EpicCategory")]
            public bool EpicCategory { get; set; }
            [JsonProperty(PropertyName = "Microsoft.FeatureCategory")]
            public bool FeatureCategory { get; set; }
            [JsonProperty(PropertyName = "Microsoft.RequirementCategory")]
            public bool RequirementCategory { get; set; }
        }
        public class Setting
        {
            public string BugsBehavior { get; set; }
            public BacklogVisibilities BacklogVisibilities { get; set; }
        }
    }
}
