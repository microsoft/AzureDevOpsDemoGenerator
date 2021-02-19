using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.WorkItem
{
    public class WorkItemPatchResponse
    {

        public class WorkItem : BaseViewModel
        {
            public int Id { get; set; }
            public int Rev { get; set; }
            public Fields Fields { get; set; }
            public Relation[] Relations { get; set; }
            public Links Links { get; set; }
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

            [JsonProperty(PropertyName = "System.BoardColumn")]
            public string SystemBoardColumn { get; set; }

            [JsonProperty(PropertyName = "System.BoardColumnDone")]
            public bool SystemBoardColumnDone { get; set; }

            [JsonProperty(PropertyName = "Microsoft.VSTS.Common.Priority")]
            public int MicrosoftVstsCommonPriority { get; set; }

            [JsonProperty(PropertyName = "Microsoft.VSTS.Common.BusinessValue")]
            public int MicrosoftVstsCommonBusinessValue { get; set; }

            [JsonProperty(PropertyName = "Microsoft.VSTS.Common.ValueArea")]
            public string MicrosoftVstsCommonValueArea { get; set; }

            [JsonProperty(PropertyName = "System.Description")]
            public string SystemDescription { get; set; }

            [JsonProperty(PropertyName = "System.History")]
            public string SystemHistory { get; set; }
        }

        public class Links
        {
            public Self Self { get; set; }
            public Workitemupdates WorkItemUpdates { get; set; }
            public Workitemrevisions WorkItemRevisions { get; set; }
            public Workitemhistory WorkItemHistory { get; set; }
            public Html Html { get; set; }
            public Workitemtype WorkItemType { get; set; }
            public Fields1 Fields { get; set; }
        }

        public class Self
        {
            public string Href { get; set; }
        }

        public class Workitemupdates
        {
            public string Href { get; set; }
        }

        public class Workitemrevisions
        {
            public string Href { get; set; }
        }

        public class Workitemhistory
        {
            public string Href { get; set; }
        }

        public class Html
        {
            public string Href { get; set; }
        }

        public class Workitemtype
        {
            public string Href { get; set; }
        }

        public class Fields1
        {
            public string Href { get; set; }
        }

        public class Relation
        {
            public string Rel { get; set; }
            public string Url { get; set; }
            public Attributes Attributes { get; set; }
        }

        public class Attributes
        {
            public bool IsLocked { get; set; }
        }
    }
}

