using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.ProjectAndTeams
{
    public class AccountMembers
    {
        public class Self
        {
            public string Href { get; set; }
        }

        public class Memberships
        {
            public string Href { get; set; }
        }

        public class Links
        {
            public Self Self { get; set; }
            public Memberships Memberships { get; set; }
        }

        public class Member
        {
            public string SubjectKind { get; set; }
            public string Domain { get; set; }
            public string PrincipalName { get; set; }
            public string MailAddress { get; set; }
            public int MetaTypeId { get; set; }
            public string Origin { get; set; }
            public string OriginId { get; set; }
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public Links Links { get; set; }
            public string Url { get; set; }
            public string Descriptor { get; set; }
        }

        public class AccessLevel
        {
            public string LicensingSource { get; set; }
            public string AccountLicenseType { get; set; }
            public string LicenseDisplayName { get; set; }
            public string Status { get; set; }
            public string StatusMessage { get; set; }
        }

        public class Value
        {
            public string Id { get; set; }
            public Member Member { get; set; }
            public AccessLevel AccessLevel { get; set; }
            public DateTime LastAccessedDate { get; set; }
            public object ProjectEntitlements { get; set; }
            public object Extensions { get; set; }
        }

        public class Account
        {
            public int Count { get; set; }
            public IList<Value> Value { get; set; }
        }
    }
}
