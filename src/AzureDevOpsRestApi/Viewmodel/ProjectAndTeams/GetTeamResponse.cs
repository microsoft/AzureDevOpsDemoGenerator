using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.ProjectAndTeams
{
    public class GetTeamResponse
    {
        public class Team : BaseViewModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
            public string Description { get; set; }
            public string IdentityUrl { get; set; }
        }
    }
}
