using Newtonsoft.Json;
using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class GetTeamSetting
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
            public string bugsBehavior { get; set; }
            public BacklogVisibilities backlogVisibilities { get; set; }
        }
    }
}
