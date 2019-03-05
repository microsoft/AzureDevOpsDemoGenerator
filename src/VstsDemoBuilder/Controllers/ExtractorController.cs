using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using VstsDemoBuilder.Extensions;
using VstsDemoBuilder.ExtractorModels;
using VstsDemoBuilder.Models;
using VstsRestAPI;
using VstsRestAPI.ExtensionManagement;
using VstsRestAPI.Extractor;
using VstsRestAPI.ProjectsAndTeams;
using VstsRestAPI.Service;
using VstsRestAPI.Viewmodel.Extractor;
using VstsRestAPI.Viewmodel.ProjectAndTeams;
using VstsRestAPI.WorkItemAndTracking;

namespace VstsDemoBuilder.Controllers
{

    public class ExtractorController : Controller
    {
        private AccessDetails accessDetails = new AccessDetails();
        private EnvironmentController con = new EnvironmentController();
        private static readonly object objLock = new object();
        private static Dictionary<string, string> statusMessages;
        public List<string> errorMessages = new List<string>();
        private delegate string[] ProcessEnvironment(Project model);

        private ExtractorAnalysis analysis = new ExtractorAnalysis();

        private string projectSelectedToExtract = string.Empty;
        private string extractedTemplatePath = string.Empty;
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
        private static Dictionary<string, string> StatusMessages
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
        [AllowAnonymous]
        public ActionResult NotFound()
        {
            return View();
        }

        // Extractor index page
        [AllowAnonymous]
        public ActionResult Index(ProjectList.ProjectDetails model)
        {
            string pat = "";
            string email = "";
            if (Session["PAT"] != null)
            {
                pat = Session["PAT"].ToString();
            }
            if (Session["Email"] != null)
            {
                email = Session["PAT"].ToString();
            }
            if (Session["EnableExtractor"] == null || Session["EnableExtractor"].ToString().ToLower() == "false")
            {
                return RedirectToAction("NotFound");
            }
            if (string.IsNullOrEmpty(pat))
            {
                return Redirect("../Account/Verify");
            }
            else
            {
                accessDetails.access_token = pat;
                ProfileDetails profile = con.GetProfile(accessDetails);
                Session["User"] = profile.displayName;
                Session["PAT"] = pat;
                AccountsResponse.AccountList accountList = con.GetAccounts(profile.id, accessDetails);
                model.accessToken = accessDetails.access_token;
                model.accountsForDropdown = new List<string>();
                if (accountList.count > 0)
                {
                    foreach (var account in accountList.value)
                    {
                        model.accountsForDropdown.Add(account.accountName);
                    }
                    model.accountsForDropdown.Sort();
                }
                else
                {
                    model.accountsForDropdown.Add("Select Organization");
                    ViewBag.AccDDError = "Could not load your organizations. Please change the directory in profile page of Azure DevOps Organization and try again.";
                }
                return View(model);
            }
        }

        // Get the current progress of work done
        [HttpGet]
        [AllowAnonymous]
        public ContentResult GetCurrentProgress(string id)
        {
            this.ControllerContext.HttpContext.Response.AddHeader("cache-control", "no-cache");
            var currentProgress = GetStatusMessage(id).ToString();
            return Content(currentProgress);
        }

        // Get status message to display
        [HttpGet]
        [AllowAnonymous]
        public string GetStatusMessage(string id)
        {
            lock (objLock)
            {
                string message = string.Empty;
                if (StatusMessages.Keys.Count(x => x == id) == 1)
                {
                    message = StatusMessages[id];
                }
                else
                {
                    return "100";
                }

                if (id.EndsWith("_Errors"))
                {
                    RemoveKey(id);
                }

                return message;
            }
        }

        public void RemoveKey(string id)
        {
            lock (objLock)
            {
                StatusMessages.Remove(id);
            }
        }

