using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.ProjectAndTeams
{
    public class TeamMemberResponse
    {
        public int count { get; set; }

        public class TeamMembers
        {
            public int count { get; set; }
            public value[] value { get; set; }
        }

        public class value
        {
            public string id { get; set; }
            public string displayName { get; set; }
            public string uniqueName { get; set; }
            public string url { get; set; }
            public string imageUrl { get; set; }
        }
    }
}
