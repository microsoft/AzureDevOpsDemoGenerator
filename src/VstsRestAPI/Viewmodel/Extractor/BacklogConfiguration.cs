using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.Extractor
{

    public class BacklogConfigurationList
    {
        public IList<BacklogConfiguration> backlogConfiguration { get; set; }
        public string teamName { get; set; }

    }
    
    public class BacklogConfiguration
    {
        public string bugsBehavior { get; set; }

        public string[] hiddenBacklogs { get; set; }

        public BacklogLevelConfiguration backlogLevelConfiguration { get; set;}


        public BacklogFields backlogFields { get; set; }

        public BacklogLevelConfiguration[] portfolioBacklogs { get; set;  }

        public BacklogLevelConfiguration requirementBacklog { get; set; }

        public BacklogLevelConfiguration taskBacklog { get; set; }

        public string url { get; set; }

        public WorkItemTypeStateInfo[] workItemTypeMappedStates { get; set;  }
    }

    public class BacklogFields
    {
        public object typeFields { get; set; }
    }

    public class BugsBehavior{
        public string asRequirements { get; set; }

        public string asTasks { get; set; }

        public string off { get; set; }

    }
    
    public class BacklogLevelConfiguration
    {

        public string Color { get; set; }

        public string id { get; set; }

        public bool isHidden { get; set; }

        public string name { get; set; }

        public int rank  { get; set; }

        public int workItemCountLimit { get; set; }

        public WorkItemTypeReference defaultWorkItemType { get; set; }

        public WorkItemTypeReference[] workItemTypes { get; set; }

        public WorkItemFieldReference[] addPanelFields { get; set; }

        public BacklogColumn[] columnFields { get; set; }

        

    }

    public class WorkItemFieldReference
    {
        public string name { get; set; }

        public string referenceName { get; set; }

        public string URL { get; set; }
    }

    public class WorkItemTypeReference
    {
        public string name { get; set; }

        public string URL { get; set; }

    }

    public class WorkItemTypeStateInfo
    {

        public string workItemTypeName { get; set; }

        public object states { get; set; }

    }

    public class BacklogColumn
    {
        public WorkItemFieldReference columnFieldReference { get; set; }
        public int width { get; set; }
    }
}
