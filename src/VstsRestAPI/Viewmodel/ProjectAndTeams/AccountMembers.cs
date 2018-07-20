using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.ProjectAndTeams
{
    public class AccountMembers
    {
        public class Self
        {
            public string href { get; set; }
        }

        public class Memberships
        {
            public string href { get; set; }
        }

        public class Links
        {
            public Self self { get; set; }
            public Memberships memberships { get; set; }
        }

        public class Member
        {
            public string subjectKind { get; set; }
            public string domain { get; set; }
            public string principalName { get; set; }
            public string mailAddress { get; set; }
            public int metaTypeId { get; set; }
            public string origin { get; set; }
            public string originId { get; set; }
            public string id { get; set; }
            public string displayName { get; set; }
            public Links _links { get; set; }
            public string url { get; set; }
            public string descriptor { get; set; }
        }

        public class AccessLevel
        {
            public string licensingSource { get; set; }
            public string accountLicenseType { get; set; }
            public string licenseDisplayName { get; set; }
            public string status { get; set; }
            public string statusMessage { get; set; }
        }

        public class Value
        {
            public string id { get; set; }
            public Member member { get; set; }
            public AccessLevel accessLevel { get; set; }
            public DateTime lastAccessedDate { get; set; }
            public object projectEntitlements { get; set; }
            public object extensions { get; set; }
        }

        public class Account
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }
    }
}
