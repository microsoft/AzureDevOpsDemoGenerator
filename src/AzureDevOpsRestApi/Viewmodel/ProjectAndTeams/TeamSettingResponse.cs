using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.ProjectAndTeams
{
    public class TeamSettingResponse
    {
        public class BacklogIteration
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Path { get; set; }
            public string Url { get; set; }
        }
        public class DefaultIteration
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Path { get; set; }
            public string Url { get; set; }
        }
        public class Project
        {
            public string Href { get; set; }
        }
        public class TeamSetting
        {
            public BacklogIteration BacklogIteration { get; set; }
            public DefaultIteration DefaultIteration { get; set; }
        }
    }
}
