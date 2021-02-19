using AzureDevOpsAPI;
using AzureDevOpsAPI.Build;
using AzureDevOpsAPI.DeploymentGRoup;
using AzureDevOpsAPI.Extractor;
using AzureDevOpsAPI.Git;
using AzureDevOpsAPI.ProjectsAndTeams;
using AzureDevOpsAPI.QueriesAndWidgets;
using AzureDevOpsAPI.Queues;
using AzureDevOpsAPI.Release;
using AzureDevOpsAPI.Service;
using AzureDevOpsAPI.Services;
using AzureDevOpsAPI.TestManagement;
using AzureDevOpsAPI.Viewmodel.Extractor;
using AzureDevOpsAPI.Viewmodel.GitHub;
using AzureDevOpsAPI.Viewmodel.Importer;
using AzureDevOpsAPI.Viewmodel.ProjectAndTeams;
using AzureDevOpsAPI.Viewmodel.QueriesAndWidgets;
using AzureDevOpsAPI.Viewmodel.Repository;
using AzureDevOpsAPI.Viewmodel.Sprint;
using AzureDevOpsAPI.Viewmodel.Wiki;
using AzureDevOpsAPI.Viewmodel.WorkItem;
using AzureDevOpsAPI.Wiki;
using AzureDevOpsAPI.WorkItemAndTracking;
using AzureDevOpsDemoBuilder.Extensions;
using AzureDevOpsDemoBuilder.Models;
using AzureDevOpsDemoBuilder.ServiceInterfaces;
using AzureDevOpsRestApi.Git;
using AzureDevOpsRestApi.Viewmodel.ProjectAndTeams;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
//using GoogleAnalyticsTracker.Simple;
//using GoogleAnalyticsTracker.WebApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.ExtensionManagement.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static VstsDemoBuilder.Controllers.Apis.ProjectController;
using ClassificationNodes = AzureDevOpsAPI.WorkItemAndTracking.ClassificationNodes;

namespace AzureDevOpsDemoBuilder.Services
{
    public class ProjectService : IProjectService
    {
        public static readonly object objLock = new object();
        public static Dictionary<string, string> statusMessages;

        public bool isDefaultRepoTodetele = true;
        public string websiteUrl = string.Empty;
        public static string projectName = string.Empty;
        public static AccessDetails AccessDetails = new AccessDetails();

        public string templateVersion = string.Empty;
        public static string enableExtractor = "";

        public IHttpContextAccessor _httpContextAccessor;
        public IConfiguration AppKeyConfiguration { get; }
        public IWebHostEnvironment HostingEnvironment;
        public ILogger<ProjectService> logger;

        /*  public void TrackFeature(string API)
            {
                SimpleTrackerEnvironment simpleTrackerEnvironment = new SimpleTrackerEnvironment(Environment.OSVersion.Platform.ToString(),
                                                                            Environment.OSVersion.Version.ToString(),
                                                                            Environment.OSVersion.VersionString);
                string GAKey = AppKeyConfiguration["AnalyticsKey"];
                if (!string.IsNullOrEmpty(GAKey))
                {
                    using (Tracker tracker = new Tracker(GAKey, simpleTrackerEnvironment))
                    {
                        var request = _httpContextAccessor.HttpContext.Request;

                        var requestMessage = new HttpRequestMessage();
                        var requestMethod = request.Method;
                        if (!HttpMethods.IsGet(requestMethod) &&
                            !HttpMethods.IsHead(requestMethod) &&
                            !HttpMethods.IsDelete(requestMethod) &&
                            !HttpMethods.IsTrace(requestMethod))
                        {
                            var streamContent = new StreamContent(request.Body);
                            requestMessage.Content = streamContent;
                        }

                        // Copy the request headers
                        foreach (var header in request.Headers)
                        {
                            if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && requestMessage.Content != null)
                            {
                                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                            }
                        }


                        UriBuilder uriBuilder = new UriBuilder();
                        uriBuilder.Scheme = request.Scheme;
                        uriBuilder.Host = request.Host.Host;
                        uriBuilder.Path = request.Path.ToString();
                        uriBuilder.Query = request.QueryString.ToString();

                        var uri = uriBuilder.Uri;

                        requestMessage.Headers.Host = uri.Authority;
                        requestMessage.RequestUri = uri;
                        requestMessage.Method = new HttpMethod(request.Method);

                        var response = tracker.TrackPageViewAsync(requestMessage, API).Result;
                        bool issuccess = response.Success;
                    }
                }

            }  */
        public static Dictionary<string, string> StatusMessages
        {
            get
            {
                if (statusMessages == null)
                {
                    statusMessages = new Dictionary<string, string>();
                }

                return statusMessages;
            }
            set
            {
                statusMessages = value;
            }
        }
        public void AddMessage(string id, string message)
        {
            lock (objLock)
            {
                if (id.EndsWith("_Errors"))
                {
                    StatusMessages[id] = (StatusMessages.ContainsKey(id) ? StatusMessages[id] : string.Empty) + message;
                }
                else
                {
                    StatusMessages[id] = message;
                }
            }
        }
        public void RemoveKey(string id)
        {
            lock (objLock)
            {
                StatusMessages.Remove(id);
            }
        }
        public string GetStatusMessage(string id)
        {
            string status = string.Empty;
            lock (ProjectService.objLock)
            {
                string message = string.Empty;
                JObject obj = new JObject();
                if (id.EndsWith("_Errors"))
                {
                    //RemoveKey(id);
                    status = "Error: \t" + ProjectService.StatusMessages[id];
                }
                if (ProjectService.StatusMessages.Keys.Count(x => x == id) == 1)
                {
                    status = ProjectService.StatusMessages[id];
                }
                else
                {
                    status = "Successfully Created";

                }
                return status;
            }
        }

        public HttpResponseMessage GetprojectList(string accname, string pat)
        {
            string defaultHost = AppKeyConfiguration["DefaultHost"];
            string ProjectCreationVersion = AppKeyConfiguration["ProjectCreationVersion"];

            AppConfiguration config = new AppConfiguration() { AccountName = accname, PersonalAccessToken = pat, UriString = defaultHost + accname, VersionNumber = ProjectCreationVersion };
            Projects projects = new Projects(config);
            HttpResponseMessage response = projects.GetListOfProjects();
            return response;
        }

        /// <summary>
        /// Get the path where we can file template related json files for selected template
        /// </summary>
        /// <param name="TemplateFolder"></param>
        /// <param name="TemplateName"></param>
        /// <param name="FileName"></param>
        public string GetJsonFilePath(bool IsPrivate, string TemplateFolder, string TemplateName, string FileName = "")
        {
            string filePath = string.Empty;
            if (IsPrivate && !string.IsNullOrEmpty(TemplateFolder))
            {
                filePath = string.Format(TemplateFolder + "/{0}", FileName);
            }
            else
            {
                filePath = string.Format(HostingEnvironment.WebRootPath + "/Templates/" + "{0}/{1}", TemplateName, FileName);
            }
            return filePath;
        }

        #region Project Setup Operations

        /// <summary>
        /// start provisioning project - calls required
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pat"></param>
        /// <param name="accountName"></param>
        /// <returns></returns>
        public string[] CreateProjectEnvironment(Project model)
        {
            string accountName = model.AccountName;
            //if (model.IsPrivatePath)
            //{
            //    templateUsed = model.PrivateTemplateName;
            //}
            //else
            //{
            string templateUsed = model.SelectedTemplate;
            //}
            logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "Project Name: " + model.ProjectName + "\t Template Selected: " + templateUsed + "\t Organization Selected: " + accountName);
            string pat = model.AccessToken;
            //define versions to be use
            string projectCreationVersion = AppKeyConfiguration["ProjectCreationVersion"];
            string repoVersion = AppKeyConfiguration["RepoVersion"];
            string buildVersion = AppKeyConfiguration["BuildVersion"];
            string releaseVersion = AppKeyConfiguration["ReleaseVersion"];
            string wikiVersion = AppKeyConfiguration["WikiVersion"];
            string boardVersion = AppKeyConfiguration["BoardVersion"];
            string workItemsVersion = AppKeyConfiguration["WorkItemsVersion"];
            string queriesVersion = AppKeyConfiguration["QueriesVersion"];
            string endPointVersion = AppKeyConfiguration["EndPointVersion"];
            string extensionVersion = AppKeyConfiguration["ExtensionVersion"];
            string dashboardVersion = AppKeyConfiguration["DashboardVersion"];
            string agentQueueVersion = AppKeyConfiguration["AgentQueueVersion"];
            string getSourceCodeVersion = AppKeyConfiguration["GetSourceCodeVersion"];
            string testPlanVersion = AppKeyConfiguration["TestPlanVersion"];
            string releaseHost = AppKeyConfiguration["ReleaseHost"];
            string defaultHost = AppKeyConfiguration["DefaultHost"];
            string deploymentGroup = AppKeyConfiguration["DeloymentGroup"];
            string graphApiVersion = AppKeyConfiguration["GraphApiVersion"];
            string graphAPIHost = AppKeyConfiguration["GraphAPIHost"];
            string gitHubBaseAddress = AppKeyConfiguration["GitHubBaseAddress"];
            string variableGroupsApiVersion = AppKeyConfiguration["VariableGroupsApiVersion"];

            string processTemplateId = Default.SCRUM;
            model.Environment = new EnvironmentValues
            {
                ServiceEndpoints = new Dictionary<string, string>(),
                RepositoryIdList = new Dictionary<string, string>(),
                PullRequests = new Dictionary<string, string>(),
                GitHubRepos = new Dictionary<string, string>()
            };
            ProjectTemplate template = null;
            ProjectSettings settings = null;
            List<WiMapData> wiMapping = new List<WiMapData>();
            AccountMembers.Account accountMembers = new AccountMembers.Account();
            model.AccountUsersForWi = new List<string>();
            websiteUrl = model.WebsiteUrl;
            projectName = model.ProjectName;

            string logWIT = AppKeyConfiguration["LogWIT"];
            if (logWIT == "true")
            {
                string patBase64 = AppKeyConfiguration["PATBase64"];
                string url = AppKeyConfiguration["URL"];
                string projectId = AppKeyConfiguration["PROJECTID"];
                string reportName = string.Format("{0}", "AzureDevOps_Analytics-DemoGenerator");
                IssueWi objIssue = new IssueWi();
                objIssue.CreateReportWi(patBase64, "4.1", url, websiteUrl, reportName, "", templateUsed, projectId, model.Region);
            }

            AppConfiguration _gitHubConfig = new AppConfiguration() { GitBaseAddress = gitHubBaseAddress, GitCredential = model.GitHubToken, MediaType = "application/json", Scheme = "Bearer" };

