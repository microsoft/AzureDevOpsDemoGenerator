using System.Collections.Generic;
using System.Web.Mvc;

namespace VstsDemoBuilder.Models
{
    public class Project
    {
        public string id { get; set; }
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
        public List<SelectListItem> accountUsersForDdl { get; set; }
        public string selectedUsers { get; set; }
        public List<string> accountUsersForWi { get; set; }
        public string SonarQubeDNS { get; set; }

        public bool isExtensionNeeded { get; set; }
        public bool isAgreeTerms { get; set; }

        public string websiteUrl { get; set; }
        public string Region { get; set; }

        public string accessToken { get; set; }
        public string refreshToken { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public List<string> accountsForDropdown { get; set; }
        public string accountName { get; set; }
        public bool hasAccount { get; set; }
        public string selectedTemplateDescription { get; set; }
        public string selectedTemplateFolder { get; set; }
        public string Message { get; set; }
        public string EnableExtractor { get; set; }
        public string tokenType { get; set; }
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
        public string templateImage { get; set; }
    }
    public class EnvironmentValues
    {
        public string UserUniquename { get; set; }
        public string UserUniqueId { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public Dictionary<string, string> repositoryIdList { get; set; }
        public Dictionary<string, int> AgentQueues { get; set; }
        public Dictionary<string, string> serviceEndpoints { get; set; }
        public Dictionary<string, string> pullRequests { get; set; }
        public Dictionary<string, string> GitHubRepos { get; set; }
        public Dictionary<int, string> VariableGroups { get; set; }
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
        public string build { get; set; }
        public string release { get; set; }
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
        public string type { get; set; }
        public string id { get; set; }
        public List<string> users { get; set; }
        public List<string> tags { get; set; }
        public List<string> queues { get; set; }
        public Dictionary<string, string> renameIterations { get; set; }
    }

    public class RequiredExtensions
    {
        public class ExtensionWithLink
        {
            public string extensionName { get; set; }
            public string link { get; set; }
            public string publisherId { get; set; }
            public string extensionId { get; set; }
            public string publisherName { get; set; }
            public string License { get; set; }
        }
        public class Extension
        {
            public List<ExtensionWithLink> Extensions { get; set; }
        }
        public class listExtension
        {
            public List<ExtensionWithLink> Extensions { get; set; }
        }
    }
    public class TestSuite
    {
        public class Plan
        {
            public string id { get; set; }
            public string name { get; set; }
        }
        public class Value
        {
            public string name { get; set; }
            public Plan plan { get; set; }
            public string suiteType { get; set; }
            public bool inheritDefaultConfigurations { get; set; }
            public string state { get; set; }
            public IList<string> TestCases { get; set; }
            public IList<string> requirementIds { get; set; }
            public int? revision { get; set; }
        }
        public class TestSuites
        {
            public IList<Value> value { get; set; }
            public int count { get; set; }
        }
    }

    public class RequestedProject
    {
        public string email { get; set; }
        public string projectName { get; set; }
        public string trackId { get; set; }
        public string status { get; set; }
    }

    public class MultiProjects
    {
        public string accessToken { get; set; }
        public string organizationName { get; set; }
        public string templateName { get; set; }
        public string templatePath { get; set; }
        public string gitHubToken { get; set; }
        public string userId { get; set; }
        public string password { get; set; }
        public bool installExtensions { get; set; }
        public IList<RequestedProject> users { get; set; }
    }

    public class ProjectResponse
    {
        public string templateName { get; set; }
        public string templatePath { get; set; }
        public IList<RequestedProject> users { get; set; }
    }

    public class PrivateTemplate
    {
        public string privateTemplateName { get; set; }
        public string privateTemplateOriginalName { get; set; }
        public string privateTemplatePath { get; set; }
        public string responseMessage { get; set; }
        public bool IsTemplateValid { get; set; }
    }

}