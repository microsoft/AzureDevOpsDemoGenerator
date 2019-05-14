using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VstsDemoBuilder.Models;
using VstsRestAPI;

namespace VstsDemoBuilder.ServiceInterfaces
{
    public interface IProjectService
    {
        void RemoveKey(string id);

        void AddMessage(string id, string message);

        JObject GetStatusMessage(string id);

        HttpResponseMessage GetprojectList(string accname, string pat);
     
        string GetJsonFilePath(string TemplateFolder, string TemplateName, string FileName = "");

        string[] CreateProjectEnvironment(Project model, bool IsAPI = false);

        bool CheckForInstalledExtensions(string extensionJsonFile, string token, string account);

        bool InstallExtensions(Project model, string accountName, string PAT);
    }
}
