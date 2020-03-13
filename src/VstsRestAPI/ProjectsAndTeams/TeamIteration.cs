using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.ProjectsAndTeams
{
    public class TeamIterations
    {
        public class TeamIteration
        {
            public string TeamName { get; set; }
            public List<string> Iterations { get; set; }
        }
        public class Map
        {
            public List<TeamIteration> TeamIterationMap { get; set; }
        }
    }
}
