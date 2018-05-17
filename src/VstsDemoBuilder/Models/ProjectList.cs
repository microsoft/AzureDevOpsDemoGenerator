using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VstsDemoBuilder.Models
{
    public class ProjectList
    {
        public class Authentication
        {
            public string accname { get; set; }
            public string pat { get; set; }
            public string Message { get; set; }
            public string ErrList { get; set; }
            public string SuccessMsg { get; set; }
            public string errors { get; set; }
            public string accURL { get; set; }
            public string SelectedID { get; set; }
        }
        public class Value
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string state { get; set; }
        }

        public class ProjectCount
        {
            public string id { get; set; }
            public bool successcode { get; set; }
            public int count { get; set; }
            public IList<Value> value { get; set; }
            public SelectList ProjectList { get; set; }
            public string SelectedID { get; set; }
            public string Message { get; set; }
            public string ProjectName { get; set; }
            public string AccName { get; set; }
            public string PAT { get; set; }
            public string errmsg { get; set; }
            public string SuccessMsg { get; set; }
            public string accURL { get; set; }
        }
    }

}