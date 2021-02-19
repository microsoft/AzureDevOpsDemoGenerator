using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.ProjectsAndTeams
{
    public class GetAllGroups
    {
        public class Value
        {
            public string SubjectKind { get; set; }
            public string Description { get; set; }
            public string Domain { get; set; }
            public string PrincipalName { get; set; }
            public object MailAddress { get; set; }
            public string DisplayName { get; set; }
            public string Url { get; set; }
            public string Descriptor { get; set; }
        }

        public class GroupList
        {
            public int Count { get; set; }
            public IList<Value> Value { get; set; }
        }
    }
}
