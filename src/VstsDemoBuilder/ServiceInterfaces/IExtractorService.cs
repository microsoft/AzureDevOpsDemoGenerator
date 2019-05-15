using System.Collections.Generic;
using VstsDemoBuilder.Models;
using VstsRestAPI;

namespace VstsDemoBuilder.ServiceInterfaces
{
    public interface IExtractorService
    {
        ProjectConfigurations ProjectConfiguration(Project model);
        int GetTeamsCount(ProjectConfigurations appConfig);
        int GetIterationsCount(ProjectConfigurations appConfig);
        Dictionary<string, int> GetWorkItemsCount(ProjectConfigurations appConfig);
        int GetBuildDefinitionCount(ProjectConfigurations appConfig);
        int GetReleaseDefinitionCount(ProjectConfigurations appConfig);
        //List<RequiredExtensions.ExtensionWithLink> GetInstalledExtensions(ProjectConfigurations appConfig);
        //void ExportQuries(ProjectConfigurations appConfig);
        //bool ExportTeams(ProjectConfigurations appConfig, string processTemplate, string projectID);
        //bool ExportIterations(ProjectConfigurations appConfig);
        //void ExportWorkItems(ProjectConfigurations appConfig);
        //void ExportRepositoryList(ProjectConfigurations appConfig);
        //int GetBuildDefinitions(ProjectConfigurations appConfig);
        //int GeneralizingGetReleaseDefinitions(ProjectConfigurations appConfig);
        //void GetServiceEndpoints(ProjectConfigurations appConfig);
        string[] GenerateTemplateArifacts(Project model);

    }
}
