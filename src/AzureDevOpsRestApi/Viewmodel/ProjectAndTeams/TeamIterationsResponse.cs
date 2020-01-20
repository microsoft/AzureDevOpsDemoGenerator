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
            public string Id { get; set; }
            public string Name { get; set; }

        }

        public class Iterations
        {
            public int Count { get; set; }
            public IList<Value> Value { get; set; }
        }
    }
}
