using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class ProjectSetting
    {
        public ProjectSetting()
        {
            this.IsPrivate = "false";
        }
        public string Description { get; set; }
        public string Teams { get; set; }
        public string SourceCode { get; set; }
        public string CreateService { get; set; }
        public string BoardColumns { get; set; }
        public string ProjectSettings { get; set; }
        public string CardStyle { get; set; }
        public string CardField { get; set; }
        public string PBIfromTemplate { get; set; }
        public string BugfromTemplate { get; set; }
        public string EpicfromTemplate { get; set; }
        public string TaskfromTemplate { get; set; }
        public string TestCasefromTemplate { get; set; }
        public string FeaturefromTemplate { get; set; }
        public string UserStoriesFromTemplate { get; set; }
        public string SetEpic { get; set; }
        public string BoardRows { get; set; }
        public string Widget { get; set; }
        public string Chart { get; set; }
        public string TeamArea { get; set; }
        public string IsPrivate { get; set; }
    }


}
