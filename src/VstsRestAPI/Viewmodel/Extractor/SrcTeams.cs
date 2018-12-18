using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class SrcTeams
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        
    }


    public class SrcTeamsList
    {
        public IList<SrcTeams> value { get; set; }
        public string  DefaultTeam { get; set; }
    }
}
