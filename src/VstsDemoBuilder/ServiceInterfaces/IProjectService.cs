using Newtonsoft.Json.Linq;
using System.Net.Http;
using VstsDemoBuilder.Models;

namespace VstsDemoBuilder.ServiceInterfaces
{
    public interface IProjectService
    {
        void RemoveKey(string id);

        void AddMessage(string id, string message);

        JObject GetStatusMessage(string id);

        HttpResponseMessage GetprojectList(string accname, string pat);
     
        string GetJsonFilePath(bool IsPrivate, string TemplateFolder, string TemplateName, string FileName = "");

        string[] CreateProjectEnvironment(Project model);

        bool CheckForInstalledExtensions(string extensionJsonFile, string token, string account);

        bool InstallExtensions(Project model, string accountName, string PAT);

        bool WhereDoseTemplateBelongTo(string templatName);
        
    }
}
