using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.ProjectAndTeams
{
    public class TeamSettingResponse
    {
        public class BacklogIteration
        {
            public string id { get; set; }
            public string name { get; set; }
            public string path { get; set; }
            public string url { get; set; }
        }
        public class DefaultIteration
        {
            public string id { get; set; }
            public string name { get; set; }
            public string path { get; set; }
            public string url { get; set; }
        }
        public class Project
        {
            public string href { get; set; }
        }
        public class TeamSetting
        {
            public BacklogIteration backlogIteration { get; set; }
            public DefaultIteration defaultIteration { get; set; }
        }
    }
}
