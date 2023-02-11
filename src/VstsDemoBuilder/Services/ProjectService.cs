using GoogleAnalyticsTracker.Simple;
using GoogleAnalyticsTracker.WebAPI2;
using log4net;
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
using System.Web.Hosting;
using System.Web.Http;
using VstsDemoBuilder.Extensions;
using VstsDemoBuilder.Models;
using VstsDemoBuilder.ServiceInterfaces;
using VstsRestAPI;
using VstsRestAPI.Build;
using VstsRestAPI.DeploymentGRoup;
using VstsRestAPI.Extractor;
using VstsRestAPI.Git;
using VstsRestAPI.ProjectsAndTeams;
using VstsRestAPI.QueriesAndWidgets;
using VstsRestAPI.Queues;
using VstsRestAPI.Release;
using VstsRestAPI.Service;
using VstsRestAPI.Services;
using VstsRestAPI.TestManagement;
using VstsRestAPI.Viewmodel.Extractor;
using VstsRestAPI.Viewmodel.GitHub;
using VstsRestAPI.Viewmodel.Importer;
using VstsRestAPI.Viewmodel.ProjectAndTeams;
using VstsRestAPI.Viewmodel.QueriesAndWidgets;
using VstsRestAPI.Viewmodel.Repository;
using VstsRestAPI.Viewmodel.Sprint;
using VstsRestAPI.Viewmodel.Wiki;
using VstsRestAPI.Viewmodel.WorkItem;
using VstsRestAPI.Wiki;
using VstsRestAPI.WorkItemAndTracking;
using ClassificationNodes = VstsRestAPI.WorkItemAndTracking.ClassificationNodes;

namespace VstsDemoBuilder.Services
{
    public class ProjectService : IProjectService
    {
        public static readonly object objLock = new object();
        public static Dictionary<string, string> statusMessages;
        public static ILog logger = LogManager.GetLogger("ErrorLog");

        public bool isDefaultRepoTodetele = true;
        public string websiteUrl = string.Empty;
        public string templateUsed = string.Empty;
        public static string projectName = string.Empty;
        public static AccessDetails AccessDetails = new AccessDetails();

        public string templateVersion = string.Empty;
        public static string enableExtractor = "";

        public static async void TrackFeature(string API)
        {
            SimpleTrackerEnvironment simpleTrackerEnvironment = new SimpleTrackerEnvironment(Environment.OSVersion.Platform.ToString(),
                                                                        Environment.OSVersion.Version.ToString(),
                                                                        Environment.OSVersion.VersionString);
            string GAKey = System.Configuration.ConfigurationManager.AppSettings["AnalyticsKey"];
            if (!string.IsNullOrEmpty(GAKey))
            {
                using (Tracker tracker = new Tracker(GAKey, simpleTrackerEnvironment))
                {
                    var response = await tracker.TrackPageViewAsync("My API - Create", API);
                    bool issuccess = response.Success;
                }
            }

        }
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
        public JObject GetStatusMessage(string id)
        {
            lock (ProjectService.objLock)
            {
                string message = string.Empty;
                JObject obj = new JObject();
                if (id.EndsWith("_Errors"))
                {
                    //RemoveKey(id);
                    obj["status"] = "Error: \t" + ProjectService.StatusMessages[id];
                }
                if (ProjectService.StatusMessages.Keys.Count(x => x == id) == 1)
                {
                    obj["status"] = ProjectService.StatusMessages[id];
                }
                else
                {
                    obj["status"] = "Successfully Created";

                }
                return obj;
            }
        }

        public HttpResponseMessage GetprojectList(string accname, string pat)
        {
            string defaultHost = System.Configuration.ConfigurationManager.AppSettings["DefaultHost"];
            string ProjectCreationVersion = System.Configuration.ConfigurationManager.AppSettings["ProjectCreationVersion"];

            Configuration config = new Configuration() { AccountName = accname, PersonalAccessToken = pat, UriString = defaultHost + accname, VersionNumber = ProjectCreationVersion };
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
                filePath = string.Format(TemplateFolder + @"\{0}", FileName);
            }
            else
            {
                filePath = string.Format(HostingEnvironment.MapPath("~") + @"\Templates\" + @"{0}\{1}", TemplateName, FileName);
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
            string accountName = model.accountName;
            //if (model.IsPrivatePath)
            //{
            //    templateUsed = model.PrivateTemplateName;
            //}
            //else
            //{
            templateUsed = model.SelectedTemplate;
            //}
            logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "Project Name: " + model.ProjectName + "\t Template Selected: " + templateUsed + "\t Organization Selected: " + accountName);
            string pat = model.accessToken;
            //define versions to be use
            string projectCreationVersion = System.Configuration.ConfigurationManager.AppSettings["ProjectCreationVersion"];
            string repoVersion = System.Configuration.ConfigurationManager.AppSettings["RepoVersion"];
            string buildVersion = System.Configuration.ConfigurationManager.AppSettings["BuildVersion"];
            string releaseVersion = System.Configuration.ConfigurationManager.AppSettings["ReleaseVersion"];
            string wikiVersion = System.Configuration.ConfigurationManager.AppSettings["WikiVersion"];
            string boardVersion = System.Configuration.ConfigurationManager.AppSettings["BoardVersion"];
            string workItemsVersion = System.Configuration.ConfigurationManager.AppSettings["WorkItemsVersion"];
            string queriesVersion = System.Configuration.ConfigurationManager.AppSettings["QueriesVersion"];
            string endPointVersion = System.Configuration.ConfigurationManager.AppSettings["EndPointVersion"];
            string extensionVersion = System.Configuration.ConfigurationManager.AppSettings["ExtensionVersion"];
            string dashboardVersion = System.Configuration.ConfigurationManager.AppSettings["DashboardVersion"];
            string agentQueueVersion = System.Configuration.ConfigurationManager.AppSettings["AgentQueueVersion"];
            string getSourceCodeVersion = System.Configuration.ConfigurationManager.AppSettings["GetSourceCodeVersion"];
            string testPlanVersion = System.Configuration.ConfigurationManager.AppSettings["TestPlanVersion"];
            string releaseHost = System.Configuration.ConfigurationManager.AppSettings["ReleaseHost"];
            string defaultHost = System.Configuration.ConfigurationManager.AppSettings["DefaultHost"];
            string deploymentGroup = System.Configuration.ConfigurationManager.AppSettings["DeloymentGroup"];
            string graphApiVersion = System.Configuration.ConfigurationManager.AppSettings["GraphApiVersion"];
            string graphAPIHost = System.Configuration.ConfigurationManager.AppSettings["GraphAPIHost"];
            string gitHubBaseAddress = System.Configuration.ConfigurationManager.AppSettings["GitHubBaseAddress"];
            string variableGroupsApiVersion = System.Configuration.ConfigurationManager.AppSettings["VariableGroupsApiVersion"];

            string processTemplateId = Default.SCRUM;
            model.Environment = new EnvironmentValues
            {
                serviceEndpoints = new Dictionary<string, string>(),
                repositoryIdList = new Dictionary<string, string>(),
                pullRequests = new Dictionary<string, string>(),
                GitHubRepos = new Dictionary<string, string>(),
                VariableGroups = new Dictionary<int, string>(),
                ReposImported = new Dictionary<string, bool>()
            };
            ProjectTemplate template = null;
            ProjectSettings settings = null;
            List<WIMapData> wiMapping = new List<WIMapData>();
            AccountMembers.Account accountMembers = new AccountMembers.Account();
            model.accountUsersForWi = new List<string>();
            websiteUrl = model.websiteUrl;
            projectName = model.ProjectName;

            string logWIT = System.Configuration.ConfigurationManager.AppSettings["LogWIT"];
            if (logWIT == "true")
            {
                string patBase64 = System.Configuration.ConfigurationManager.AppSettings["PATBase64"];
                string url = System.Configuration.ConfigurationManager.AppSettings["URL"];
                string projectId = System.Configuration.ConfigurationManager.AppSettings["PROJECTID"];
                string reportName = string.Format("{0}", "AzureDevOps_Analytics-DemoGenerator");
                IssueWI objIssue = new IssueWI();
                objIssue.CreateReportWI(patBase64, "4.1", url, websiteUrl, reportName, "", templateUsed, projectId, model.Region);
            }

            Configuration _gitHubConfig = new Configuration() { _gitbaseAddress = gitHubBaseAddress, _gitcredential = model.GitHubToken, _mediaType = "application/json", _scheme = "Bearer" };

            if (model.GitHubFork && model.GitHubToken != null)
            {
                GitHubImportRepo gitHubImport = new GitHubImportRepo(_gitHubConfig);
                HttpResponseMessage userResponse = gitHubImport.GetUserDetail();
                GitHubUserDetail userDetail = new GitHubUserDetail();
                if (userResponse.IsSuccessStatusCode)
                {
                    userDetail = JsonConvert.DeserializeObject<GitHubUserDetail>(userResponse.Content.ReadAsStringAsync().Result);
                    _gitHubConfig.userName = userDetail.login;
                    model.GitHubUserName = userDetail.login;
                }
            }
            //configuration setup
            string _credentials = model.accessToken;
            Configuration _projectCreationVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = projectCreationVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _releaseVersion = new Configuration() { UriString = releaseHost + accountName + "/", VersionNumber = releaseVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _buildVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = buildVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName, _gitbaseAddress = gitHubBaseAddress, _gitcredential = model.GitHubToken };
            Configuration _workItemsVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = workItemsVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _queriesVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = queriesVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _boardVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = boardVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _wikiVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = wikiVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _endPointVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = endPointVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName, _gitbaseAddress = gitHubBaseAddress, _gitcredential = model.GitHubToken };
            Configuration _extensionVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = extensionVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _dashboardVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = dashboardVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _repoVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = repoVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName, _gitbaseAddress = gitHubBaseAddress, _gitcredential = model.GitHubToken };

