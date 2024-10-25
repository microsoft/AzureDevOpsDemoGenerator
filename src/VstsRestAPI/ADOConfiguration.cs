
namespace VstsRestAPI
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
        public ADOConfiguration AgentQueueConfig { get; set; }
        public ADOConfiguration WorkItemConfig { get; set; }
        public ADOConfiguration BuildDefinitionConfig { get; set; }
        public ADOConfiguration ReleaseDefinitionConfig { get; set; }
        public ADOConfiguration RepoConfig { get; set; }
        public ADOConfiguration BoardConfig { get; set; }
        public ADOConfiguration Config { get; set; }
        public ADOConfiguration GetReleaseConfig { get; set; }
        public ADOConfiguration ExtensionConfig { get; set; }
        public ADOConfiguration EndpointConfig { get; set; }
        public ADOConfiguration QueriesConfig { get; set; }
        public ADOConfiguration VariableGroupConfig { get; set; }

    }

    public enum TemplateType
    {
        Agile,
        Scrum,
        CMMI,
        Basic
    }
}
