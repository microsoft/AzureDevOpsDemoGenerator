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
        string[] GenerateTemplateArifacts(Project model);
    }
}
