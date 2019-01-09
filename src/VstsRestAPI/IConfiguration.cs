
namespace VstsRestAPI
{
    public interface IConfiguration
    {        
        string PersonalAccessToken { get; set; }
        string AccountName { get; set; }
        string Project { get; set; }
        string UriString { get; set; }        
        string VersionNumber { get; set; }
        string Id { get; set; }
        string Team { get; set; }
    }
}