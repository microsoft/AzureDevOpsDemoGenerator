using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.Extractor
{

    public class TeamMembersList
    {
        public IList<TeamMembers> teamMembers { get; set; }
        public string teamName { get; set; }
    }

    public class TeamMembers
    {
        [JsonProperty(PropertyName = "value")]
        public IList<TeamMember> teamMember { get; set; }
        public int count { get; set; }
    }


    public class TeamMember
    {
        public bool isAdmin { get; set; }
        public Identity identity { get; set;  } 
    }

    public class Identity
    {
        public string Id { get; set; }
        public string displayName { get; set; }
        public string uniqueName { get; set; }
        public string url{ get; set; }
        public string imageUrl { get; set; }
    }
}