            Configuration _getSourceCodeVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = getSourceCodeVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName, _gitbaseAddress = gitHubBaseAddress, _gitcredential = model.GitHubToken };
            Configuration _agentQueueVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = agentQueueVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _testPlanVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = testPlanVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _deploymentGroup = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = deploymentGroup, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _graphApiVersion = new Configuration() { UriString = graphAPIHost + accountName + "/", VersionNumber = graphApiVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _variableGroupApiVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = variableGroupsApiVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };

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

                        if (!string.IsNullOrWhiteSpace(settings.type))
                        {
                            if (settings.type.ToLower() == TemplateType.Scrum.ToString().ToLower())
                            {
                                processTemplateId = Default.SCRUM;
                            }
                            else if (settings.type.ToLower() == TemplateType.Agile.ToString().ToLower())
                            {
                                processTemplateId = Default.Agile;
                            }
                            else if (settings.type.ToLower() == TemplateType.CMMI.ToString().ToLower())
                            {
                                processTemplateId = Default.CMMI;
                            }
                            else if (settings.type.ToLower() == TemplateType.Basic.ToString().ToLower())
                            {
                                processTemplateId = Default.BASIC;
                            }
                            else if (!string.IsNullOrEmpty(settings.id))
                            {
                                processTemplateId = settings.id;
                            }
                            else
                            {
                                AddMessage(model.id.ErrorId(), "Could not recognize process template. Make sure that the exported project template is belog to standard process template or project setting file has valid process template id.");
                                StatusMessages[model.id] = "100";
                                return new string[] { model.id, accountName, templateUsed };
                            }
                        }
                        else
                        {
                            settings.type = "scrum";
                            processTemplateId = Default.SCRUM;
                        }
                    }
                }
                else
                {
                    AddMessage(model.id.ErrorId(), "Project Template not found");
                    StatusMessages[model.id] = "100";
                    return new string[] { model.id, accountName, templateUsed };
                }
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            //create team project
            string jsonProject = model.ReadJsonFile(HostingEnvironment.MapPath("~") + @"\Templates\" + "CreateProject.json");
            jsonProject = jsonProject.Replace("$projectName$", model.ProjectName).Replace("$processTemplateId$", processTemplateId);

            Projects proj = new Projects(_projectCreationVersion);
            string projectID = proj.CreateTeamProject(jsonProject);

            if (projectID == "-1")
            {
                if (!string.IsNullOrEmpty(proj.LastFailureMessage))
                {
                    if (proj.LastFailureMessage.Contains("TF400813"))
                    {
                        AddMessage(model.id, "OAUTHACCESSDENIED");
                    }
                    else if (proj.LastFailureMessage.Contains("TF50309"))
                    {
                        AddMessage(model.id.ErrorId(), proj.LastFailureMessage);
                    }
                    else
                    {
                        AddMessage(model.id.ErrorId(), proj.LastFailureMessage);
                    }
                }
                Thread.Sleep(2000); // Adding Delay to Get Error message
                return new string[] { model.id, accountName, templateUsed };
            }
            else
            {
                AddMessage(model.id, string.Format("Project {0} created", model.ProjectName));
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
                    return new string[] { model.id, accountName, templateUsed };
                }
            }
            watch.Stop();

            //get project id after successfull in VSTS
            model.Environment.ProjectId = objProject.GetProjectIdByName(model.ProjectName);
            model.Environment.ProjectName = model.ProjectName;

            // Fork Repo
            if (model.GitHubFork && model.GitHubToken != null)
            {
                ForkGitHubRepository(model, _gitHubConfig);
            }

            //Add user as project admin
            bool isAdded = AddUserToProject(_graphApiVersion, model);
            if (isAdded)
            {
                AddMessage(model.id, string.Format("Added user {0} as project admin ", model.Email));
            }

            //Install required extensions
            if (!model.IsApi && model.isExtensionNeeded && model.isAgreeTerms)
            {
                bool isInstalled = InstallExtensions(model, model.accountName, model.accessToken);
                Thread.Sleep(1000);
                if (isInstalled) { AddMessage(model.id, "Required extensions are installed"); }
            }

            //current user Details
            string teamName = model.ProjectName + " team";
            TeamMemberResponse.TeamMembers teamMembers = GetTeamMembers(model.ProjectName, teamName, _projectCreationVersion, model.id);

            var teamMember = teamMembers.value != null ? teamMembers.value.FirstOrDefault() : new TeamMemberResponse.Value();

            if (teamMember != null)
            {
                model.Environment.UserUniquename = model.Environment.UserUniquename ?? teamMember.identity.uniqueName;
            }
            if (teamMember != null)
            {
                model.Environment.UserUniqueId = model.Environment.UserUniqueId ?? teamMember.identity.id;
            }
            //model.Environment.UserUniqueId = model.Email;
            //model.Environment.UserUniquename = model.Email;
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
                UpdateIterations(model, _boardVersion, "Iterations.json");
                //create teams
                CreateTeams(model, template.Teams, _projectCreationVersion, model.id, template.TeamArea);

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
                    //string.Format(templatesFolder + @"{0}\{1}", templateUsed, template.SetEpic);
                    if (File.Exists(json))
                    {
                        json = model.ReadJsonFile(json);
                        EnableEpic(model, json, _boardVersion, model.id, team);
                    }
                }

                if (template.BoardColumns != null)
                {
                    string team = model.ProjectName + " Team";
                    string json = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.BoardColumns);
                    //string.Format(templatesFolder + @"{0}\{1}", templateUsed, template.BoardColumns);
                    if (File.Exists(json))
                    {
                        json = model.ReadJsonFile(json);
                        bool success = UpdateBoardColumn(model, json, _boardVersion, model.id, boardType, team);
                        if (success)
                        {
                            //update Card Fields
                            if (template.CardField != null)
                            {
                                string cardFieldJson = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.CardField);
                                //string.Format(templatesFolder + @"{0}\{1}", templateUsed, template.CardField);
                                if (File.Exists(cardFieldJson))
                                {
                                    cardFieldJson = model.ReadJsonFile(cardFieldJson);
                                    UpdateCardFields(model, cardFieldJson, _boardVersion, model.id, boardType, model.ProjectName + " Team");
                                }
                            }
                            //Update card styles
                            if (template.CardStyle != null)
                            {
                                string cardStyleJson = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.CardStyle);
                                //string.Format(templatesFolder + @"{0}\{1}", templateUsed, template.CardStyle);
                                if (File.Exists(cardStyleJson))
                                {
                                    cardStyleJson = model.ReadJsonFile(cardStyleJson);
                                    UpdateCardStyles(model, cardStyleJson, _boardVersion, model.id, boardType, model.ProjectName + " Team");
                                }
                            }
                            //Enable Epic Backlog
                            AddMessage(model.id, "Board-Column, Swimlanes, Styles updated");
                        }
                    }
                }

                //update sprint dates
                UpdateSprintItems(model, _boardVersion, settings);
                RenameIterations(model, _boardVersion, settings.renameIterations);
            }
            else
            {
                UpdateIterations(model, _boardVersion, "Iterations.json");
                // for newer version of templates
                string teamsJsonPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "Teams\\Teams.json");
                // Path.Combine(templatesFolder + templateUsed, "Teams\\Teams.json");
                if (File.Exists(teamsJsonPath))
                {
                    template.Teams = "Teams\\Teams.json";
                    template.TeamArea = "TeamArea.json";
                    CreateTeams(model, template.Teams, _projectCreationVersion, model.id, template.TeamArea);
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
                        string teamFolderPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"Teams\" + jteam["name"].ToString());
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
                                    bool isUpdated = objSwimLanes.UpdateSwimLanes(JsonConvert.SerializeObject(board.value), model.ProjectName, board.BoardName, _teamName);
                                }
                            }

                            // updating team setting for each team
                            string teamSettingJson = "";
                            template.SetEpic = "TeamSetting.json";
                            teamSettingJson = Path.Combine(teamFolderPath, template.SetEpic);
                            if (File.Exists(teamSettingJson))
                            {
                                teamSettingJson = File.ReadAllText(teamSettingJson);
                                EnableEpic(model, teamSettingJson, _boardVersion, model.id, _teamName);
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
                                    bool success = UpdateBoardColumn(model, JsonConvert.SerializeObject(board.value, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), _boardVersion, model.id, board.BoardName, _teamName);
                                }
                            }

                            // updating card fields for each team and each board
                            try
                            {
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
                                        UpdateCardFields(model, JsonConvert.SerializeObject(card, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), _boardVersion, model.id, card.BoardName, _teamName);
                                    }
                                }
                            }
                            catch(Exception ex)
                            {

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
                                    if (cardStyle.rules.fill != null)
                                    {
                                        UpdateCardStyles(model, JsonConvert.SerializeObject(cardStyle), _boardVersion, model.id, cardStyle.BoardName, _teamName);
                                    }
                                }
                            }
                        }
                        AddMessage(model.id, "Board-Column, Swimlanes, Styles updated");
                    }
                    UpdateSprintItems(model, _boardVersion, settings);

                    RenameIterations(model, _boardVersion, settings.renameIterations);
                }
            }
            //Create Deployment Group
            //CreateDeploymentGroup(templatesFolder, model, _deploymentGroup);

            //create service endpoint
            List<string> listEndPointsJsonPath = new List<string>();
            string serviceEndPointsPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"ServiceEndpoints");
            if (Directory.Exists(serviceEndPointsPath))
            {
                Directory.GetFiles(serviceEndPointsPath).ToList().ForEach(i => listEndPointsJsonPath.Add(i));
            }
            CreateServiceEndPoint(model, listEndPointsJsonPath, _endPointVersion);
            //create agent queues on demand
            Queue queue = new Queue(_agentQueueVersion);
            model.Environment.AgentQueues = queue.GetQueues();
            if (settings.queues != null && settings.queues.Count > 0)
            {
                foreach (string aq in settings.queues)
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
            string importSourceCodePath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\ImportSourceCode");
            //templatesFolder + templateUsed + @"\ImportSourceCode";
            if (Directory.Exists(importSourceCodePath))
            {
                Directory.GetFiles(importSourceCodePath).ToList().ForEach(i => listImportSourceCodeJsonPaths.Add(i));
                if (listImportSourceCodeJsonPaths.Contains(importSourceCodePath + "\\GitRepository.json"))
                {
                    listImportSourceCodeJsonPaths.Remove(importSourceCodePath + "\\GitRepository.json");
                }
            }
            foreach (string importSourceCode in listImportSourceCodeJsonPaths)
            {
                ImportSourceCode(model, importSourceCode, _repoVersion, model.id, _getSourceCodeVersion);
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
            CreateProjetWiki(HostingEnvironment.MapPath("~") + @"\Templates\", model, _wikiVersion);
            CreateCodeWiki(model, _wikiVersion);

            List<string> listPullRequestJsonPaths = new List<string>();
            string pullRequestFolder = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\PullRequests");
            //templatesFolder + templateUsed + @"\PullRequests";
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
                model.selectedUsers = model.selectedUsers.TrimEnd(',');
                model.accountUsersForWi = model.selectedUsers.Split(',').ToList();
            }
            else if (model.UserMethod == "Random")
            {
                //GetAccount Members
                VstsRestAPI.ProjectsAndTeams.Accounts objAccount = new VstsRestAPI.ProjectsAndTeams.Accounts(_projectCreationVersion);
                //accountMembers = objAccount.GetAccountMembers(accountName, AccessToken);
                foreach (var member in accountMembers.value)
                {
                    model.accountUsersForWi.Add(member.member.mailAddress);
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
                string _WitPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\WorkItems");
                //Path.Combine(templatesFolder + templateUsed + "\\WorkItems");
                if (Directory.Exists(_WitPath))
                {
                    string[] workItemFilePaths = Directory.GetFiles(_WitPath);
                    if (workItemFilePaths.Length > 0)
                    {
                        foreach (var workItem in workItemFilePaths)
                        {
                            string[] workItemPatSplit = workItem.Split('\\');
                            if (workItemPatSplit.Length > 0)
                            {
                                string workItemName = workItemPatSplit[workItemPatSplit.Length - 1];
                                if (!string.IsNullOrEmpty(workItemName))
                                {
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
                }
            }

            ImportWorkItems import = new ImportWorkItems(_workItemsVersion, model.Environment.BoardRowFieldName);
            if (File.Exists(projectSettingsFile))
            {
                string attchmentFilesFolder = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\WorkItemAttachments");
                //string.Format(templatesFolder + @"{0}\WorkItemAttachments", templateUsed);
                if (listPullRequestJsonPaths.Count > 0)
                {
                    if (templateUsed == "MyHealthClinic")
                    {
                        wiMapping = import.ImportWorkitems(workItems, model.ProjectName, model.Environment.UserUniquename, model.ReadJsonFile(projectSettingsFile), attchmentFilesFolder, model.Environment.repositoryIdList.ContainsKey("MyHealthClinic") ? model.Environment.repositoryIdList["MyHealthClinic"] : string.Empty, model.Environment.ProjectId, model.Environment.pullRequests, model.UserMethod, model.accountUsersForWi, templateUsed);
                    }
                    else if (templateUsed == "SmartHotel360")
                    {
                        wiMapping = import.ImportWorkitems(workItems, model.ProjectName, model.Environment.UserUniquename, model.ReadJsonFile(projectSettingsFile), attchmentFilesFolder, model.Environment.repositoryIdList.ContainsKey("PublicWeb") ? model.Environment.repositoryIdList["PublicWeb"] : string.Empty, model.Environment.ProjectId, model.Environment.pullRequests, model.UserMethod, model.accountUsersForWi, templateUsed);
                    }
                    else
                    {
                        wiMapping = import.ImportWorkitems(workItems, model.ProjectName, model.Environment.UserUniquename, model.ReadJsonFile(projectSettingsFile), attchmentFilesFolder, model.Environment.repositoryIdList.ContainsKey(templateUsed) ? model.Environment.repositoryIdList[templateUsed] : string.Empty, model.Environment.ProjectId, model.Environment.pullRequests, model.UserMethod, model.accountUsersForWi, templateUsed);
                    }
                }
                else
                {
                    wiMapping = import.ImportWorkitems(workItems, model.ProjectName, model.Environment.UserUniquename, model.ReadJsonFile(projectSettingsFile), attchmentFilesFolder, string.Empty, model.Environment.ProjectId, model.Environment.pullRequests, model.UserMethod, model.accountUsersForWi, templateUsed);
                }
                AddMessage(model.id, "Work Items created");
            }
            //Creat TestPlans and TestSuites
            List<string> listTestPlansJsonPaths = new List<string>();
            string testPlansFolder = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\TestPlans");
            //templatesFolder + templateUsed + @"\TestPlans";
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
                //AddMessage(model.id, "TestPlans, TestSuites and TestCases created");
            }
            // create varibale groups

            CreateVaribaleGroups(model, _variableGroupApiVersion);

            //create build Definition
            string buildDefinitionsPath = string.Empty;
            model.BuildDefinitions = new List<BuildDef>();
            // if the template is private && agreed to GitHubFork && GitHub Token is not null
            if (setting.IsPrivate == "true" && model.GitHubFork && !string.IsNullOrEmpty(model.GitHubToken))
            {
                buildDefinitionsPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\BuildDefinitions");
                if (Directory.Exists(buildDefinitionsPath))
                {
                    Directory.GetFiles(buildDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.BuildDefinitions.Add(new BuildDef() { FilePath = i }));
                }
                buildDefinitionsPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\BuildDefinitionGitHub");
                if (Directory.Exists(buildDefinitionsPath))
                {
                    Directory.GetFiles(buildDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.BuildDefinitions.Add(new BuildDef() { FilePath = i }));
                }
            }
            // if the template is private && not agreed to GitHubFork && GitHub Token is null
            else if (setting.IsPrivate == "true" && !model.GitHubFork && string.IsNullOrEmpty(model.GitHubToken))
            {
                buildDefinitionsPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\BuildDefinitions");
                if (Directory.Exists(buildDefinitionsPath))
                {
                    Directory.GetFiles(buildDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.BuildDefinitions.Add(new BuildDef() { FilePath = i }));
                }
            }
            // if the template is not private && agreed to GitHubFork && GitHub Token is not null
            else if (string.IsNullOrEmpty(setting.IsPrivate) && model.GitHubFork && !string.IsNullOrEmpty(model.GitHubToken))
            {
                buildDefinitionsPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\BuildDefinitionGitHub");
                if (Directory.Exists(buildDefinitionsPath))
                {
                    Directory.GetFiles(buildDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.BuildDefinitions.Add(new BuildDef() { FilePath = i }));
                }
            }
            // if the template is not private && not agreed to GitHubFork && GitHub Token is null
            else if (string.IsNullOrEmpty(setting.IsPrivate) && !model.GitHubFork && string.IsNullOrEmpty(model.GitHubToken))
            {
                buildDefinitionsPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\BuildDefinitions");
                if (Directory.Exists(buildDefinitionsPath))
                {
                    Directory.GetFiles(buildDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.BuildDefinitions.Add(new BuildDef() { FilePath = i }));
                }
            }
            bool isBuild = CreateBuildDefinition(model, _buildVersion, model.id);
            if (isBuild)
            {
                AddMessage(model.id, "Build definition created");
            }

            //Queue a Build
            string buildJson = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "QueueBuild.json");
            //string.Format(templatesFolder + @"{0}\QueueBuild.json", templateUsed);
            if (File.Exists(buildJson))
            {
                QueueABuild(model, buildJson, _buildVersion);
            }

            //create release Definition
            string releaseDefinitionsPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\ReleaseDefinitions");
            //templatesFolder + templateUsed + @"\ReleaseDefinitions";
            model.ReleaseDefinitions = new List<ReleaseDef>();
            if (Directory.Exists(releaseDefinitionsPath))
            {
                Directory.GetFiles(releaseDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.ReleaseDefinitions.Add(new Models.ReleaseDef() { FilePath = i }));
            }
            bool isReleased = CreateReleaseDefinition(model, _releaseVersion, model.id, teamMembers);
            if (isReleased)
            {
                AddMessage(model.id, "Release definition created");
            }

            //Create query and widgets
            List<string> listDashboardQueriesPath = new List<string>();
            string dashboardQueriesPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\Dashboard\Queries");
            //templatesFolder + templateUsed + @"\Dashboard\Queries";
            string dashboardPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\Dashboard");
            //templatesFolder + templateUsed + @"\Dashboard";

            if (Directory.Exists(dashboardQueriesPath))
            {
                Directory.GetFiles(dashboardQueriesPath).ToList().ForEach(i => listDashboardQueriesPath.Add(i));
            }
            if (Directory.Exists(dashboardPath))
            {
                CreateQueryAndWidgets(model, listDashboardQueriesPath, _queriesVersion, _dashboardVersion, _releaseVersion, _projectCreationVersion, _boardVersion);
                AddMessage(model.id, "Queries, Widgets and Charts created");
            }

            StatusMessages[model.id] = "100";
            return new string[] { model.id, accountName, templateUsed };
        }

        private void ForkGitHubRepository(Project model, Configuration _gitHubConfig)
        {
            try
            {
                List<string> listRepoFiles = new List<string>();
                string repoFilePath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, @"\ImportSourceCode\GitRepository.json");
                if (File.Exists(repoFilePath))
                {
                    string readRepoFile = model.ReadJsonFile(repoFilePath);
                    if (!string.IsNullOrEmpty(readRepoFile))
                    {
                        ForkRepos.Fork forkRepos = new ForkRepos.Fork();
                        forkRepos = JsonConvert.DeserializeObject<ForkRepos.Fork>(readRepoFile);
                        if (forkRepos.repositories != null && forkRepos.repositories.Count > 0)
                        {
                            foreach (var repo in forkRepos.repositories)
                            {
                                GitHubImportRepo user = new GitHubImportRepo(_gitHubConfig);
                                GitHubUserDetail userDetail = new GitHubUserDetail();
                                GitHubRepoResponse.RepoCreated GitHubRepo = new GitHubRepoResponse.RepoCreated();
                                //HttpResponseMessage listForks = user.ListForks(repo.fullName);
                                HttpResponseMessage forkResponse = user.ForkRepo(repo.fullName);
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
                                    AddMessage(model.id, string.Format("Forked {0} repository to {1} user", model.GitRepoName, _gitHubConfig.userName));
                                }
                                else
                                {
                                    AddMessage(model.id.ErrorId(), "Error while forking the repository: " + forkResponse.Content.ReadAsStringAsync().Result);
                                    logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + "Error while forking the repository: " + forkResponse.Content.ReadAsStringAsync().Result + "\n");
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.id.ErrorId(), "Error while forking repository :" + ex.Message);
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
        private void CreateTeams(Project model, string teamsJSON, VstsRestAPI.Configuration _projectConfig, string id, string teamAreaJSON)
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
                                            if (!(string.IsNullOrEmpty(teamResponse.id)))
                                            {
                                                string areaName = objTeam.CreateArea(model.ProjectName, teamResponse.name);
                                                string updateAreaJSON = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, teamAreaJSON);

                                                //updateAreaJSON = string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, teamAreaJSON);

                                                if (File.Exists(updateAreaJSON))
                                                {
                                                    updateAreaJSON = model.ReadJsonFile(updateAreaJSON);
                                                    updateAreaJSON = updateAreaJSON.Replace("$ProjectName$", model.ProjectName).Replace("$AreaName$", areaName);
                                                    bool isUpdated = objTeam.SetAreaForTeams(model.ProjectName, teamResponse.name, updateAreaJSON);
                                                }
                                                bool isBackLogIterationUpdated = objTeam.SetBackLogIterationForTeam(backlogIteration, model.ProjectName, teamResponse.name);
                                                if (iterations.count > 0)
                                                {
                                                    foreach (var iteration in iterations.value)
                                                    {
                                                        if (iteration.structureType == "iteration")
                                                        {
                                                            foreach (var child in iteration.children)
                                                            {
                                                                if (teamMap.Iterations.Contains(child.name))
                                                                {
                                                                    bool isIterationUpdated = objTeam.SetIterationsForTeam(child.identifier, teamResponse.name, model.ProjectName);
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
                                if (!(string.IsNullOrEmpty(teamResponse.id)))
                                {
                                    string areaName = objTeam.CreateArea(model.ProjectName, teamResponse.name);
                                    string updateAreaJSON = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, teamAreaJSON);

                                    //updateAreaJSON = string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, teamAreaJSON);

                                    if (File.Exists(updateAreaJSON))
                                    {
                                        updateAreaJSON = model.ReadJsonFile(updateAreaJSON);
                                        updateAreaJSON = updateAreaJSON.Replace("$ProjectName$", model.ProjectName).Replace("$AreaName$", areaName);
                                        bool isUpdated = objTeam.SetAreaForTeams(model.ProjectName, teamResponse.name, updateAreaJSON);
                                    }
                                    bool isBackLogIterationUpdated = objTeam.SetBackLogIterationForTeam(backlogIteration, model.ProjectName, teamResponse.name);
                                    if (iterations.count > 0)
                                    {
                                        foreach (var iteration in iterations.value)
                                        {
                                            if (iteration.structureType == "iteration")
                                            {
                                                foreach (var child in iteration.children)
                                                {
                                                    bool isIterationUpdated = objTeam.SetIterationsForTeam(child.identifier, teamResponse.name, model.ProjectName);
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
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
        private TeamMemberResponse.TeamMembers GetTeamMembers(string projectName, string teamName, VstsRestAPI.Configuration _configuration, string id)
        {
            try
            {
                TeamMemberResponse.TeamMembers viewModel = new TeamMemberResponse.TeamMembers();
                VstsRestAPI.ProjectsAndTeams.Teams objTeam = new VstsRestAPI.ProjectsAndTeams.Teams(_configuration);
                viewModel = objTeam.GetTeamMembers(projectName, teamName);

                if (!(string.IsNullOrEmpty(objTeam.LastFailureMessage)))
                {
                    AddMessage(id.ErrorId(), "Error while getting team members: " + objTeam.LastFailureMessage + Environment.NewLine);
                }
                return viewModel;
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
        private void CreateWorkItems(Project model, string workItemJSON, VstsRestAPI.Configuration _defaultConfiguration, string id)
        {
            try
            {
                string jsonWorkItems = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, workItemJSON);
                //string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, workItemJSON);
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
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
        private bool UpdateBoardColumn(Project model, string BoardColumnsJSON, VstsRestAPI.Configuration _BoardConfig, string id, string BoardType, string team)
        {
            bool result = false;
            try
            {
                BoardColumn objBoard = new BoardColumn(_BoardConfig);
                bool boardColumnResult = objBoard.UpdateBoard(model.ProjectName, BoardColumnsJSON, BoardType, team);
                if (boardColumnResult)
                {
                    model.Environment.BoardRowFieldName = objBoard.rowFieldName;
                    result = true;
                }
                else if (!(string.IsNullOrEmpty(objBoard.LastFailureMessage)))
                {
                    AddMessage(id.ErrorId(), "Error while updating board column " + objBoard.LastFailureMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
        private void UpdateCardFields(Project model, string json, Configuration _configuration, string id, string boardType, string team)
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
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
        private void UpdateCardStyles(Project model, string json, Configuration _configuration, string id, string boardType, string team)
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
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
        private void EnableEpic(Project model, string json, VstsRestAPI.Configuration _boardVersion, string id, string team)
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
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
        private void UpdateWorkItems(Project model, string workItemUpdateJSON, VstsRestAPI.Configuration _defaultConfiguration, string id, string currentUser, string projectSettingsJSON)
        {
            try
            {
                string jsonWorkItemsUpdate = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, workItemUpdateJSON);
                //string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, workItemUpdateJSON);
                string jsonProjectSettings = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, projectSettingsJSON);
                //string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, projectSettingsJSON);
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
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");

                AddMessage(id.ErrorId(), "Error while updating work items: " + ex.Message);

            }
        }

        /// <summary>
        /// Update Iterations
        /// </summary>
        /// <param name="model"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="iterationsJSON"></param>
        private void UpdateIterations(Project model, VstsRestAPI.Configuration _boardConfig, string iterationsJSON)
        {
            try
            {
                string jsonIterations = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, iterationsJSON);
                //string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, iterationsJSON);
                if (File.Exists(jsonIterations))
                {
                    iterationsJSON = model.ReadJsonFile(jsonIterations);
                    VstsRestAPI.WorkItemAndTracking.ClassificationNodes objClassification = new VstsRestAPI.WorkItemAndTracking.ClassificationNodes(_boardConfig);

                    GetNodesResponse.Nodes nodes = objClassification.GetIterations(model.ProjectName);

                    GetNodesResponse.Nodes projectNode = JsonConvert.DeserializeObject<GetNodesResponse.Nodes>(iterationsJSON);

                    if (projectNode.hasChildren)
                    {
                        foreach (var child in projectNode.children)
                        {
                            CreateIterationNode(model, objClassification, child, nodes);
                        }
                    }

                    if (projectNode.hasChildren)
                    {
                        foreach (var child in projectNode.children)
                        {
                            path = string.Empty;
                            MoveIterationNode(model, objClassification, child);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");

                AddMessage(model.id.ErrorId(), "Error while updating iteration: " + ex.Message);
            }
        }

        /// <summary>
        /// Create Iterations
        /// </summary>
        /// <param name="model"></param>
        /// <param name="objClassification"></param>
        /// <param name="child"></param>
        /// <param name="currentIterations"></param>
        private void CreateIterationNode(Project model, VstsRestAPI.WorkItemAndTracking.ClassificationNodes objClassification, GetNodesResponse.Child child, GetNodesResponse.Nodes currentIterations)
        {
            string[] defaultSprints = new string[] { "Sprint 1", "Sprint 2", "Sprint 3", "Sprint 4", "Sprint 5", "Sprint 6", };
            if (defaultSprints.Contains(child.name))
            {
                var nd = (currentIterations.hasChildren) ? currentIterations.children.FirstOrDefault(i => i.name == child.name) : null;
                if (nd != null)
                {
                    child.id = nd.id;
                }
                else
                {
                    var node = objClassification.CreateIteration(model.ProjectName, child.name);
                    child.id = node.id;
                }
            }
            else
            {
                var node = objClassification.CreateIteration(model.ProjectName, child.name);
                child.id = node.id;
            }

            if (child.hasChildren && child.children != null)
            {
                foreach (var c in child.children)
                {
                    CreateIterationNode(model, objClassification, c, currentIterations);
                }
            }
        }

        private string path = string.Empty;
        /// <summary>
        /// Move Iterations to nodes
        /// </summary>
        /// <param name="model"></param>
        /// <param name="objClassification"></param>
        /// <param name="child"></param>
        private void MoveIterationNode(Project model, VstsRestAPI.WorkItemAndTracking.ClassificationNodes objClassification, GetNodesResponse.Child child)
        {
            if (child.hasChildren && child.children != null)
            {
                foreach (var c in child.children)
                {
                    path += child.name + "\\";
                    var nd = objClassification.MoveIteration(model.ProjectName, path, c.id);

                    if (c.hasChildren)
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
        private void UpdateSprintItems(Project model, VstsRestAPI.Configuration _boardConfig, ProjectSettings settings)
        {
            try
            {
                if (settings.type.ToLower() == "scrum" || settings.type.ToLower() == "agile" || settings.type.ToLower() == "basic")
                {
                    string teamIterationMap = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "TeamIterationMap.json");

                    ClassificationNodes objClassification = new ClassificationNodes(_boardConfig);
                    bool classificationNodesResult = objClassification.UpdateIterationDates(model.ProjectName, settings.type, model.SelectedTemplate, teamIterationMap);

                    if (!(string.IsNullOrEmpty(objClassification.LastFailureMessage)))
                    {
                        AddMessage(model.id.ErrorId(), "Error while updating sprint items: " + objClassification.LastFailureMessage + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.id.ErrorId(), "Error while updating sprint items: " + ex.Message);

            }
        }

        /// <summary>
        /// Rename Iterations
        /// </summary>
        /// <param name="model"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="renameIterations"></param>
        public void RenameIterations(Project model, VstsRestAPI.Configuration _defaultConfiguration, Dictionary<string, string> renameIterations)
        {
            try
            {
                if (renameIterations != null && renameIterations.Count > 0)
                {
                    VstsRestAPI.WorkItemAndTracking.ClassificationNodes objClassification = new VstsRestAPI.WorkItemAndTracking.ClassificationNodes(_defaultConfiguration);
                    bool IsRenamed = objClassification.RenameIteration(model.ProjectName, renameIterations);
                }
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.id.ErrorId(), "Error while renaming iterations: " + ex.Message);
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
        private void ImportSourceCode(Project model, string sourceCodeJSON, VstsRestAPI.Configuration _repo, string id, VstsRestAPI.Configuration _retSourceCodeVersion)
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
                        model.Environment.repositoryIdList[repositoryDetail[1]] = repositoryDetail[0];
                    }

                    string jsonSourceCode = model.ReadJsonFile(sourceCodeJSON);

                    //update endpoint ids
                    foreach (string endpoint in model.Environment.serviceEndpoints.Keys)
                    {
                        string placeHolder = string.Format("${0}$", endpoint);
                        jsonSourceCode = jsonSourceCode.Replace(placeHolder, model.Environment.serviceEndpoints[endpoint]);
                    }

                    Repository objRepositorySourceCode = new Repository(_retSourceCodeVersion);
                    bool copySourceCode = objRepositorySourceCode.GetSourceCodeFromGitHub(jsonSourceCode, model.ProjectName, repositoryDetail[0]);
                    if (!model.Environment.ReposImported.ContainsKey(repositoryDetail[0]))
                    {
                        model.Environment.ReposImported.Add(repositoryDetail[0], copySourceCode);
                    }

                    if (!(string.IsNullOrEmpty(objRepository.LastFailureMessage)))
                    {
                        AddMessage(id.ErrorId(), "Error while importing source code: " + objRepository.LastFailureMessage + Environment.NewLine);
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(id.ErrorId(), "Error while importing source code: " + ex.Message);
            }
        }

        /// <summary>
        /// Creates pull request
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pullRequestJsonPath"></param>
        /// <param name="_configuration3_0"></param>
        private void CreatePullRequest(Project model, string pullRequestJsonPath, VstsRestAPI.Configuration _workItemConfig)
        {
            try
            {
                if (File.Exists(pullRequestJsonPath))
                {
                    string commentFile = Path.GetFileName(pullRequestJsonPath);
                    string repositoryId = string.Empty;
                    if (model.SelectedTemplate == "MyHealthClinic") { repositoryId = model.Environment.repositoryIdList["MyHealthClinic"]; }
                    if (model.SelectedTemplate == "SmartHotel360") { repositoryId = model.Environment.repositoryIdList["PublicWeb"]; }
                    else { repositoryId = model.Environment.repositoryIdList.ContainsKey(model.SelectedTemplate) ? model.Environment.repositoryIdList[model.SelectedTemplate] : ""; }

                    pullRequestJsonPath = model.ReadJsonFile(pullRequestJsonPath);
                    pullRequestJsonPath = pullRequestJsonPath.Replace("$reviewer$", model.Environment.UserUniqueId);
                    Repository objRepository = new Repository(_workItemConfig);
                    string[] pullReqResponse = new string[2];

                    pullReqResponse = objRepository.CreatePullRequest(pullRequestJsonPath, repositoryId);
                    if (pullReqResponse.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(pullReqResponse[0]) && !string.IsNullOrEmpty(pullReqResponse[1]))
                        {
                            model.Environment.pullRequests.Add(pullReqResponse[1], pullReqResponse[0]);
                            commentFile = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, @"\PullRequests\Comments\" + commentFile);
                            //string.Format(templatesFolder + @"{0}\PullRequests\Comments\{1}", model.SelectedTemplate, commentFile);
                            if (File.Exists(commentFile))
                            {
                                commentFile = model.ReadJsonFile(commentFile);
                                PullRequestComments.Comments commentsList = JsonConvert.DeserializeObject<PullRequestComments.Comments>(commentFile);
                                if (commentsList.count > 0)
                                {
                                    foreach (PullRequestComments.Value thread in commentsList.value)
                                    {
                                        string threadID = objRepository.CreateCommentThread(repositoryId, pullReqResponse[0], JsonConvert.SerializeObject(thread));
                                        if (!string.IsNullOrEmpty(threadID))
                                        {
                                            if (thread.Replies != null && thread.Replies.Count > 0)
                                            {
                                                foreach (var reply in thread.Replies)
                                                {
                                                    objRepository.AddCommentToThread(repositoryId, pullReqResponse[0], threadID, JsonConvert.SerializeObject(reply));
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
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.id.ErrorId(), "Error while creating pull Requests: " + ex.Message);
            }
        }

        /// <summary>
        /// Creates service end points
        /// </summary>
        /// <param name="model"></param>
        /// <param name="jsonPaths"></param>
        /// <param name="_defaultConfiguration"></param>
        private void CreateServiceEndPoint(Project model, List<string> jsonPaths, Configuration _endpointConfig)
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
                        string username = System.Configuration.ConfigurationManager.AppSettings["UserID"];
                        string password = System.Configuration.ConfigurationManager.AppSettings["Password"];
                        //string extractPath = HostingEnvironment.MapPath("~/Templates/" + model.SelectedTemplate);
                        string projectFileData = File.ReadAllText(GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "ProjectTemplate.json"));
                        ProjectSetting settings = JsonConvert.DeserializeObject<ProjectSetting>(projectFileData);
                        ServiceEndPoint objService = new ServiceEndPoint(_endpointConfig);

                        string gitUserName = System.Configuration.ConfigurationManager.AppSettings["GitUserName"];
                        string gitUserPassword = System.Configuration.ConfigurationManager.AppSettings["GitUserPassword"];

                        jsonCreateService = model.ReadJsonFile(jsonCreateService);

                        if (!string.IsNullOrEmpty(settings.IsPrivate))
                        {
                            jsonCreateService = jsonCreateService.Replace("$ProjectName$", model.ProjectName);
                            jsonCreateService = jsonCreateService.Replace("$username$", model.Email).Replace("$password$", model.accessToken);
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
                            string bikeSharing360username = System.Configuration.ConfigurationManager.AppSettings["UserID"];
                            string bikeSharing360password = System.Configuration.ConfigurationManager.AppSettings["BikeSharing360Password"];
                            jsonCreateService = jsonCreateService.Replace("$BikeSharing360username$", bikeSharing360username).Replace("$BikeSharing360password$", bikeSharing360password);
                        }
                        else if (model.SelectedTemplate.ToLower() == "contososhuttle" || model.SelectedTemplate.ToLower() == "contososhuttle2")
                        {
                            string contosousername = System.Configuration.ConfigurationManager.AppSettings["ContosoUserID"];
                            string contosopassword = System.Configuration.ConfigurationManager.AppSettings["ContosoPassword"];
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
                            AddMessage(model.id.ErrorId(), "Error while creating service endpoint: " + objService.LastFailureMessage + Environment.NewLine);
                        }
                        else
                        {
                            model.Environment.serviceEndpoints[endpoint.name] = endpoint.id;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.id.ErrorId(), "Error while creating service endpoint: " + ex.Message);
            }
        }

        /// <summary>
        /// Create Test Cases
        /// </summary>
        /// <param name="wiMapping"></param>
        /// <param name="model"></param>
        /// <param name="testPlanJson"></param>
        /// <param name="_defaultConfiguration"></param>
        private void CreateTestManagement(List<WIMapData> wiMapping, Project model, string testPlanJson, VstsRestAPI.Configuration _testPlanVersion)
        {
            try
            {
                if (File.Exists(testPlanJson))
                {
                    List<WIMapData> testCaseMap = new List<WIMapData>();
                    testCaseMap = wiMapping.Where(x => x.WIType == "Test Case").ToList();

                    string fileName = Path.GetFileName(testPlanJson);
                    testPlanJson = model.ReadJsonFile(testPlanJson);

                    testPlanJson = testPlanJson.Replace("$project$", model.ProjectName);
                    TestManagement objTest = new TestManagement(_testPlanVersion);
                    string[] testPlanResponse = new string[2];
                    testPlanResponse = objTest.CreateTestPlan(testPlanJson, model.ProjectName);

                    if (testPlanResponse.Length > 0)
                    {
                        string testSuiteJson = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, @"\TestPlans\TestSuites\" + fileName);
                        //string.Format(templateFolder + @"{0}\TestPlans\TestSuites\{1}", model.SelectedTemplate, fileName);
                        if (File.Exists(testSuiteJson))
                        {
                            testSuiteJson = model.ReadJsonFile(testSuiteJson);
                            testSuiteJson = testSuiteJson.Replace("$planID$", testPlanResponse[0]).Replace("$planName$", testPlanResponse[1]);
                            foreach (var wi in wiMapping)
                            {
                                string placeHolder = string.Format("${0}$", wi.OldID);
                                testSuiteJson = testSuiteJson.Replace(placeHolder, wi.NewID);
                            }
                            TestSuite.TestSuites listTestSuites = JsonConvert.DeserializeObject<TestSuite.TestSuites>(testSuiteJson);
                            if (listTestSuites.count > 0)
                            {
                                foreach (var TS in listTestSuites.value)
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
                                                if (wiMap.OldID == id)
                                                {
                                                    testCasesToAdd = testCasesToAdd + wiMap.NewID + ",";
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
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.id.ErrorId(), "Error while creating test plan and test suites: " + ex.Message);
            }
        }

        /// <summary>
        /// Creates Build Definitions
        /// </summary>
        /// <param name="model"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool CreateBuildDefinition(Project model, VstsRestAPI.Configuration _buildConfig, string id)
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
                                             .Replace("$username$", model.GitHubUserName)
                                             .Replace("$Organization$", model.accountName);

                        if (model.Environment.VariableGroups.Count > 0)
                        {
                            foreach (var vGroupsId in model.Environment.VariableGroups)
                            {
                                string placeHolder = string.Format("${0}$", vGroupsId.Value);
                                jsonBuildDefinition = jsonBuildDefinition.Replace(placeHolder, vGroupsId.Key.ToString());
                            }
                        }

                        //update repositoryId 
                        foreach (string repository in model.Environment.repositoryIdList.Keys)
                        {
                            string placeHolder = string.Format("${0}$", repository);
                            jsonBuildDefinition = jsonBuildDefinition.Replace(placeHolder, model.Environment.repositoryIdList[repository]);
                        }
                        //update endpoint ids
                        foreach (string endpoint in model.Environment.serviceEndpoints.Keys)
                        {
                            string placeHolder = string.Format("${0}$", endpoint);
                            jsonBuildDefinition = jsonBuildDefinition.Replace(placeHolder, model.Environment.serviceEndpoints[endpoint]);
                        }
                        if (model.Environment.AgentQueues.Count > 0)
                        {
                            foreach (var agentPool in model.Environment.AgentQueues.Keys)
                            {
                                string placeHolder = string.Format("${0}$", agentPool);
                                jsonBuildDefinition = jsonBuildDefinition.Replace(placeHolder, Convert.ToString(model.Environment.AgentQueues[agentPool]));
                            }
                        }

                        string[] buildResult = objBuild.CreateBuildDefinition(jsonBuildDefinition, model.ProjectName, model.SelectedTemplate);

                        if (!(string.IsNullOrEmpty(objBuild.LastFailureMessage)))
                        {
                            AddMessage(id.ErrorId(), "Error while creating build definition: " + objBuild.LastFailureMessage + Environment.NewLine);
                        }
                        if (buildResult.Length > 0)
                        {
                            buildDef.Id = buildResult[0];
                            buildDef.Name = buildResult[1];
                        }
                    }
                    flag = true;
                }
                return flag;
            }

            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
        private void QueueABuild(Project model, string json, VstsRestAPI.Configuration _buildConfig)
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
                        AddMessage(model.id.ErrorId(), "Error while Queueing build: " + objBuild.LastFailureMessage + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.id.ErrorId(), "Error while Queueing Build: " + ex.Message);
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
        private bool CreateReleaseDefinition(Project model, VstsRestAPI.Configuration _releaseConfiguration, string id, TeamMemberResponse.TeamMembers teamMembers)
        {
            bool flag = false;
            try
            {
                var teamMember = teamMembers.value.FirstOrDefault();
                foreach (ReleaseDef relDef in model.ReleaseDefinitions)
                {
                    if (File.Exists(relDef.FilePath))
                    {
                        ReleaseDefinition objRelease = new ReleaseDefinition(_releaseConfiguration);
                        string jsonReleaseDefinition = model.ReadJsonFile(relDef.FilePath);
                        jsonReleaseDefinition = jsonReleaseDefinition.Replace("$ProjectName$", model.Environment.ProjectName)
                                             .Replace("$ProjectId$", model.Environment.ProjectId)
                                             .Replace("$OwnerUniqueName$", teamMember.identity.uniqueName)
                                             .Replace("$OwnerId$", teamMember.identity.id)
                                  .Replace("$OwnerDisplayName$", teamMember.identity.displayName);

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
                        jsonReleaseDefinition = jsonReleaseDefinition.Replace("$UUID$", uuid).Replace("$RandomNumber$", uuid).Replace("$AccountName$", model.accountName); ;

                        //update agent queue ids
                        foreach (string queue in model.Environment.AgentQueues.Keys)
                        {
                            string placeHolder = string.Format("${0}$", queue);
                            jsonReleaseDefinition = jsonReleaseDefinition.Replace(placeHolder, model.Environment.AgentQueues[queue].ToString());
                        }

                        //update endpoint ids
                        foreach (string endpoint in model.Environment.serviceEndpoints.Keys)
                        {
                            string placeHolder = string.Format("${0}$", endpoint);
                            jsonReleaseDefinition = jsonReleaseDefinition.Replace(placeHolder, model.Environment.serviceEndpoints[endpoint]);
                        }

                        foreach (BuildDef objBuildDef in model.BuildDefinitions)
                        {
                            //update build ids
                            string placeHolder = string.Format("${0}-id$", objBuildDef.Name);
                            jsonReleaseDefinition = jsonReleaseDefinition.Replace(placeHolder, objBuildDef.Id);
                        }
                        string[] releaseDef = objRelease.CreateReleaseDefinition(jsonReleaseDefinition, model.ProjectName);
                        if (!(string.IsNullOrEmpty(objRelease.LastFailureMessage)))
                        {
                            if (objRelease.LastFailureMessage.TrimEnd() == "Tasks with versions 'ARM Outputs:3.*' are not valid for deploy job 'Function' in stage Azure-Dev.")
                            {
                                jsonReleaseDefinition = jsonReleaseDefinition.Replace("3.*", "4.*");
                                releaseDef = objRelease.CreateReleaseDefinition(jsonReleaseDefinition, model.ProjectName);
                                if (releaseDef.Length > 0)
                                {
                                    relDef.Id = releaseDef[0];
                                    relDef.Name = releaseDef[1];
                                }
                                if (!string.IsNullOrEmpty(relDef.Name))
                                {
                                    objRelease.LastFailureMessage = string.Empty;
                                }
                            }
                        }
                        relDef.Id = releaseDef[0];
                        relDef.Name = releaseDef[1];

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
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
        public void CreateQueryAndWidgets(Project model, List<string> listQueries, VstsRestAPI.Configuration _queriesVersion, VstsRestAPI.Configuration _dashboardVersion, VstsRestAPI.Configuration _releaseConfig, VstsRestAPI.Configuration _projectConfig, VstsRestAPI.Configuration _boardConfig)
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
                    AddMessage(model.id.ErrorId(), "Error while getting dashboardId: " + objWidget.LastFailureMessage + Environment.NewLine);
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
                        AddMessage(model.id.ErrorId(), "Error while creating query: " + _newobjQuery.LastFailureMessage + Environment.NewLine);
                    }

                }
                //Create DashBoards
                string dashBoardTemplate = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, @"\Dashboard\Dashboard.json");
                //string.Format(templatesFolder + @"{0}\Dashboard\Dashboard.json", model.SelectedTemplate);
                if (File.Exists(dashBoardTemplate))
                {
                    dynamic dashBoard = new System.Dynamic.ExpandoObject();
                    dashBoard.name = "Working";
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
                            .Replace("$repositoryId$", model.Environment.repositoryIdList.Where(x => x.Key.ToLower() == "bikesharing360").FirstOrDefault().Value)
                            .Replace("$IOSBuildId$", iosBuildDefId).Replace("$IOSReleaseId$", iosReleaseDefId).Replace("$IOSEnv1$", iosEnvironmentIds[0].ToString()).Replace("$IOSEnv2$", iosEnvironmentIds[1].ToString())
                            .Replace("$Xamarin.iOS$", xamarin_IOSBuild)
                            .Replace("$Xamarin.Droid$", xamarin_DroidBuild)
                            .Replace("$AndroidBuildId$", androidbuildDefId).Replace("$AndroidreleaseDefId$", androidreleaseDefId).Replace("$AndroidEnv1$", androidEnvironmentIds[0].ToString()).Replace("$AndroidEnv2$", androidEnvironmentIds[1].ToString())
                            .Replace("$OpenUserStoriesId$", openUserStories.id)
                            .Replace("$projectId$", model.Environment.ProjectId);

                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }
                    if (model.SelectedTemplate == "Gen-MyHealthClinic" || model.SelectedTemplate == "Gen-PartsUnlimited")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);

                            QueryResponse feedBack = objQuery.GetQueryByPathAndName(model.ProjectName, "Feedback_WI", "Shared%20Queries");
                            QueryResponse unfinishedWork = objQuery.GetQueryByPathAndName(model.ProjectName, "Unfinished Work_WI", "Shared%20Queries");


                            dashBoardTemplate = dashBoardTemplate.Replace("$Feedback$", feedBack.id).
                                         Replace("$AllItems$", queryResults.Where(x => x.name == "All Items_WI").FirstOrDefault() != null ? queryResults.Where(x => x.name == "All Items_WI").FirstOrDefault().id : string.Empty).
                                         Replace("$UserStories$", queryResults.Where(x => x.name == "User Stories").FirstOrDefault() != null ? queryResults.Where(x => x.name == "User Stories").FirstOrDefault().id : string.Empty).
                                         Replace("$TestCase$", queryResults.Where(x => x.name == "Test Case-Readiness").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Test Case-Readiness").FirstOrDefault().id : string.Empty).
                                         Replace("$teamID$", "").
                                         Replace("$teamName$", model.ProjectName + " Team").
                                         Replace("$projectID$", model.Environment.ProjectId).
                                         Replace("$Unfinished Work$", unfinishedWork.id).
                                         Replace("$projectId$", model.Environment.ProjectId).
                                         Replace("$projectName$", model.ProjectName);


                            if (model.SelectedTemplate == "Gen-MyHealthClinic")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$ReleaseDefId$", model.ReleaseDefinitions.Where(x => x.Name == "MyHealthClinicE2E").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "MyHealthClinicE2E").FirstOrDefault().Id : string.Empty).
                                             Replace("$ActiveBugs$", queryResults.Where(x => x.name == "Active Bugs_WI").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Active Bugs_WI").FirstOrDefault().id : string.Empty).
                                             Replace("$MyHealthClinicE2E$", model.BuildDefinitions.Where(x => x.Name == "MyHealthClinicE2E").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "MyHealthClinicE2E").FirstOrDefault().Id : string.Empty).
                                                 Replace("$RepositoryId$", model.Environment.repositoryIdList.Any(i => i.Key.ToLower().Contains("myhealthclinic")) ? model.Environment.repositoryIdList.Where(x => x.Key.ToLower() == "myhealthclinic").FirstOrDefault().Value : string.Empty);
                            }
                            if (model.SelectedTemplate == "Gen-MyHealthClinic" || model.SelectedTemplate == "Gen-PartsUnlimited")
                            {
                                QueryResponse workInProgress = objQuery.GetQueryByPathAndName(model.ProjectName, "Work in Progress_WI", "Shared%20Queries");

                                dashBoardTemplate = dashBoardTemplate.Replace("$ReleaseDefId$", model.ReleaseDefinitions.Where(x => x.Name == "PartsUnlimitedE2E").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "PartsUnlimitedE2E").FirstOrDefault().Id : string.Empty).
                                          Replace("$ActiveBugs$", queryResults.Where(x => x.name == "Critical Bugs").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Critical Bugs").FirstOrDefault().id : string.Empty).
                                          Replace("$PartsUnlimitedE2E$", model.BuildDefinitions.Where(x => x.Name == "PartsUnlimitedE2E").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "PartsUnlimitedE2E").FirstOrDefault().Id : string.Empty)
                                          .Replace("$WorkinProgress$", workInProgress.id)
                                .Replace("$RepositoryId$", model.Environment.repositoryIdList.Any(i => i.Key.ToLower().Contains("partsunlimited")) ? model.Environment.repositoryIdList.Where(x => x.Key.ToLower() == "partsunlimited").FirstOrDefault().Value : string.Empty);

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
                            string allItems = queryResults.Where(x => x.name == "All Items_WI").FirstOrDefault().id;
                            string repositoryId = model.Environment.repositoryIdList.Where(x => x.Key.ToLower() == "bikesharing360").FirstOrDefault().Key;
                            string bikeSharing360_PublicWeb = model.BuildDefinitions.Where(x => x.Name == "BikeSharing360-PublicWeb").FirstOrDefault().Id;

                            dashBoardTemplate = dashBoardTemplate.Replace("$BikeSharing360-PublicWeb$", bikeSharing360_PublicWeb)
                                         .Replace("$All Items$", allItems)
                                         .Replace("$repositoryId$", repositoryId)
                                         .Replace("$Unfinished Work$", unfinishedWork.id)
                                         .Replace("$projectId$", model.Environment.ProjectId);

                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }
                    if (model.SelectedTemplate == "DL-Docker")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);
                            var buildDefId = model.BuildDefinitions.FirstOrDefault();
                            dashBoardTemplate = dashBoardTemplate.Replace("$BuildDefId$", buildDefId.Id)
                                  .Replace("$projectId$", model.Environment.ProjectId)
                                  .Replace("$PBI$", queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault().id != null ? queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault().id : string.Empty)
                                  .Replace("$Bugs$", queryResults.Where(x => x.name == "Bugs").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Bugs").FirstOrDefault().id : string.Empty)
                                  .Replace("$AllWorkItems$", queryResults.Where(x => x.name == "All Work Items").FirstOrDefault() != null ? queryResults.Where(x => x.name == "All Work Items").FirstOrDefault().id : string.Empty)
                                  .Replace("$Feature$", queryResults.Where(x => x.name == "Feature").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Feature").FirstOrDefault().id : string.Empty)
                                  .Replace("$Tasks$", queryResults.Where(x => x.name == "Tasks").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Tasks").FirstOrDefault().id : string.Empty)
                                         .Replace("$RepoMyShuttleDocker$", model.Environment.repositoryIdList.Where(x => x.Key == "MyShuttleDocker").FirstOrDefault().ToString() != "" ? model.Environment.repositoryIdList.Where(x => x.Key == "MyShuttleDocker").FirstOrDefault().Value : string.Empty);


                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }
                    if (model.SelectedTemplate == "Gen-MyShuttle")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);
                            dashBoardTemplate = dashBoardTemplate
                            .Replace("$PBI$", queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault().id : string.Empty)
                            .Replace("$Bugs$", queryResults.Where(x => x.name == "Bugs").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Bugs").FirstOrDefault().id : string.Empty)
                            .Replace("$AllWorkItems$", queryResults.Where(x => x.name == "All Work Items").FirstOrDefault() != null ? queryResults.Where(x => x.name == "All Work Items").FirstOrDefault().id : string.Empty)
                            .Replace("$TestPlan$", queryResults.Where(x => x.name == "Test Plans").FirstOrDefault().id != null ? queryResults.Where(x => x.name == "Test Plans").FirstOrDefault().id : string.Empty)
                            .Replace("$Test Cases$", queryResults.Where(x => x.name == "Test Cases").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Test Cases").FirstOrDefault().id : string.Empty)
                            .Replace("$Features$", queryResults.Where(x => x.name == "Feature").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Feature").FirstOrDefault().id : string.Empty)
                            .Replace("$Tasks$", queryResults.Where(x => x.name == "Tasks").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Tasks").FirstOrDefault().id : string.Empty)
                            .Replace("$TestSuite$", queryResults.Where(x => x.name == "Test Suites").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Test Suites").FirstOrDefault().id : string.Empty);


                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }
                    if (model.SelectedTemplate.ToLower() == "myshuttle2")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);

                            dashBoardTemplate = dashBoardTemplate.Replace("$TestCases$", queryResults.Where(x => x.name == "Test Cases").FirstOrDefault().id != null ? queryResults.Where(x => x.name == "Test Cases").FirstOrDefault().id : string.Empty)
                                         .Replace("$AllWorkItems$", queryResults.Where(x => x.name == "All Work Items").FirstOrDefault() != null ? queryResults.Where(x => x.name == "All Work Items").FirstOrDefault().id : string.Empty)
                                         .Replace("$PBI$", queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault().id : string.Empty)
                                         .Replace("$RepoMyShuttleCalc$", model.Environment.repositoryIdList["MyShuttleCalc"] != null ? model.Environment.repositoryIdList["MyShuttleCalc"] : string.Empty)
                                         .Replace("$TestPlan$", queryResults.Where(x => x.name == "Test Plans").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Test Plans").FirstOrDefault().id : string.Empty)
                                         .Replace("$Tasks$", queryResults.Where(x => x.name == "Tasks").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Tasks").FirstOrDefault().id : string.Empty)
                                         .Replace("$Bugs$", queryResults.Where(x => x.name == "Bugs").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Bugs").FirstOrDefault().id : string.Empty)
                                         .Replace("$Features$", queryResults.Where(x => x.name == "Feature").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Feature").FirstOrDefault().id : string.Empty)
                                         .Replace("$RepoMyShuttle2$", model.Environment.repositoryIdList.Where(x => x.Key.ToLower() == "myshuttle2").FirstOrDefault().ToString() != "" ? model.Environment.repositoryIdList.Where(x => x.Key.ToLower() == "myshuttle2").FirstOrDefault().Value : string.Empty);


                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }
                    if (model.SelectedTemplate.ToLower() == "dl-docker" || model.SelectedTemplate.ToLower() == "dl-php" || model.SelectedTemplate.ToLower() == "dl-sonarqube" || model.SelectedTemplate.ToLower() == "dl-github" || model.SelectedTemplate.ToLower() == "dl-whitesource bolt" || model.SelectedTemplate.ToLower() == "dl-DeploymentGroups" || model.SelectedTemplate.ToLower() == "dl-octopus")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);
                            dashBoardTemplate = dashBoardTemplate.Replace("$Task$", queryResults.Where(x => x.name == "Tasks").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Tasks").FirstOrDefault().id : string.Empty)
                                         .Replace("$AllWorkItems$", queryResults.Where(x => x.name == "All Work Items").FirstOrDefault() != null ? queryResults.Where(x => x.name == "All Work Items").FirstOrDefault().id : string.Empty)
                                         .Replace("$Feature$", queryResults.Where(x => x.name == "Feature").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Feature").FirstOrDefault().id : string.Empty)
                                         .Replace("$Projectid$", model.Environment.ProjectId)
                                         .Replace("$Epic$", queryResults.Where(x => x.name == "Epics").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Epics").FirstOrDefault().id : string.Empty);

                            if (model.SelectedTemplate.ToLower() == "dl-docker")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$BuildDocker$", model.BuildDefinitions.Where(x => x.Name == "MHCDocker.build").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "MHCDocker.build").FirstOrDefault().Id : string.Empty)
                                .Replace("$ReleaseDocker$", model.ReleaseDefinitions.Where(x => x.Name == "MHCDocker.release").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "MHCDocker.release").FirstOrDefault().Id : string.Empty)
                                  .Replace("$PBI$", queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault().id : string.Empty);
                            }
                            else if (model.SelectedTemplate.ToLower() == "dl-php")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$buildPHP$", model.BuildDefinitions.Where(x => x.Name == "PHP").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "PHP").FirstOrDefault().Id : string.Empty)
                        .Replace("$releasePHP$", model.ReleaseDefinitions.Where(x => x.Name == "PHP").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "PHP").FirstOrDefault().Id : string.Empty)
                                 .Replace("$PBI$", queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault().id : string.Empty);
                            }
                            else if (model.SelectedTemplate.ToLower() == "dl-sonarqube")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$BuildSonarQube$", model.BuildDefinitions.Where(x => x.Name == "SonarQube").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "SonarQube").FirstOrDefault().Id : string.Empty)
                                .Replace("$PBI$", queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault().id : string.Empty);

                            }
                            else if (model.SelectedTemplate.ToLower() == "dl-github")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$PBI$", queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault().id : string.Empty)
                                             .Replace("$buildGitHub$", model.BuildDefinitions.Where(x => x.Name == "GitHub").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "GitHub").FirstOrDefault().Id : string.Empty)
                                             .Replace("$Hosted$", model.Environment.AgentQueues["Hosted"].ToString())
                                             .Replace("$releaseGitHub$", model.ReleaseDefinitions.Where(x => x.Name == "GitHub").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "GitHub").FirstOrDefault().Id : string.Empty);

                            }
                            else if (model.SelectedTemplate.ToLower() == "dl-whitesource bolt")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$PBI$", queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault().id : string.Empty)
                                          .Replace("$buildWhiteSource$", model.BuildDefinitions.Where(x => x.Name == "WhiteSourceBolt").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "WhiteSourceBolt").FirstOrDefault().Id : string.Empty);
                            }

                            else if (model.SelectedTemplate.ToLower() == "dl-deploymentGroups")
                            {
                                QueryResponse WorkInProgress = objQuery.GetQueryByPathAndName(model.ProjectName, "Work in Progress_WI", "Shared%20Queries");
                                dashBoardTemplate = dashBoardTemplate.Replace("$WorkinProgress$", WorkInProgress.id);
                            }

                            else if (model.SelectedTemplate.ToLower() == "dl-octopus")
                            {
                                var BuildDefId = model.BuildDefinitions.FirstOrDefault();
                                if (BuildDefId != null)
                                {
                                    dashBoardTemplate = dashBoardTemplate.Replace("$BuildDefId$", BuildDefId.Id)
                                            .Replace("$PBI$", queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault().id : string.Empty);
                                }
                            }


                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }

                    if (model.SelectedTemplate.ToLower() == "gen-smarthotel360")
                    {
                        if (isDashboardDeleted)
                        {
                            string startdate = DateTime.Now.ToString("yyyy-MM-dd");
                            VstsRestAPI.ProjectsAndTeams.Teams objTeam = new VstsRestAPI.ProjectsAndTeams.Teams(_projectConfig);
                            TeamResponse defaultTeam = objTeam.GetTeamByName(model.ProjectName, model.ProjectName + " team");
                            VstsRestAPI.WorkItemAndTracking.ClassificationNodes objnodes = new VstsRestAPI.WorkItemAndTracking.ClassificationNodes(_boardConfig);
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
                            dashBoardTemplate = dashBoardTemplate.Replace("$WorkinProgress$", workInProgress.id)
                                .Replace("$projectId$", model.Environment.ProjectId != null ? model.Environment.ProjectId : string.Empty)
                                .Replace("$PublicWebBuild$", model.BuildDefinitions.Where(x => x.Name == "SmartHotel_Petchecker-Web").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "SmartHotel_Petchecker-Web").FirstOrDefault().Id : string.Empty)
                                .Replace("$DefaultTeamId$", defaultTeam.id != null ? defaultTeam.id : string.Empty).Replace("$AllItems$", allItems.id != null ? allItems.id : string.Empty)
                                .Replace("$BacklogBoardWI$", backlogBoardWI.id != null ? backlogBoardWI.id : string.Empty)
                                .Replace("$StateofTestCases$", stateofTestCase.id != null ? stateofTestCase.id : string.Empty)
                                .Replace("$Feedback$", feedback.id != null ? feedback.id : string.Empty)
                                .Replace("$RepoPublicWeb$", model.Environment.repositoryIdList.ContainsKey("PublicWeb") ? model.Environment.repositoryIdList["PublicWeb"] : string.Empty)
                                .Replace("$MobileTeamWork$", mobileTeamWork.id != null ? mobileTeamWork.id : string.Empty).Replace("$WebTeamWork$", webTeamWork.id != null ? webTeamWork.id : string.Empty)
                                .Replace("$Bugs$", bugs.id != null ? bugs.id : string.Empty)
                                .Replace("$sprint2$", sprints.value.Where(x => x.name == "Sprint 2").FirstOrDefault() != null ? sprints.value.Where(x => x.name == "Sprint 2").FirstOrDefault().id : string.Empty)
                                .Replace("$sprint3$", sprints.value.Where(x => x.name == "Sprint 3").FirstOrDefault() != null ? sprints.value.Where(x => x.name == "Sprint 3").FirstOrDefault().id : string.Empty)
                                .Replace("$startDate$", startdate)
                                .Replace("$BugswithoutRepro$", bugsWithoutReproSteps.id != null ? bugsWithoutReproSteps.id : string.Empty).Replace("$UnfinishedWork$", unfinishedWork.id != null ? unfinishedWork.id : string.Empty)
                                .Replace("$RepoSmartHotel360$", model.Environment.repositoryIdList.ContainsKey("SmartHotel360") ? model.Environment.repositoryIdList["SmartHotel360"] : string.Empty)
                                .Replace("$SmartHotel360_Website-Deploy$", model.ReleaseDefinitions.Where(x => x.Name == "SmartHotel360_Website-Deploy").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "SmartHotel360_Website-Deploy").FirstOrDefault().Id : string.Empty);

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
                            dashBoardTemplate = dashBoardTemplate.Replace("$WorkinProgress$", workInProgress.id);

                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }
                }
            }
            catch (OperationCanceledException oce)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + oce.Message + "\t" + oce.InnerException.Message + "\n" + oce.StackTrace + "\n");
                AddMessage(model.id.ErrorId(), "Error while creating Queries and Widgets: Operation cancelled exception " + oce.Message + "\r\n");
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.id.ErrorId(), "Error while creating Queries and Widgets: " + ex.Message);
            }
        }

        public bool InstallExtensions(Project model, string accountName, string PAT)
        {
            try
            {
                //string templatesFolder = HostingEnvironment.MapPath("~") + @"\Templates\";
                string projTemplateFile = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "Extensions.json");
                //string.Format(templatesFolder + @"{0}\Extensions.json", model.SelectedTemplate);
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
                        if (!dict.ContainsKey(ext.extensionName))
                        {
                            dict.Add(ext.extensionName, false);
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
                            if (extension.extensionName.ToLower() == ext.ExtensionDisplayName.ToLower() && extension.extensionId.ToLower() == ext.ExtensionName.ToLower())
                            {
                                dict[extension.extensionName] = true;
                            }
                        }
                    }
                    var required = dict.Where(x => x.Value == false).ToList();

                    if (required.Count > 0)
                    {
                        Parallel.ForEach(required, async req =>
                        {
                            string publisherName = template.Extensions.Where(x => x.extensionName == req.Key).FirstOrDefault().publisherId;
                            string extensionName = template.Extensions.Where(x => x.extensionName == req.Key).FirstOrDefault().extensionId;
                            try
                            {
                                InstalledExtension extension = null;
                                extension = await client.InstallExtensionByNameAsync(publisherName, extensionName);
                            }
                            catch (OperationCanceledException cancelException)
                            {
                                AddMessage(model.id.ErrorId(), "Error while Installing extensions - operation cancelled: " + cancelException.Message + Environment.NewLine);
                            }
                            catch (Exception exc)
                            {
                                AddMessage(model.id.ErrorId(), "Error while Installing extensions: " + exc.Message);
                            }
                        });
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.id.ErrorId(), "Error while Installing extensions: " + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// WIKI set up operations 
        /// Project as Wiki and Code as Wiki
        /// </summary>
        /// <param name="model"></param>
        /// <param name="_wikiConfiguration"></param>
        public void CreateProjetWiki(string templatesFolder, Project model, Configuration _wikiConfiguration)
        {
            try
            {
                ManageWiki manageWiki = new ManageWiki(_wikiConfiguration);
                string projectWikiFolderPath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, @"\Wiki\ProjectWiki");
                //templatesFolder + model.SelectedTemplate + "\\Wiki\\ProjectWiki";
                if (Directory.Exists(projectWikiFolderPath))
                {
                    string createWiki = string.Format(templatesFolder + "\\CreateWiki.json"); // check is path
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
                            string sampleContent = File.ReadAllText(templatesFolder + "\\SampleContent.json");
                            sampleContent = sampleContent.Replace("$Content$", "Sample wiki content");
                            bool isPage = manageWiki.CreateUpdatePages(sampleContent, model.Environment.ProjectName, projectWikiResponse.id, dirName);//check is created

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
                                            manageWiki.DeletePage(model.Environment.ProjectName, projectWikiResponse.id, fileName);
                                            isCreated = manageWiki.CreateUpdatePages(newContent, model.Environment.ProjectName, projectWikiResponse.id, fileName);
                                        }
                                        else
                                        {
                                            isCreated = manageWiki.CreateUpdatePages(newContent, model.Environment.ProjectName, projectWikiResponse.id, fileName);
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
                                                string movePages = File.ReadAllText(templatesFolder + @"\MovePages.json");
                                                if (!string.IsNullOrEmpty(movePages))
                                                {
                                                    movePages = movePages.Replace("$ParentFile$", dirName).Replace("$ChildFile$", child);
                                                    manageWiki.MovePages(movePages, model.Environment.ProjectId, projectWikiResponse.id);
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
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
        }
        public void CreateCodeWiki(Project model, VstsRestAPI.Configuration _wikiConfiguration)
        {
            try
            {
                string wikiFolder = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, @"\Wiki");
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
                            bool isImported = false;
                            foreach (string repository in model.Environment.repositoryIdList.Keys)
                            {
                                if (model.Environment.repositoryIdList.ContainsKey(repository) && !string.IsNullOrEmpty(model.Environment.repositoryIdList[repository]))
                                {
                                    if (model.Environment.ReposImported.ContainsKey(model.Environment.repositoryIdList[repository]))
                                    {
                                        isImported = model.Environment.ReposImported[model.Environment.repositoryIdList[repository]];
                                    }
                                    
                                    string placeHolder = string.Format("${0}$", repository);
                                    if(json.Contains(placeHolder))
                                    {
                                        json = json.Replace(placeHolder, model.Environment.repositoryIdList[repository])
                                            .Replace("$Name$", name).Replace("$ProjectID$", model.Environment.ProjectId);
                                        break;
                                    }
                                }
                            }
                            if(isImported)
                            {
                                bool isWiki = manageWiki.CreateCodeWiki(json);
                                if (isWiki)
                                {
                                    AddMessage(model.id, "Created Wiki");
                                }
                                else if (!string.IsNullOrEmpty(manageWiki.LastFailureMessage))
                                {
                                    AddMessage(model.id.ErrorId(), "Error while creating wiki: " + manageWiki.LastFailureMessage);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.id.ErrorId(), "Error while creating wiki: " + ex.Message);
            }
        }
        public void CreateDeploymentGroup(string templateFolder, Project model, Configuration _deploymentGroup)
        {
            string path = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, @"\DeploymentGroups\CreateDeploymentGroup.json");
            //templateFolder + model.SelectedTemplate + "\\DeploymentGroups\\CreateDeploymentGroup.json";
            if (File.Exists(path))
            {
                string json = model.ReadJsonFile(path);
                if (!string.IsNullOrEmpty(json))
                {
                    DeploymentGroup deploymentGroup = new DeploymentGroup(_deploymentGroup);
                    bool isCreated = deploymentGroup.CreateDeploymentGroup(json);
                    if (isCreated) { } else if (!string.IsNullOrEmpty(deploymentGroup.LastFailureMessage)) { AddMessage(model.id.ErrorId(), "Error while creating deployment group: " + deploymentGroup.LastFailureMessage); }
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
                string templatesPath = ""; templatesPath = HostingEnvironment.MapPath("~") + @"\Templates\";
                if (File.Exists(templatesPath + "TemplateSetting.json"))
                {
                    groupDetails = File.ReadAllText(templatesPath + @"\TemplateSetting.json");
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
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return string.Empty;
        }

        private bool AddUserToProject(Configuration con, Project model)
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
                    string getGroupDescriptor = string.Format("_apis/graph/groups?scopeDescriptor={0}&api-version={1}", Convert.ToString(obj.value), con.VersionNumber);
                    var getAllGroups = httpService.Get(getGroupDescriptor);
                    if (getAllGroups.IsSuccessStatusCode)
                    {
                        GetAllGroups.GroupList allGroups = JsonConvert.DeserializeObject<GetAllGroups.GroupList>(getAllGroups.Content.ReadAsStringAsync().Result);
                        foreach (var group in allGroups.value)
                        {
                            if (group.displayName.ToLower() == "project administrators")
                            {
                                string urpParams = string.Format("_apis/graph/users?groupDescriptors={0}&api-version={1}", Convert.ToString(group.descriptor), con.VersionNumber);
                                var json = CreatePrincipalReqBody(model.Email);
                                var response = httpService.Post(json, urpParams);
                            }
                            if (group.displayName.ToLower() == model.ProjectName.ToLower() + " team")
                            {
                                string urpParams = string.Format("_apis/graph/users?groupDescriptors={0}&api-version={1}", Convert.ToString(group.descriptor), con.VersionNumber);
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
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
                        dict.Add(ext.extensionName, false);
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
                            if (extension.extensionName.ToLower() == ext.ExtensionDisplayName.ToLower())
                            {
                                dict[extension.extensionName] = true;
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
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                //return Json(new { message = "Error", status = "false" }, JsonRequestBehavior.AllowGet);
                ExtensionRequired = false;
            }
            return ExtensionRequired;
        }

        private void CreateVaribaleGroups(Project model, Configuration _variableGroups)
        {
            VariableGroups variableGroups = new VariableGroups(_variableGroups);
            model.Environment.VariableGroups = new Dictionary<int, string>();
            string filePath = GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, @"\VariableGroups\VariableGroup.json");
            if (File.Exists(filePath))
            {
                string jsonString = model.ReadJsonFile(filePath);
                GetVariableGroups.Groups groups = JsonConvert.DeserializeObject<GetVariableGroups.Groups>(jsonString);
                if (groups.count > 0)
                {
                    foreach (var group in groups.value)
                    {
                        GetVariableGroups.VariableGroupsCreateResponse response = variableGroups.PostVariableGroups(JsonConvert.SerializeObject(group));
                        if (!string.IsNullOrEmpty(response.name))
                        {
                            model.Environment.VariableGroups.Add(response.id, response.name);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checkign for template existance - if template is present in private path, return true else return false
        /// </summary>
        /// <param name="templatName"></param>
        /// <returns></returns>
        public bool WhereDoseTemplateBelongTo(string templatName)
        {
            string privatePath = HostingEnvironment.MapPath("~") + @"\PrivateTemplates\";
            string privateTemplate = Path.Combine(privatePath, templatName);

            if (!Directory.Exists(privatePath))
            {
                Directory.CreateDirectory(privatePath);
            }
            string[] privatedirs = Directory.GetDirectories(privatePath);
            
            if (privatedirs.Contains(privateTemplate))
            {
                return true;
            }
            return false;
        }
    }
}