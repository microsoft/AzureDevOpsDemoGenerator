using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.WorkItem
{
    public class WorkItemPatchResponse
    {

        public class WorkItem : BaseViewModel
        {
            public int id { get; set; }
            public int rev { get; set; }
            public Fields fields { get; set; }
            public Relation[] relations { get; set; }
            public _Links _links { get; set; }
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

            [JsonProperty(PropertyName = "System.BoardColumn")]
            public string SystemBoardColumn { get; set; }

            [JsonProperty(PropertyName = "System.BoardColumnDone")]
            public bool SystemBoardColumnDone { get; set; }

            [JsonProperty(PropertyName = "Microsoft.VSTS.Common.Priority")]
            public int MicrosoftVSTSCommonPriority { get; set; }

            [JsonProperty(PropertyName = "Microsoft.VSTS.Common.BusinessValue")]
            public int MicrosoftVSTSCommonBusinessValue { get; set; }

            [JsonProperty(PropertyName = "Microsoft.VSTS.Common.ValueArea")]
            public string MicrosoftVSTSCommonValueArea { get; set; }

            [JsonProperty(PropertyName = "System.Description")]
            public string SystemDescription { get; set; }

            [JsonProperty(PropertyName = "System.History")]
            public string SystemHistory { get; set; }
        }

        public class _Links
        {
            public Self self { get; set; }
            public Workitemupdates workItemUpdates { get; set; }
            public Workitemrevisions workItemRevisions { get; set; }
            public Workitemhistory workItemHistory { get; set; }
            public Html html { get; set; }
            public Workitemtype workItemType { get; set; }
            public Fields1 fields { get; set; }
        }

        public class Self
        {
            public string href { get; set; }
        }

        public class Workitemupdates
        {
            public string href { get; set; }
        }

        public class Workitemrevisions
        {
            public string href { get; set; }
        }

        public class Workitemhistory
        {
            public string href { get; set; }
        }

        public class Html
        {
            public string href { get; set; }
        }

        public class Workitemtype
        {
            public string href { get; set; }
        }

        public class Fields1
        {
            public string href { get; set; }
        }

        public class Relation
        {
            public string rel { get; set; }
            public string url { get; set; }
            public Attributes attributes { get; set; }
        }

        public class Attributes
        {
            public bool isLocked { get; set; }
        }
    }
}

