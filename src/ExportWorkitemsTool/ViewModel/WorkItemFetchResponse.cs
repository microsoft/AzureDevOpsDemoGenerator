using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatesGeneratorTool.ViewModel
{
    public class WorkItemFetchResponse
    {
        public class WorkItems : BaseViewModel
        {
            public int count { get; set; }
            public Value[] value { get; set; }

        }

        public class Value
        {
            public int id { get; set; }
            public int rev { get; set; }
            public Fields fields { get; set; }
            public Relations[] relations { get; set; }
            public string url { get; set; }


        }

        public class Fields
        {
            [JsonProperty(PropertyName = "System.AreaPath")]
            public string SystemAreaPath { get; set; }

            [JsonProperty(PropertyName = "System.TeamProject")]
            public string SystemTeamProject { get; set; }

            [JsonProperty(PropertyName = "System.IterationPath")]
            public string SystemIterationPath { get; set; }

            [JsonProperty(PropertyName = "System.WorkItemType")]
            public string SystemWorkItemType { get; set; }

            [JsonProperty(PropertyName = "System.State")]
            public string SystemState { get; set; }

            [JsonProperty(PropertyName = "System.Reason")]
            public string SystemReason { get; set; }

            [JsonProperty(PropertyName = "System.CreatedDate")]
            public DateTime SystemCreatedDate { get; set; }

            [JsonProperty(PropertyName = "System.CreatedBy")]
            public string SystemCreatedBy { get; set; }

            [JsonProperty(PropertyName = "System.ChangedDate")]
            public DateTime SystemChangedDate { get; set; }

            [JsonProperty(PropertyName = "System.ChangedBy")]
            public string SystemChangedBy { get; set; }

            [JsonProperty(PropertyName = "System.Title")]
            public string SystemTitle { get; set; }

            [JsonProperty(PropertyName = "Microsoft.VSTS.Scheduling.Effort")]
            public float MicrosoftVSTSSchedulingEffort { get; set; }

            [JsonProperty(PropertyName = "System.Description")]
            public string SystemDescription { get; set; }

            [JsonProperty(PropertyName = "System.AssignedTo")]
            public string SystemAssignedTo { get; set; }

            [JsonProperty(PropertyName = "Microsoft.VSTS.Scheduling.RemainingWork")]
            public float MicrosoftVSTSSchedulingRemainingWork { get; set; }

            [JsonProperty(PropertyName = "Microsoft.VSTS.Common.Priority")]
            public float MicrosoftVSTSCommonPriority { get; set; }

            [JsonProperty(PropertyName = "System.BoardLane")]
            public string SystemBoardLane { get; set; }

            [JsonProperty(PropertyName = "System.Tags")]
            public string SystemTags { get; set; }
            [JsonProperty(PropertyName = "Microsoft.VSTS.TCM.Steps")]
            public string MicrosoftVSTSTCMSteps { get; set; }
            [JsonProperty(PropertyName = "Microsoft.VSTS.TCM.Parameters")]
            public string MicrosoftVSTSTCMParameters { get; set; }
            [JsonProperty(PropertyName = "Microsoft.VSTS.TCM.LocalDataSource")]
            public string MicrosoftVSTSTCMLocalDataSource { get; set; }
            [JsonProperty(PropertyName = "Microsoft.VSTS.TCM.AutomationStatus")]
            public string MicrosoftVSTSTCMAutomationStatus { get; set; }

            [JsonProperty(PropertyName ="System.History")]
            public string SystemHistory { get; set; }
        }

        public class Relations
        {
            public string rel { get; set; }
            public string url { get; set; }
            public Dictionary<string, string> attributes { get; set; }
        }

        public class Attributes
        {
            public string isLocked { get; set; }
            public string comment { get; set; }
        }
    }
}