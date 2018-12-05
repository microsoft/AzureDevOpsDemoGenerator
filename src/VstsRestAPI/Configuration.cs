
namespace VstsRestAPI
{
    public class Configuration : IConfiguration
    {
        public string UriString { get; set; }

        public string  UriParams { get; set; }

        public string RequestBody { get; set; }

        public string AccountName { get; set; }
        public string PersonalAccessToken { get; set; }
        public string Project { get; set; }
        public string Team { get; set; }
        public string MoveToProject { get; set; }
        public string Query { get; set; }
        public string Identity { get; set; }
        public string WorkItemIds { get; set; }
        public string WorkItemId { get; set; }
        public string ProcessId { get; set; }
        public string PickListId { get; set; }
        public string QueryId { get; set; }
        public string FilePath { get; set; }
        public string GitRepositoryId { get; set; }
        public string VersionNumber { get; set; }
        public string Id { get; set; }
    }
    public class ProjectConfigurationDetails
    {
        public static ProjectConfigurations AppConfig { get; set; }

    }
    public class ProjectConfigurations
    {
        public Configuration AgentQueueConfig { get; set; }
        public Configuration WorkItemConfig { get; set; }
        public Configuration BuildDefinitionConfig { get; set; }
        public Configuration ReleaseDefinitionConfig { get; set; }
        public Configuration RepoConfig { get; set; }
        public Configuration BoardConfig { get; set; }
        public Configuration Config { get; set; }
        public Configuration GetReleaseConfig { get; set; }

    }

    public enum TemplateType
    {
        Agile,
        Scrum,
        CMMI
    }
}
