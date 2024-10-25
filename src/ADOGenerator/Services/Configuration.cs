using VstsRestAPI;

namespace ADOGenerator.Services
{
    public class ADOConfiguration : IADOConfiguration
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
        public string _gitcredential { get; set; }
        public string _gitbaseAddress { get; set; }
        public string _mediaType { get; set; }
        public string _scheme { get; set; }
        public string userName { get; set; }
    }
    public class ProjectConfigurations
    {
        public VstsRestAPI.ADOConfiguration AgentQueueConfig { get; set; }
        public VstsRestAPI.ADOConfiguration WorkItemConfig { get; set; }
        public VstsRestAPI.ADOConfiguration BuildDefinitionConfig { get; set; }
        public VstsRestAPI.ADOConfiguration ReleaseDefinitionConfig { get; set; }
        public VstsRestAPI.ADOConfiguration RepoConfig { get; set; }
        public VstsRestAPI.ADOConfiguration BoardConfig { get; set; }
        public VstsRestAPI.ADOConfiguration Config { get; set; }
        public VstsRestAPI.ADOConfiguration GetReleaseConfig { get; set; }
        public VstsRestAPI.ADOConfiguration ExtensionConfig { get; set; }
        public VstsRestAPI.ADOConfiguration EndpointConfig { get; set; }
        public VstsRestAPI.ADOConfiguration QueriesConfig { get; set; }
        public VstsRestAPI.ADOConfiguration VariableGroupConfig { get; set; }

    }

    public enum TemplateType
    {
        Agile,
        Scrum,
        CMMI,
        Basic
    }
}
