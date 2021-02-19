using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureDevOpsDemoBuilder.Models
{
    public class AccountsResponse
    {
        public class Properties
        {
        }
        public class Value
        {
            public string AccountId { get; set; }
            public string AccountUri { get; set; }
            public string AccountName { get; set; }
            public Properties Properties { get; set; }
        }

        public class AccountList
        {
            public int Count { get; set; }
            public IList<Value> Value { get; set; }
        }
    }
}