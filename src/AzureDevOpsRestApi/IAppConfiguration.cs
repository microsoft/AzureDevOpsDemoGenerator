
namespace AzureDevOpsAPI
{
    public interface IAppConfiguration
    {
        string PersonalAccessToken { get; set; }
        string AccountName { get; set; }
        string Project { get; set; }
        string ProjectId { get; set; }
        string UriString { get; set; }
        string VersionNumber { get; set; }
        string Id { get; set; }
        string Team { get; set; }

        string GitBaseAddress { get; set; }
        string MediaType { get; set; }
        string Scheme { get; set; }
        string GitCredential { get; set; }
        string UserName { get; set; }
    }
}