using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace AzureDevOpsDemoBuilder.Models
{
    public class Project
    {
        public string Id { get; set; }
        public string MemberID { get; set; }
        public string ProcessTemplate { get; set; }
        public string ProjectName { get; set; }
        public string ProjectId { get; set; }
        public string SelectedTemplate { get; set; }
        public string TemplateName { get; set; }
        public bool IsAuthenticated { get; set; }
        public string SupportEmail { get; set; }
        public List<string> Templates { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public EnvironmentValues Environment { get; set; }
        public List<BuildDef> BuildDefinitions { get; set; }
        public List<ReleaseDef> ReleaseDefinitions { get; set; }
        public string UserMethod { get; set; }
        public List<SelectListItem> AccountUsersForDdl { get; set; }
        public string SelectedUsers { get; set; }
        public List<string> AccountUsersForWi { get; set; }
        public string SonarQubeDNS { get; set; }

        public bool IsExtensionNeeded { get; set; }
        public bool IsAgreeTerms { get; set; }

        public string WebsiteUrl { get; set; }
        public string Region { get; set; }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public List<string> AccountsForDropdown { get; set; }
        public string AccountName { get; set; }
        public bool HasAccount { get; set; }
        public string SelectedTemplateDescription { get; set; }
        public string SelectedTemplateFolder { get; set; }
        public string Message { get; set; }
        public string EnableExtractor { get; set; }
        public string TokenType { get; set; }
        public string GitHubUserName { get; set; }
        public string GitHubToken { get; set; }
        public bool GitHubFork { get; set; }
        public string GitRepoName { get; set; }
        public string GitRepoURL { get; set; }
        public string ForkGitHubRepo { get; set; }
        public bool IsApi { get; set; }
        public bool IsPrivatePath { get; set; }

        public string PrivateTemplateName { get; set; }
        public string PrivateTemplatePath { get; set; }
        public string TemplateImage { get; set; }
        public string Document { get; set; }
    }
    public class EnvironmentValues
    {
        public string UserUniqueName { get; set; }
        public string UserUniqueId { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public Dictionary<string, string> RepositoryIdList { get; set; }
        public Dictionary<string, int> AgentQueues { get; set; }
        public Dictionary<string, string> ServiceEndpoints { get; set; }
        public Dictionary<string, string> PullRequests { get; set; }
        public Dictionary<string, string> GitHubRepos { get; set; }
        public Dictionary<int, string> VariableGroups { get; set; }
        public Dictionary<string, bool> ReposImported { get; set; }
        public string BoardRowFieldName { get; set; }
    }
    public class BuildDef
    {
        public string FilePath { get; set; }
        public string FileName
        {
            get { return string.IsNullOrEmpty(this.FilePath) ? string.Empty : System.IO.Path.GetFileName(this.FilePath); }
            set { FileName = value; }
        }
        public string Id { get; set; }
        public string Name { get; set; }

    }
    public class ReleaseDef
    {
        public string FilePath { get; set; }
        public string FileName
        {
            get { return string.IsNullOrEmpty(this.FilePath) ? string.Empty : System.IO.Path.GetFileName(this.FilePath); }
            set { FileName = value; }
        }
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class BuildReleaseMapping
    {
        private BuildDef _buildDef = null;
        public string Build { get; set; }
        public string Release { get; set; }
        public BuildDef BuildDefinition
        {
            get
            {
                if (_buildDef == null)
                {
                    _buildDef = new BuildDef();
                }

                return _buildDef;
            }
            set
            {
                _buildDef = value;
            }
        }
    }

    public class ProjectTemplate
    {
        public string Teams { get; set; }
        public string BoardColumns { get; set; }
        public string ProjectSettings { get; set; }
        public string CardStyle { get; set; }
        public string CardField { get; set; }
        public string PBIfromTemplate { get; set; }
        public string BugfromTemplate { get; set; }
        public string EpicfromTemplate { get; set; }
        public string TaskfromTemplate { get; set; }
        public string TestCasefromTemplate { get; set; }
        public string FeaturefromTemplate { get; set; }
        public string UserStoriesFromTemplate { get; set; }
        public string SetEpic { get; set; }
        public string BoardRows { get; set; }
        public string TeamArea { get; set; }
        public string TestPlanfromTemplate { get; set; }
        public string TestSuitefromTemplate { get; set; }
    }

    public class ProjectSettings
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public List<string> Users { get; set; }
        public List<string> Tags { get; set; }
        public List<string> Queues { get; set; }
        public Dictionary<string, string> RenameIterations { get; set; }
    }

    public class RequiredExtensions
    {
        public class ExtensionWithLink
        {
            public string ExtensionName { get; set; }
            public string Link { get; set; }
            public string PublisherId { get; set; }
            public string ExtensionId { get; set; }
            public string PublisherName { get; set; }
            public string License { get; set; }
        }
        public class Extension
        {
            public List<ExtensionWithLink> Extensions { get; set; }
        }
        public class ListExtension
        {
            public List<ExtensionWithLink> Extensions { get; set; }
        }
    }
    public class TestSuite
    {
        public class Plan
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
        public class Value
        {
            public string Name { get; set; }
            public Plan Plan { get; set; }
            public string SuiteType { get; set; }
            public bool InheritDefaultConfigurations { get; set; }
            public string State { get; set; }
            public IList<string> TestCases { get; set; }
            public IList<string> RequirementIds { get; set; }
            public int? Revision { get; set; }
        }
        public class TestSuites
        {
            public IList<Value> Value { get; set; }
            public int Count { get; set; }
        }
    }

    public class RequestedProject
    {
        public string Email { get; set; }
        public string ProjectName { get; set; }
        public string TrackId { get; set; }
        public string Status { get; set; }
    }

    public class MultiProjects
    {
        public string AccessToken { get; set; }
        public string OrganizationName { get; set; }
        public string TemplateName { get; set; }
        public string TemplatePath { get; set; }
        public string GitHubToken { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public bool InstallExtensions { get; set; }
        public IList<RequestedProject> Users { get; set; }
    }

    public class ProjectResponse
    {
        public string TemplateName { get; set; }
        public string TemplatePath { get; set; }
        public IList<RequestedProject> Users { get; set; }
    }

    public class PrivateTemplate
    {
        public string PrivateTemplateName { get; set; }
        public string PrivateTemplatePath { get; set; }
        public string ResponseMessage { get; set; }
    }
}