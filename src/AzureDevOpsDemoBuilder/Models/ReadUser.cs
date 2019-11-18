using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureDevOpsDemoBuilder.Models
{
    public class ReadUser
    {
        public class User
        {
            public IList<string> Users { get; set; }
        }
    }
}