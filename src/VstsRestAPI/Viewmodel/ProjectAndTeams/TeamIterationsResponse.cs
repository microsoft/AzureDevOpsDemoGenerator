using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.ProjectAndTeams
{
    public class TeamIterationsResponse
    {
        public class Child
        {
            public string identifier { get; set; }
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public string path { get; set; }
        }

        public class Value
        {
            public string identifier { get; set; }
            public string name { get; set; }
            public string structureType { get; set; }
            public bool hasChildren { get; set; }
            public string path { get; set; }
            public List<Child> children { get; set; }
        }

        public class Iterations
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }
        //public class Value
        //{
        //    public string id { get; set; }
        //    public string name { get; set; }

        //}

        //public class Iterations
        //{
        //    public int count { get; set; }
        //    public IList<Value> value { get; set; }
        //}
    }
}
