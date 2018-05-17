using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.ProjectAndTeams
{
    public class GetTeamResponse
    {
        public class Team : BaseViewModel
        {
            public string id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public string description { get; set; }
            public string identityUrl { get; set; }
        }
    }
}
