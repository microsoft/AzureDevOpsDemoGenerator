using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VstsDemoBuilder.Models
{
    public class Feedback
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Noofyears { get; set; }
        public string Know { get; set; }
        public string Purpose { get; set; }
        public string Used { get; set; }
        public string Usedtemplatenames { get; set; }
        public string Kindoftemplates { get; set; }
        public string Otherfeedback { get; set; }
    }
}