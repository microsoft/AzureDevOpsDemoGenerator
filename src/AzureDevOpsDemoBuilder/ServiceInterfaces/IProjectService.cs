using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using AzureDevOpsDemoBuilder.Models;
using System;

namespace AzureDevOpsDemoBuilder.ServiceInterfaces
{
    public interface IProjectService
    {
        void RemoveKey(string id);

        void AddMessage(string id, string message);

        string GetStatusMessage(string id);

        HttpResponseMessage GetprojectList(string accname, string pat);
     
        string GetJsonFilePath(bool IsPrivate, string TemplateFolder, string TemplateName, string FileName = "");

        string[] CreateProjectEnvironment(Project model);

        bool CheckForInstalledExtensions(string extensionJsonFile, string token, string account);

        bool InstallExtensions(Project model, string accountName, string PAT);

        void EndEnvironmentSetupProcess(IAsyncResult result, Project model, int usercount);

    }
}
