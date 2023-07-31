
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.ProjectAndTeams
{
    public class TeamResponse
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class RootTeams
    {
        public List<Value> value { get; set; }
        public int count { get; set; }
    }

    public class Value
    {
        public string id { get; set; }
        public string name { get; set; }
        public string projectName { get; set; }
        public string projectId { get; set; }
    }
}
