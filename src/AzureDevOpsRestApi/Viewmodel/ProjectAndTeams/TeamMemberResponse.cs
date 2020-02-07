using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.ProjectAndTeams
{
    public class TeamMemberResponse
    {
        public class Identity
        {
            public string DisplayName { get; set; }
            public string Url { get; set; }
            public string Id { get; set; }
            public string UniqueName { get; set; }
            public string ImageUrl { get; set; }
            public string Descriptor { get; set; }
        }

        public class Value
        {
            public Identity Identity { get; set; }
        }

        public class TeamMembers
        {
            public IList<Value> Value { get; set; }
            public int Count { get; set; }
        }


    }
}
