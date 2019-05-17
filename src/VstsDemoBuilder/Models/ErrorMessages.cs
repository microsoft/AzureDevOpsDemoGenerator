using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VstsDemoBuilder.Models
{
    public class Messages
    {
        public ErrorMessages ErrorMessages { get; set; }
    }

    public class ErrorMessages
    {
        public AccountMessages AccountMessages { get; set; }
        public ProjectMessages ProjectMessages { get; set; }
        public TemplateMessages TemplateMessages { get; set; }
    }


    public class AccountMessages
    {
        public string AccountName { get; set; }
        public string AccessToken { get; set; }
    }

    public class ProjectMessages
    {
        public string InvalidProjectName { get; set; }
        public string ProjectNameReservedKeyword { get; set; }
        public string ProjectNameOrEmailID { get; set; }
        public string DuplicateProject { get; set; }
        public string ExtensionNotInstalled { get; set; }
    }

    public class TemplateMessages
    {
        public string TemplateNameOrTemplatePath { get; set; }
        public string TemplateNotFound { get; set; }
        public string FailedTemplate { get; set; }
        public string PrivateTemplateFileExtension { get; set; }
    }

}