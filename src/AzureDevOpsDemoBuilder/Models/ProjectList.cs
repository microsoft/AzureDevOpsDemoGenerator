using System.Collections.Generic;

namespace AzureDevOpsDemoBuilder.Models
{
    public class ProjectList
    {
        public class Authentication
        {
            public string accname { get; set; }
            public string pat { get; set; }
        }
        public class Value
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string state { get; set; }
        }

        public class ProjectDetails
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
            public string errmsg { get; set; }
            public string accessToken { get; set; }
            public string accountName { get; set; }
            public List<string> accountsForDropdown { get; set; }
        }
    }
}