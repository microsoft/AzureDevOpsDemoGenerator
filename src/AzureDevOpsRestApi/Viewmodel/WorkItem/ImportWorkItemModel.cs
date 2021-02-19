using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.WorkItem
{
    public class ImportWorkItemModel
    {
        public class WorkItems : BaseViewModel
        {
            public int Count { get; set; }
            public Value[] Value { get; set; }

        }

        public class Value
        {
            public int Id { get; set; }
            public int Rev { get; set; }
            public Fields Fields { get; set; }
            public Relations[] Relations { get; set; }
            public string Url { get; set; }


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
            public float MicrosoftVstsSchedulingEffort { get; set; }

            [JsonProperty(PropertyName = "System.Description")]
            public string SystemDescription { get; set; }

            [JsonProperty(PropertyName = "System.AssignedTo")]
            public string SystemAssignedTo { get; set; }

            [JsonProperty(PropertyName = "System.BoardLane")]
            public string SystemBoardLane { get; set; }

            [JsonProperty(PropertyName = "Microsoft.VSTS.Scheduling.RemainingWork")]
            public float MicrosoftVstsSchedulingRemainingWork { get; set; }

            [JsonProperty(PropertyName = "Microsoft.VSTS.Common.Priority")]
            public float MicrosoftVstsCommonPriority { get; set; }

            [JsonProperty(PropertyName = "System.Tags")]
            public string SystemTags { get; set; }
            [JsonProperty(PropertyName = "Microsoft.VSTS.TCM.Steps")]
            public string MicrosoftVststcmSteps { get; set; }
            [JsonProperty(PropertyName = "Microsoft.VSTS.TCM.Parameters")]
            public string MicrosoftVststcmParameters { get; set; }
            [JsonProperty(PropertyName = "Microsoft.VSTS.TCM.LocalDataSource")]
            public string MicrosoftVststcmLocalDataSource { get; set; }
            [JsonProperty(PropertyName = "Microsoft.VSTS.TCM.AutomationStatus")]
            public string MicrosoftVststcmAutomationStatus { get; set; }

            [JsonProperty(PropertyName = "Microsoft.VSTS.Common.AcceptanceCriteria")]
            public string MicrosoftVstsCommonAcceptanceCriteria { get; set; }
        }

        public class Relations
        {
            public string Rel { get; set; }
            public string Url { get; set; }
            public Dictionary<string, string> Attributes { get; set; }
        }

        public class Attributes
        {
            public string IsLocked { get; set; }
            public string Comment { get; set; }
        }
    }
}
