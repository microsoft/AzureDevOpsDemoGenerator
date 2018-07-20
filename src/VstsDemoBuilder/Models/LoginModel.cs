using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VstsDemoBuilder.Models
{
    public class LoginModel
    {        
        public string AccountName { get; set; }
        
        public string PAT { get; set; }

        public string Message { get; set; }

        public string Event { get; set; }

        public string name { get; set; }

        public string templateid { get; set; }
    }
}