            if (model.GitHubFork && model.GitHubToken != null)
            {
                GitHubImportRepo gitHubImport = new GitHubImportRepo(_gitHubConfig);
                HttpResponseMessage userResponse = gitHubImport.GetUserDetail();
                GitHubUserDetail userDetail = new GitHubUserDetail();
                if (userResponse.IsSuccessStatusCode)
                {
                    userDetail = JsonConvert.DeserializeObject<GitHubUserDetail>(userResponse.Content.ReadAsStringAsync().Result);
                    _gitHubConfig.UserName = userDetail.login;
                    model.GitHubUserName = userDetail.login;
                }
            }
            //configuration setup
            string _credentials = model.AccessToken;
            AppConfiguration _projectCreationVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = projectCreationVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _releaseVersion = new AppConfiguration() { UriString = releaseHost + accountName + "/", VersionNumber = releaseVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _buildVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = buildVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName, GitBaseAddress = gitHubBaseAddress, GitCredential = model.GitHubToken };
            AppConfiguration _workItemsVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = workItemsVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _queriesVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = queriesVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _boardVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = boardVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _wikiVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = wikiVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _endPointVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = endPointVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName, GitBaseAddress = gitHubBaseAddress, GitCredential = model.GitHubToken };
            AppConfiguration _extensionVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = extensionVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _dashboardVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = dashboardVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _repoVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = repoVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName, GitBaseAddress = gitHubBaseAddress, GitCredential = model.GitHubToken };

            AppConfiguration _getSourceCodeVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = getSourceCodeVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName, GitBaseAddress = gitHubBaseAddress, GitCredential = model.GitHubToken };
            AppConfiguration _agentQueueVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = agentQueueVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _testPlanVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = testPlanVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _deploymentGroup = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = deploymentGroup, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _graphApiVersion = new AppConfiguration() { UriString = graphAPIHost + accountName + "/", VersionNumber = graphApiVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _variableGroupApiVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = variableGroupsApiVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };

            string projTemplateFile = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "ProjectTemplate.json");
            string projectSettingsFile = string.Empty;
            string _checkIsPrivate = string.Empty;
            ProjectSetting setting = new ProjectSetting();
            if (File.Exists(projTemplateFile))
            {
                _checkIsPrivate = File.ReadAllText(projTemplateFile);
            }
            if (_checkIsPrivate != "")
            {
                setting = JsonConvert.DeserializeObject<ProjectSetting>(_checkIsPrivate);
            }

            //initialize project template and settings
            try
            {
                if (File.Exists(projTemplateFile))
                {
                    string templateItems = model.ReadJsonFile(projTemplateFile);
                    template = JsonConvert.DeserializeObject<ProjectTemplate>(templateItems);
                    projectSettingsFile = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.ProjectSettings);

                    if (File.Exists(projectSettingsFile))
                    {
                        settings = JsonConvert.DeserializeObject<ProjectSettings>(model.ReadJsonFile(projectSettingsFile));

                        if (!string.IsNullOrWhiteSpace(settings.Type))
                        {
                            if (settings.Type.ToLower() == TemplateType.Scrum.ToString().ToLower())
                            {
                                processTemplateId = Default.SCRUM;
                            }
                            else if (settings.Type.ToLower() == TemplateType.Agile.ToString().ToLower())
                            {
                                processTemplateId = Default.Agile;
                            }
                            else if (settings.Type.ToLower() == TemplateType.CMMI.ToString().ToLower())
                            {
                                processTemplateId = Default.CMMI;
                            }
                            else if (settings.Type.ToLower() == TemplateType.Basic.ToString().ToLower())
                            {
                                processTemplateId = Default.BASIC;
                            }
                            else if (!string.IsNullOrEmpty(settings.Id))
                            {
                                processTemplateId = settings.Id;
                            }
                            else
                            {
                                AddMessage(model.Id.ErrorId(), "Could not recognize process template. Make sure that the exported project template is belog to standard process template or project setting file has valid process template id.");
                                StatusMessages[model.Id] = "100";
                                return new string[] { model.Id, accountName, templateUsed };
                            }
                        }
                        else
                        {
                            settings.Type = "scrum";
                            processTemplateId = Default.SCRUM;
                        }
                    }
                }
                else
                {
                    AddMessage(model.Id.ErrorId(), "Project Template not found");
                    StatusMessages[model.Id] = "100";
                    return new string[] { model.Id, accountName, templateUsed };
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            //create team project
            string jsonProject = model.ReadJsonFile(HostingEnvironment.WebRootPath + "/Templates/" + "CreateProject.json");
            jsonProject = jsonProject.Replace("$projectName$", model.ProjectName).Replace("$processTemplateId$", processTemplateId);

            Projects proj = new Projects(_projectCreationVersion);
            string projectID = proj.CreateTeamProject(jsonProject);

            if (projectID == "-1")
            {
                if (!string.IsNullOrEmpty(proj.LastFailureMessage))
                {
                    if (proj.LastFailureMessage.Contains("TF400813"))
                    {
                        AddMessage(model.Id, "OAUTHACCESSDENIED");
                    }
                    else if (proj.LastFailureMessage.Contains("TF50309"))
                    {
                        AddMessage(model.Id.ErrorId(), proj.LastFailureMessage);
                    }
                    else
                    {
                        AddMessage(model.Id.ErrorId(), proj.LastFailureMessage);
                    }
                }
                Thread.Sleep(2000); // Adding Delay to Get Error message
                return new string[] { model.Id, accountName, templateUsed };
            }
            else
            {
                AddMessage(model.Id, string.Format("Project {0} created", model.ProjectName));
            }
            // waiting to add first message
            Thread.Sleep(2000);

            //Check for project state 
            Stopwatch watch = new Stopwatch();
            watch.Start();
            string projectStatus = string.Empty;
            Projects objProject = new Projects(_projectCreationVersion);
            while (projectStatus.ToLower() != "wellformed")
            {
                projectStatus = objProject.GetProjectStateByName(model.ProjectName);
                if (watch.Elapsed.Minutes >= 5)
                {
                    return new string[] { model.Id, accountName, templateUsed };
                }
            }
            watch.Stop();

            //get project id after successfull in VSTS
            model.Environment.ProjectId = objProject.GetProjectIdByName(model.ProjectName);
            model.Environment.ProjectName = model.ProjectName;

            // Fork Repo
            if (model.GitHubFork && model.GitHubToken != null)
            {
                ImportGitRepository(model, _gitHubConfig);
                //ForkGitHubRepository(model, _gitHubConfig);
            }

            //Add user as project admin
            bool isAdded = AddUserToProject(_graphApiVersion, model);
            if (isAdded)
            {
                AddMessage(model.Id, string.Format("Added user {0} as project admin ", model.Email));
            }

            //Install required extensions
            if (!model.IsApi && model.IsExtensionNeeded && model.IsAgreeTerms)
            {
                bool isInstalled = InstallExtensions(model, model.AccountName, model.AccessToken);
                if (isInstalled) { AddMessage(model.Id, "Required extensions are installed"); }
            }

            //current user Details
            string teamName = model.ProjectName + " team";
            TeamMemberResponse.TeamMembers teamMembers = GetTeamMembers(model.ProjectName, teamName, _projectCreationVersion, model.Id);

            var teamMember = teamMembers.Value != null ? teamMembers.Value.FirstOrDefault() : new TeamMemberResponse.Value();

            if (teamMember != null)
            {
                model.Environment.UserUniqueName = model.Environment.UserUniqueName ?? teamMember.Identity.UniqueName;
            }
            if (teamMember != null)
            {
                model.Environment.UserUniqueId = model.Environment.UserUniqueId ?? teamMember.Identity.Id;
            }
            //model.Environment.UserUniqueId = model.Email;
            //model.Environment.UserUniqueName = model.Email;
            //update board columns and rows
            // Checking for template version
            string projectTemplate = File.ReadAllText(GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "ProjectTemplate.json"));

            if (!string.IsNullOrEmpty(projectTemplate))
            {
                JObject jObject = JsonConvert.DeserializeObject<JObject>(projectTemplate);
                templateVersion = jObject["TemplateVersion"] == null ? string.Empty : jObject["TemplateVersion"].ToString();
            }
            if (templateVersion != "2.0")
            {
                AddMessage(model.Id, "Updated Iteration Dates");
                UpdateIterations(model, _boardVersion, "Iterations.json");
                //create teams
                CreateTeams(model, template.Teams, _projectCreationVersion, model.Id, template.TeamArea);

                // for older templates
                string projectSetting = File.ReadAllText(GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "ProjectSettings.json"));
                // File.ReadAllText( Path.Combine(templatesFolder + templateUsed, "ProjectSettings.json"));
                JObject projectObj = JsonConvert.DeserializeObject<JObject>(projectSetting);
                string processType = projectObj["type"] == null ? string.Empty : projectObj["type"].ToString();
                string boardType = string.Empty;
                if (processType == "" || processType == "Scrum")
                {
                    processType = "Scrum";
                    boardType = "Backlog%20items";
                }
                else if (processType == "Basic")
                {
                    boardType = "Issue";
                }
                else
                {
                    boardType = "Stories";
                }
                BoardColumn objBoard = new BoardColumn(_boardVersion);
                string updateSwimLanesJSON = "";
                if (template.BoardRows != null)
                {
                    updateSwimLanesJSON = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.BoardRows);
                    // Path.Combine(templatesFolder + templateUsed, template.BoardRows);
                    SwimLanes objSwimLanes = new SwimLanes(_boardVersion);
                    if (File.Exists(updateSwimLanesJSON))
                    {
                        updateSwimLanesJSON = File.ReadAllText(updateSwimLanesJSON);
                        bool isUpdated = objSwimLanes.UpdateSwimLanes(updateSwimLanesJSON, model.ProjectName, boardType, model.ProjectName + " Team");
                    }
                }
                if (template.SetEpic != null)
                {
                    string team = model.ProjectName + " Team";
                    string json = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.SetEpic);
                    if (File.Exists(json))
                    {
                        json = model.ReadJsonFile(json);
                        EnableEpic(model, json, _boardVersion, model.Id, team);
                    }
                }

                if (template.BoardColumns != null)
                {
                    string team = model.ProjectName + " Team";
                    string json = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.BoardColumns);
                    if (File.Exists(json))
                    {
                        json = model.ReadJsonFile(json);
                        bool success = UpdateBoardColumn(model, json, _boardVersion, model.Id, boardType, team);
                        if (success)
                        {
                            //update Card Fields
                            if (template.CardField != null)
                            {
                                string cardFieldJson = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.CardField);
                                if (File.Exists(cardFieldJson))
                                {
                                    cardFieldJson = model.ReadJsonFile(cardFieldJson);
                                    UpdateCardFields(model, cardFieldJson, _boardVersion, model.Id, boardType, model.ProjectName + " Team");
                                }
                            }
                            //Update card styles
                            if (template.CardStyle != null)
                            {
                                string cardStyleJson = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.CardStyle);
                                if (File.Exists(cardStyleJson))
                                {
                                    cardStyleJson = model.ReadJsonFile(cardStyleJson);
                                    UpdateCardStyles(model, cardStyleJson, _boardVersion, model.Id, boardType, model.ProjectName + " Team");
                                }
                            }
                            //Enable Epic Backlog
                            AddMessage(model.Id, "Board-Column, Swimlanes, Styles updated");
                        }
                    }
                }

                //update sprint dates
                UpdateSprintItems(model, _boardVersion, settings);
                RenameIterations(model, _boardVersion, settings.RenameIterations);
            }
            else
            {
                AddMessage(model.Id, "Updated Iteration Dates");
                UpdateIterations(model, _boardVersion, "Iterations.json");
                // for newer version of templates
                string teamsJsonPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "Teams\\Teams.json");
                // Path.Combine(templatesFolder + templateUsed, "Teams\\Teams.json");
                if (File.Exists(teamsJsonPath))
                {
                    template.Teams = "Teams\\Teams.json";
                    template.TeamArea = "TeamArea.json";
                    CreateTeams(model, template.Teams, _projectCreationVersion, model.Id, template.TeamArea);
                    string jsonTeams = model.ReadJsonFile(teamsJsonPath);
                    JArray jTeams = JsonConvert.DeserializeObject<JArray>(jsonTeams);
                    JContainer teamsParsed = JsonConvert.DeserializeObject<JContainer>(jsonTeams);
                    foreach (var jteam in jTeams)
                    {
                        string _teamName = string.Empty;
                        string isDefault = jteam["isDefault"] != null ? jteam["isDefault"].ToString() : string.Empty;
                        if (isDefault == "true")
                        {
                            _teamName = model.ProjectName + " Team";
                        }
                        else
                        {
                            _teamName = jteam["name"].ToString();
                        }
                        string teamFolderPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "/Teams/" + jteam["name"].ToString());
                        // Path.Combine(templatesFolder + templateUsed, "Teams", jteam["name"].ToString());
                        if (Directory.Exists(teamFolderPath))
                        {
                            BoardColumn objBoard = new BoardColumn(_boardVersion);

                            // updating swimlanes for each teams each board(epic, feature, PBI, Stories) 
                            string updateSwimLanesJSON = "";
                            SwimLanes objSwimLanes = new SwimLanes(_boardVersion);
                            template.BoardRows = "BoardRows.json";
                            updateSwimLanesJSON = Path.Combine(teamFolderPath, template.BoardRows);
                            if (File.Exists(updateSwimLanesJSON))
                            {
                                updateSwimLanesJSON = File.ReadAllText(updateSwimLanesJSON);
                                List<ImportBoardRows.Rows> importRows = JsonConvert.DeserializeObject<List<ImportBoardRows.Rows>>(updateSwimLanesJSON);
                                foreach (var board in importRows)
                                {
                                    bool isUpdated = objSwimLanes.UpdateSwimLanes(JsonConvert.SerializeObject(board.Value), model.ProjectName, board.BoardName, _teamName);
                                }
                            }

                            // updating team setting for each team
                            string teamSettingJson = "";
                            template.SetEpic = "TeamSetting.json";
                            teamSettingJson = Path.Combine(teamFolderPath, template.SetEpic);
                            if (File.Exists(teamSettingJson))
                            {
                                teamSettingJson = File.ReadAllText(teamSettingJson);
                                EnableEpic(model, teamSettingJson, _boardVersion, model.Id, _teamName);
                            }

                            // updating board columns for each teams each board
                            string teamBoardColumns = "";
                            template.BoardColumns = "BoardColumns.json";
                            teamBoardColumns = Path.Combine(teamFolderPath, template.BoardColumns);
                            if (File.Exists(teamBoardColumns))
                            {
                                teamBoardColumns = File.ReadAllText(teamBoardColumns);
                                List<ImportBoardColumns.ImportBoardCols> importBoardCols = JsonConvert.DeserializeObject<List<ImportBoardColumns.ImportBoardCols>>(teamBoardColumns);
                                foreach (var board in importBoardCols)
                                {
                                    bool success = UpdateBoardColumn(model, JsonConvert.SerializeObject(board.Value, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), _boardVersion, model.Id, board.BoardName, _teamName);
                                }
                            }

                            // updating card fields for each team and each board
                            string teamCardFields = "";
                            template.CardField = "CardFields.json";
                            teamCardFields = Path.Combine(teamFolderPath, template.CardField);
                            if (File.Exists(teamCardFields))
                            {
                                teamCardFields = File.ReadAllText(teamCardFields);
                                List<ImportCardFields.CardFields> cardFields = new List<ImportCardFields.CardFields>();
                                cardFields = JsonConvert.DeserializeObject<List<ImportCardFields.CardFields>>(teamCardFields);
                                foreach (var card in cardFields)
                                {
                                    UpdateCardFields(model, JsonConvert.SerializeObject(card, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), _boardVersion, model.Id, card.BoardName, _teamName);
                                }
                            }

                            // updating card styles for each team and each board
                            string teamCardStyle = "";
                            template.CardStyle = "CardStyles.json";
                            teamCardStyle = Path.Combine(teamFolderPath, template.CardStyle);
                            if (File.Exists(teamCardStyle))
                            {
                                teamCardStyle = File.ReadAllText(teamCardStyle);
                                List<CardStyle.Style> cardStyles = new List<CardStyle.Style>();
                                cardStyles = JsonConvert.DeserializeObject<List<CardStyle.Style>>(teamCardStyle);
                                foreach (var cardStyle in cardStyles)
                                {
                                    if (cardStyle.Rules.Fill != null)
                                    {
                                        UpdateCardStyles(model, JsonConvert.SerializeObject(cardStyle), _boardVersion, model.Id, cardStyle.BoardName, _teamName);
                                    }
                                }
                            }
                        }
                        AddMessage(model.Id, "Board-Column, Swimlanes, Styles updated");
                    }
                    UpdateSprintItems(model, _boardVersion, settings);
                    RenameIterations(model, _boardVersion, settings.RenameIterations);
                }
            }
            //Create Deployment Group
            //CreateDeploymentGroup(templatesFolder, model, _deploymentGroup);

            //create service endpoint
            List<string> listEndPointsJsonPath = new List<string>();
            string serviceEndPointsPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "/ServiceEndpoints");
            if (Directory.Exists(serviceEndPointsPath))
            {
                Directory.GetFiles(serviceEndPointsPath).ToList().ForEach(i => listEndPointsJsonPath.Add(i));
            }
            CreateServiceEndPoint(model, listEndPointsJsonPath, _endPointVersion);
            //create agent queues on demand
            Queue queue = new Queue(_agentQueueVersion);
            model.Environment.AgentQueues = queue.GetQueues();
            if (settings.Queues != null && settings.Queues.Count > 0)
            {
                foreach (string aq in settings.Queues)
                {
                    if (model.Environment.AgentQueues.ContainsKey(aq))
                    {
                        continue;
                    }

                    var id = queue.CreateQueue(aq);
                    if (id > 0)
                    {
                        model.Environment.AgentQueues[aq] = id;
                    }
                }
            }

            //import source code from GitHub
            List<string> listImportSourceCodeJsonPaths = new List<string>();
            string importSourceCodePath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "/ImportSourceCode/");
            if (Directory.Exists(importSourceCodePath))
            {
                Directory.GetFiles(importSourceCodePath).ToList().ForEach(i => listImportSourceCodeJsonPaths.Add(i));
                if (listImportSourceCodeJsonPaths.Contains(importSourceCodePath + "GitRepository.json"))
                {
                    listImportSourceCodeJsonPaths.Remove(importSourceCodePath + "GitRepository.json");
                }
            }
            foreach (string importSourceCode in listImportSourceCodeJsonPaths)
            {
                ImportSourceCode(model, importSourceCode, _repoVersion, model.Id, _getSourceCodeVersion);
            }
            if (isDefaultRepoTodetele)
            {
                Repository objRepository = new Repository(_repoVersion);
                string repositoryToDelete = objRepository.GetRepositoryToDelete(model.ProjectName);
                bool isDeleted = objRepository.DeleteRepository(repositoryToDelete);
            }

            //Create Pull request
            Thread.Sleep(10000); //Adding delay to wait for the repository to create and import from the source

            //Create WIKI
            //CreateProjetWiki(HostingEnvironment.WebRootPath + "/Templates/", model, _wikiVersion);
            //CreateCodeWiki(model, _wikiVersion);

            List<string> listPullRequestJsonPaths = new List<string>();
            string pullRequestFolder = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "/PullRequests");
            if (Directory.Exists(pullRequestFolder))
            {
                Directory.GetFiles(pullRequestFolder).ToList().ForEach(i => listPullRequestJsonPaths.Add(i));
            }
            foreach (string pullReq in listPullRequestJsonPaths)
            {
                CreatePullRequest(model, pullReq, _workItemsVersion);
            }

            //Configure account users
            if (model.UserMethod == "Select")
            {
                model.SelectedUsers = model.SelectedUsers.TrimEnd(',');
                model.AccountUsersForWi = model.SelectedUsers.Split(',').ToList();
            }
            else if (model.UserMethod == "Random")
            {
                //GetAccount Members
                AzureDevOpsAPI.ProjectsAndTeams.Accounts objAccount = new AzureDevOpsAPI.ProjectsAndTeams.Accounts(_projectCreationVersion);
                //accountMembers = objAccount.GetAccountMembers(accountName, AccessToken);
                foreach (var member in accountMembers.Value)
                {
                    model.AccountUsersForWi.Add(member.Member.MailAddress);
                }
            }
            Dictionary<string, string> workItems = new Dictionary<string, string>();

            if (templateVersion != "2.0")
            {

                //import work items
                string featuresFilePath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.FeaturefromTemplate == null ? string.Empty : template.FeaturefromTemplate);
                // Path.Combine(templatesFolder + templateUsed, template.FeaturefromTemplate == null ? string.Empty : template.FeaturefromTemplate);
                string productBackLogPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.PBIfromTemplate == null ? string.Empty : template.PBIfromTemplate);
                // Path.Combine(templatesFolder + templateUsed, template.PBIfromTemplate == null ? string.Empty : template.PBIfromTemplate);
                string taskPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.TaskfromTemplate == null ? string.Empty : template.TaskfromTemplate);
                // Path.Combine(templatesFolder + templateUsed, template.TaskfromTemplate == null ? string.Empty : template.TaskfromTemplate);
                string testCasePath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.TestCasefromTemplate == null ? string.Empty : template.TestCasefromTemplate);
                // Path.Combine(templatesFolder + templateUsed, template.TestCasefromTemplate == null ? string.Empty : template.TestCasefromTemplate);
                string bugPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.BugfromTemplate == null ? string.Empty : template.BugfromTemplate);
                // Path.Combine(templatesFolder + templateUsed, template.BugfromTemplate == null ? string.Empty : template.BugfromTemplate);
                string epicPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.EpicfromTemplate == null ? string.Empty : template.EpicfromTemplate);
                // Path.Combine(templatesFolder + templateUsed, template.EpicfromTemplate == null ? string.Empty : template.EpicfromTemplate);
                string userStoriesPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.UserStoriesFromTemplate == null ? string.Empty : template.UserStoriesFromTemplate);
                // Path.Combine(templatesFolder + templateUsed, template.UserStoriesFromTemplate == null ? string.Empty : template.UserStoriesFromTemplate);
                string testPlansPath = string.Empty;
                string testSuitesPath = string.Empty;
                if (templateUsed.ToLower() == "myshuttle2")
                {
                    testPlansPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.TestPlanfromTemplate);
                    // Path.Combine(templatesFolder + templateUsed, template.TestPlanfromTemplate);
                    testSuitesPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.TestSuitefromTemplate);
                    // Path.Combine(templatesFolder + templateUsed, template.TestSuitefromTemplate);
                }

                if (templateUsed.ToLower() == "myshuttle")
                {
                    testPlansPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.TestPlanfromTemplate);
                    // Path.Combine(templatesFolder + templateUsed, template.TestPlanfromTemplate);
                    testSuitesPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.TestSuitefromTemplate);
                    // Path.Combine(templatesFolder + templateUsed, template.TestSuitefromTemplate);
                }

                if (File.Exists(featuresFilePath))
                {
                    workItems.Add("Feature", model.ReadJsonFile(featuresFilePath));
                }

                if (File.Exists(productBackLogPath))
                {
                    workItems.Add("Product Backlog Item", model.ReadJsonFile(productBackLogPath));
                }

                if (File.Exists(taskPath))
                {
                    workItems.Add("Task", model.ReadJsonFile(taskPath));
                }

                if (File.Exists(testCasePath))
                {
                    workItems.Add("Test Case", model.ReadJsonFile(testCasePath));
                }

                if (File.Exists(bugPath))
                {
                    workItems.Add("Bug", model.ReadJsonFile(bugPath));
                }

                if (File.Exists(userStoriesPath))
                {
                    workItems.Add("User Story", model.ReadJsonFile(userStoriesPath));
                }

                if (File.Exists(epicPath))
                {
                    workItems.Add("Epic", model.ReadJsonFile(epicPath));
                }

                if (File.Exists(testPlansPath))
                {
                    workItems.Add("Test Plan", model.ReadJsonFile(testPlansPath));
                }

                if (File.Exists(testSuitesPath))
                {
                    workItems.Add("Test Suite", model.ReadJsonFile(testSuitesPath));
                }
            }
            //// Modified Work Item import logic
            else
            {
                string _WitPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "/WorkItems");
                //Path.Combine(templatesFolder + templateUsed + "\\WorkItems");
                if (Directory.Exists(_WitPath))
                {
                    string[] workItemFilePaths = Directory.GetFiles(_WitPath);
                    if (workItemFilePaths.Length > 0)
                    {
                        foreach (var workItem in workItemFilePaths)
                        {
                            string workItemName = Path.GetFileName(workItem);
                            string[] nameExtension = workItemName.Split('.');
                            string name = nameExtension[0];
                            if (!workItems.ContainsKey(name))
                            {
                                workItems.Add(name, model.ReadJsonFile(workItem));
                            }
                        }
                    }
                }
            }

            ImportWorkItems import = new ImportWorkItems(_workItemsVersion, model.Environment.BoardRowFieldName);
            if (File.Exists(projectSettingsFile))
            {
                AddMessage(model.Id, "Validating work item(s) definitions");
                string attchmentFilesFolder = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "/WorkItemAttachments");
                if (listPullRequestJsonPaths.Count > 0)
                {
                    if (templateUsed == "MyHealthClinic")
                    {
                        wiMapping = import.ImportWorkitems(workItems, model.ProjectName, model.Environment.UserUniqueName, model.ReadJsonFile(projectSettingsFile), attchmentFilesFolder, model.Environment.RepositoryIdList.ContainsKey("MyHealthClinic") ? model.Environment.RepositoryIdList["MyHealthClinic"] : string.Empty, model.Environment.ProjectId, model.Environment.PullRequests, model.UserMethod, model.AccountUsersForWi, templateUsed);
                    }
                    else if (templateUsed == "SmartHotel360")
                    {
                        wiMapping = import.ImportWorkitems(workItems, model.ProjectName, model.Environment.UserUniqueName, model.ReadJsonFile(projectSettingsFile), attchmentFilesFolder, model.Environment.RepositoryIdList.ContainsKey("PublicWeb") ? model.Environment.RepositoryIdList["PublicWeb"] : string.Empty, model.Environment.ProjectId, model.Environment.PullRequests, model.UserMethod, model.AccountUsersForWi, templateUsed);
                    }
                    else
                    {
                        wiMapping = import.ImportWorkitems(workItems, model.ProjectName, model.Environment.UserUniqueName, model.ReadJsonFile(projectSettingsFile), attchmentFilesFolder, model.Environment.RepositoryIdList.ContainsKey(templateUsed) ? model.Environment.RepositoryIdList[templateUsed] : string.Empty, model.Environment.ProjectId, model.Environment.PullRequests, model.UserMethod, model.AccountUsersForWi, templateUsed);
                    }
                }
                else
                {
                    wiMapping = import.ImportWorkitems(workItems, model.ProjectName, model.Environment.UserUniqueName, model.ReadJsonFile(projectSettingsFile), attchmentFilesFolder, string.Empty, model.Environment.ProjectId, model.Environment.PullRequests, model.UserMethod, model.AccountUsersForWi, templateUsed);
                }
                AddMessage(model.Id, "Work Items created");
            }
            //Creat TestPlans and TestSuites
            List<string> listTestPlansJsonPaths = new List<string>();
            string testPlansFolder = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "/TestPlans");
            if (Directory.Exists(testPlansFolder))
            {
                Directory.GetFiles(testPlansFolder).ToList().ForEach(i => listTestPlansJsonPaths.Add(i));
            }
            foreach (string testPlan in listTestPlansJsonPaths)
            {
                CreateTestManagement(wiMapping, model, testPlan, _testPlanVersion);
            }
            if (listTestPlansJsonPaths.Count > 0)
            {
                //AddMessage(model.Id, "TestPlans, TestSuites and TestCases created");
            }
            // create varibale groups

            CreateVaribaleGroups(model, _variableGroupApiVersion);

            //create build Definition
            string buildDefinitionsPath = string.Empty;
            model.BuildDefinitions = new List<BuildDef>();
            // if the template is private && agreed to GitHubFork && GitHub Token is not null
            if (setting.IsPrivate == "true" && model.GitHubFork && !string.IsNullOrEmpty(model.GitHubToken))
            {
                buildDefinitionsPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "/BuildDefinitions");
                if (Directory.Exists(buildDefinitionsPath))
                {
                    Directory.GetFiles(buildDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.BuildDefinitions.Add(new BuildDef() { FilePath = i }));
                }
                buildDefinitionsPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "/BuildDefinitionGitHub");
                if (Directory.Exists(buildDefinitionsPath))
                {
                    Directory.GetFiles(buildDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.BuildDefinitions.Add(new BuildDef() { FilePath = i }));
                }
            }
            // if the template is private && not agreed to GitHubFork && GitHub Token is null
            else if (setting.IsPrivate == "true" && !model.GitHubFork && string.IsNullOrEmpty(model.GitHubToken))
            {
                buildDefinitionsPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "/BuildDefinitions");
                if (Directory.Exists(buildDefinitionsPath))
                {
                    Directory.GetFiles(buildDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.BuildDefinitions.Add(new BuildDef() { FilePath = i }));
                }
            }
            // if the template is not private && agreed to GitHubFork && GitHub Token is not null
            else if (setting.IsPrivate == "false" && model.GitHubFork && !string.IsNullOrEmpty(model.GitHubToken))
            {
                buildDefinitionsPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "/BuildDefinitionGitHub");
                if (Directory.Exists(buildDefinitionsPath))
                {
                    Directory.GetFiles(buildDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.BuildDefinitions.Add(new BuildDef() { FilePath = i }));
                }
            }
            // if the template is not private && not agreed to GitHubFork && GitHub Token is null
            else if (setting.IsPrivate == "false" && !model.GitHubFork && string.IsNullOrEmpty(model.GitHubToken))
            {
                buildDefinitionsPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "/BuildDefinitions");
                if (Directory.Exists(buildDefinitionsPath))
                {
                    Directory.GetFiles(buildDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.BuildDefinitions.Add(new BuildDef() { FilePath = i }));
                }
            }
            if (model.BuildDefinitions.Count > 0)
            {
                bool isBuild = CreateBuildDefinition(model, _buildVersion, model.Id);
                if (isBuild)
                {
                    AddMessage(model.Id, "Build definition created");
                }
            }


            //Queue a Build
            string buildJson = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "QueueBuild.json");
            if (File.Exists(buildJson))
            {
                QueueABuild(model, buildJson, _buildVersion);
            }

            //create release Definition
            string releaseDefinitionsPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "/ReleaseDefinitions");
            model.ReleaseDefinitions = new List<ReleaseDef>();
            if (Directory.Exists(releaseDefinitionsPath))
            {
                Directory.GetFiles(releaseDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.ReleaseDefinitions.Add(new ReleaseDef() { FilePath = i }));
            }
            bool isReleased = CreateReleaseDefinition(model, _releaseVersion, model.Id, teamMembers);
            if (isReleased)
            {
                AddMessage(model.Id, "Release definition created");
            }

            //Create query and widgets
            List<string> listDashboardQueriesPath = new List<string>();
            string dashboardQueriesPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "/Dashboard/Queries");
            string dashboardPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "/Dashboard");

            if (Directory.Exists(dashboardQueriesPath))
            {
                Directory.GetFiles(dashboardQueriesPath).ToList().ForEach(i => listDashboardQueriesPath.Add(i));
            }
            if (Directory.Exists(dashboardPath))
            {
                CreateQueryAndWidgets(model, listDashboardQueriesPath, _queriesVersion, _dashboardVersion, _releaseVersion, _projectCreationVersion, _boardVersion);
                AddMessage(model.Id, "Queries, Widgets and Charts created");
            }

            StatusMessages[model.Id] = "100";
            return new string[] { model.Id, accountName, templateUsed };
        }
        private void ImportGitRepository(Project model, AppConfiguration _gitHubConfig)
        {
            List<string> listRepoFiles = new List<string>();
            string repoFilePath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "/ImportSourceCode/GitRepository.json");
            string createRepo = string.Format("{0}/{1}/{2}", HostingEnvironment.WebRootPath, "Templates", "CreateGitHubRepo.json");
            string readRepoFile = model.ReadJsonFile(repoFilePath);

            if (!string.IsNullOrEmpty(readRepoFile))
            {
                GitHubRepos.Fork forkRepos = new GitHubRepos.Fork();
                forkRepos = JsonConvert.DeserializeObject<GitHubRepos.Fork>(readRepoFile);
                if (forkRepos.Repositories != null && forkRepos.Repositories.Count > 0)
                {
                    foreach (var repo in forkRepos.Repositories)
                    {
                        string repoName = Path.GetFileName(repo.vcs_url);
                        string readCreateRepoFile = model.ReadJsonFile(createRepo).Replace("$NAME$", repoName);
                        GitHubImportRepo importRepo = new GitHubImportRepo(_gitHubConfig);
                        GitHubUserDetail userDetail = new GitHubUserDetail();
                        GitHubRepoResponse.RepoCreated GitHubRepo = new GitHubRepoResponse.RepoCreated();
                        var createRepoRes = importRepo.CreateRepo(readCreateRepoFile);
                        if (createRepoRes.IsSuccessStatusCode)
                        {
                            var importRepoRes = importRepo.ImportRepo(repoName, repo);
                            bool flag = false;
                            if (importRepoRes.IsSuccessStatusCode)
                            {
                                importStat:
                                var importStatusRes = importRepo.GetImportStatus(repoName);
                                var res = importStatusRes.Content.ReadAsStringAsync().Result;
                                ImportRepoResponse.Import importStatus = JsonConvert.DeserializeObject<ImportRepoResponse.Import>(res);
                                if (!flag)
                                {
                                    AddMessage(model.Id, "Importing repository, this may take some time. View status <a href='" + importStatus.html_url + "' target='_blank'>here</a>"); flag = true;
                                }
                                while (importStatus.status != "complete")
                                {
                                    goto importStat;
                                }
                                model.GitRepoURL = importStatus.repository_url;
                                model.GitRepoURL = model.GitRepoURL.Replace("api.", "").Replace("/repos", "/");

                                model.GitRepoName = repoName;
                                if (!model.Environment.GitHubRepos.ContainsKey(model.GitRepoName))
                                {
                                    model.Environment.GitHubRepos.Add(model.GitRepoName, model.GitRepoURL);
                                }
                                AddMessage(model.Id, string.Format("Imported GitHub repository", model.GitRepoName, _gitHubConfig.UserName));
                            }
                            else if (importRepoRes.StatusCode == System.Net.HttpStatusCode.Conflict)
                            {
                                AddMessage(model.Id, string.Format("Imported GitHub repository", model.GitRepoName = repoName, _gitHubConfig.UserName));
                                if (!model.Environment.GitHubRepos.ContainsKey(model.GitRepoName))
                                {
                                    model.Environment.GitHubRepos.Add(model.GitRepoName, model.GitRepoURL = string.Format("https://github.com/{0}/{1}", _gitHubConfig.UserName, model.GitRepoName));
                                }
                            }
                            else
                            {
                                var res = importRepoRes.Content.ReadAsStringAsync().Result;
                                AddMessage(model.Id.ErrorId(), res.ToString());
                            }
                        }
                    }
                }
            }
        }

        private void ForkGitHubRepository(Project model, AppConfiguration _gitHubConfig)
        {
            try
            {
                List<string> listRepoFiles = new List<string>();
                string repoFilePath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "/ImportSourceCode/GitRepository.json");
                if (File.Exists(repoFilePath))
                {
                    string readRepoFile = model.ReadJsonFile(repoFilePath);
                    if (!string.IsNullOrEmpty(readRepoFile))
                    {
                        GitHubRepos.Fork forkRepos = new GitHubRepos.Fork();
                        forkRepos = JsonConvert.DeserializeObject<GitHubRepos.Fork>(readRepoFile);
                        if (forkRepos.Repositories != null && forkRepos.Repositories.Count > 0)
                        {
                            foreach (var repo in forkRepos.Repositories)
                            {
                                GitHubImportRepo user = new GitHubImportRepo(_gitHubConfig);
                                GitHubUserDetail userDetail = new GitHubUserDetail();
                                GitHubRepoResponse.RepoCreated GitHubRepo = new GitHubRepoResponse.RepoCreated();
                                //HttpResponseMessage listForks = user.ListForks(repo.fullName);
                                HttpResponseMessage forkResponse = user.ForkRepo(repo.FullName);
                                if (forkResponse.IsSuccessStatusCode)
                                {
                                    string forkedRepo = forkResponse.Content.ReadAsStringAsync().Result;
                                    dynamic fr = JsonConvert.DeserializeObject<dynamic>(forkedRepo);
                                    model.GitRepoName = fr.name;
                                    model.GitRepoURL = fr.html_url;
                                    if (!model.Environment.GitHubRepos.ContainsKey(model.GitRepoName))
                                    {
                                        model.Environment.GitHubRepos.Add(model.GitRepoName, model.GitRepoURL);
                                    }
                                    AddMessage(model.Id, string.Format("Forked {0} repository to {1} user", model.GitRepoName, _gitHubConfig.UserName));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.Id.ErrorId(), "Error while forking repo: " + ex.Message);
            }
        }

        /// <summary>
        /// Create Teams
        /// </summary>
        /// <param name="model"></param>
        /// <param name="teamsJSON"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="id"></param>
        /// <param name="teamAreaJSON"></param>
        private void CreateTeams(Project model, string teamsJSON, AzureDevOpsAPI.AppConfiguration _projectConfig, string id, string teamAreaJSON)
        {
            try
            {
                string jsonTeams = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, teamsJSON);
                if (File.Exists(jsonTeams))
                {
                    Teams objTeam = new Teams(_projectConfig);
                    jsonTeams = model.ReadJsonFile(jsonTeams);
                    JArray jTeams = JsonConvert.DeserializeObject<JArray>(jsonTeams);
                    JContainer teamsParsed = JsonConvert.DeserializeObject<JContainer>(jsonTeams);

                    //get Backlog Iteration Id
                    string backlogIteration = objTeam.GetTeamSetting(model.ProjectName);
                    //get all Iterations
                    TeamIterationsResponse.Iterations iterations = objTeam.GetAllIterations(model.ProjectName);

                    foreach (var jTeam in jTeams)
                    {
                        string teamIterationMap = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "TeamIterationMap.json");
                        if (File.Exists(teamIterationMap))
                        {
                            //BEGIN - Mapping only given iterations for team in Team Iteration Mapping file
                            if (!string.IsNullOrEmpty(teamIterationMap))
                            {
                                string data = model.ReadJsonFile(teamIterationMap);
                                TeamIterations.Map iterationMap = new TeamIterations.Map();
                                iterationMap = JsonConvert.DeserializeObject<TeamIterations.Map>(data);
                                if (iterationMap.TeamIterationMap.Count > 0)
                                {
                                    foreach (var teamMap in iterationMap.TeamIterationMap)
                                    {
                                        if (teamMap.TeamName.ToLower() == jTeam["name"].ToString().ToLower())
                                        {
                                            // AS IS

                                            GetTeamResponse.Team teamResponse = objTeam.CreateNewTeam(jTeam.ToString(), model.ProjectName);
                                            if (!(string.IsNullOrEmpty(teamResponse.Id)))
                                            {
                                                string areaName = objTeam.CreateArea(model.ProjectName, teamResponse.Name);
                                                string updateAreaJSON = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, teamAreaJSON);

                                                //updateAreaJSON = string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, teamAreaJSON);

                                                if (File.Exists(updateAreaJSON))
                                                {
                                                    updateAreaJSON = model.ReadJsonFile(updateAreaJSON);
                                                    updateAreaJSON = updateAreaJSON.Replace("$ProjectName$", model.ProjectName).Replace("$AreaName$", areaName);
                                                    bool isUpdated = objTeam.SetAreaForTeams(model.ProjectName, teamResponse.Name, updateAreaJSON);
                                                }
                                                bool isBackLogIterationUpdated = objTeam.SetBackLogIterationForTeam(backlogIteration, model.ProjectName, teamResponse.Name);
                                                if (iterations.Count > 0)
                                                {
                                                    foreach (var iteration in iterations.Value)
                                                    {
                                                        if (iteration.structureType == "iteration")
                                                        {
                                                            foreach (var child in iteration.children)
                                                            {
                                                                if (teamMap.Iterations.Contains(child.name))
                                                                {
                                                                    bool isIterationUpdated = objTeam.SetIterationsForTeam(child.identifier, teamResponse.Name, model.ProjectName);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            // TILL HERE
                                        }
                                    }

                                }
                            }
                            // END
                        }
                        else
                        {
                            string isDefault = jTeam["isDefault"] != null ? jTeam["isDefault"].ToString() : string.Empty;
                            if (isDefault == "false" || isDefault == "")
                            {
                                GetTeamResponse.Team teamResponse = objTeam.CreateNewTeam(jTeam.ToString(), model.ProjectName);
                                if (!(string.IsNullOrEmpty(teamResponse.Id)))
                                {
                                    string areaName = objTeam.CreateArea(model.ProjectName, teamResponse.Name);
                                    string updateAreaJSON = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, teamAreaJSON);

                                    if (File.Exists(updateAreaJSON))
                                    {
                                        updateAreaJSON = model.ReadJsonFile(updateAreaJSON);
                                        updateAreaJSON = updateAreaJSON.Replace("$ProjectName$", model.ProjectName).Replace("$AreaName$", areaName);
                                        bool isUpdated = objTeam.SetAreaForTeams(model.ProjectName, teamResponse.Name, updateAreaJSON);
                                    }
                                    bool isBackLogIterationUpdated = objTeam.SetBackLogIterationForTeam(backlogIteration, model.ProjectName, teamResponse.Name);
                                    if (iterations.Count > 0)
                                    {
                                        foreach (var iteration in iterations.Value)
                                        {
                                            if (iteration.structureType == "iteration")
                                            {
                                                foreach (var child in iteration.children)
                                                {
                                                    bool isIterationUpdated = objTeam.SetIterationsForTeam(child.identifier, teamResponse.Name, model.ProjectName);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (!(string.IsNullOrEmpty(objTeam.LastFailureMessage)))
                            {
                                AddMessage(id.ErrorId(), "Error while creating teams: " + objTeam.LastFailureMessage + Environment.NewLine);
                            }
                            else
                            {
                                AddMessage(id, string.Format("{0} team(s) created", teamsParsed.Count));
                            }
                            if (model.SelectedTemplate.ToLower() == "smarthotel360")
                            {
                                string updateAreaJSON = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "UpdateTeamArea.json");

                                //updateAreaJSON = string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, "UpdateTeamArea.json");
                                if (File.Exists(updateAreaJSON))
                                {
                                    updateAreaJSON = model.ReadJsonFile(updateAreaJSON);
                                    updateAreaJSON = updateAreaJSON.Replace("$ProjectName$", model.ProjectName);
                                    bool isUpdated = objTeam.UpdateTeamsAreas(model.ProjectName, updateAreaJSON);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(id.ErrorId(), "Error while creating teams: " + ex.Message);

            }
        }

        /// <summary>
        /// Get Team members
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="teamName"></param>
        /// <param name="_configuration"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private TeamMemberResponse.TeamMembers GetTeamMembers(string projectName, string teamName, AzureDevOpsAPI.AppConfiguration _configuration, string id)
        {
            try
            {
                TeamMemberResponse.TeamMembers viewModel = new TeamMemberResponse.TeamMembers();
                AzureDevOpsAPI.ProjectsAndTeams.Teams objTeam = new AzureDevOpsAPI.ProjectsAndTeams.Teams(_configuration);
                viewModel = objTeam.GetTeamMembers(projectName, teamName);

                if (!(string.IsNullOrEmpty(objTeam.LastFailureMessage)))
                {
                    AddMessage(id.ErrorId(), "Error while getting team members: " + objTeam.LastFailureMessage + Environment.NewLine);
                }
                return viewModel;
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(id.ErrorId(), "Error while getting team members: " + ex.Message);
            }

            return new TeamMemberResponse.TeamMembers();
        }

        /// <summary>
        /// Create Work Items
        /// </summary>
        /// <param name="model"></param>
        /// <param name="workItemJSON"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="id"></param>
        private void CreateWorkItems(Project model, string workItemJSON, AzureDevOpsAPI.AppConfiguration _defaultConfiguration, string id)
        {
            try
            {
                string jsonWorkItems = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, workItemJSON);
                if (File.Exists(jsonWorkItems))
                {
                    WorkItem objWorkItem = new WorkItem(_defaultConfiguration);
                    jsonWorkItems = model.ReadJsonFile(jsonWorkItems);
                    JContainer workItemsParsed = JsonConvert.DeserializeObject<JContainer>(jsonWorkItems);

                    AddMessage(id, "Creating " + workItemsParsed.Count + " work items...");

                    jsonWorkItems = jsonWorkItems.Replace("$version$", _defaultConfiguration.VersionNumber);
                    bool workItemResult = objWorkItem.CreateWorkItemUsingByPassRules(model.ProjectName, jsonWorkItems);

                    if (!(string.IsNullOrEmpty(objWorkItem.LastFailureMessage)))
                    {
                        AddMessage(id.ErrorId(), "Error while creating workitems: " + objWorkItem.LastFailureMessage + Environment.NewLine);
                    }
                }

            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(id.ErrorId(), "Error while creating workitems: " + ex.Message);

            }
        }

        /// <summary>
        /// Update Board Columns styles
        /// </summary>
        /// <param name="model"></param>
        /// <param name="BoardColumnsJSON"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool UpdateBoardColumn(Project model, string BoardColumnsJSON, AzureDevOpsAPI.AppConfiguration _BoardConfig, string id, string BoardType, string team)
        {
            bool result = false;
            try
            {
                BoardColumn objBoard = new BoardColumn(_BoardConfig);
                bool boardColumnResult = objBoard.UpdateBoard(model.ProjectName, BoardColumnsJSON, BoardType, team);
                if (boardColumnResult)
                {
                    model.Environment.BoardRowFieldName = objBoard.RowFieldName;
                    result = true;
                }
                else if (!(string.IsNullOrEmpty(objBoard.LastFailureMessage)))
                {
                    AddMessage(id.ErrorId(), "Error while updating board column " + objBoard.LastFailureMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(id.ErrorId(), "Error while updating board column " + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Updates Card Fields
        /// </summary>
        /// <param name="model"></param>
        /// <param name="json"></param>
        /// <param name="_configuration"></param>
        /// <param name="id"></param>
        private void UpdateCardFields(Project model, string json, AppConfiguration _configuration, string id, string boardType, string team)
        {
            try
            {
                json = json.Replace("null", "\"\"");
                Cards objCards = new Cards(_configuration);
                objCards.UpdateCardField(model.ProjectName, json, boardType, team);

                if (!string.IsNullOrEmpty(objCards.LastFailureMessage))
                {
                    AddMessage(id.ErrorId(), "Error while updating card fields: " + objCards.LastFailureMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(id.ErrorId(), "Error while updating card fields: " + ex.Message);

            }

        }

        /// <summary>
        /// Udpate Card Styles
        /// </summary>
        /// <param name="model"></param>
        /// <param name="json"></param>
        /// <param name="_configuration"></param>
        /// <param name="id"></param>
        private void UpdateCardStyles(Project model, string json, AppConfiguration _configuration, string id, string boardType, string team)
        {
            try
            {
                Cards objCards = new Cards(_configuration);
                objCards.ApplyRules(model.ProjectName, json, boardType, team);

                if (!string.IsNullOrEmpty(objCards.LastFailureMessage))
                {
                    AddMessage(id.ErrorId(), "Error while updating card styles: " + objCards.LastFailureMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(id.ErrorId(), "Error while updating card styles: " + ex.Message);
            }

        }

        /// <summary>
        /// Enable Epic
        /// </summary>
        /// <param name="model"></param>
        /// <param name="json"></param>
        /// <param name="_config3_0"></param>
        /// <param name="id"></param>
        private void EnableEpic(Project model, string json, AzureDevOpsAPI.AppConfiguration _boardVersion, string id, string team)
        {
            try
            {
                Cards objCards = new Cards(_boardVersion);
                Projects project = new Projects(_boardVersion);
                objCards.EnablingEpic(model.ProjectName, json, model.ProjectName, team);

                if (!string.IsNullOrEmpty(objCards.LastFailureMessage))
                {
                    AddMessage(id.ErrorId(), "Error while Setting Epic Settings: " + objCards.LastFailureMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(id.ErrorId(), "Error while Setting Epic Settings: " + ex.Message);
            }

        }

        /// <summary>
        /// Updates work items with parent child links
        /// </summary>
        /// <param name="model"></param>
        /// <param name="workItemUpdateJSON"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="id"></param>
        /// <param name="currentUser"></param>
        /// <param name="projectSettingsJSON"></param>
        private void UpdateWorkItems(Project model, string workItemUpdateJSON, AzureDevOpsAPI.AppConfiguration _defaultConfiguration, string id, string currentUser, string projectSettingsJSON)
        {
            try
            {
                string jsonWorkItemsUpdate = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, workItemUpdateJSON);
                string jsonProjectSettings = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, projectSettingsJSON);
                if (File.Exists(jsonWorkItemsUpdate))
                {
                    WorkItem objWorkItem = new WorkItem(_defaultConfiguration);
                    jsonWorkItemsUpdate = model.ReadJsonFile(jsonWorkItemsUpdate);
                    jsonProjectSettings = model.ReadJsonFile(jsonProjectSettings);

                    bool workItemUpdateResult = objWorkItem.UpdateWorkItemUsingByPassRules(jsonWorkItemsUpdate, model.ProjectName, currentUser, jsonProjectSettings);
                    if (!(string.IsNullOrEmpty(objWorkItem.LastFailureMessage)))
                    {
                        AddMessage(id.ErrorId(), "Error while updating work items: " + objWorkItem.LastFailureMessage + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");

                AddMessage(id.ErrorId(), "Error while updating work items: " + ex.Message);

            }
        }

        /// <summary>
        /// Update Iterations
        /// </summary>
        /// <param name="model"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="iterationsJSON"></param>
        private void UpdateIterations(Project model, AzureDevOpsAPI.AppConfiguration _boardConfig, string iterationsJSON)
        {
            try
            {
                string jsonIterations = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, iterationsJSON);
                if (File.Exists(jsonIterations))
                {
                    iterationsJSON = model.ReadJsonFile(jsonIterations);
                    AzureDevOpsAPI.WorkItemAndTracking.ClassificationNodes objClassification = new AzureDevOpsAPI.WorkItemAndTracking.ClassificationNodes(_boardConfig);

                    GetNodesResponse.Nodes nodes = objClassification.GetIterations(model.ProjectName);

                    GetNodesResponse.Nodes projectNode = JsonConvert.DeserializeObject<GetNodesResponse.Nodes>(iterationsJSON);

                    if (projectNode.HasChildren)
                    {
                        foreach (var child in projectNode.Children)
                        {
                            CreateIterationNode(model, objClassification, child, nodes);
                        }
                    }

                    if (projectNode.HasChildren)
                    {
                        foreach (var child in projectNode.Children)
                        {
                            path = string.Empty;
                            MoveIterationNode(model, objClassification, child);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");

                AddMessage(model.Id.ErrorId(), "Error while updating iteration: " + ex.Message);
            }
        }

        /// <summary>
        /// Create Iterations
        /// </summary>
        /// <param name="model"></param>
        /// <param name="objClassification"></param>
        /// <param name="child"></param>
        /// <param name="currentIterations"></param>
        private void CreateIterationNode(Project model, AzureDevOpsAPI.WorkItemAndTracking.ClassificationNodes objClassification, GetNodesResponse.Child child, GetNodesResponse.Nodes currentIterations)
        {
            string[] defaultSprints = new string[] { "Sprint 1", "Sprint 2", "Sprint 3", "Sprint 4", "Sprint 5", "Sprint 6", };
            if (defaultSprints.Contains(child.Name))
            {
                var nd = (currentIterations.HasChildren) ? currentIterations.Children.FirstOrDefault(i => i.Name == child.Name) : null;
                if (nd != null)
                {
                    child.Id = nd.Id;
                }
            }
            else
            {
                var node = objClassification.CreateIteration(model.ProjectName, child.Name);
                child.Id = node.Id;
            }

            if (child.HasChildren && child.Children != null)
            {
                foreach (var c in child.Children)
                {
                    CreateIterationNode(model, objClassification, c, currentIterations);
                }
            }
        }

        private string path = string.Empty;

        public ProjectService(IConfiguration appKeyConfiguration, IWebHostEnvironment hostEnvironment, ILogger<ProjectService> _logger)
        {
            AppKeyConfiguration = appKeyConfiguration;
            HostingEnvironment = hostEnvironment;
            logger = _logger;
        }



        /// <summary>
        /// Move Iterations to nodes
        /// </summary>
        /// <param name="model"></param>
        /// <param name="objClassification"></param>
        /// <param name="child"></param>
        private void MoveIterationNode(Project model, AzureDevOpsAPI.WorkItemAndTracking.ClassificationNodes objClassification, GetNodesResponse.Child child)
        {
            if (child.HasChildren && child.Children != null)
            {
                foreach (var c in child.Children)
                {
                    path += child.Name + "\\";
                    var nd = objClassification.MoveIteration(model.ProjectName, path, c.Id);

                    if (c.HasChildren)
                    {
                        MoveIterationNode(model, objClassification, c);
                    }
                }
            }
        }

        /// <summary>
        /// Udpate Sprints dates
        /// </summary>
        /// <param name="model"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="settings"></param>
        private void UpdateSprintItems(Project model, AzureDevOpsAPI.AppConfiguration _boardConfig, ProjectSettings settings)
        {
            try
            {
                if (settings.Type.ToLower() == "scrum" || settings.Type.ToLower() == "agile" || settings.Type.ToLower() == "basic")
                {
                    string teamIterationMap = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "TeamIterationMap.json");

                    ClassificationNodes objClassification = new ClassificationNodes(_boardConfig);
                    bool classificationNodesResult = objClassification.UpdateIterationDates(model.ProjectName, settings.Type, model.SelectedTemplate, teamIterationMap);

                    if (!(string.IsNullOrEmpty(objClassification.LastFailureMessage)))
                    {
                        AddMessage(model.Id.ErrorId(), "Error while updating sprint items: " + objClassification.LastFailureMessage + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.Id.ErrorId(), "Error while updating sprint items: " + ex.Message);

            }
        }

        /// <summary>
        /// Rename Iterations
        /// </summary>
        /// <param name="model"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="renameIterations"></param>
        public void RenameIterations(Project model, AzureDevOpsAPI.AppConfiguration _defaultConfiguration, Dictionary<string, string> renameIterations)
        {
            try
            {
                if (renameIterations != null && renameIterations.Count > 0)
                {
                    AzureDevOpsAPI.WorkItemAndTracking.ClassificationNodes objClassification = new AzureDevOpsAPI.WorkItemAndTracking.ClassificationNodes(_defaultConfiguration);
                    bool IsRenamed = objClassification.RenameIteration(model.ProjectName, renameIterations);
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.Id.ErrorId(), "Error while renaming iterations: " + ex.Message);
            }
        }

        /// <summary>
        /// Import source code from sourec repo or GitHub
        /// </summary>
        /// <param name="model"></param>
        /// <param name="sourceCodeJSON"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="importSourceConfiguration"></param>
        /// <param name="id"></param>
        private void ImportSourceCode(Project model, string sourceCodeJSON, AzureDevOpsAPI.AppConfiguration _repo, string id, AzureDevOpsAPI.AppConfiguration _retSourceCodeVersion)
        {

            try
            {
                string[] repositoryDetail = new string[2];
                if (model.GitHubFork)
                {

                }
                if (File.Exists(sourceCodeJSON))
                {
                    Repository objRepository = new Repository(_repo);
                    string repositoryName = Path.GetFileName(sourceCodeJSON).Replace(".json", "");
                    if (model.ProjectName.ToLower() == repositoryName.ToLower())
                    {
                        repositoryDetail = objRepository.GetDefaultRepository(model.ProjectName);
                    }
                    else
                    {
                        repositoryDetail = objRepository.CreateRepository(repositoryName, model.Environment.ProjectId);
                    }
                    if (repositoryDetail.Length > 0)
                    {
                        model.Environment.RepositoryIdList[repositoryDetail[1]] = repositoryDetail[0];
                    }

                    string jsonSourceCode = model.ReadJsonFile(sourceCodeJSON);

                    //update endpoint ids
                    foreach (string endpoint in model.Environment.ServiceEndpoints.Keys)
                    {
                        string placeHolder = string.Format("${0}$", endpoint);
                        jsonSourceCode = jsonSourceCode.Replace(placeHolder, model.Environment.ServiceEndpoints[endpoint]);
                    }

                    Repository objRepositorySourceCode = new Repository(_retSourceCodeVersion);
                    bool copySourceCode = objRepositorySourceCode.GetSourceCodeFromGitHub(jsonSourceCode, model.ProjectName, repositoryDetail[0]);

                    if (!(string.IsNullOrEmpty(objRepository.LastFailureMessage)))
                    {
                        AddMessage(id.ErrorId(), "Error while importing source code: " + objRepository.LastFailureMessage + Environment.NewLine);
                    }
                }

            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(id.ErrorId(), "Error while importing source code: " + ex.Message);
            }
        }

        /// <summary>
        /// Creates pull request
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pullRequestJsonPath"></param>
        /// <param name="_configuration3_0"></param>
        private void CreatePullRequest(Project model, string pullRequestJsonPath, AzureDevOpsAPI.AppConfiguration _workItemConfig)
        {
            try
            {
                if (File.Exists(pullRequestJsonPath))
                {
                    string commentFile = Path.GetFileName(pullRequestJsonPath);
                    string repositoryId = string.Empty;
                    if (model.SelectedTemplate == "MyHealthClinic") { repositoryId = model.Environment.RepositoryIdList.ContainsKey("MyHealthClinic") ? model.Environment.RepositoryIdList["MyHealthClinic"] : string.Empty; }
                    if (model.SelectedTemplate == "SmartHotel360") { repositoryId = model.Environment.RepositoryIdList.ContainsKey("PublicWeb") ? model.Environment.RepositoryIdList["PublicWeb"] : string.Empty; }
                    else { repositoryId = model.Environment.RepositoryIdList[model.SelectedTemplate]; }

                    pullRequestJsonPath = model.ReadJsonFile(pullRequestJsonPath);
                    pullRequestJsonPath = pullRequestJsonPath.Replace("$reviewer$", model.Environment.UserUniqueId);
                    Repository objRepository = new Repository(_workItemConfig);

                    (string pullRequestId, string title) = objRepository.CreatePullRequest(pullRequestJsonPath, repositoryId);
                    if (!string.IsNullOrEmpty(pullRequestId) && !string.IsNullOrEmpty(title))
                    {
                        model.Environment.PullRequests.Add(pullRequestId, title);
                        commentFile = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "/PullRequests/Comments/" + commentFile);
                        if (File.Exists(commentFile))
                        {
                            commentFile = model.ReadJsonFile(commentFile);
                            PullRequestComments.Comments commentsList = JsonConvert.DeserializeObject<PullRequestComments.Comments>(commentFile);
                            if (commentsList.Count > 0)
                            {
                                foreach (PullRequestComments.Value thread in commentsList.Value)
                                {
                                    string threadID = objRepository.CreateCommentThread(repositoryId, title, JsonConvert.SerializeObject(thread));
                                    if (!string.IsNullOrEmpty(threadID))
                                    {
                                        if (thread.Replies != null && thread.Replies.Count > 0)
                                        {
                                            foreach (var reply in thread.Replies)
                                            {
                                                objRepository.AddCommentToThread(repositoryId, title, threadID, JsonConvert.SerializeObject(reply));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.Id.ErrorId(), "Error while creating pull Requests: " + ex.Message);
            }
        }

        /// <summary>
        /// Creates service end points
        /// </summary>
        /// <param name="model"></param>
        /// <param name="jsonPaths"></param>
        /// <param name="_defaultConfiguration"></param>
        private void CreateServiceEndPoint(Project model, List<string> jsonPaths, AppConfiguration _endpointConfig)
        {
            try
            {
                string serviceEndPointId = string.Empty;
                foreach (string jsonPath in jsonPaths)
                {
                    string fileName = Path.GetFileName(jsonPath);
                    string jsonCreateService = jsonPath;
                    if (File.Exists(jsonCreateService))
                    {
                        string username = AppKeyConfiguration["UserID"];
                        string password = AppKeyConfiguration["Password"];
                        //string extractPath = HostingEnvironment.MapPath("~/Templates/" + model.SelectedTemplate);
                        string projectFileData = File.ReadAllText(GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "ProjectTemplate.json"));
                        ProjectSetting settings = JsonConvert.DeserializeObject<ProjectSetting>(projectFileData);
                        ServiceEndPoint objService = new ServiceEndPoint(_endpointConfig);

                        string gitUserName = AppKeyConfiguration["GitUserName"];
                        string gitUserPassword = AppKeyConfiguration["GitUserPassword"];

                        jsonCreateService = model.ReadJsonFile(jsonCreateService);

                        if (!string.IsNullOrEmpty(settings.IsPrivate))
                        {
                            jsonCreateService = jsonCreateService.Replace("$ProjectName$", model.ProjectName);
                            jsonCreateService = jsonCreateService.Replace("$username$", model.Email).Replace("$password$", model.AccessToken);
                        }
                        // File contains "GitHub_" means - it contains GitHub URL, user wanted to fork repo to his github
                        if (fileName.Contains("GitHub_") && model.GitHubFork && model.GitHubToken != null)
                        {
                            JObject jsonToCreate = JObject.Parse(jsonCreateService);
                            string type = jsonToCreate["type"].ToString();
                            string url = jsonToCreate["url"].ToString();
                            string repoNameInUrl = Path.GetFileName(url);
                            // Endpoint type is Git(External Git), so we should point Build def to his repo by creating endpoint of Type GitHub(Public)
                            foreach (var repo in model.Environment.GitHubRepos.Keys)
                            {
                                if (repoNameInUrl.Contains(repo))
                                {
                                    if (type.ToLower() == "git")
                                    {
                                        jsonToCreate["type"] = "GitHub"; //Changing endpoint type
                                        jsonToCreate["url"] = model.Environment.GitHubRepos[repo].ToString(); // updating endpoint URL with User forked repo URL
                                    }
                                    // Endpoint type is GitHub(Public), so we should point the build def to his repo by updating the URL
                                    else if (type.ToLower() == "github")
                                    {
                                        jsonToCreate["url"] = model.Environment.GitHubRepos[repo].ToString(); // Updating repo URL to user repo
                                    }
                                    else
                                    {

                                    }
                                }
                            }
                            jsonCreateService = jsonToCreate.ToString();
                            jsonCreateService = jsonCreateService.Replace("$GitUserName$", model.GitHubUserName).Replace("$GitUserPassword$", model.GitHubToken);
                        }
                        // user doesn't want to fork repo
                        else
                        {
                            jsonCreateService = jsonCreateService.Replace("$ProjectName$", model.ProjectName); // Replaces the Place holder with project name if exists
                            jsonCreateService = jsonCreateService.Replace("$username$", username).Replace("$password$", password) // Replaces user name and password with app setting username and password if require[to import soure code to Azure Repos]
                                .Replace("$GitUserName$", gitUserName).Replace("$GitUserPassword$", gitUserPassword); // Replaces GitUser name and passwords with Demo gen username and password [Just to point build def to respective repo]
                        }
                        if (model.SelectedTemplate.ToLower() == "bikesharing360")
                        {
                            string bikeSharing360username = AppKeyConfiguration["UserID"];
                            string bikeSharing360password = AppKeyConfiguration["BikeSharing360Password"];
                            jsonCreateService = jsonCreateService.Replace("$BikeSharing360username$", bikeSharing360username).Replace("$BikeSharing360password$", bikeSharing360password);
                        }
                        else if (model.SelectedTemplate.ToLower() == "contososhuttle" || model.SelectedTemplate.ToLower() == "contososhuttle2")
                        {
                            string contosousername = AppKeyConfiguration["ContosoUserID"];
                            string contosopassword = AppKeyConfiguration["ContosoPassword"];
                            jsonCreateService = jsonCreateService.Replace("$ContosoUserID$", contosousername).Replace("$ContosoPassword$", contosopassword);
                        }
                        else if (model.SelectedTemplate.ToLower() == "sonarqube")
                        {
                            if (!string.IsNullOrEmpty(model.SonarQubeDNS))
                            {
                                jsonCreateService = jsonCreateService.Replace("$URL$", model.SonarQubeDNS);
                            }
                        }
                        else if (model.SelectedTemplate.ToLower() == "octopus")
                        {
                            var url = model.Parameters["OctopusURL"];
                            var apiKey = model.Parameters["APIkey"];
                            if (!string.IsNullOrEmpty(url.ToString()) && !string.IsNullOrEmpty(apiKey.ToString()))
                            {
                                jsonCreateService = jsonCreateService.Replace("$URL$", url).Replace("$Apikey$", apiKey);

                            }
                        }
                        var endpoint = objService.CreateServiceEndPoint(jsonCreateService, model.ProjectName);

                        if (!(string.IsNullOrEmpty(objService.LastFailureMessage)))
                        {
                            AddMessage(model.Id.ErrorId(), "Error while creating service endpoint: " + objService.LastFailureMessage + Environment.NewLine);
                        }
                        else
                        {
                            model.Environment.ServiceEndpoints[endpoint.Name] = endpoint.Id;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.Id.ErrorId(), "Error while creating service endpoint: " + ex.Message);
            }
        }

        /// <summary>
        /// Create Test Cases
        /// </summary>
        /// <param name="wiMapping"></param>
        /// <param name="model"></param>
        /// <param name="testPlanJson"></param>
        /// <param name="_defaultConfiguration"></param>
        private void CreateTestManagement(List<WiMapData> wiMapping, Project model, string testPlanJson, AzureDevOpsAPI.AppConfiguration _testPlanVersion)
        {
            try
            {
                if (File.Exists(testPlanJson))
                {
                    List<WiMapData> testCaseMap = new List<WiMapData>();
                    testCaseMap = wiMapping.Where(x => x.WiType == "Test Case").ToList();

                    string fileName = Path.GetFileName(testPlanJson);
                    testPlanJson = model.ReadJsonFile(testPlanJson);

                    testPlanJson = testPlanJson.Replace("$project$", model.ProjectName);
                    TestManagement objTest = new TestManagement(_testPlanVersion);
                    string[] testPlanResponse = new string[2];
                    testPlanResponse = objTest.CreateTestPlan(testPlanJson, model.ProjectName);

                    if (testPlanResponse.Length > 0)
                    {
                        string testSuiteJson = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "/TestPlans/TestSuites/" + fileName);
                        if (File.Exists(testSuiteJson))
                        {
                            testSuiteJson = model.ReadJsonFile(testSuiteJson);
                            testSuiteJson = testSuiteJson.Replace("$planID$", testPlanResponse[0]).Replace("$planName$", testPlanResponse[1]);
                            foreach (var wi in wiMapping)
                            {
                                string placeHolder = string.Format("${0}$", wi.OldId);
                                testSuiteJson = testSuiteJson.Replace(placeHolder, wi.NewId);
                            }
                            TestSuite.TestSuites listTestSuites = JsonConvert.DeserializeObject<TestSuite.TestSuites>(testSuiteJson);
                            if (listTestSuites.Count > 0)
                            {
                                foreach (var TS in listTestSuites.Value)
                                {
                                    string[] testSuiteResponse = new string[2];
                                    string testSuiteJSON = JsonConvert.SerializeObject(TS);
                                    testSuiteResponse = objTest.CreatTestSuite(testSuiteJSON, testPlanResponse[0], model.ProjectName);
                                    if (testSuiteResponse[0] != null && testSuiteResponse[1] != null)
                                    {
                                        string testCasesToAdd = string.Empty;
                                        foreach (string id in TS.TestCases)
                                        {
                                            foreach (var wiMap in testCaseMap)
                                            {
                                                if (wiMap.OldId == id)
                                                {
                                                    testCasesToAdd = testCasesToAdd + wiMap.NewId + ",";
                                                }
                                            }
                                        }
                                        testCasesToAdd = testCasesToAdd.TrimEnd(',');
                                        bool isTestCasesAddded = objTest.AddTestCasesToSuite(testCasesToAdd, testPlanResponse[0], testSuiteResponse[0], model.ProjectName);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.Id.ErrorId(), "Error while creating test plan and test suites: " + ex.Message);
            }
        }

        /// <summary>
        /// Creates Build Definitions
        /// </summary>
        /// <param name="model"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool CreateBuildDefinition(Project model, AzureDevOpsAPI.AppConfiguration _buildConfig, string id)
        {
            bool flag = false;
            try
            {
                foreach (BuildDef buildDef in model.BuildDefinitions)
                {
                    if (File.Exists(buildDef.FilePath))
                    {
                        BuildDefinition objBuild = new BuildDefinition(_buildConfig);
                        string jsonBuildDefinition = model.ReadJsonFile(buildDef.FilePath);
                        jsonBuildDefinition = jsonBuildDefinition.Replace("$ProjectName$", model.Environment.ProjectName)
                                             .Replace("$ProjectId$", model.Environment.ProjectId)
                                             .Replace("$username$", model.GitHubUserName).Replace("$reponame$", model.GitRepoName);

                        if (model.Environment.VariableGroups.Count > 0)
                        {
                            foreach (var vGroupsId in model.Environment.VariableGroups)
                            {
                                string placeHolder = string.Format("${0}$", vGroupsId.Value);
                                jsonBuildDefinition = jsonBuildDefinition.Replace(placeHolder, vGroupsId.Key.ToString());
                            }
                        }

                        //update repositoryId 
                        foreach (string repository in model.Environment.RepositoryIdList.Keys)
                        {
                            string placeHolder = string.Format("${0}$", repository);
                            jsonBuildDefinition = jsonBuildDefinition.Replace(placeHolder, model.Environment.RepositoryIdList[repository]);
                        }
                        //update endpoint ids
                        foreach (string endpoint in model.Environment.ServiceEndpoints.Keys)
                        {
                            string placeHolder = string.Format("${0}$", endpoint);
                            jsonBuildDefinition = jsonBuildDefinition.Replace(placeHolder, model.Environment.ServiceEndpoints[endpoint]);
                        }

                        (string buildId, string buildName) buildResult = objBuild.CreateBuildDefinition(jsonBuildDefinition, model.ProjectName, model.SelectedTemplate);

                        if (!(string.IsNullOrEmpty(objBuild.LastFailureMessage)))
                        {
                            AddMessage(id.ErrorId(), "Error while creating build definition: " + objBuild.LastFailureMessage + Environment.NewLine);
                        }
                        if (!string.IsNullOrEmpty(buildResult.buildId))
                        {
                            buildDef.Id = buildResult.buildId;
                            buildDef.Name = buildResult.buildName;
                        }
                    }
                    flag = true;
                }
                return flag;
            }

            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(id.ErrorId(), "Error while creating build definition: " + ex.Message);
            }
            return flag;
        }

        /// <summary>
        /// Queue build after provisioning project
        /// </summary>
        /// <param name="model"></param>
        /// <param name="json"></param>
        /// <param name="_configuration"></param>
        private void QueueABuild(Project model, string json, AzureDevOpsAPI.AppConfiguration _buildConfig)
        {
            try
            {
                string jsonQueueABuild = json;
                if (File.Exists(jsonQueueABuild))
                {
                    string buildId = model.BuildDefinitions.FirstOrDefault().Id;

                    jsonQueueABuild = model.ReadJsonFile(jsonQueueABuild);
                    jsonQueueABuild = jsonQueueABuild.Replace("$buildId$", buildId.ToString());
                    BuildDefinition objBuild = new BuildDefinition(_buildConfig);
                    int queueId = objBuild.QueueBuild(jsonQueueABuild, model.ProjectName);

                    if (!string.IsNullOrEmpty(objBuild.LastFailureMessage))
                    {
                        AddMessage(model.Id.ErrorId(), "Error while Queueing build: " + objBuild.LastFailureMessage + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.Id.ErrorId(), "Error while Queueing Build: " + ex.Message);
            }
        }

        /// <summary>
        /// Create Release Definitions
        /// </summary>
        /// <param name="model"></param>
        /// <param name="_releaseConfiguration"></param>
        /// <param name="_config3_0"></param>
        /// <param name="id"></param>
        /// <param name="teamMembers"></param>
        /// <returns></returns>
        private bool CreateReleaseDefinition(Project model, AzureDevOpsAPI.AppConfiguration _releaseConfiguration, string id, TeamMemberResponse.TeamMembers teamMembers)
        {
            bool flag = false;
            try
            {
                var teamMember = teamMembers.Value.FirstOrDefault();
                foreach (ReleaseDef relDef in model.ReleaseDefinitions)
                {
                    if (File.Exists(relDef.FilePath))
                    {
                        ReleaseDefinition objRelease = new ReleaseDefinition(_releaseConfiguration);
                        string jsonReleaseDefinition = model.ReadJsonFile(relDef.FilePath);
                        jsonReleaseDefinition = jsonReleaseDefinition.Replace("$ProjectName$", model.Environment.ProjectName)
                                             .Replace("$ProjectId$", model.Environment.ProjectId)
                                             .Replace("$OwnerUniqueName$", teamMember.Identity.UniqueName)
                                             .Replace("$OwnerId$", teamMember.Identity.Id)
                                  .Replace("$OwnerDisplayName$", teamMember.Identity.DisplayName);

                        if (model.Environment.VariableGroups.Count > 0)
                        {
                            foreach (var vGroupsId in model.Environment.VariableGroups)
                            {
                                string placeHolder = string.Format("${0}$", vGroupsId.Value);
                                jsonReleaseDefinition = jsonReleaseDefinition.Replace(placeHolder, vGroupsId.Key.ToString());
                            }
                        }
                        //Adding randon UUID to website name
                        string uuid = Guid.NewGuid().ToString();
                        uuid = uuid.Substring(0, 8);
                        jsonReleaseDefinition = jsonReleaseDefinition.Replace("$UUID$", uuid).Replace("$RandomNumber$", uuid).Replace("$AccountName$", model.AccountName); ;

                        //update agent queue ids
                        foreach (string queue in model.Environment.AgentQueues.Keys)
                        {
                            string placeHolder = string.Format("${0}$", queue);
                            jsonReleaseDefinition = jsonReleaseDefinition.Replace(placeHolder, model.Environment.AgentQueues[queue].ToString());
                        }

                        //update endpoint ids
                        foreach (string endpoint in model.Environment.ServiceEndpoints.Keys)
                        {
                            string placeHolder = string.Format("${0}$", endpoint);
                            jsonReleaseDefinition = jsonReleaseDefinition.Replace(placeHolder, model.Environment.ServiceEndpoints[endpoint]);
                        }

                        foreach (BuildDef objBuildDef in model.BuildDefinitions)
                        {
                            //update build ids
                            string placeHolder = string.Format("${0}-id$", objBuildDef.Name);
                            jsonReleaseDefinition = jsonReleaseDefinition.Replace(placeHolder, objBuildDef.Id);
                        }

                        (string releaseDefId, string releaseDefName) releaseDef = objRelease.CreateReleaseDefinition(jsonReleaseDefinition, model.ProjectName);
                        if (!(string.IsNullOrEmpty(objRelease.LastFailureMessage)))
                        {
                            if (objRelease.LastFailureMessage.TrimEnd() == "Tasks with versions 'ARM Outputs:3.*' are not valid for deploy job 'Function' in stage Azure-Dev.")
                            {
                                jsonReleaseDefinition = jsonReleaseDefinition.Replace("3.*", "4.*");
                                releaseDef = objRelease.CreateReleaseDefinition(jsonReleaseDefinition, model.ProjectName);
                                if (!string.IsNullOrWhiteSpace(releaseDef.releaseDefId))
                                {
                                    relDef.Id = releaseDef.releaseDefId;
                                    relDef.Name = releaseDef.releaseDefName;
                                }
                                if (!string.IsNullOrEmpty(relDef.Name))
                                {
                                    objRelease.LastFailureMessage = string.Empty;
                                }
                            }
                        }
                        relDef.Id = releaseDef.releaseDefId;
                        relDef.Name = releaseDef.releaseDefName;

                        if (!(string.IsNullOrEmpty(objRelease.LastFailureMessage)))
                        {
                            AddMessage(id.ErrorId(), "Error while creating release definition: " + objRelease.LastFailureMessage + Environment.NewLine);
                        }
                    }
                    flag = true;
                }
                return flag;
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(id.ErrorId(), "Error while creating release definition: " + ex.Message);
            }
            flag = false;
            return flag;
        }

        /// <summary>
        /// Dashboard set up operations
        /// </summary>
        /// <param name="model"></param>
        /// <param name="listQueries"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="_configuration2"></param>
        /// <param name="_configuration3"></param>
        /// <param name="releaseConfig"></param>
        public void CreateQueryAndWidgets(Project model, List<string> listQueries, AzureDevOpsAPI.AppConfiguration _queriesVersion, AzureDevOpsAPI.AppConfiguration _dashboardVersion, AzureDevOpsAPI.AppConfiguration _releaseConfig, AzureDevOpsAPI.AppConfiguration _projectConfig, AzureDevOpsAPI.AppConfiguration _boardConfig)
        {
            try
            {
                Queries objWidget = new Queries(_dashboardVersion);
                Queries objQuery = new Queries(_queriesVersion);
                List<QueryResponse> queryResults = new List<QueryResponse>();

                //GetDashBoardDetails
                string dashBoardId = objWidget.GetDashBoardId(model.ProjectName);
                Thread.Sleep(2000); // Adding delay to get the existing dashboard ID 

                if (!string.IsNullOrEmpty(objQuery.LastFailureMessage))
                {
                    AddMessage(model.Id.ErrorId(), "Error while getting dashboardId: " + objWidget.LastFailureMessage + Environment.NewLine);
                }

                foreach (string query in listQueries)
                {
                    Queries _newobjQuery = new Queries(_queriesVersion);

                    //create query
                    string json = model.ReadJsonFile(query);
                    json = json.Replace("$projectId$", model.Environment.ProjectName);
                    QueryResponse response = _newobjQuery.CreateQuery(model.ProjectName, json);
                    queryResults.Add(response);

                    if (!string.IsNullOrEmpty(_newobjQuery.LastFailureMessage))
                    {
                        AddMessage(model.Id.ErrorId(), "Error while creating query: " + _newobjQuery.LastFailureMessage + Environment.NewLine);
                    }

                }
                //Create DashBoards
                string dashBoardTemplate = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "/Dashboard/Dashboard.json");
                if (File.Exists(dashBoardTemplate))
                {
                    dynamic dashBoard = new System.Dynamic.ExpandoObject();
                    dashBoard.Name = "Working";
                    dashBoard.position = 4;

                    string jsonDashBoard = Newtonsoft.Json.JsonConvert.SerializeObject(dashBoard);
                    string dashBoardIdToDelete = objWidget.CreateNewDashBoard(model.ProjectName, jsonDashBoard);

                    bool isDashboardDeleted = objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardId);

                    if (model.SelectedTemplate.ToLower() == "bikesharing360")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);

                            string xamarin_DroidBuild = model.BuildDefinitions.Where(x => x.Name == "Xamarin.Droid").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "Xamarin.Droid").FirstOrDefault().Id : string.Empty;
                            string xamarin_IOSBuild = model.BuildDefinitions.Where(x => x.Name == "Xamarin.iOS").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "Xamarin.iOS").FirstOrDefault().Id : string.Empty;
                            string ridesApiBuild = model.BuildDefinitions.Where(x => x.Name == "RidesApi").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "RidesApi").FirstOrDefault().Id : string.Empty;

                            ReleaseDefinition objrelease = new ReleaseDefinition(_releaseConfig);
                            int[] androidEnvironmentIds = objrelease.GetEnvironmentIdsByName(model.ProjectName, "Xamarin.Android", "Test in HockeyApp", "Publish to store");
                            string androidbuildDefId = model.BuildDefinitions.Where(x => x.Name == "Xamarin.Droid").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "Xamarin.Droid").FirstOrDefault().Id : string.Empty;
                            string androidreleaseDefId = model.ReleaseDefinitions.Where(x => x.Name == "Xamarin.Android").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "Xamarin.Android").FirstOrDefault().Id : string.Empty;

                            int[] iosEnvironmentIds = objrelease.GetEnvironmentIdsByName(model.ProjectName, "Xamarin.iOS", "Test in HockeyApp", "Publish to store");
                            string iosBuildDefId = model.BuildDefinitions.Where(x => x.Name == "Xamarin.iOS").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "Xamarin.iOS").FirstOrDefault().Id : string.Empty;
                            string iosReleaseDefId = model.ReleaseDefinitions.Where(x => x.Name == "Xamarin.iOS").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "Xamarin.iOS").FirstOrDefault().Id : string.Empty;

                            string ridesApireleaseDefId = model.ReleaseDefinitions.Where(x => x.Name == "RidesApi").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "RidesApi").FirstOrDefault().Id : string.Empty;
                            QueryResponse openUserStories = objQuery.GetQueryByPathAndName(model.ProjectName, "Open User Stories", "Shared%20Queries/Current%20Iteration");

                            dashBoardTemplate = dashBoardTemplate.Replace("$RidesAPIReleaseId$", ridesApireleaseDefId)
                            .Replace("$RidesAPIBuildId$", ridesApiBuild)
                            .Replace("$repositoryId$", model.Environment.RepositoryIdList.Where(x => x.Key.ToLower() == "bikesharing360").FirstOrDefault().Value)
                            .Replace("$IOSBuildId$", iosBuildDefId).Replace("$IOSReleaseId$", iosReleaseDefId).Replace("$IOSEnv1$", iosEnvironmentIds[0].ToString()).Replace("$IOSEnv2$", iosEnvironmentIds[1].ToString())
                            .Replace("$Xamarin.iOS$", xamarin_IOSBuild)
                            .Replace("$Xamarin.Droid$", xamarin_DroidBuild)
                            .Replace("$AndroidBuildId$", androidbuildDefId).Replace("$AndroidreleaseDefId$", androidreleaseDefId).Replace("$AndroidEnv1$", androidEnvironmentIds[0].ToString()).Replace("$AndroidEnv2$", androidEnvironmentIds[1].ToString())
                            .Replace("$OpenUserStoriesId$", openUserStories.Id)
                            .Replace("$projectId$", model.Environment.ProjectId);

                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }
                    if (model.SelectedTemplate == "MyHealthClinic" || model.SelectedTemplate == "PartsUnlimited" || model.SelectedTemplate == "PartsUnlimited-agile")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);

                            QueryResponse feedBack = objQuery.GetQueryByPathAndName(model.ProjectName, "Feedback_WI", "Shared%20Queries");
                            QueryResponse unfinishedWork = objQuery.GetQueryByPathAndName(model.ProjectName, "Unfinished Work_WI", "Shared%20Queries");


                            dashBoardTemplate = dashBoardTemplate.Replace("$Feedback$", feedBack.Id).
                                         Replace("$AllItems$", queryResults.Where(x => x.Name == "All Items_WI").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "All Items_WI").FirstOrDefault().Id : string.Empty).
                                         Replace("$UserStories$", queryResults.Where(x => x.Name == "User Stories").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "User Stories").FirstOrDefault().Id : string.Empty).
                                         Replace("$TestCase$", queryResults.Where(x => x.Name == "Test Case-Readiness").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Test Case-Readiness").FirstOrDefault().Id : string.Empty).
                                         Replace("$teamID$", "").
                                         Replace("$teamName$", model.ProjectName + " Team").
                                         Replace("$projectID$", model.Environment.ProjectId).
                                         Replace("$Unfinished Work$", unfinishedWork.Id).
                                         Replace("$projectId$", model.Environment.ProjectId).
                                         Replace("$projectName$", model.ProjectName);


                            if (model.SelectedTemplate == "MyHealthClinic")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$ReleaseDefId$", model.ReleaseDefinitions.Where(x => x.Name == "MyHealthClinicE2E").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "MyHealthClinicE2E").FirstOrDefault().Id : string.Empty).
                                             Replace("$ActiveBugs$", queryResults.Where(x => x.Name == "Active Bugs_WI").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Active Bugs_WI").FirstOrDefault().Id : string.Empty).
                                             Replace("$MyHealthClinicE2E$", model.BuildDefinitions.Where(x => x.Name == "MyHealthClinicE2E").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "MyHealthClinicE2E").FirstOrDefault().Id : string.Empty).
                                                 Replace("$RepositoryId$", model.Environment.RepositoryIdList.Any(i => i.Key.ToLower().Contains("myhealthclinic")) ? model.Environment.RepositoryIdList.Where(x => x.Key.ToLower() == "myhealthclinic").FirstOrDefault().Value : string.Empty);
                            }
                            if (model.SelectedTemplate == "PartsUnlimited" || model.SelectedTemplate == "PartsUnlimited-agile")
                            {
                                QueryResponse workInProgress = objQuery.GetQueryByPathAndName(model.ProjectName, "Work in Progress_WI", "Shared%20Queries");

                                dashBoardTemplate = dashBoardTemplate.Replace("$ReleaseDefId$", model.ReleaseDefinitions.Where(x => x.Name == "PartsUnlimitedE2E").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "PartsUnlimitedE2E").FirstOrDefault().Id : string.Empty).
                                          Replace("$ActiveBugs$", queryResults.Where(x => x.Name == "Critical Bugs").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Critical Bugs").FirstOrDefault().Id : string.Empty).
                                          Replace("$PartsUnlimitedE2E$", model.BuildDefinitions.Where(x => x.Name == "PartsUnlimitedE2E").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "PartsUnlimitedE2E").FirstOrDefault().Id : string.Empty)
                                          .Replace("$WorkinProgress$", workInProgress.Id)
                                .Replace("$RepositoryId$", model.Environment.RepositoryIdList.Any(i => i.Key.ToLower().Contains("partsunlimited")) ? model.Environment.RepositoryIdList.Where(x => x.Key.ToLower() == "partsunlimited").FirstOrDefault().Value : string.Empty);

                            }
                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);

                        }
                    }
                    if (model.SelectedTemplate.ToLower() == "bikesharing 360")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);
                            QueryResponse unfinishedWork = objQuery.GetQueryByPathAndName(model.ProjectName, "Unfinished Work_WI", "Shared%20Queries");
                            string allItems = queryResults.Where(x => x.Name == "All Items_WI").FirstOrDefault().Id;
                            string repositoryId = model.Environment.RepositoryIdList.Where(x => x.Key.ToLower() == "bikesharing360").FirstOrDefault().Key;
                            string bikeSharing360_PublicWeb = model.BuildDefinitions.Where(x => x.Name == "BikeSharing360-PublicWeb").FirstOrDefault().Id;

                            dashBoardTemplate = dashBoardTemplate.Replace("$BikeSharing360-PublicWeb$", bikeSharing360_PublicWeb)
                                         .Replace("$All Items$", allItems)
                                         .Replace("$repositoryId$", repositoryId)
                                         .Replace("$Unfinished Work$", unfinishedWork.Id)
                                         .Replace("$projectId$", model.Environment.ProjectId);

                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }
                    if (model.SelectedTemplate == "MyShuttleDocker")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);
                            var buildDefId = model.BuildDefinitions.FirstOrDefault();
                            dashBoardTemplate = dashBoardTemplate.Replace("$BuildDefId$", buildDefId.Id)
                                  .Replace("$projectId$", model.Environment.ProjectId)
                                  .Replace("$PBI$", queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id != null ? queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id : string.Empty)
                                  .Replace("$Bugs$", queryResults.Where(x => x.Name == "Bugs").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Bugs").FirstOrDefault().Id : string.Empty)
                                  .Replace("$AllWorkItems$", queryResults.Where(x => x.Name == "All Work Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "All Work Items").FirstOrDefault().Id : string.Empty)
                                  .Replace("$Test Plan$", queryResults.Where(x => x.Name == "Test Plans").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Test Plans").FirstOrDefault().Id : string.Empty)
                                  .Replace("$Test Cases$", queryResults.Where(x => x.Name == "Test Cases").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Test Cases").FirstOrDefault().Id : string.Empty)
                                  .Replace("$Feature$", queryResults.Where(x => x.Name == "Feature").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Feature").FirstOrDefault().Id : string.Empty)
                                  .Replace("$Tasks$", queryResults.Where(x => x.Name == "Tasks").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Tasks").FirstOrDefault().Id : string.Empty)
                                         .Replace("$RepoMyShuttleDocker$", model.Environment.RepositoryIdList.Where(x => x.Key == "MyShuttleDocker").FirstOrDefault().ToString() != "" ? model.Environment.RepositoryIdList.Where(x => x.Key == "MyShuttleDocker").FirstOrDefault().Value : string.Empty);


                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }
                    if (model.SelectedTemplate == "MyShuttle")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);
                            dashBoardTemplate = dashBoardTemplate
                            .Replace("$PBI$", queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id : string.Empty)
                            .Replace("$Bugs$", queryResults.Where(x => x.Name == "Bugs").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Bugs").FirstOrDefault().Id : string.Empty)
                            .Replace("$AllWorkItems$", queryResults.Where(x => x.Name == "All Work Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "All Work Items").FirstOrDefault().Id : string.Empty)
                            .Replace("$TestPlan$", queryResults.Where(x => x.Name == "Test Plans").FirstOrDefault().Id != null ? queryResults.Where(x => x.Name == "Test Plans").FirstOrDefault().Id : string.Empty)
                            .Replace("$Test Cases$", queryResults.Where(x => x.Name == "Test Cases").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Test Cases").FirstOrDefault().Id : string.Empty)
                            .Replace("$Features$", queryResults.Where(x => x.Name == "Feature").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Feature").FirstOrDefault().Id : string.Empty)
                            .Replace("$Tasks$", queryResults.Where(x => x.Name == "Tasks").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Tasks").FirstOrDefault().Id : string.Empty)
                            .Replace("$TestSuite$", queryResults.Where(x => x.Name == "Test Suites").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Test Suites").FirstOrDefault().Id : string.Empty);


                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }
                    if (model.SelectedTemplate.ToLower() == "myshuttle2")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);

                            dashBoardTemplate = dashBoardTemplate.Replace("$TestCases$", queryResults.Where(x => x.Name == "Test Cases").FirstOrDefault().Id != null ? queryResults.Where(x => x.Name == "Test Cases").FirstOrDefault().Id : string.Empty)
                                         .Replace("$AllWorkItems$", queryResults.Where(x => x.Name == "All Work Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "All Work Items").FirstOrDefault().Id : string.Empty)
                                         .Replace("$PBI$", queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id : string.Empty)
                                         .Replace("$RepoMyShuttleCalc$", model.Environment.RepositoryIdList["MyShuttleCalc"] != null ? model.Environment.RepositoryIdList["MyShuttleCalc"] : string.Empty)
                                         .Replace("$TestPlan$", queryResults.Where(x => x.Name == "Test Plans").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Test Plans").FirstOrDefault().Id : string.Empty)
                                         .Replace("$Tasks$", queryResults.Where(x => x.Name == "Tasks").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Tasks").FirstOrDefault().Id : string.Empty)
                                         .Replace("$Bugs$", queryResults.Where(x => x.Name == "Bugs").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Bugs").FirstOrDefault().Id : string.Empty)
                                         .Replace("$Features$", queryResults.Where(x => x.Name == "Feature").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Feature").FirstOrDefault().Id : string.Empty)
                                         .Replace("$RepoMyShuttle2$", model.Environment.RepositoryIdList.Where(x => x.Key.ToLower() == "myshuttle2").FirstOrDefault().ToString() != "" ? model.Environment.RepositoryIdList.Where(x => x.Key.ToLower() == "myshuttle2").FirstOrDefault().Value : string.Empty);


                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }
                    if (model.SelectedTemplate.ToLower() == "docker" || model.SelectedTemplate.ToLower() == "php" || model.SelectedTemplate.ToLower() == "sonarqube" || model.SelectedTemplate.ToLower() == "github" || model.SelectedTemplate.ToLower() == "whitesource bolt" || model.SelectedTemplate == "DeploymentGroups" || model.SelectedTemplate == "Octopus")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);
                            dashBoardTemplate = dashBoardTemplate.Replace("$Task$", queryResults.Where(x => x.Name == "Tasks").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Tasks").FirstOrDefault().Id : string.Empty)
                                         .Replace("$AllWorkItems$", queryResults.Where(x => x.Name == "All Work Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "All Work Items").FirstOrDefault().Id : string.Empty)
                                         .Replace("$Feature$", queryResults.Where(x => x.Name == "Feature").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Feature").FirstOrDefault().Id : string.Empty)
                                         .Replace("$Projectid$", model.Environment.ProjectId)
                                         .Replace("$Epic$", queryResults.Where(x => x.Name == "Epics").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Epics").FirstOrDefault().Id : string.Empty);

                            if (model.SelectedTemplate.ToLower() == "docker")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$BuildDocker$", model.BuildDefinitions.Where(x => x.Name == "MHCDocker.build").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "MHCDocker.build").FirstOrDefault().Id : string.Empty)
                                .Replace("$ReleaseDocker$", model.ReleaseDefinitions.Where(x => x.Name == "MHCDocker.release").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "MHCDocker.release").FirstOrDefault().Id : string.Empty)
                                  .Replace("$PBI$", queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id : string.Empty);
                            }
                            else if (model.SelectedTemplate.ToLower() == "php")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$buildPHP$", model.BuildDefinitions.Where(x => x.Name == "PHP").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "PHP").FirstOrDefault().Id : string.Empty)
                        .Replace("$releasePHP$", model.ReleaseDefinitions.Where(x => x.Name == "PHP").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "PHP").FirstOrDefault().Id : string.Empty)
                                 .Replace("$PBI$", queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id : string.Empty);
                            }
                            else if (model.SelectedTemplate.ToLower() == "sonarqube")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$BuildSonarQube$", model.BuildDefinitions.Where(x => x.Name == "SonarQube").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "SonarQube").FirstOrDefault().Id : string.Empty)
                                .Replace("$PBI$", queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id : string.Empty);

                            }
                            else if (model.SelectedTemplate.ToLower() == "github")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$PBI$", queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id : string.Empty)
                                             .Replace("$buildGitHub$", model.BuildDefinitions.Where(x => x.Name == "GitHub").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "GitHub").FirstOrDefault().Id : string.Empty)
                                             .Replace("$Hosted$", model.Environment.AgentQueues["Hosted"].ToString())
                                             .Replace("$releaseGitHub$", model.ReleaseDefinitions.Where(x => x.Name == "GitHub").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "GitHub").FirstOrDefault().Id : string.Empty);

                            }
                            else if (model.SelectedTemplate.ToLower() == "whitesource bolt")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$PBI$", queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id : string.Empty)
                                          .Replace("$buildWhiteSource$", model.BuildDefinitions.Where(x => x.Name == "WhiteSourceBolt").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "WhiteSourceBolt").FirstOrDefault().Id : string.Empty);
                            }

                            else if (model.SelectedTemplate == "DeploymentGroups")
                            {
                                QueryResponse WorkInProgress = objQuery.GetQueryByPathAndName(model.ProjectName, "Work in Progress_WI", "Shared%20Queries");
                                dashBoardTemplate = dashBoardTemplate.Replace("$WorkinProgress$", WorkInProgress.Id);
                            }

                            else if (model.SelectedTemplate == "Octopus")
                            {
                                var BuildDefId = model.BuildDefinitions.FirstOrDefault();
                                if (BuildDefId != null)
                                {
                                    dashBoardTemplate = dashBoardTemplate.Replace("$BuildDefId$", BuildDefId.Id)
                                            .Replace("$PBI$", queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id : string.Empty);
                                }
                            }


                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }

                    if (model.SelectedTemplate.ToLower() == "smarthotel360")
                    {
                        if (isDashboardDeleted)
                        {
                            string startdate = DateTime.Now.ToString("yyyy-MM-dd");
                            AzureDevOpsAPI.ProjectsAndTeams.Teams objTeam = new AzureDevOpsAPI.ProjectsAndTeams.Teams(_projectConfig);
                            TeamResponse defaultTeam = objTeam.GetTeamByName(model.ProjectName, model.ProjectName + " team");
                            AzureDevOpsAPI.WorkItemAndTracking.ClassificationNodes objnodes = new AzureDevOpsAPI.WorkItemAndTracking.ClassificationNodes(_boardConfig);
                            SprintResponse.Sprints sprints = objnodes.GetSprints(model.ProjectName);
                            QueryResponse allItems = objQuery.GetQueryByPathAndName(model.ProjectName, "All Items_WI", "Shared%20Queries");
                            QueryResponse backlogBoardWI = objQuery.GetQueryByPathAndName(model.ProjectName, "BacklogBoard WI", "Shared%20Queries");
                            QueryResponse boardWI = objQuery.GetQueryByPathAndName(model.ProjectName, "Board WI", "Shared%20Queries");
                            QueryResponse bugsWithoutReproSteps = objQuery.GetQueryByPathAndName(model.ProjectName, "Bugs without Repro Steps", "Shared%20Queries");
                            QueryResponse feedback = objQuery.GetQueryByPathAndName(model.ProjectName, "Feedback_WI", "Shared%20Queries");
                            QueryResponse mobileTeamWork = objQuery.GetQueryByPathAndName(model.ProjectName, "MobileTeam_Work", "Shared%20Queries");
                            QueryResponse webTeamWork = objQuery.GetQueryByPathAndName(model.ProjectName, "WebTeam_Work", "Shared%20Queries");
                            QueryResponse stateofTestCase = objQuery.GetQueryByPathAndName(model.ProjectName, "State of TestCases", "Shared%20Queries");
                            QueryResponse bugs = objQuery.GetQueryByPathAndName(model.ProjectName, "Open Bugs_WI", "Shared%20Queries");

                            QueryResponse unfinishedWork = objQuery.GetQueryByPathAndName(model.ProjectName, "Unfinished Work_WI", "Shared%20Queries");
                            QueryResponse workInProgress = objQuery.GetQueryByPathAndName(model.ProjectName, "Work in Progress_WI", "Shared%20Queries");
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);
                            dashBoardTemplate = dashBoardTemplate.Replace("$WorkinProgress$", workInProgress.Id)
                                .Replace("$projectId$", model.Environment.ProjectId != null ? model.Environment.ProjectId : string.Empty)
                                .Replace("$PublicWebBuild$", model.BuildDefinitions.Where(x => x.Name == "SmartHotel_Petchecker-Web").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "SmartHotel_Petchecker-Web").FirstOrDefault().Id : string.Empty)
                                .Replace("$DefaultTeamId$", defaultTeam.Id != null ? defaultTeam.Id : string.Empty).Replace("$AllItems$", allItems.Id != null ? allItems.Id : string.Empty)
                                .Replace("$BacklogBoardWI$", backlogBoardWI.Id != null ? backlogBoardWI.Id : string.Empty)
                                .Replace("$StateofTestCases$", stateofTestCase.Id != null ? stateofTestCase.Id : string.Empty)
                                .Replace("$Feedback$", feedback.Id != null ? feedback.Id : string.Empty)
                                .Replace("$RepoPublicWeb$", model.Environment.RepositoryIdList.ContainsKey("PublicWeb") ? model.Environment.RepositoryIdList["PublicWeb"] : string.Empty)
                                .Replace("$MobileTeamWork$", mobileTeamWork.Id != null ? mobileTeamWork.Id : string.Empty).Replace("$WebTeamWork$", webTeamWork.Id != null ? webTeamWork.Id : string.Empty)
                                .Replace("$Bugs$", bugs.Id != null ? bugs.Id : string.Empty)
                                .Replace("$sprint2$", sprints.Value.Where(x => x.Name == "Sprint 2").FirstOrDefault() != null ? sprints.Value.Where(x => x.Name == "Sprint 2").FirstOrDefault().Id : string.Empty)
                                .Replace("$sprint3$", sprints.Value.Where(x => x.Name == "Sprint 3").FirstOrDefault() != null ? sprints.Value.Where(x => x.Name == "Sprint 3").FirstOrDefault().Id : string.Empty)
                                .Replace("$startDate$", startdate)
                                .Replace("$BugswithoutRepro$", bugsWithoutReproSteps.Id != null ? bugsWithoutReproSteps.Id : string.Empty).Replace("$UnfinishedWork$", unfinishedWork.Id != null ? unfinishedWork.Id : string.Empty)
                                .Replace("$RepoSmartHotel360$", model.Environment.RepositoryIdList.ContainsKey("SmartHotel360") ? model.Environment.RepositoryIdList["SmartHotel360"] : string.Empty)
                                .Replace("$PublicWebSiteCD$", model.ReleaseDefinitions.Where(x => x.Name == "PublicWebSiteCD").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "PublicWebSiteCD").FirstOrDefault().Id : string.Empty);

                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);

                        }
                    }
                    if (model.SelectedTemplate.ToLower() == "contososhuttle" || model.SelectedTemplate.ToLower() == "contososhuttle2")
                    {
                        if (isDashboardDeleted)
                        {
                            QueryResponse workInProgress = objQuery.GetQueryByPathAndName(model.ProjectName, "Work in Progress_WI", "Shared%20Queries");
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);
                            dashBoardTemplate = dashBoardTemplate.Replace("$WorkinProgress$", workInProgress.Id);

                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }
                }
            }
            catch (OperationCanceledException oce)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + oce.Message + "\t" + oce.InnerException.Message + "\n" + oce.StackTrace + "\n");
                AddMessage(model.Id.ErrorId(), "Error while creating Queries and Widgets: Operation cancelled exception " + oce.Message + "\r\n");
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.Id.ErrorId(), "Error while creating Queries and Widgets: " + ex.Message);
            }
        }

        public bool InstallExtensions(Project model, string accountName, string PAT)
        {
            try
            {
                string projTemplateFile = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "Extensions.json");
                if (!(File.Exists(projTemplateFile)))
                {
                    return false;
                }
                string templateItems = File.ReadAllText(projTemplateFile);
                var template = JsonConvert.DeserializeObject<RequiredExtensions.Extension>(templateItems);
                string requiresExtensionNames = string.Empty;

                //Check for existing extensions
                if (template.Extensions.Count > 0)
                {
                    Dictionary<string, bool> dict = new Dictionary<string, bool>();
                    foreach (RequiredExtensions.ExtensionWithLink ext in template.Extensions)
                    {
                        if (!dict.ContainsKey(ext.ExtensionName))
                        {
                            dict.Add(ext.ExtensionName, false);
                        }
                    }
                    //var connection = new VssConnection(new Uri(string.Format("https://{0}.visualstudio.com", accountName)), new Microsoft.VisualStudio.Services.OAuth.VssOAuthAccessTokenCredential(PAT));// VssOAuthCredential(PAT));
                    var connection = new VssConnection(new Uri(string.Format("https://{0}.visualstudio.com", accountName)), new VssBasicCredential(string.Empty, PAT));// VssOAuthCredential(PAT));

                    var client = connection.GetClient<ExtensionManagementHttpClient>();
                    var installed = client.GetInstalledExtensionsAsync().Result;
                    var extensions = installed.Where(x => x.Flags == 0).ToList();

                    var trustedFlagExtensions = installed.Where(x => x.Flags == ExtensionFlags.Trusted).ToList();
                    var builtInExtensions = installed.Where(x => x.Flags.ToString() == "BuiltIn, Trusted").ToList();
                    extensions.AddRange(trustedFlagExtensions);
                    extensions.AddRange(builtInExtensions);

                    foreach (var ext in extensions)
                    {
                        foreach (var extension in template.Extensions)
                        {
                            if (extension.ExtensionName.ToLower() == ext.ExtensionDisplayName.ToLower() && extension.ExtensionId.ToLower() == ext.ExtensionName.ToLower())
                            {
                                dict[extension.ExtensionName] = true;
                            }
                        }
                    }
                    var required = dict.Where(x => x.Value == false).ToList();

                    if (required.Count > 0)
                    {
                        Parallel.ForEach(required, async req =>
                        {
                            string publisherName = template.Extensions.Where(x => x.ExtensionName == req.Key).FirstOrDefault().PublisherId;
                            string extensionName = template.Extensions.Where(x => x.ExtensionName == req.Key).FirstOrDefault().ExtensionId;
                            try
                            {
                                InstalledExtension extension = null;
                                extension = await client.InstallExtensionByNameAsync(publisherName, extensionName);
                            }
                            catch (OperationCanceledException cancelException)
                            {
                                AddMessage(model.Id.ErrorId(), "Error while Installing extensions - operation cancelled: " + cancelException.Message + Environment.NewLine);
                            }
                            catch (Exception exc)
                            {
                                AddMessage(model.Id.ErrorId(), "Error while Installing extensions: " + exc.Message);
                            }
                        });
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.Id.ErrorId(), "Error while Installing extensions: " + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// WIKI set up operations 
        /// Project as Wiki and Code as Wiki
        /// </summary>
        /// <param name="model"></param>
        /// <param name="_wikiConfiguration"></param>
        public void CreateProjetWiki(string templatesFolder, Project model, AppConfiguration _wikiConfiguration)
        {
            try
            {
                ManageWiki manageWiki = new ManageWiki(_wikiConfiguration);
                string projectWikiFolderPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "/Wiki/ProjectWiki");
                if (Directory.Exists(projectWikiFolderPath))
                {
                    string createWiki = string.Format(templatesFolder + "/CreateWiki.json"); // check is path
                    if (File.Exists(createWiki))
                    {
                        string jsonString = File.ReadAllText(createWiki);
                        jsonString = jsonString.Replace("$ProjectID$", model.Environment.ProjectId)
                            .Replace("$Name$", model.Environment.ProjectName);
                        ProjectwikiResponse.Projectwiki projectWikiResponse = manageWiki.CreateProjectWiki(jsonString, model.Environment.ProjectId);
                        string[] subDirectories = Directory.GetDirectories(projectWikiFolderPath);
                        foreach (var dir in subDirectories)
                        {
                            //dirName==parentName//
                            string[] dirSplit = dir.Split('\\');
                            string dirName = dirSplit[dirSplit.Length - 1];
                            string sampleContent = File.ReadAllText(templatesFolder + "//SampleContent.json");
                            sampleContent = sampleContent.Replace("$Content$", "Sample wiki content");
                            bool isPage = manageWiki.CreateUpdatePages(sampleContent, model.Environment.ProjectName, projectWikiResponse.Id, dirName);//check is created

                            if (isPage)
                            {
                                string[] getFiles = Directory.GetFiles(dir);
                                if (getFiles.Length > 0)
                                {
                                    List<string> childFileNames = new List<string>();
                                    foreach (var file in getFiles)
                                    {
                                        string[] fileNameExtension = file.Split('\\');
                                        string fileName = (fileNameExtension[fileNameExtension.Length - 1].Split('.'))[0];
                                        string fileContent = model.ReadJsonFile(file);
                                        bool isCreated = false;
                                        Dictionary<string, string> dic = new Dictionary<string, string>();
                                        dic.Add("content", fileContent);
                                        string newContent = JsonConvert.SerializeObject(dic);
                                        if (fileName == dirName)
                                        {
                                            manageWiki.DeletePage(model.Environment.ProjectName, projectWikiResponse.Id, fileName);
                                            isCreated = manageWiki.CreateUpdatePages(newContent, model.Environment.ProjectName, projectWikiResponse.Id, fileName);
                                        }
                                        else
                                        {
                                            isCreated = manageWiki.CreateUpdatePages(newContent, model.Environment.ProjectName, projectWikiResponse.Id, fileName);
                                        }
                                        if (isCreated)
                                        {
                                            childFileNames.Add(fileName);
                                        }
                                    }
                                    if (childFileNames.Count > 0)
                                    {
                                        foreach (var child in childFileNames)
                                        {
                                            if (child != dirName)
                                            {
                                                string movePages = File.ReadAllText(templatesFolder + "/MovePages.json");
                                                if (!string.IsNullOrEmpty(movePages))
                                                {
                                                    movePages = movePages.Replace("$ParentFile$", dirName).Replace("$ChildFile$", child);
                                                    manageWiki.MovePages(movePages, model.Environment.ProjectId, projectWikiResponse.Id);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
        }
        public void CreateCodeWiki(Project model, AzureDevOpsAPI.AppConfiguration _wikiConfiguration)
        {
            try
            {
                string wikiFolder = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "/Wiki");
                //templatesFolder + model.SelectedTemplate + "\\Wiki";
                if (Directory.Exists(wikiFolder))
                {
                    string[] wikiFilePaths = Directory.GetFiles(wikiFolder);
                    if (wikiFilePaths.Length > 0)
                    {
                        ManageWiki manageWiki = new ManageWiki(_wikiConfiguration);

                        foreach (string wiki in wikiFilePaths)
                        {
                            string[] nameExtension = wiki.Split('\\');
                            string name = (nameExtension[nameExtension.Length - 1]).Split('.')[0];
                            string json = model.ReadJsonFile(wiki);
                            foreach (string repository in model.Environment.RepositoryIdList.Keys)
                            {
                                string placeHolder = string.Format("${0}$", repository);
                                json = json.Replace(placeHolder, model.Environment.RepositoryIdList[repository])
                                    .Replace("$Name$", name).Replace("$ProjectID$", model.Environment.ProjectId);
                            }
                            bool isWiki = manageWiki.CreateCodeWiki(json);
                            if (isWiki)
                            {
                                AddMessage(model.Id, "Created Wiki");
                            }
                            else if (!string.IsNullOrEmpty(manageWiki.LastFailureMessage))
                            {
                                AddMessage(model.Id.ErrorId(), "Error while creating wiki: " + manageWiki.LastFailureMessage);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.Id.ErrorId(), "Error while creating wiki: " + ex.Message);
            }
        }
        public void CreateDeploymentGroup(string templateFolder, Project model, AppConfiguration _deploymentGroup)
        {
            string path = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "/DeploymentGroups/CreateDeploymentGroup.json");
            //templateFolder + model.SelectedTemplate + "\\DeploymentGroups\\CreateDeploymentGroup.json";
            if (File.Exists(path))
            {
                string json = model.ReadJsonFile(path);
                if (!string.IsNullOrEmpty(json))
                {
                    DeploymentGroup deploymentGroup = new DeploymentGroup(_deploymentGroup);
                    bool isCreated = deploymentGroup.CreateDeploymentGroup(json);
                    if (isCreated) { } else if (!string.IsNullOrEmpty(deploymentGroup.LastFailureMessage)) { AddMessage(model.Id.ErrorId(), "Error while creating deployment group: " + deploymentGroup.LastFailureMessage); }
                }
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public string GetTemplateMessage(string TemplateName)
        {
            try
            {
                string groupDetails = "";
                TemplateSelection.Templates templates = new TemplateSelection.Templates();
                string templatesPath = ""; templatesPath = HostingEnvironment.WebRootPath + "/Templates/";
                if (File.Exists(templatesPath + "TemplateSetting.json"))
                {
                    groupDetails = File.ReadAllText(templatesPath + "/TemplateSetting.json");
                    templates = JsonConvert.DeserializeObject<TemplateSelection.Templates>(groupDetails);
                    foreach (var template in templates.GroupwiseTemplates.FirstOrDefault().Template)
                    {
                        if (template.TemplateFolder.ToLower() == TemplateName.ToLower())
                        {
                            return template.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return string.Empty;
        }

        private bool AddUserToProject(AppConfiguration con, Project model)
        {
            try
            {
                HttpServices httpService = new HttpServices(con);
                string PAT = string.Empty;
                string descriptorUrl = string.Format("_apis/graph/descriptors/{0}?api-version={1}", Convert.ToString(model.Environment.ProjectId), con.VersionNumber);
                var groups = httpService.Get(descriptorUrl);
                //dynamic obj = new dynamic();
                if (groups.IsSuccessStatusCode)
                {
                    dynamic obj = JsonConvert.DeserializeObject<dynamic>(groups.Content.ReadAsStringAsync().Result);
                    string getGroupDescriptor = string.Format("_apis/graph/groups?scopeDescriptor={0}&api-version={1}", Convert.ToString(obj.Value), con.VersionNumber);
                    var getAllGroups = httpService.Get(getGroupDescriptor);
                    if (getAllGroups.IsSuccessStatusCode)
                    {
                        GetAllGroups.GroupList allGroups = JsonConvert.DeserializeObject<GetAllGroups.GroupList>(getAllGroups.Content.ReadAsStringAsync().Result);
                        foreach (var group in allGroups.Value)
                        {
                            if (group.DisplayName.ToLower() == "project administrators")
                            {
                                string urpParams = string.Format("_apis/graph/users?groupDescriptors={0}&api-version={1}", Convert.ToString(group.Descriptor), con.VersionNumber);
                                var json = CreatePrincipalReqBody(model.Email);
                                var response = httpService.Post(json, urpParams);
                            }
                            if (group.DisplayName.ToLower() == model.ProjectName.ToLower() + " team")
                            {
                                string urpParams = string.Format("_apis/graph/users?groupDescriptors={0}&api-version={1}", Convert.ToString(group.Descriptor), con.VersionNumber);
                                var json = CreatePrincipalReqBody(model.Email);
                                var response = httpService.Post(json, urpParams);
                            }
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return false;
        }

        public static string CreatePrincipalReqBody(string name)
        {
            return "{\"principalName\": \"" + name + "\"}";
        }
        #endregion

        public bool CheckForInstalledExtensions(string extensionJsonFile, string token, string account)
        {
            bool ExtensionRequired = false;
            try
            {
                string accountName = account;
                string pat = token;
                string listedExtension = File.ReadAllText(extensionJsonFile);
                var template = JsonConvert.DeserializeObject<RequiredExtensions.Extension>(listedExtension);
                string requiresExtensionNames = string.Empty;
                string requiredMicrosoftExt = string.Empty;
                string requiredThirdPartyExt = string.Empty;
                string finalExtensionString = string.Empty;

                //Check for existing extensions
                if (template.Extensions.Count > 0)
                {
                    Dictionary<string, bool> dict = new Dictionary<string, bool>();
                    foreach (RequiredExtensions.ExtensionWithLink ext in template.Extensions)
                    {
                        dict.Add(ext.ExtensionName, false);
                    }
                    //pat = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", pat)));//configuration.PersonalAccessToken;

                    //var connection = new VssConnection(new Uri(string.Format("https://{0}.visualstudio.com", accountName)), new Microsoft.VisualStudio.Services.OAuth.VssOAuthAccessTokenCredential(pat));// VssOAuthCredential(PAT));
                    var connection = new VssConnection(new Uri(string.Format("https://{0}.visualstudio.com", accountName)), new VssBasicCredential(string.Empty, pat));// VssOAuthCredential(PAT));

                    var client = connection.GetClient<ExtensionManagementHttpClient>();
                    var installed = client.GetInstalledExtensionsAsync().Result;
                    var extensions = installed.Where(x => x.Flags == 0).ToList();

                    var trustedFlagExtensions = installed.Where(x => x.Flags == ExtensionFlags.Trusted).ToList();
                    var builtInExtensions = installed.Where(x => x.Flags.ToString() == "BuiltIn, Trusted").ToList();

                    extensions.AddRange(trustedFlagExtensions);
                    extensions.AddRange(builtInExtensions);

                    foreach (var ext in extensions)
                    {
                        foreach (var extension in template.Extensions)
                        {
                            if (extension.ExtensionName.ToLower() == ext.ExtensionDisplayName.ToLower())
                            {
                                dict[extension.ExtensionName] = true;
                            }
                        }
                    }
                    var required = dict.Where(x => x.Value == false).ToList();
                    if (required.Count > 0)
                    {
                        ExtensionRequired = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                //return Json(new { message = "Error", status = "false" }, JsonRequestBehavior.AllowGet);
                ExtensionRequired = false;
            }
            return ExtensionRequired;
        }

        private void CreateVaribaleGroups(Project model, AppConfiguration _variableGroups)
        {
            VariableGroups variableGroups = new VariableGroups(_variableGroups);
            model.Environment.VariableGroups = new Dictionary<int, string>();
            string filePath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "/VariableGroups/VariableGroup.json");
            if (File.Exists(filePath))
            {
                string jsonString = model.ReadJsonFile(filePath);
                GetVariableGroups.Groups groups = JsonConvert.DeserializeObject<GetVariableGroups.Groups>(jsonString);
                if (groups.Count > 0)
                {
                    foreach (var group in groups.Value)
                    {
                        GetVariableGroups.VariableGroupsCreateResponse response = variableGroups.PostVariableGroups(JsonConvert.SerializeObject(group));
                        if (!string.IsNullOrEmpty(response.Name))
                        {
                            model.Environment.VariableGroups.Add(response.Id, response.Name);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// End the process
        /// </summary>
        /// <param name="result"></param>
        public void EndEnvironmentSetupProcess(IAsyncResult result, Project model, int usercount)
        {
            string ID = string.Empty;
            string accName = string.Empty;
            try
            {
                ProcessEnvironment processTask = (ProcessEnvironment)result.AsyncState;
                //string[] strResult = processTask.EndInvoke(result);
                RemoveKey(model.Id);
                if (ProjectService.StatusMessages.Keys.Count(x => x == model.Id + "_Errors") == 1)
                {
                    string errorMessages = ProjectService.StatusMessages[model.Id + "_Errors"];
                    if (errorMessages != "")
                    {
                        //also, log message to file system
                        string logPath = HostingEnvironment.WebRootPath + "/log";
                        string fileName = string.Format("{0}_{1}.txt", model.SelectedTemplate, DateTime.Now.ToString("ddMMMyyyy_HHmmss"));

                        if (!Directory.Exists(logPath))
                        {
                            Directory.CreateDirectory(logPath);
                        }

                        System.IO.File.AppendAllText(Path.Combine(logPath, fileName), errorMessages);

                        //Create ISSUE work item with error details in VSTSProjectgenarator account
                        string patBase64 = AppKeyConfiguration["PATBase64"];
                        string url = AppKeyConfiguration["URL"];
                        string projectId = AppKeyConfiguration["PROJECTID"];
                        string issueName = string.Format("{0}_{1}", model.SelectedTemplate, DateTime.Now.ToString("ddMMMyyyy_HHmmss"));
                        IssueWi objIssue = new IssueWi();

                        errorMessages = errorMessages + "\t" + "TemplateUsed: " + model.SelectedTemplate;
                        errorMessages = errorMessages + "\t" + "ProjectCreated : " + ProjectService.projectName;

                        logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t  Error: " + errorMessages);

                        string logWIT = AppKeyConfiguration["LogWIT"];
                        if (logWIT == "true")
                        {
                            objIssue.CreateIssueWi(patBase64, "4.1", url, issueName, errorMessages, projectId, "Demo Generator");
                        }
                    }
                }
                //usercount--;
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            finally
            {
                //if (usercount == 0 && !string.IsNullOrEmpty(templateUsed))
                //{
                //    templateService.deletePrivateTemplate(templateUsed);
                //}
            }
        }
    }
}
