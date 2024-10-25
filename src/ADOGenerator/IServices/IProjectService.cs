using ADOGenerator.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOGenerator.IServices
{
    public interface IProjectService
    {
        void RemoveKey(string id);

        void AddMessage(string id, string message);

        JObject GetStatusMessage(string id);

        HttpResponseMessage GetprojectList(string accname, string pat);

        string GetJsonFilePath(bool IsPrivate, string TemplateFolder, string TemplateName, string FileName = "");

        string[] CreateProjectEnvironment(Project model);
        // string[] CreateProjectEnvironment(string organizationName, string newProjectName, string token, string templateUsed, string templateFolder);

        public bool CheckForInstalledExtensions(string extensionJsonFile, string token, string account);

        public bool InstallExtensions(Project model, string accountName, string PAT);

        public bool WhereDoseTemplateBelongTo(string templatName);

    }
}
