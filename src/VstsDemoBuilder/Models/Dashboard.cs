using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VstsDemoBuilder.Models
{
    public class Dashboard
    {
        public string uid { get; set; }
        public string id { get; set; }
        public string accessToken { get; set; }
        public string accountName { get; set; }
        public string TargetAccountName { get; set; }
        public string VersionControl { get; set; }

        public string refreshToken { get; set; }
        public ProjectList Projects { get; set; }
        public string srcProjectId { get; set; }
        public string srcTeamId { get; set; }
        public List<string> accountsForDdl { get; set; }
        public bool hasAccount { get; set; }
        public string accURL { get; set; }
        public string SuccessMsg { get; set; }
        public string ErrList { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string errpath { get; set; }
        public string NewPat { get; set; }
        public string Message { get; set; }
        public string SelectedID { get; set; }
        public string SrcProjectName { get; set; }
        public string NewProjectName { get; set; }
        public string errors { get; set; }

    }

}