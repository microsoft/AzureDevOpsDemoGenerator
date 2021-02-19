
namespace AzureDevOpsAPI
{
    public class AppConfiguration : IAppConfiguration
    {
        public string UriString { get; set; }
        public string UriParams { get; set; }
        public string RequestBody { get; set; }
        public string AccountName { get; set; }
        public string PersonalAccessToken { get; set; }
        public string Project { get; set; }
        public string ProjectId { get; set; }
        public string VersionNumber { get; set; }
        public string Id { get; set; }
        public string Team { get; set; }
        public string GitCredential { get; set; }
        public string GitBaseAddress { get; set; }
        public string MediaType { get; set; }
        public string Scheme { get; set; }
        public string UserName { get; set; }
    }
    public class ProjectConfigurations
    {
        public AppConfiguration AgentQueueConfig { get; set; }
        public AppConfiguration WorkItemConfig { get; set; }
        public AppConfiguration BuildDefinitionConfig { get; set; }
        public AppConfiguration ReleaseDefinitionConfig { get; set; }
        public AppConfiguration RepoConfig { get; set; }
        public AppConfiguration BoardConfig { get; set; }
        public AppConfiguration Config { get; set; }
        public AppConfiguration GetReleaseConfig { get; set; }
        public AppConfiguration ExtensionConfig { get; set; }
        public AppConfiguration EndpointConfig { get; set; }
        public AppConfiguration QueriesConfig { get; set; }
        public AppConfiguration VariableGroupConfig { get; set; }

    }

    public enum TemplateType
    {
        Agile,
        Scrum,
        CMMI,
        Basic
    }
}
