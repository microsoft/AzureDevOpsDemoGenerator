using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.ProjectAndTeams
{
    public class TeamIterationsResponse
    {
        public class Value
        {
            public string id { get; set; }
            public string name { get; set; }

        }

        public class Iterations
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }
    }
}
