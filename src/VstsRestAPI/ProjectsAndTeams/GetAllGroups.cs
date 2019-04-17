using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.ProjectsAndTeams
{
    public class GetAllGroups
    {
        public class Value
        {
            public string subjectKind { get; set; }
            public string description { get; set; }
            public string domain { get; set; }
            public string principalName { get; set; }
            public object mailAddress { get; set; }
            public string displayName { get; set; }
            public string url { get; set; }
            public string descriptor { get; set; }
        }

        public class GroupList
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }
    }
}