        // Get Project List from the selected Organization
        [AllowAnonymous]
        public JsonResult GetprojectList(string accname, string pat)
        {
            string defaultHost = System.Configuration.ConfigurationManager.AppSettings["DefaultHost"];
            string ProjectCreationVersion = System.Configuration.ConfigurationManager.AppSettings["ProjectCreationVersion"];

            Configuration config = new Configuration() { AccountName = accname, PersonalAccessToken = pat, UriString = defaultHost + accname, VersionNumber = ProjectCreationVersion };
            Projects projects = new Projects(config);
            ProjectsResponse.ProjectResult projectResult = projects.GetListOfProjects();
            try
            {
                if (string.IsNullOrEmpty(projectResult.errmsg))
                {
                    if (projectResult.count == 0)
                    {
                        projectResult.errmsg = "No projects found!";
                    }
                    return Json(projectResult, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                projectResult.errmsg = ex.Message.ToString();
                string message = ex.Message.ToString();
            }
            return Json(projectResult, JsonRequestBehavior.AllowGet);
        }

        //Get Project Properties to knwo which process template it is following
        [AllowAnonymous]
        public JsonResult GetProjectProperties(string accname, string project, string _credentials)
        {
            string defaultHost = System.Configuration.ConfigurationManager.AppSettings["DefaultHost"];
            string ProjectPropertyVersion = System.Configuration.ConfigurationManager.AppSettings["ProjectPropertyVersion"];

            Configuration config = new Configuration() { AccountName = accname, PersonalAccessToken = _credentials, UriString = defaultHost + accname, VersionNumber = ProjectPropertyVersion, ProjectId = project };

            ProjectProperties.Properties load = new ProjectProperties.Properties();
            Projects projects = new Projects(config);
            load = projects.GetProjectProperties();
            if (load.count > 0)
            {
                if (load.TypeClass != null)
                {
                    return Json(load, JsonRequestBehavior.AllowGet);
                }
            }
            return new JsonResult();

        }

        // End the extraction process
        public void EndEnvironmentSetupProcess(IAsyncResult result)
        {
            ProcessEnvironment processTask = (ProcessEnvironment)result.AsyncState;
            string[] strResult = processTask.EndInvoke(result);

            RemoveKey(strResult[0]);
            if (StatusMessages.Keys.Count(x => x == strResult[0] + "_Errors") == 1)
            {
                string errorMessages = statusMessages[strResult[0] + "_Errors"];
                if (errorMessages != "")
                {
                    //also, log message to file system
                    string logPath = Server.MapPath("~") + @"\Log";
                    string accountName = strResult[1];
                    string fileName = string.Format("{0}_{1}.txt", "Extractor_", DateTime.Now.ToString("ddMMMyyyy_HHmmss"));

                    if (!Directory.Exists(logPath))
                    {
                        Directory.CreateDirectory(logPath);
                    }
                    System.IO.File.AppendAllText(Path.Combine(logPath, fileName), errorMessages);

                    //Create ISSUE work item with error details in VSTSProjectgenarator account
                    string patBase64 = System.Configuration.ConfigurationManager.AppSettings["PATBase64"];
                    string url = System.Configuration.ConfigurationManager.AppSettings["URL"];
                    string projectId = System.Configuration.ConfigurationManager.AppSettings["PROJECTID"];
                    string issueName = string.Format("{0}_{1}", "Extractor_", DateTime.Now.ToString("ddMMMyyyy_HHmmss"));
                    IssueWI objIssue = new IssueWI();

                    string logWIT = "true"; //System.Configuration.ConfigurationManager.AppSettings["LogWIT"];
                    if (logWIT == "true")
                    {
                        objIssue.CreateIssueWI(patBase64, "4.1", url, issueName, errorMessages, projectId, "Extractor");
                    }
                }
            }
        }

        //Analyze the selected project to know what all the artifacts it has
        public static ProjectConfigurations ProjectConfiguration(Project model)
        {
            string repoVersion = System.Configuration.ConfigurationManager.AppSettings["RepoVersion"];
            string buildVersion = System.Configuration.ConfigurationManager.AppSettings["BuildVersion"];
            string releaseVersion = System.Configuration.ConfigurationManager.AppSettings["ReleaseVersion"];
            string wikiVersion = System.Configuration.ConfigurationManager.AppSettings["WikiVersion"];
            string boardVersion = System.Configuration.ConfigurationManager.AppSettings["BoardVersion"];
            string workItemsVersion = System.Configuration.ConfigurationManager.AppSettings["WorkItemsVersion"];
            string releaseHost = System.Configuration.ConfigurationManager.AppSettings["ReleaseHost"];
            string defaultHost = System.Configuration.ConfigurationManager.AppSettings["DefaultHost"];
            string extensionHost = System.Configuration.ConfigurationManager.AppSettings["ExtensionHost"];
            string getReleaseVersion = System.Configuration.ConfigurationManager.AppSettings["GetRelease"];
            string agentQueueVersion = System.Configuration.ConfigurationManager.AppSettings["AgentQueueVersion"];
            string extensionVersion = System.Configuration.ConfigurationManager.AppSettings["ExtensionVersion"];
            string endpointVersion = System.Configuration.ConfigurationManager.AppSettings["EndPointVersion"];
            ProjectConfigurations projectConfig = new ProjectConfigurations();

            projectConfig.AgentQueueConfig = new Configuration() { UriString = defaultHost + model.accountName + "/", PersonalAccessToken = model.accessToken, Project = model.ProjectName, AccountName = model.accountName, Id = model.id, VersionNumber = wikiVersion };
            projectConfig.WorkItemConfig = new Configuration() { UriString = defaultHost + model.accountName + "/", PersonalAccessToken = model.accessToken, Project = model.ProjectName, AccountName = model.accountName, Id = model.id, VersionNumber = wikiVersion };
            projectConfig.BuildDefinitionConfig = new Configuration() { UriString = defaultHost + model.accountName + "/", PersonalAccessToken = model.accessToken, Project = model.ProjectName, AccountName = model.accountName, Id = model.id, VersionNumber = buildVersion };
            projectConfig.ReleaseDefinitionConfig = new Configuration() { UriString = releaseHost + model.accountName + "/", PersonalAccessToken = model.accessToken, Project = model.ProjectName, AccountName = model.accountName, Id = model.id, VersionNumber = releaseVersion };
            projectConfig.RepoConfig = new Configuration() { UriString = defaultHost + model.accountName + "/", PersonalAccessToken = model.accessToken, Project = model.ProjectName, AccountName = model.accountName, Id = model.id, VersionNumber = repoVersion };
            projectConfig.BoardConfig = new Configuration() { UriString = defaultHost + model.accountName + "/", PersonalAccessToken = model.accessToken, Project = model.ProjectName, AccountName = model.accountName, Id = model.id, VersionNumber = boardVersion };
            projectConfig.Config = new Configuration() { UriString = defaultHost + model.accountName + "/", PersonalAccessToken = model.accessToken, Project = model.ProjectName, AccountName = model.accountName, Id = model.id };
            projectConfig.GetReleaseConfig = new Configuration() { UriString = releaseHost + model.accountName + "/", PersonalAccessToken = model.accessToken, Project = model.ProjectName, AccountName = model.accountName, Id = model.id, VersionNumber = getReleaseVersion };
            projectConfig.ExtensionConfig = new Configuration() { UriString = extensionHost + model.accountName + "/", PersonalAccessToken = model.accessToken, Project = model.ProjectName, AccountName = model.accountName, Id = model.id, VersionNumber = extensionVersion };
            projectConfig.EndpointConfig = new Configuration() { UriString = defaultHost + model.accountName + "/", PersonalAccessToken = model.accessToken, Project = model.ProjectName, AccountName = model.accountName, Id = model.id, VersionNumber = endpointVersion };

            return projectConfig;
        }

        #region GetCounts
        // Start Analysis process
        [AllowAnonymous]
        public JsonResult AnalyzeProject(Project model)
        {
            ProjectConfigurationDetails.AppConfig = ProjectConfiguration(model);
            projectSelectedToExtract = model.ProjectName;
            analysis.teamCount = GetTeamsCount(ProjectConfigurationDetails.AppConfig.BoardConfig);
            analysis.IterationCount = GetIterationsCount(ProjectConfigurationDetails.AppConfig.BoardConfig);
            analysis.WorkItemCounts = GetWorkItemsCount(ProjectConfigurationDetails.AppConfig.WorkItemConfig);
            GetBuildDefinitionCount(ProjectConfigurationDetails.AppConfig.BuildDefinitionConfig);
            GetReleaseDefinitionCount(ProjectConfigurationDetails.AppConfig.ReleaseDefinitionConfig);
            analysis.ErrorMessages = errorMessages;
            return Json(analysis, JsonRequestBehavior.AllowGet);
        }

        // Get Teams Count
        [AllowAnonymous]
        public int GetTeamsCount(Configuration con)
        {
            VstsRestAPI.Extractor.ClassificationNodes nodes = new VstsRestAPI.Extractor.ClassificationNodes(con);
            TeamList teamList = nodes.ExportTeamList("");
            int count = 0;
            if (teamList.value != null)
            {
                count = teamList.value.Count;
            }
            return count;
        }
        // Get Iteration Count
        public int GetIterationsCount(Configuration con)
        {
            VstsRestAPI.Extractor.ClassificationNodes nodes = new VstsRestAPI.Extractor.ClassificationNodes(con);
            GetINumIteration.Iterations iterations = new GetINumIteration.Iterations();
            iterations = nodes.GetiterationCount();
            if (iterations.count > 0)
            {
                return iterations.count;
            }
            else
            {
                if (!(string.IsNullOrEmpty(nodes.LastFailureMessage)))
                {
                    errorMessages.Add("Error while fetching iteration(s) count: " + nodes.LastFailureMessage);
                }
                return 0;
            }

        }

        // Get Work Items Details
        public Dictionary<string, int> GetWorkItemsCount(Configuration con)
        {
            string[] workItemtypes = { "Epic", "Feature", "Product Backlog Item", "Task", "Test Case", "Bug", "User Story", "Test Suite", "Test Plan" };
            GetWorkItemsCount itemsCount = new GetWorkItemsCount(con);
            Dictionary<string, int> fetchedWorkItemsCount = new Dictionary<string, int>();
            if (workItemtypes.Length > 0)
            {
                foreach (var workItem in workItemtypes)
                {
                    WorkItemFetchResponse.WorkItems WITCount = itemsCount.GetWorkItemsfromSource(workItem);
                    if (WITCount.count > 0)
                    {
                        fetchedWorkItemsCount.Add(workItem, WITCount.count);
                    }
                    else if (!string.IsNullOrEmpty(itemsCount.LastFailureMessage))
                    {
                        errorMessages.Add("Error while querying work items: " + itemsCount.LastFailureMessage);
                    }
                }
            }

            return fetchedWorkItemsCount;
        }

        //Get Build Definitions count
        public void GetBuildDefinitionCount(Configuration con)
        {
            BuildandReleaseDefs buildandReleaseDefs = new BuildandReleaseDefs(con);
            GetBuildDefResponse.BuildDef buildDef = new GetBuildDefResponse.BuildDef();
            buildDef = buildandReleaseDefs.GetBuildDefCount();
            if (buildDef.count > 0)
            {
                analysis.BuildDefCount = buildDef.count;
            }
            else if (!string.IsNullOrEmpty(buildandReleaseDefs.LastFailureMessage))
            {
                errorMessages.Add("Error while fetching build definition count: " + buildandReleaseDefs.LastFailureMessage);
            }
            else
            {
                analysis.BuildDefCount = 0;
            }
        }

        // Get Release Definitions count
        public void GetReleaseDefinitionCount(Configuration con)
        {
            BuildandReleaseDefs buildandReleaseDefs = new BuildandReleaseDefs(con);
            GetReleaseDefResponse.ReleaseDef releaseDef = new GetReleaseDefResponse.ReleaseDef();
            releaseDef = buildandReleaseDefs.GetReleaseDefCount();
            if (releaseDef.count > 0)
            {
                analysis.ReleaseDefCount = releaseDef.count;
            }
            else if (!string.IsNullOrEmpty(buildandReleaseDefs.LastFailureMessage))
            {
                errorMessages.Add("Error while fetching release definition count: " + buildandReleaseDefs.LastFailureMessage);
            }
            else
            {
                analysis.ReleaseDefCount = 0;
            }
        }
        #endregion

        #region Extract Template
        //Initiate the extraction process
        [HttpPost]
        [AllowAnonymous]
        public bool StartEnvironmentSetupProcess(Project model)
        {
            System.Web.HttpContext.Current.Session["Project"] = model.ProjectName;
            AddMessage(model.id, string.Empty);
            AddMessage(model.id.ErrorId(), string.Empty);
            ProcessEnvironment processTask = new ProcessEnvironment(GenerateTemplateArifacts);
            processTask.BeginInvoke(model, new AsyncCallback(EndEnvironmentSetupProcess), processTask);
            return true;
        }

        //Extract the project artifacts
        [AllowAnonymous]
        public string[] GenerateTemplateArifacts(Project model)
        {
            extractedTemplatePath = Server.MapPath("~") + @"ExtractedTemplate\";
            AddMessage(model.id, "");

            GetInstalledExtensions(ProjectConfigurationDetails.AppConfig.ExtensionConfig);

            ProjectConfigurationDetails.AppConfig = ProjectConfiguration(model);
            ExportTeams(ProjectConfigurationDetails.AppConfig.BoardConfig, model.ProcessTemplate, model.ProjectId);

            if (ExportIterations(ProjectConfigurationDetails.AppConfig.BoardConfig))
            {
                AddMessage(model.id, "Iterations Definition");
            }
            string extractedFolderName = extractedTemplatePath + model.ProjectName;
            string filePathToRead = Server.MapPath("~") + @"\\PreSetting";

            string projectSetting = "";
            projectSetting = filePathToRead + "\\ProjectSettings.json";
            projectSetting = System.IO.File.ReadAllText(projectSetting);
            projectSetting = projectSetting.Replace("$type$", model.ProcessTemplate);
            System.IO.File.WriteAllText(extractedFolderName + "\\ProjectSettings.json", projectSetting);

            string Extensions = "";
            Extensions = filePathToRead + "\\DemoExtensions.json";
            Extensions = System.IO.File.ReadAllText(Extensions);
            System.IO.File.WriteAllText(extractedFolderName + "\\DemoExtensions.json", Extensions);

            string projectTemplate = "";
            projectTemplate = filePathToRead + "\\ProjectTemplate.json";
            projectTemplate = System.IO.File.ReadAllText(projectTemplate);
            System.IO.File.WriteAllText(extractedFolderName + "\\ProjectTemplate.json", projectTemplate);

            string teamArea = "";
            teamArea = filePathToRead + "\\TeamArea.json";
            teamArea = System.IO.File.ReadAllText(teamArea);
            System.IO.File.WriteAllText(extractedFolderName + "\\TeamArea.json", teamArea);
            AddMessage(model.id, "Team Areas");

            ExportWorkItems(ProjectConfigurationDetails.AppConfig.WorkItemConfig);
            AddMessage(model.id, "Work Items");

            ExportRepositoryList(ProjectConfigurationDetails.AppConfig.RepoConfig);
            AddMessage(model.id, "Repository and Service Endpoint");

            GetServiceEndpoints(ProjectConfigurationDetails.AppConfig.EndpointConfig);

            int count = GetBuildDefinitions(ProjectConfigurationDetails.AppConfig.BuildDefinitionConfig, ProjectConfigurationDetails.AppConfig.RepoConfig);
            if (count >= 1)
            {
                AddMessage(model.id, "Build Definition");
            }

            int relCount = GeneralizingGetReleaseDefinitions(ProjectConfigurationDetails.AppConfig.ReleaseDefinitionConfig, ProjectConfigurationDetails.AppConfig.AgentQueueConfig);
            if (relCount >= 1)
            {
                AddMessage(model.id, "Release Definition");
            }

            //string startPath = Path.Combine(Server.MapPath("~") + @"ExtractedTemplate\", model.ProjectName);

            //string zipPath = Path.Combine(Server.MapPath("~") + @"ExtractedTemplate\", model.ProjectName + ".zip");
            //if (System.IO.File.Exists(zipPath))
            //{
            //    System.IO.File.Delete(zipPath);
            //}
            //zipPath = Path.Combine(Server.MapPath("~") + @"ExtractedTemplate\", model.ProjectName + ".zip");
            //ZipFile.CreateFromDirectory(startPath, zipPath);
            //Directory.Delete(Path.Combine(Server.MapPath("~") + @"ExtractedTemplate\", model.ProjectName), true);
            StatusMessages[model.id] = "100";
            return new string[] { model.id, "" };
        }
        // Get Team List to write into file
        public bool ExportTeams(Configuration con, string processTemplate, string projectID)
        {
            string defaultTeamID = string.Empty;
            VstsRestAPI.Extractor.ClassificationNodes nodes = new VstsRestAPI.Extractor.ClassificationNodes(con);
            TeamList _team = new TeamList();
            string ProjectPropertyVersion = System.Configuration.ConfigurationManager.AppSettings["ProjectPropertyVersion"];
            con.VersionNumber = ProjectPropertyVersion;
            con.ProjectId = projectID;
            Projects projects = new Projects(con);
            ProjectProperties.Properties projectProperties = projects.GetProjectProperties();
            if (projectProperties.count > 0)
            {
                defaultTeamID = projectProperties.value.Where(x => x.name == "System.Microsoft.TeamFoundation.Team.Default").FirstOrDefault().value;
            }
            _team = nodes.ExportTeamList(defaultTeamID);
            if (_team.value != null)
            {
                AddMessage(con.Id, "Teams");

                string fetchedJson = JsonConvert.SerializeObject(_team.value, Formatting.Indented);
                if (fetchedJson != "")
                {
                    if (!Directory.Exists(extractedTemplatePath + con.Project + "\\Teams"))
                    {
                        Directory.CreateDirectory(extractedTemplatePath + con.Project + "\\Teams");
                    }
                    System.IO.File.WriteAllText(extractedTemplatePath + con.Project + "\\Teams\\Teams.json", fetchedJson);

                    List<string> boardTypes = new List<string>();
                    boardTypes.Add("Epics"); boardTypes.Add("Features");

                    if (processTemplate.ToLower() == "agile")
                    { boardTypes.Add("Stories"); }
                    else { boardTypes.Add("Backlog Items"); }

                    foreach (var team in _team.value)
                    {
                        List<BoardColumnResponseScrum.ColumnResponse> columnResponsesScrum = new List<BoardColumnResponseScrum.ColumnResponse>();
                        List<BoardColumnResponseAgile.ColumnResponse> columnResponsesAgile = new List<BoardColumnResponseAgile.ColumnResponse>();
                        List<ExportBoardRows.Rows> boardRows = new List<ExportBoardRows.Rows>();

                        ExportTeamSetting.Setting listTeamSetting = new ExportTeamSetting.Setting();

                        List<JObject> jObjCardFieldList = new List<JObject>();
                        List<JObject> jObjcardStyleList = new List<JObject>();
                        string teamFolderPath = extractedTemplatePath + con.Project + "\\Teams\\" + team.name;
                        if (!Directory.Exists(teamFolderPath))
                        {
                            Directory.CreateDirectory(teamFolderPath);
                        }
                        //Export Board Colums for each team
                        con.Team = team.name;

                        VstsRestAPI.Extractor.ClassificationNodes teamNodes = new VstsRestAPI.Extractor.ClassificationNodes(con);
                        foreach (var boardType in boardTypes)
                        {
                            var response = teamNodes.ExportBoardColums(boardType);
                            if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                if (processTemplate.ToLower() == "scrum")
                                {
                                    string res = response.Content.ReadAsStringAsync().Result;
                                    BoardColumnResponseScrum.ColumnResponse scrumColumns = JsonConvert.DeserializeObject<BoardColumnResponseScrum.ColumnResponse>(res);
                                    scrumColumns.BoardName = boardType;
                                    columnResponsesScrum.Add(scrumColumns);
                                }
                                else if (processTemplate.ToLower() == "agile")
                                {
                                    string res = response.Content.ReadAsStringAsync().Result;
                                    BoardColumnResponseAgile.ColumnResponse agileColumns = JsonConvert.DeserializeObject<BoardColumnResponseAgile.ColumnResponse>(res);
                                    agileColumns.BoardName = boardType;
                                    columnResponsesAgile.Add(agileColumns);
                                }
                                AddMessage(con.Id, "Board Columns");
                                Thread.Sleep(2000);
                            }
                            else
                            {
                                var errorMessage = response.Content.ReadAsStringAsync();
                                string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                                teamNodes.LastFailureMessage = error;
                                AddMessage(con.Id.ErrorId(), "Error occured while exporting Board Columns: " + teamNodes.LastFailureMessage);
                            }

                            //Export board rows for each team
                            ExportBoardRows.Rows rows = teamNodes.ExportBoardRows(boardType);
                            if (rows.value != null && rows.value.Count > 0)
                            {
                                rows.BoardName = boardType;
                                boardRows.Add(rows);
                                AddMessage(con.Id, "Board Rows");
                                Thread.Sleep(2000);
                            }
                            else if (!string.IsNullOrEmpty(teamNodes.LastFailureMessage))
                            {
                                AddMessage(con.Id.ErrorId(), "Error occured while exporting Board Rows: " + teamNodes.LastFailureMessage);
                            }


                            //Export Card Fields for each team
                            var cardFieldResponse = teamNodes.ExportCardFields(boardType);
                            if (cardFieldResponse.IsSuccessStatusCode && cardFieldResponse.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                string res = cardFieldResponse.Content.ReadAsStringAsync().Result;
                                JObject jObj = JsonConvert.DeserializeObject<JObject>(res);
                                jObj["BoardName"] = boardType;
                                jObjCardFieldList.Add(jObj);
                                AddMessage(con.Id, "Card fields Definition");

                            }
                            else
                            {
                                var errorMessage = cardFieldResponse.Content.ReadAsStringAsync();
                                string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                                teamNodes.LastFailureMessage = error;
                                AddMessage(con.Id.ErrorId(), "Error occured while exporting Card Fields: " + teamNodes.LastFailureMessage);
                            }

                            //// Export card styles for each team
                            var cardStyleResponse = teamNodes.ExportCardStyle(boardType);
                            if (cardStyleResponse.IsSuccessStatusCode && cardStyleResponse.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                string res = cardStyleResponse.Content.ReadAsStringAsync().Result;
                                res = res.Replace(con.Project, "$ProjectName$");
                                JObject jObj = JsonConvert.DeserializeObject<JObject>(res);
                                jObj["BoardName"] = boardType;
                                var style = jObj;
                                style["url"] = "";
                                style["_links"] = "{}";
                                jObjcardStyleList.Add(jObj);
                                AddMessage(con.Id, "Card style");

                            }
                            else
                            {
                                var errorMessage = cardStyleResponse.Content.ReadAsStringAsync();
                                string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                                teamNodes.LastFailureMessage = error;
                                AddMessage(con.Id.ErrorId(), "Error occured while exporting Card Styles: " + teamNodes.LastFailureMessage);
                            }
                        }
                        //Export Team Setting for each team
                        ExportTeamSetting.Setting teamSetting = teamNodes.ExportTeamSetting();
                        if (teamSetting.backlogVisibilities != null)
                        {
                            listTeamSetting = teamSetting;
                            AddMessage(con.Id, "Team Settings Definition");
                        }
                        else if (!string.IsNullOrEmpty(teamNodes.LastFailureMessage))
                        {
                            AddMessage(con.Id.ErrorId(), "Error occured while exporting Team Setting: " + teamNodes.LastFailureMessage);
                        }

                        if (columnResponsesAgile.Count > 0)
                        {
                            System.IO.File.WriteAllText(teamFolderPath + "\\BoardColumns.json", JsonConvert.SerializeObject(columnResponsesAgile, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                        }
                        if (columnResponsesScrum.Count > 0)
                        {
                            System.IO.File.WriteAllText(teamFolderPath + "\\BoardColumns.json", JsonConvert.SerializeObject(columnResponsesScrum, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                        }
                        if (boardRows.Count > 0)
                        {
                            System.IO.File.WriteAllText(teamFolderPath + "\\BoardRows.json", JsonConvert.SerializeObject(boardRows, Formatting.Indented));
                        }
                        if (!string.IsNullOrEmpty(listTeamSetting.bugsBehavior))
                        {
                            System.IO.File.WriteAllText(teamFolderPath + "\\TeamSetting.json", JsonConvert.SerializeObject(listTeamSetting, Formatting.Indented));
                        }
                        if (jObjCardFieldList.Count > 0)
                        {
                            System.IO.File.WriteAllText(teamFolderPath + "\\CardFields.json", JsonConvert.SerializeObject(jObjCardFieldList, Formatting.Indented));
                        }
                        if (jObjcardStyleList.Count > 0)
                        {
                            System.IO.File.WriteAllText(teamFolderPath + "\\CardStyles.json", JsonConvert.SerializeObject(jObjcardStyleList, Formatting.Indented));
                        }
                    }

                    return true;
                }
                else if (!string.IsNullOrEmpty(nodes.LastFailureMessage))
                {
                    AddMessage(con.Id.ErrorId(), nodes.LastFailureMessage);
                    string error = nodes.LastFailureMessage;
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                AddMessage(con.Id.ErrorId(), nodes.LastFailureMessage);
                return false;
            }
        }

        //Export Iterations
        public bool ExportIterations(Configuration con)
        {
            try
            {
                VstsRestAPI.Extractor.ClassificationNodes nodes = new VstsRestAPI.Extractor.ClassificationNodes(con);
                //ExportIterations.Iterations viewModel = new ExportIterations.Iterations();
                ExportedIterations.Iterations viewModel = nodes.ExportIterationsToSave();
                string fetchedJson = JsonConvert.SerializeObject(viewModel, Formatting.Indented);
                if (fetchedJson != "")
                {
                    if (!Directory.Exists(extractedTemplatePath + con.Project))
                    {
                        Directory.CreateDirectory(extractedTemplatePath + con.Project);
                    }
                    System.IO.File.WriteAllText(extractedTemplatePath + con.Project + "\\Iterations.json", fetchedJson);
                    return true;
                }
                else
                {
                    string error = nodes.LastFailureMessage;
                    AddMessage(con.Id.ErrorId(), error);
                    return false;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        // Get Work items to write into file
        public void ExportWorkItems(Configuration con)
        {
            string[] workItemtypes = { "Epic", "Feature", "Product Backlog Item", "Task", "Test Case", "Bug", "User Story", "Test Suite", "Test Plan" };
            if (!Directory.Exists(extractedTemplatePath + con.Project))
            {
                Directory.CreateDirectory(extractedTemplatePath + con.Project);
            }

            if (workItemtypes.Length > 0)
            {
                foreach (var WIT in workItemtypes)
                {
                    GetWorkItemsCount WorkitemsCount = new GetWorkItemsCount(con);
                    WorkItemFetchResponse.WorkItems fetchedWorkItem = WorkitemsCount.GetWorkItemsfromSource(WIT);
                    string workItemJson = JsonConvert.SerializeObject(fetchedWorkItem, Formatting.Indented);
                    if (fetchedWorkItem.count > 0)
                    {
                        string item = WIT;
                        if (!Directory.Exists(extractedTemplatePath + con.Project + "\\WorkItems"))
                        {
                            Directory.CreateDirectory(extractedTemplatePath + con.Project + "\\WorkItems");
                        }
                        System.IO.File.WriteAllText(extractedTemplatePath + con.Project + "\\WorkItems\\" + item + ".json", workItemJson);
                    }
                    else if (!string.IsNullOrEmpty(WorkitemsCount.LastFailureMessage))
                    {
                        AddMessage(con.Id.ErrorId(), WorkitemsCount.LastFailureMessage);
                    }
                }
            }
        }

        // Get Repository list to create Service end point json with respect to the repositiory
        // and also create the import source code json
        // It works only for the user who is having access to both Source and Target repositories in the organization with the same UserID
        public void ExportRepositoryList(Configuration con)
        {
            BuildandReleaseDefs repolist = new BuildandReleaseDefs(con);
            RepositoryList.Repository repos = repolist.GetRepoList();
            if (repos.count > 0)
            {
                foreach (var repo in repos.value)
                {
                    string preSettingPath = Server.MapPath("~") + @"PreSetting";
                    string templateFolderPath = extractedTemplatePath + con.Project;
                    string host = con.UriString + con.Project;
                    string sourceCodeJson = System.IO.File.ReadAllText(preSettingPath + "\\ImportSourceCode.json");
                    sourceCodeJson = sourceCodeJson.Replace("$Host$", host).Replace("$Repo$", repo.name);
                    string endPointJson = System.IO.File.ReadAllText(preSettingPath + "\\ServiceEndPoint.json");
                    endPointJson = endPointJson.Replace("$Host$", host).Replace("$Repo$", repo.name);
                    if (!Directory.Exists(templateFolderPath + "\\ImportSourceCode"))
                    {
                        Directory.CreateDirectory(templateFolderPath + "\\ImportSourceCode");
                        System.IO.File.WriteAllText(templateFolderPath + "\\ImportSourceCode\\" + repo.name + ".json", sourceCodeJson);
                    }
                    else
                    {
                        System.IO.File.WriteAllText(templateFolderPath + "\\ImportSourceCode\\" + repo.name + ".json", sourceCodeJson);
                    }
                    if (!Directory.Exists(templateFolderPath + "\\ServiceEndpoints"))
                    {
                        Directory.CreateDirectory(templateFolderPath + "\\ServiceEndpoints");
                        System.IO.File.WriteAllText(templateFolderPath + "\\ServiceEndpoints\\" + repo.name + "-code.json", endPointJson);
                    }
                    else
                    {
                        System.IO.File.WriteAllText(templateFolderPath + "\\ServiceEndpoints\\" + repo.name + "-code.json", endPointJson);
                    }
                }
            }
        }

        // Get the Build definitions to write into file
        public int GetBuildDefinitions(Configuration con, Configuration repoCon)
        {
            BuildandReleaseDefs buildandReleaseDefs = new BuildandReleaseDefs(con);
            List<JObject> builds = buildandReleaseDefs.ExportBuildDefinitions();
            BuildandReleaseDefs repoDefs = new BuildandReleaseDefs(repoCon);
            RepositoryList.Repository repo = repoDefs.GetRepoList();
            if (builds.Count > 0)
            {
                int count = 1;
                //creating ImportCode Json file
                string templatePath = extractedTemplatePath + con.Project;
                foreach (JObject def in builds)
                {
                    string repoID = "";
                    var repoName = def["repository"]["name"];
                    foreach (var re in repo.value)
                    {
                        if (re.name == repoName.ToString())
                        {
                            repoID = re.id;
                        }
                    }
                    def["authoredBy"] = "{}";
                    def["project"] = "{}";
                    def["url"] = "";
                    def["uri"] = "";
                    def["id"] = "";
                    def["queue"]["id"] = "";
                    def["queue"]["url"] = "";
                    def["queue"]["_links"] = "{}";
                    def["queue"]["pool"]["id"] = "";
                    def["_links"] = "{}";
                    def["createdDate"] = "";

                    var process = def["process"];
                    if (process != null)
                    {
                        var phases = process["phases"];
                        if (phases != null)
                        {
                            foreach (var phase in phases)
                            {
                                phase["target"]["queue"] = "{}";
                                var steps = phase["steps"];
                                if (steps != null)
                                {
                                    foreach (var step in steps)
                                    {
                                        string keyConfig = System.IO.File.ReadAllText(Server.MapPath("~") + @"\\Templates\EndpointKeyConfig.json");
                                        KeyConfig.Keys keyC = new KeyConfig.Keys();
                                        keyC = JsonConvert.DeserializeObject<KeyConfig.Keys>(keyConfig);
                                        foreach (var key in keyC.keys)
                                        {
                                            string keyVal = step[key] != null ? step[key].ToString() : "";
                                            if (!string.IsNullOrEmpty(keyVal))
                                            {
                                                step[key] = "";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    var type = def["repository"]["type"];
                    if (type.ToString().ToLower() == "github")
                    {
                        def["repository"]["type"] = "Git";
                        def["repository"]["properties"]["fullName"] = "repository";
                        def["repository"]["properties"]["connectedServiceId"] = "$GitHub$";
                        def["repository"]["name"] = "repository";
                        string url = def["repository"]["url"].ToString();
                        if (url != "")
                        {
                            string endPointString = System.IO.File.ReadAllText(Server.MapPath("~") + @"PreSetting\\GitHubEndPoint.json");
                            endPointString = endPointString.Replace("$GitHubURL$", url);
                            Guid g = Guid.NewGuid();
                            string randStr = g.ToString().Substring(0, 8);
                            if (!Directory.Exists(extractedTemplatePath + con.Project + "\\ServiceEndpoints"))
                            {
                                Directory.CreateDirectory(extractedTemplatePath + con.Project + "\\ServiceEndpoints");
                                System.IO.File.WriteAllText(extractedTemplatePath + con.Project + "\\ServiceEndpoints\\GitHub-" + randStr + "-EndPoint.json", endPointString);
                            }
                            else
                            {
                                System.IO.File.WriteAllText(extractedTemplatePath + con.Project + "\\ServiceEndpoints\\GitHub-" + randStr + "-EndPoint.json", endPointString);
                            }
                        }
                    }
                    else if (type.ToString().ToLower() == "git")
                    {
                        string url = def["repository"]["url"].ToString();
                        string endPointString = System.IO.File.ReadAllText(Server.MapPath("~") + @"PreSetting\\GitHubEndPoint.json");
                        endPointString = endPointString.Replace("$GitHubURL$", url);
                        Guid g = Guid.NewGuid();
                        string randStr = g.ToString().Substring(0, 8);
                        if (!Directory.Exists(extractedTemplatePath + con.Project + "\\ServiceEndpoints"))
                        {
                            Directory.CreateDirectory(extractedTemplatePath + con.Project + "\\ServiceEndpoints");
                            System.IO.File.WriteAllText(extractedTemplatePath + con.Project + "\\ServiceEndpoints\\GitHub-" + randStr + "-EndPoint.json", endPointString);
                        }
                        else
                        {
                            System.IO.File.WriteAllText(extractedTemplatePath + con.Project + "\\ServiceEndpoints\\GitHub-" + randStr + "-EndPoint.json", endPointString);
                        }
                        def["repository"]["properties"]["connectedServiceId"] = "$GitHub$";
                    }
                    else
                    {
                        def["repository"]["id"] = "$" + repoName + "$";
                        def["repository"]["url"] = "";
                        def["repository"]["properties"]["connectedServiceId"] = "";
                    }
                    var input = def["processParameters"]["inputs"];
                    if (input != null)
                    {
                        if (input.HasValues)
                        {
                            foreach (var i in input)
                            {
                                i["defaultValue"] = "";

                            }
                        }
                    }
                    var build = def["build"];
                    if (build != null)
                    {
                        if (build.HasValues)
                        {
                            foreach (var b in build)
                            {
                                b["inputs"]["serverEndpoint"] = "";
                            }
                        }
                    }

                    if (!(Directory.Exists(templatePath + "\\BuildDefinitions")))
                    {
                        Directory.CreateDirectory(templatePath + "\\BuildDefinitions");
                        System.IO.File.WriteAllText(templatePath + "\\BuildDefinitions\\BuildDef" + count + ".json", JsonConvert.SerializeObject(def, Formatting.Indented));
                    }
                    else
                    {
                        System.IO.File.WriteAllText(templatePath + "\\BuildDefinitions\\BuildDef" + count + ".json", JsonConvert.SerializeObject(def, Formatting.Indented));
                    }
                    count = count + 1;
                }
                return count;
            }
            return 0;
        }

        // Generalizing the release definition method to make it work for All kind of Release definition
        public int GeneralizingGetReleaseDefinitions(Configuration con, Configuration _agentQueue)
        {
            try
            {
                BuildandReleaseDefs releaseDefs = new BuildandReleaseDefs(con);
                List<JObject> releases = releaseDefs.GetReleaseDefs();
                BuildandReleaseDefs agent = new BuildandReleaseDefs(_agentQueue);

                Dictionary<string, int> queue = agent.GetQueues();
                string templatePath = extractedTemplatePath + con.Project;
                int releasecount = 1;
                if (releases.Count > 0)
                {
                    foreach (JObject rel in releases)
                    {
                        rel["id"] = "";
                        rel["url"] = "";
                        rel["_links"] = "{}";
                        rel["createdBy"] = "{}";
                        rel["createdOn"] = "";
                        rel["modifiedBy"] = "{}";
                        rel["modifiedOn"] = "";
                        var env = rel["environments"];
                        foreach (var e in env)
                        {
                            e["badgeUrl"] = "";
                            var owner = e["owner"];
                            owner["id"] = "$OwnerId$";
                            owner["displayName"] = "$OwnerDisplayName$";
                            owner["uniqueName"] = "$OwnerUniqueName$";
                            owner["url"] = "";
                            owner["_links"] = "{}";
                            owner["imageUrl"] = "";
                            owner["descriptor"] = "";

                            var deployPhases = e["deployPhases"];
                            if (deployPhases.HasValues)
                            {
                                foreach (var dep in deployPhases)
                                {

                                    var deploymentInput = dep["deploymentInput"];
                                    var queueID = deploymentInput["queueId"];
                                    string queueName = "";
                                    if (queue != null)
                                    {
                                        if (queue.Count > 0)
                                        {
                                            var agenetName = queue.Where(x => x.Value.ToString() == queueID.ToString()).FirstOrDefault();
                                            if (agenetName.Key != null)
                                            {
                                                queueName = agenetName.Key.ToString();
                                            }
                                            else
                                            {
                                                queueName = "";
                                            }
                                        }
                                    }
                                    if (queueName != "")
                                    {
                                        deploymentInput["queueId"] = "$" + queueName + "$";
                                    }
                                    else
                                    {
                                        deploymentInput["queueId"] = "";
                                    }

                                    var workflow = dep["workflowTasks"];
                                    if (workflow.HasValues)
                                    {
                                        foreach (var flow in workflow)
                                        {
                                            var input = flow["inputs"];
                                            string keyConfig = System.IO.File.ReadAllText(Server.MapPath("~") + @"\\Templates\EndpointKeyConfig.json");
                                            KeyConfig.Keys keyC = new KeyConfig.Keys();
                                            keyC = JsonConvert.DeserializeObject<KeyConfig.Keys>(keyConfig);
                                            foreach (var key in keyC.keys)
                                            {
                                                string keyVal = input[key] != null ? input[key].ToString() : "";
                                                if (!string.IsNullOrEmpty(keyVal))
                                                {
                                                    input[key] = "";
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        var artifact = rel["artifacts"];
                        if (artifact.HasValues)
                        {
                            foreach (var art in artifact)
                            {
                                string buildName = art["definitionReference"]["definition"]["name"].ToString();

                                art["sourceId"] = "$ProjectId$:" + "$" + buildName + "-id$";
                                art["definitionReference"]["definition"]["id"] = "$" + buildName + "-id$";
                                art["definitionReference"]["project"]["id"] = "$ProjectId$";
                                art["definitionReference"]["project"]["name"] = "$ProjectName$";
                                art["definitionReference"]["artifactSourceDefinitionUrl"] = "{}";
                            }
                        }
                        if (!(Directory.Exists(templatePath + "\\ReleaseDefinitions")))
                        {
                            Directory.CreateDirectory(templatePath + "\\ReleaseDefinitions");
                            System.IO.File.WriteAllText(templatePath + "\\ReleaseDefinitions\\ReleaseDef" + releasecount + ".json", JsonConvert.SerializeObject(rel, Formatting.Indented));
                        }
                        else
                        {
                            System.IO.File.WriteAllText(templatePath + "\\ReleaseDefinitions\\ReleaseDef" + releasecount + ".json", JsonConvert.SerializeObject(rel, Formatting.Indented));
                        }
                        releasecount++;
                    }
                }
                return releasecount;
            }
            catch (Exception ex)
            {
                AddMessage(con.Id.ErrorId(), ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return 0;
        }

        public JsonResult GetInstalledExtensions(Configuration con)
        {
            GetListExtenison listExtenison = new GetListExtenison(con);
            List<RequiredExtensions.ExtensionWithLink> extensionList = new List<RequiredExtensions.ExtensionWithLink>();
            GetExtensions.ExtensionsList returnExtensionsList = listExtenison.GetInstalledExtensions();

            if (returnExtensionsList != null && returnExtensionsList.count > 0)
            {
                var builtInExtensions = returnExtensionsList.value.Where(x => x.flags == null).ToList();
                var trustedExtensions = returnExtensionsList.value.Where(x => x.flags != null && x.flags.ToString() == "trusted").ToList();
                builtInExtensions.AddRange(trustedExtensions);
                returnExtensionsList.value = builtInExtensions;

                foreach (var data in returnExtensionsList.value)
                {
                    RequiredExtensions.ExtensionWithLink extension = new RequiredExtensions.ExtensionWithLink();

                    extension.extensionId = data.extensionId;
                    extension.extensionName = data.extensionName;
                    extension.publisherId = data.publisherId;
                    extension.publisherName = data.publisherName;
                    extension.link = "<a href='" + string.Format("https://marketplace.visualstudio.com/items?itemName={0}.{1}", data.publisherId, data.extensionId) + "' target='_blank'><b>" + data.extensionName + "</b></a>";
                    extensionList.Add(extension);
                }
                RequiredExtensions.listExtension listExtension = new RequiredExtensions.listExtension();
                if (extensionList.Count > 0)
                {
                    listExtension.Extensions = extensionList;
                    if (!Directory.Exists(extractedTemplatePath + con.Project))
                    {
                        Directory.CreateDirectory(extractedTemplatePath + con.Project);
                    }
                    string fetchedJson = JsonConvert.SerializeObject(listExtension, Formatting.Indented);

                    System.IO.File.WriteAllText(extractedTemplatePath + con.Project + "\\Extensions.json", JsonConvert.SerializeObject(listExtension, Formatting.Indented));
                }
            }
            else if (!string.IsNullOrEmpty(listExtenison.LastFailureMessage))
            {
                AddMessage(con.Id.ErrorId(), "Some error occured while fetching extensions");
            }
            return Json(extensionList, JsonRequestBehavior.AllowGet);

        }

        public void GetServiceEndpoints(Configuration con)
        {
            ServiceEndPoint serviceEndPoint = new ServiceEndPoint(con);
            GetServiceEndpoints.ServiceEndPoint getServiceEndPoint = serviceEndPoint.GetServiceEndPoints();
            if (getServiceEndPoint.count > 0)
            {
                foreach (var endpoint in getServiceEndPoint.value)
                {
                    string endpointString = JsonConvert.SerializeObject(endpoint);
                    if (!Directory.Exists(extractedTemplatePath + con.Project + "\\ServiceEndpoints"))
                    {
                        Directory.CreateDirectory(extractedTemplatePath + con.Project + "\\ServiceEndpoints");
                        System.IO.File.WriteAllText(extractedTemplatePath + con.Project + "\\ServiceEndpoints\\", JsonConvert.SerializeObject(endpoint, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                    }
                    else
                    {
                        System.IO.File.WriteAllText(extractedTemplatePath + con.Project + "\\ServiceEndpoints\\" + endpoint.name + ".json", JsonConvert.SerializeObject(endpoint, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                    }
                }
            }
            else if (!string.IsNullOrEmpty(serviceEndPoint.LastFailureMessage))
            {
                AddMessage(con.Id.ErrorId(), "Error occured while fetchin service endpoints");
            }

        }
        #endregion end extract template
        // Remove the template folder after zipping it
        [AllowAnonymous]
        private void RemoveFolder()
        {
            string projectName = Session["Project"].ToString();
            if (projectName != "")
            {
                System.IO.File.Delete(extractedTemplatePath + projectName);
                System.IO.File.Delete(extractedTemplatePath + projectName + ".zip");
            }

        }

        [AllowAnonymous]
        public ActionResult ZipAndDownloadFiles(string fileName)
        {
            string filePath = Server.MapPath("~") + @"ExtractedTemplate\" + fileName;
            CreateZips.SourceDirectoriesFiles sfiles = new CreateZips.SourceDirectoriesFiles();
            if (System.IO.Directory.Exists(filePath))
            {
                string[] files = Directory.GetFiles(filePath);
                string[] subDirs = Directory.GetDirectories(filePath);
                if (files.Length > 0)
                {
                    sfiles.Files = new List<CreateZips.FileInfo>();

                    foreach (var f in files)
                    {
                        CreateZips.FileInfo fileInfo = new CreateZips.FileInfo();

                        string[] fSplit = f.Split('\\');
                        string splitLength = fSplit[fSplit.Length - 1];
                        fSplit = splitLength.Split('.');

                        fileInfo.Name = fSplit[0];
                        fileInfo.Extension = fSplit[1];
                        fileInfo.FileBytes = System.IO.File.ReadAllBytes(f);
                        sfiles.Files.Add(fileInfo);
                    }
                }

                if (subDirs.Length > 0)
                {
                    sfiles.Folder = new List<CreateZips.Folder>();

                    foreach (var dir in subDirs)
                    {
                        string[] subDirFiles = System.IO.Directory.GetFiles(dir);
                        string[] subDirsLevel2 = Directory.GetDirectories(dir);

                        if (subDirFiles.Length > 0)
                        {
                            CreateZips.Folder folder = new CreateZips.Folder();
                            string[] getFolderName = dir.Split('\\');
                            string subFolderName = getFolderName[getFolderName.Length - 1];
                            folder.FolderName = subFolderName;
                            folder.FolderItems = new List<CreateZips.FolderItem>();

                            foreach (var sdf in subDirFiles)
                            {
                                CreateZips.FolderItem folderItem = new CreateZips.FolderItem();
                                string[] fSplit = sdf.Split('\\');
                                string splitLength = fSplit[fSplit.Length - 1];
                                fSplit = splitLength.Split('.');

                                folderItem.Name = fSplit[0];
                                folderItem.Extension = fSplit[1];
                                folderItem.FileBytes = System.IO.File.ReadAllBytes(sdf);
                                folder.FolderItems.Add(folderItem);
                            }
                            if (subDirsLevel2.Length > 0)
                            {
                                folder.FolderL2 = new List<CreateZips.FolderL2>();
                                foreach (var dirL2 in subDirsLevel2)
                                {
                                    string[] subDirFilesL2 = System.IO.Directory.GetFiles(dirL2);
                                    if (subDirFilesL2.Length > 0)
                                    {
                                        CreateZips.FolderL2 folderFL2 = new CreateZips.FolderL2();
                                        string[] getFolderNameL2 = dirL2.Split('\\');
                                        string subFolderNameL2 = getFolderNameL2[getFolderNameL2.Length - 1];
                                        folderFL2.FolderName = subFolderNameL2;
                                        folderFL2.FolderItems = new List<CreateZips.FolderItem>();

                                        foreach (var sdfL2 in subDirFilesL2)
                                        {
                                            CreateZips.FolderItem folderItem = new CreateZips.FolderItem();
                                            string[] fSplit = sdfL2.Split('\\');
                                            string splitLength = fSplit[fSplit.Length - 1];
                                            fSplit = splitLength.Split('.');

                                            folderItem.Name = fSplit[0];
                                            folderItem.Extension = fSplit[1];
                                            folderItem.FileBytes = System.IO.File.ReadAllBytes(sdfL2);
                                            folderFL2.FolderItems.Add(folderItem);
                                        }
                                        folder.FolderL2.Add(folderFL2);
                                    }
                                }
                            }
                            sfiles.Folder.Add(folder);
                        }
                    }
                }
            }
            // ...

            // the output bytes of the zip
            byte[] fileBytes = null;

            //create a working memory stream
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                // create a zip
                using (System.IO.Compression.ZipArchive zip = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    // interate through the source files
                    if (sfiles.Folder != null && sfiles.Folder.Count > 0)
                    {
                        //each folder in source file [depth 1]
                        foreach (var folder in sfiles.Folder)
                        {
                            // add the item name to the zip
                            // each file in the folder
                            foreach (var file in folder.FolderItems)
                            {
                                // folder items - file name, extension, and file bytes or content in bytes
                                // zip.CreateEntry can create folder or the file. If you just provide a name, it will create a folder (if it doesn't not exist). If you provide with extension, it will create file 
                                System.IO.Compression.ZipArchiveEntry zipItem = zip.CreateEntry(folder.FolderName + "/" + file.Name + "." + file.Extension); // Creating folder and create file inside that folder

                                using (System.IO.MemoryStream originalFileMemoryStream = new System.IO.MemoryStream(file.FileBytes)) // adding file bytes to memory stream object
                                {
                                    using (System.IO.Stream entryStream = zipItem.Open()) // opening the folder/file
                                    {
                                        originalFileMemoryStream.CopyTo(entryStream); // copy memory stream dat bytes to file created
                                    }
                                }
                                // for second level of folder like /Template/Teams/BoardColums.json
                                //each folder in source file [depth 2]
                                if (folder.FolderL2 != null && folder.FolderL2.Count > 0)
                                {
                                    foreach (var folder2 in folder.FolderL2)
                                    {
                                        foreach (var file2 in folder2.FolderItems)
                                        {
                                            System.IO.Compression.ZipArchiveEntry zipItem2 = zip.CreateEntry(folder.FolderName + "/" + folder2.FolderName + "/" + file2.Name + "." + file2.Extension);
                                            using (System.IO.MemoryStream originalFileMemoryStreamL2 = new System.IO.MemoryStream(file2.FileBytes))
                                            {
                                                using (System.IO.Stream entryStreamL2 = zipItem2.Open())
                                                {
                                                    originalFileMemoryStreamL2.CopyTo(entryStreamL2);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (sfiles.Files != null && sfiles.Files.Count > 0)
                    {
                        foreach (var outerFile in sfiles.Files)
                        {
                            // add the item name to the zip
                            System.IO.Compression.ZipArchiveEntry zipItem = zip.CreateEntry(outerFile.Name + "." + outerFile.Extension);
                            // add the item bytes to the zip entry by opening the original file and copying the bytes 
                            using (System.IO.MemoryStream originalFileMemoryStream = new System.IO.MemoryStream(outerFile.FileBytes))
                            {
                                using (System.IO.Stream entryStream = zipItem.Open())
                                {
                                    originalFileMemoryStream.CopyTo(entryStream);
                                }
                            }
                        }
                    }
                }
                fileBytes = memoryStream.ToArray();
            }
            // download the constructed zip
            System.IO.Directory.Delete(filePath, true);
            Response.AddHeader("Content-Disposition", "attachment; filename=DemoGeneratorTemplate.zip");
            return File(fileBytes, "application/zip");
        }
    }
}