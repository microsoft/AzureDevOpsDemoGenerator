using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AzureDevOpsDemoBuilder.Extensions;
using AzureDevOpsDemoBuilder.Models;
using AzureDevOpsDemoBuilder.ServiceInterfaces;
using AzureDevOpsAPI;
using AzureDevOpsAPI.ExtensionManagement;
using AzureDevOpsAPI.Extractor;
using AzureDevOpsAPI.ProjectsAndTeams;
using AzureDevOpsAPI.QueriesAndWidgets;
using AzureDevOpsAPI.Service;
using AzureDevOpsAPI.Viewmodel.Extractor;
using AzureDevOpsAPI.Viewmodel.GitHub;
using Parameters = AzureDevOpsAPI.Viewmodel.Extractor.GetServiceEndpoints;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace AzureDevOpsDemoBuilder.Services
{
    public class ExtractorService : IExtractorService
    {
        #region STATIC DECLARATIONS
        public static readonly object objLock = new object();
        public static Dictionary<string, string> statusMessages;
        public static List<string> errorMessages = new List<string>();
        public static string[] workItemTypes = new string[] { };
        public static string extractedTemplatePath = string.Empty;
        private ProjectProperties.Properties projectProperties = new ProjectProperties.Properties();
        private static IWebHostEnvironment HostingEnvironment;
        private ILogger<ExtractorService> logger;

        public IConfiguration AppKeyConfiguration { get; }

        public ExtractorService(IWebHostEnvironment _host, ILogger<ExtractorService> _logger, IConfiguration configuration)
        {
            HostingEnvironment = _host;
            logger = _logger;
            AppKeyConfiguration = configuration;
        }
        public static void AddMessage(string id, string message)
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
        public static string GetStatusMessage(string id)
        {
            lock (objLock)
            {
                string message = string.Empty;
                if (id.EndsWith("_Errors"))
                {
                    //RemoveKey(id);
                    message = "Error: \t" + StatusMessages[id];
                }
                if (StatusMessages.Keys.Count(x => x == id) == 1)
                {
                    message = StatusMessages[id];
                }
                else
                {
                    message = "100";
                }
                return message;
            }
        }

        public static void RemoveKey(string id)
        {
            lock (objLock)
            {
                StatusMessages.Remove(id);
            }
        }

        #endregion  STATIC DECLARATIONS

        #region ANALYSIS - GET COUNTS
        public ProjectConfigurations ProjectConfiguration(Project model)
        {
            string repoVersion = AppKeyConfiguration["RepoVersion"];
            string buildVersion = AppKeyConfiguration["BuildVersion"];
            string releaseVersion = AppKeyConfiguration["ReleaseVersion"];
            string wikiVersion = AppKeyConfiguration["WikiVersion"];
            string boardVersion = AppKeyConfiguration["BoardVersion"];
            string workItemsVersion = AppKeyConfiguration["WorkItemsVersion"];
            string releaseHost = AppKeyConfiguration["ReleaseHost"];
            string defaultHost = AppKeyConfiguration["DefaultHost"];
            string extensionHost = AppKeyConfiguration["ExtensionHost"];
            string getReleaseVersion = AppKeyConfiguration["GetRelease"];
            string agentQueueVersion = AppKeyConfiguration["AgentQueueVersion"];
            string extensionVersion = AppKeyConfiguration["ExtensionVersion"];
            string endpointVersion = AppKeyConfiguration["EndPointVersion"];
            string queriesVersion = AppKeyConfiguration["QueriesVersion"];
            string variableGroupsApiVersion = AppKeyConfiguration["VariableGroupsApiVersion"];
            ProjectConfigurations projectConfig = new ProjectConfigurations();

            projectConfig.AgentQueueConfig = new AppConfiguration() { UriString = defaultHost + model.AccountName + "/", PersonalAccessToken = model.AccessToken, Project = model.ProjectName, AccountName = model.AccountName, Id = model.Id, VersionNumber = wikiVersion };
            projectConfig.WorkItemConfig = new AppConfiguration() { UriString = defaultHost + model.AccountName + "/", PersonalAccessToken = model.AccessToken, Project = model.ProjectName, AccountName = model.AccountName, Id = model.Id, VersionNumber = wikiVersion };
            projectConfig.BuildDefinitionConfig = new AppConfiguration() { UriString = defaultHost + model.AccountName + "/", PersonalAccessToken = model.AccessToken, Project = model.ProjectName, AccountName = model.AccountName, Id = model.Id, VersionNumber = buildVersion };
            projectConfig.ReleaseDefinitionConfig = new AppConfiguration() { UriString = releaseHost + model.AccountName + "/", PersonalAccessToken = model.AccessToken, Project = model.ProjectName, AccountName = model.AccountName, Id = model.Id, VersionNumber = releaseVersion };
            projectConfig.RepoConfig = new AppConfiguration() { UriString = defaultHost + model.AccountName + "/", PersonalAccessToken = model.AccessToken, Project = model.ProjectName, AccountName = model.AccountName, Id = model.Id, VersionNumber = repoVersion };
            projectConfig.BoardConfig = new AppConfiguration() { UriString = defaultHost + model.AccountName + "/", PersonalAccessToken = model.AccessToken, Project = model.ProjectName, AccountName = model.AccountName, Id = model.Id, VersionNumber = boardVersion };
            projectConfig.Config = new AppConfiguration() { UriString = defaultHost + model.AccountName + "/", PersonalAccessToken = model.AccessToken, Project = model.ProjectName, AccountName = model.AccountName, Id = model.Id };
            projectConfig.GetReleaseConfig = new AppConfiguration() { UriString = releaseHost + model.AccountName + "/", PersonalAccessToken = model.AccessToken, Project = model.ProjectName, AccountName = model.AccountName, Id = model.Id, VersionNumber = getReleaseVersion };
            projectConfig.ExtensionConfig = new AppConfiguration() { UriString = extensionHost + model.AccountName + "/", PersonalAccessToken = model.AccessToken, Project = model.ProjectName, AccountName = model.AccountName, Id = model.Id, VersionNumber = extensionVersion };
            projectConfig.EndpointConfig = new AppConfiguration() { UriString = defaultHost + model.AccountName + "/", PersonalAccessToken = model.AccessToken, Project = model.ProjectName, AccountName = model.AccountName, Id = model.Id, VersionNumber = endpointVersion };
            projectConfig.QueriesConfig = new AppConfiguration() { UriString = defaultHost + model.AccountName + "/", PersonalAccessToken = model.AccessToken, Project = model.ProjectName, AccountName = model.AccountName, Id = model.Id, VersionNumber = queriesVersion };
            projectConfig.VariableGroupConfig = new AppConfiguration() { UriString = defaultHost + model.AccountName + "/", PersonalAccessToken = model.AccessToken, Project = model.ProjectName, AccountName = model.AccountName, Id = model.Id, VersionNumber = variableGroupsApiVersion };

            return projectConfig;
        }
        public int GetTeamsCount(ProjectConfigurations appConfig)
        {
            AzureDevOpsAPI.Extractor.ClassificationNodes nodes = new AzureDevOpsAPI.Extractor.ClassificationNodes(appConfig.BoardConfig);
            TeamList teamList = nodes.ExportTeamList("");
            int count = 0;
            if (teamList.Value != null)
            {
                count = teamList.Value.Count;
            }
            return count;
        }
        public int GetIterationsCount(ProjectConfigurations appConfig)
        {
            AzureDevOpsAPI.Extractor.ClassificationNodes nodes = new AzureDevOpsAPI.Extractor.ClassificationNodes(appConfig.BoardConfig);
            GetINumIteration.Iterations iterations = new GetINumIteration.Iterations();
            iterations = nodes.GetiterationCount();
            if (iterations.Count > 0)
            {
                return iterations.Count;
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
        public int GetBuildDefinitionCount(ProjectConfigurations appConfig)
        {
            int BuildDefCount = 0;
            BuildandReleaseDefs buildandReleaseDefs = new BuildandReleaseDefs(appConfig.BuildDefinitionConfig);
            GetBuildDefResponse.BuildDef buildDef = new GetBuildDefResponse.BuildDef();
            buildDef = buildandReleaseDefs.GetBuildDefCount();
            if (buildDef.Count > 0)
            {
                BuildDefCount = buildDef.Count;
            }
            else if (!string.IsNullOrEmpty(buildandReleaseDefs.LastFailureMessage))
            {
                errorMessages.Add("Error while fetching build definition count: " + buildandReleaseDefs.LastFailureMessage);
            }
            return BuildDefCount;
        }
        public int GetReleaseDefinitionCount(ProjectConfigurations appConfig)
        {
            int ReleaseDefCount = 0;
            BuildandReleaseDefs buildandReleaseDefs = new BuildandReleaseDefs(appConfig.ReleaseDefinitionConfig);
            GetReleaseDefResponse.ReleaseDef releaseDef = new GetReleaseDefResponse.ReleaseDef();
            releaseDef = buildandReleaseDefs.GetReleaseDefCount();
            if (releaseDef.Count > 0)
            {
                ReleaseDefCount = releaseDef.Count;
            }
            else if (!string.IsNullOrEmpty(buildandReleaseDefs.LastFailureMessage))
            {
                errorMessages.Add("Error while fetching release definition count: " + buildandReleaseDefs.LastFailureMessage);
            }
            return ReleaseDefCount;
        }
        #endregion ANALYSIS - GET COUNTS

        #region GENERATE ARTIFACTS
        public string[] GenerateTemplateArifacts(Project model)
        {
            extractedTemplatePath = HostingEnvironment.ContentRootPath + "/ExtractedTemplate/";
            if (!Directory.Exists(extractedTemplatePath))
            {
                Directory.CreateDirectory(extractedTemplatePath);
            }
            if (Directory.Exists(extractedTemplatePath))
            {
                string[] subdirs = Directory.GetDirectories(extractedTemplatePath)
                               .Select(Path.GetFileName)
                               .ToArray();
                foreach (string folderName in subdirs)
                {
                    DirectoryInfo d = new DirectoryInfo(extractedTemplatePath + folderName);
                    if (d.CreationTime < DateTime.Now.AddHours(-1))
                        Directory.Delete(extractedTemplatePath + folderName, true);
                }
            }

            AddMessage(model.Id, "");
            ProjectConfigurations appConfig = ProjectConfiguration(model);

            GetInstalledExtensions(appConfig);

            ExportQuries(appConfig);
            ExportTeams(appConfig.BoardConfig, model.ProcessTemplate, model.ProjectId);

            if (ExportIterations(appConfig))
            {
                AddMessage(model.Id, "Iterations Definition");
            }
            string extractedFolderName = extractedTemplatePath + model.ProjectName;
            string filePathToRead = HostingEnvironment.ContentRootPath + "/PreSetting";

            string projectSetting = "";
            projectSetting = filePathToRead + "/ProjectSettings.json";
            projectSetting = File.ReadAllText(projectSetting);
            projectSetting = projectSetting.Replace("$type$", model.ProcessTemplate).Replace("$id$", projectProperties.Value.Where(x => x.Name == "System.ProcessTemplateType").FirstOrDefault().RefValue);
            File.WriteAllText(extractedFolderName + "/ProjectSettings.json", projectSetting);

            string projectTemplate = "";
            projectTemplate = filePathToRead + "/ProjectTemplate.json";
            projectTemplate = File.ReadAllText(projectTemplate);
            File.WriteAllText(extractedFolderName + "/ProjectTemplate.json", projectTemplate);

            string teamArea = "";
            teamArea = filePathToRead + "/TeamArea.json";
            teamArea = File.ReadAllText(teamArea);
            File.WriteAllText(extractedFolderName + "/TeamArea.json", teamArea);
            AddMessage(model.Id, "Team Areas");

            ExportWorkItems(appConfig);
            AddMessage(model.Id, "Work Items");

            ExportRepositoryList(appConfig);
            AddMessage(model.Id, "Repository and Service Endpoint");

            GetServiceEndpoints(appConfig);

            int count = GetBuildDefinitions(appConfig);
            if (count >= 1)
            {
                AddMessage(model.Id, "Build Definition");
            }

            int relCount = GeneralizingGetReleaseDefinitions(appConfig);
            if (relCount >= 1)
            {
                AddMessage(model.Id, "Release Definition");
            }

            StatusMessages[model.Id] = "100";
            return new string[] { model.Id, "" };
        }

        public Dictionary<string, int> GetWorkItemsCount(ProjectConfigurations appConfig)
        {
            string[] workItemtypes = GetAllWorkItemsName(appConfig);//{ "Epic", "Feature", "Product Backlog Item", "Task", "Test Case", "Bug", "User Story", "Test Suite", "Test Plan", "Issue" };
            GetWorkItemsCount itemsCount = new GetWorkItemsCount(appConfig.WorkItemConfig);
            Dictionary<string, int> fetchedWorkItemsCount = new Dictionary<string, int>();
            if (workItemtypes.Length > 0)
            {
                foreach (var workItem in workItemtypes)
                {
                    WorkItemFetchResponse.WorkItems WITCount = itemsCount.GetWorkItemsfromSource(workItem);
                    if (WITCount.Count > 0)
                    {
                        fetchedWorkItemsCount.Add(workItem, WITCount.Count);
                    }
                    else if (!string.IsNullOrEmpty(itemsCount.LastFailureMessage))
                    {
                        errorMessages.Add("Error while querying work items: " + itemsCount.LastFailureMessage);
                    }
                }
            }

            return fetchedWorkItemsCount;
        }

        public List<RequiredExtensions.ExtensionWithLink> GetInstalledExtensions(ProjectConfigurations appConfig)
        {
            try
            {
                GetListExtenison listExtenison = new GetListExtenison(appConfig.ExtensionConfig);
                List<RequiredExtensions.ExtensionWithLink> extensionList = new List<RequiredExtensions.ExtensionWithLink>();
                GetExtensions.ExtensionsList returnExtensionsList = listExtenison.GetInstalledExtensions();
                if (returnExtensionsList != null && returnExtensionsList.Count > 0)
                {
                    List<GetExtensions.Value> builtInExtensions = returnExtensionsList.Value.Where(x => x.Flags == null).ToList();
                    List<GetExtensions.Value> trustedExtensions = returnExtensionsList.Value.Where(x => x.Flags != null && x.Flags.ToString() == "trusted").ToList();
                    builtInExtensions.AddRange(trustedExtensions);
                    returnExtensionsList.Value = builtInExtensions;

                    foreach (GetExtensions.Value data in returnExtensionsList.Value)
                    {
                        RequiredExtensions.ExtensionWithLink extension = new RequiredExtensions.ExtensionWithLink();
                        if (data.ExtensionName.ToLower() != "analytics")
                        {
                            extension.ExtensionId = data.ExtensionId;
                            extension.ExtensionName = data.ExtensionName;
                            extension.PublisherId = data.PublisherId;
                            extension.PublisherName = data.PublisherName;
                            extension.Link = "<a href='" + string.Format("https://marketplace.visualstudio.com/items?itemName={0}.{1}", data.PublisherId, data.ExtensionId) + "' target='_blank'><b>" + data.ExtensionName + "</b></a>";
                            extension.License = "<a href='" + string.Format("https://marketplace.visualstudio.com/items?itemName={0}.{1}", data.PublisherId, data.ExtensionId) + "' target='_blank'>License Terms</a>";
                            extensionList.Add(extension);
                        }
                    }
                    RequiredExtensions.ListExtension listExtension = new RequiredExtensions.ListExtension();
                    if (extensionList.Count > 0)
                    {
                        listExtension.Extensions = extensionList;
                        if (!Directory.Exists(extractedTemplatePath + appConfig.ExtensionConfig.Project))
                        {
                            Directory.CreateDirectory(extractedTemplatePath + appConfig.ExtensionConfig.Project);
                        }
                        string fetchedJson = JsonConvert.SerializeObject(listExtension, Formatting.Indented);

                        File.WriteAllText(extractedTemplatePath + appConfig.ExtensionConfig.Project + "//Extensions.json", JsonConvert.SerializeObject(listExtension, Formatting.Indented));
                    }
                }
                else if (!string.IsNullOrEmpty(listExtenison.LastFailureMessage))
                {
                    AddMessage(appConfig.ExtensionConfig.Id.ErrorId(), "Some error occured while fetching extensions");
                }
                return extensionList;
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace + "\n");
                AddMessage(appConfig.ExtensionConfig.Id.ErrorId(), ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return new List<RequiredExtensions.ExtensionWithLink>();
        }

        public void ExportQuries(ProjectConfigurations appConfig)
        {
            try
            {
                Queries queries = new Queries(appConfig.QueriesConfig);
                GetQueries.Queries listQueries = queries.GetQueriesWiql();
                if (listQueries.Count > 0)
                {
                    foreach (var _queries in listQueries.Value)
                    {
                        if (_queries.HasChildren)
                        {
                            foreach (var query in _queries.Children)
                            {
                                if (!query.HasChildren)
                                {
                                    if (query.Wiql != null)
                                    {
                                        query.Wiql = query.Wiql.Replace(appConfig.QueriesConfig.Project, "$projectId$");
                                        JObject jobj = new JObject();
                                        jobj["name"] = query.Name;
                                        jobj["wiql"] = query.Wiql;
                                        if (!Directory.Exists(extractedTemplatePath + appConfig.QueriesConfig.Project + "/Dashboard/Queries"))
                                        {
                                            Directory.CreateDirectory(extractedTemplatePath + appConfig.QueriesConfig.Project + "/Dashboard");
                                            File.WriteAllText(extractedTemplatePath + appConfig.QueriesConfig.Project + "/Dashboard/Dashboard.json", JsonConvert.SerializeObject("text", Formatting.Indented));
                                        }
                                        if (!Directory.Exists(extractedTemplatePath + appConfig.QueriesConfig.Project + "/Dashboard/Queries"))
                                        {
                                            Directory.CreateDirectory(extractedTemplatePath + appConfig.QueriesConfig.Project + "/Dashboard/Queries");
                                            File.WriteAllText(extractedTemplatePath + appConfig.QueriesConfig.Project + "/Dashboard/Queries/" + query.Name + ".json", JsonConvert.SerializeObject(jobj, Formatting.Indented));
                                        }
                                        else
                                        {
                                            File.WriteAllText(extractedTemplatePath + appConfig.QueriesConfig.Project + "/Dashboard/Queries/" + query.Name + ".json", JsonConvert.SerializeObject(jobj, Formatting.Indented));
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var child1 in query.Children)
                                    {
                                        if (child1.Wiql != null)
                                        {
                                            child1.Wiql = child1.Wiql.Replace(appConfig.QueriesConfig.Project, "$projectId$");
                                            JObject jobj = new JObject();
                                            jobj["name"] = child1.Name;
                                            jobj["wiql"] = child1.Wiql;
                                            if (!Directory.Exists(extractedTemplatePath + appConfig.QueriesConfig.Project + "/Dashboard/Queries"))
                                            {
                                                Directory.CreateDirectory(extractedTemplatePath + appConfig.QueriesConfig.Project + "/Dashboard/Queries");

                                                File.WriteAllText(extractedTemplatePath + appConfig.QueriesConfig.Project + "/Dashboard/Queries/" + child1.Name + ".json", JsonConvert.SerializeObject(jobj, Formatting.Indented));
                                            }
                                            else
                                            {
                                                File.WriteAllText(extractedTemplatePath + appConfig.QueriesConfig.Project + "/Dashboard/Queries/" + child1.Name + ".json", JsonConvert.SerializeObject(jobj, Formatting.Indented));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(queries.LastFailureMessage))
                {
                    AddMessage(appConfig.QueriesConfig.Id.ErrorId(), "Error while fetching queries");
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace + "\n");
            }

        }

        public bool ExportTeams(AppConfiguration con, string processTemplate, string projectID)
        {
            try
            {
                if (!string.IsNullOrEmpty(processTemplate))
                {
                    string defaultTeamID = string.Empty;
                    AzureDevOpsAPI.Extractor.ClassificationNodes nodes = new AzureDevOpsAPI.Extractor.ClassificationNodes(con);
                    TeamList _team = new TeamList();
                    string ProjectPropertyVersion = AppKeyConfiguration["ProjectPropertyVersion"];
                    con.VersionNumber = ProjectPropertyVersion;
                    con.ProjectId = projectID;
                    Projects projects = new Projects(con);
                    projectProperties = projects.GetProjectProperties();
                    if (projectProperties.Count > 0)
                    {
                        defaultTeamID = projectProperties.Value.Where(x => x.Name == "System.Microsoft.TeamFoundation.Team.Default").FirstOrDefault().RefValue;
                    }
                    _team = nodes.ExportTeamList(defaultTeamID);
                    if (_team.Value != null)
                    {
                        AddMessage(con.Id, "Teams");

                        string fetchedJson = JsonConvert.SerializeObject(_team.Value, Formatting.Indented);
                        if (fetchedJson != "")
                        {
                            if (!Directory.Exists(extractedTemplatePath + con.Project + "/Teams"))
                            {
                                Directory.CreateDirectory(extractedTemplatePath + con.Project + "/Teams");
                            }
                            File.WriteAllText(extractedTemplatePath + con.Project + "/Teams/Teams.json", fetchedJson);

                            List<string> boardTypes = new List<string>();
                            boardTypes.Add("Epics");
                            if (processTemplate.ToLower() == "agile")
                            {
                                boardTypes.Add("Features");
                                boardTypes.Add("Stories");
                            }
                            else if (processTemplate.ToLower() == "basic")
                            {
                                boardTypes.Add("Issues");
                            }
                            else if (processTemplate.ToLower() == "scrum")
                            {
                                boardTypes.Add("Features");
                                boardTypes.Add("Backlog Items");
                            }

                            foreach (var team in _team.Value)
                            {
                                List<BoardColumnResponseScrum.ColumnResponse> columnResponsesScrum = new List<BoardColumnResponseScrum.ColumnResponse>();
                                List<BoardColumnResponseAgile.ColumnResponse> columnResponsesAgile = new List<BoardColumnResponseAgile.ColumnResponse>();
                                List<BoardColumnResponseBasic.ColumnResponse> columnResponsesBasic = new List<BoardColumnResponseBasic.ColumnResponse>();
                                List<ExportBoardRows.Rows> boardRows = new List<ExportBoardRows.Rows>();

                                ExportTeamSetting.Setting listTeamSetting = new ExportTeamSetting.Setting();

                                List<JObject> jObjCardFieldList = new List<JObject>();
                                List<JObject> jObjcardStyleList = new List<JObject>();
                                string teamFolderPath = extractedTemplatePath + con.Project + "/Teams/" + team.Name;
                                if (!Directory.Exists(teamFolderPath))
                                {
                                    Directory.CreateDirectory(teamFolderPath);
                                }
                                //Export Board Colums for each team
                                con.Team = team.Name;

                                ClassificationNodes teamNodes = new ClassificationNodes(con);
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
                                        else if (processTemplate.ToLower() == "basic")
                                        {
                                            string res = response.Content.ReadAsStringAsync().Result;
                                            BoardColumnResponseBasic.ColumnResponse basicColumns = JsonConvert.DeserializeObject<BoardColumnResponseBasic.ColumnResponse>(res);
                                            basicColumns.BoardName = boardType;
                                            columnResponsesBasic.Add(basicColumns);
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
                                    if (rows.Value != null && rows.Value.Count > 0)
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
                                        var tagStyle = style["rules"]["tagStyle"];
                                        if (tagStyle == null)
                                        {
                                            style["rules"]["tagStyle"] = new JArray();
                                        }
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
                                if (processTemplate.ToLower() != "basic")
                                {
                                    ExportTeamSetting.Setting teamSetting = teamNodes.ExportTeamSetting();
                                    if (teamSetting.BacklogVisibilities != null)
                                    {
                                        listTeamSetting = teamSetting;
                                        AddMessage(con.Id, "Team Settings Definition");
                                    }
                                }
                                else if (!string.IsNullOrEmpty(teamNodes.LastFailureMessage))
                                {
                                    AddMessage(con.Id.ErrorId(), "Error occured while exporting Team Setting: " + teamNodes.LastFailureMessage);
                                }

                                if (columnResponsesAgile.Count > 0)
                                {
                                    File.WriteAllText(teamFolderPath + "/BoardColumns.json", JsonConvert.SerializeObject(columnResponsesAgile, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                                }
                                if (columnResponsesScrum.Count > 0)
                                {
                                    File.WriteAllText(teamFolderPath + "/BoardColumns.json", JsonConvert.SerializeObject(columnResponsesScrum, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                                }
                                if (columnResponsesBasic.Count > 0)
                                {
                                    File.WriteAllText(teamFolderPath + "/BoardColumns.json", JsonConvert.SerializeObject(columnResponsesBasic, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                                }
                                if (boardRows.Count > 0)
                                {
                                    File.WriteAllText(teamFolderPath + "/BoardRows.json", JsonConvert.SerializeObject(boardRows, Formatting.Indented));
                                }
                                if (!string.IsNullOrEmpty(listTeamSetting.BugsBehavior))
                                {
                                    File.WriteAllText(teamFolderPath + "/TeamSetting.json", JsonConvert.SerializeObject(listTeamSetting, Formatting.Indented));
                                }
                                if (jObjCardFieldList.Count > 0)
                                {
                                    File.WriteAllText(teamFolderPath + "/CardFields.json", JsonConvert.SerializeObject(jObjCardFieldList, Formatting.Indented));
                                }
                                if (jObjcardStyleList.Count > 0)
                                {
                                    File.WriteAllText(teamFolderPath + "/CardStyles.json", JsonConvert.SerializeObject(jObjcardStyleList, Formatting.Indented));
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
                else
                {
                    logger.LogDebug("Could not fetch teams and board details since one of the param is null");
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace + "\n");
                AddMessage(con.Id.ErrorId(), ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return false;
        }

        public bool ExportIterations(ProjectConfigurations appConfig)
        {
            try
            {
                ClassificationNodes nodes = new AzureDevOpsAPI.Extractor.ClassificationNodes(appConfig.BoardConfig);
                ExportedIterations.Iterations viewModel = nodes.ExportIterationsToSave();
                string fetchedJson = JsonConvert.SerializeObject(viewModel, Formatting.Indented);
                if (fetchedJson != "")
                {
                    if (!Directory.Exists(extractedTemplatePath + appConfig.BoardConfig.Project))
                    {
                        Directory.CreateDirectory(extractedTemplatePath + appConfig.BoardConfig.Project);
                    }
                    File.WriteAllText(extractedTemplatePath + appConfig.BoardConfig.Project + "/Iterations.json", fetchedJson);
                    return true;
                }
                else
                {
                    string error = nodes.LastFailureMessage;
                    AddMessage(appConfig.BoardConfig.Id.ErrorId(), error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace + "\n");
                AddMessage(appConfig.BoardConfig.Id.ErrorId(), ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return false;
        }

        public void ExportWorkItems(ProjectConfigurations appConfig)
        {
            try
            {
                string[] workItemtypes = GetAllWorkItemsName(appConfig);//{ "Epic", "Feature", "Product Backlog Item", "Task", "Test Case", "Bug", "User Story", "Test Suite", "Test Plan", "Issue" };
                if (!Directory.Exists(extractedTemplatePath + appConfig.WorkItemConfig.Project))
                {
                    Directory.CreateDirectory(extractedTemplatePath + appConfig.WorkItemConfig.Project);
                }

                if (workItemtypes.Length > 0)
                {
                    foreach (var WIT in workItemtypes)
                    {
                        GetWorkItemsCount WorkitemsCount = new GetWorkItemsCount(appConfig.WorkItemConfig);
                        WorkItemFetchResponse.WorkItems fetchedWorkItem = WorkitemsCount.GetWorkItemsfromSource(WIT);
                        string workItemJson = JsonConvert.SerializeObject(fetchedWorkItem, Formatting.Indented);
                        if (fetchedWorkItem.Count > 0)
                        {
                            workItemJson = workItemJson.Replace(appConfig.WorkItemConfig.Project + "\\", "$ProjectName$\\");
                            string item = WIT;
                            if (!Directory.Exists(extractedTemplatePath + appConfig.WorkItemConfig.Project + "/WorkItems"))
                            {
                                Directory.CreateDirectory(extractedTemplatePath + appConfig.WorkItemConfig.Project + "/WorkItems");
                            }
                            File.WriteAllText(extractedTemplatePath + appConfig.WorkItemConfig.Project + "/WorkItems/" + item + ".json", workItemJson);
                        }
                        else if (!string.IsNullOrEmpty(WorkitemsCount.LastFailureMessage))
                        {
                            AddMessage(appConfig.WorkItemConfig.Id.ErrorId(), WorkitemsCount.LastFailureMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace + "\n");
                AddMessage(appConfig.WorkItemConfig.Id.ErrorId(), ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        public void ExportRepositoryList(ProjectConfigurations appConfig)
        {
            try
            {
                BuildandReleaseDefs repolist = new BuildandReleaseDefs(appConfig.RepoConfig);
                RepositoryList.Repository repos = repolist.GetRepoList();
                if (repos.Count > 0)
                {
                    foreach (var repo in repos.Value)
                    {
                        string preSettingPath = HostingEnvironment.ContentRootPath + "/PreSetting";
                        string templateFolderPath = extractedTemplatePath + appConfig.RepoConfig.Project;
                        string host = appConfig.RepoConfig.UriString + appConfig.RepoConfig.Project;
                        string sourceCodeJson = File.ReadAllText(preSettingPath + "/ImportSourceCode.json");
                        sourceCodeJson = sourceCodeJson.Replace("$Host$", host).Replace("$Repo$", repo.Name);
                        string endPointJson = File.ReadAllText(preSettingPath + "/ServiceEndPoint.json");
                        endPointJson = endPointJson.Replace("$Host$", host).Replace("$Repo$", repo.Name);
                        if (!Directory.Exists(templateFolderPath + "/ImportSourceCode"))
                        {
                            Directory.CreateDirectory(templateFolderPath + "/ImportSourceCode");
                            File.WriteAllText(templateFolderPath + "/ImportSourceCode/" + repo.Name + ".json", sourceCodeJson);
                        }
                        else
                        {
                            File.WriteAllText(templateFolderPath + "/ImportSourceCode/" + repo.Name + ".json", sourceCodeJson);
                        }
                        if (!Directory.Exists(templateFolderPath + "/ServiceEndpoints"))
                        {
                            Directory.CreateDirectory(templateFolderPath + "/ServiceEndpoints");
                            File.WriteAllText(templateFolderPath + "/ServiceEndpoints/" + repo.Name + "-code.json", endPointJson);
                        }
                        else
                        {
                            File.WriteAllText(templateFolderPath + "/ServiceEndpoints/" + repo.Name + "-code.json", endPointJson);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace + "\n");
                AddMessage(appConfig.RepoConfig.Id.ErrorId(), ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        /// <summary>
        /// Get the Build definitions to write into file
        /// </summary>
        /// <param name="appConfig"></param>
        /// <returns></returns>
        public int GetBuildDefinitions(ProjectConfigurations appConfig)
        {
            try
            {
                BuildandReleaseDefs buildandReleaseDefs = new BuildandReleaseDefs(appConfig.BuildDefinitionConfig);
                List<JObject> builds = buildandReleaseDefs.ExportBuildDefinitions();
                BuildandReleaseDefs repoDefs = new BuildandReleaseDefs(appConfig.RepoConfig);
                Dictionary<string, string> variableGroupNameId = GetVariableGroups(appConfig);
                RepositoryList.Repository repo = repoDefs.GetRepoList();
                if (builds.Count > 0)
                {
                    int count = 1;
                    //creating ImportCode Json file
                    string templatePath = extractedTemplatePath + appConfig.BuildDefinitionConfig.Project;
                    foreach (JObject def in builds)
                    {
                        string repoID = "";
                        var buildName = def["name"];
                        string fileName = buildName.ToString().Replace(".", "") + ".json";
                        var repoName = def["repository"]["name"];
                        var type = def["repository"]["type"];
                        foreach (var re in repo.Value)
                        {
                            if (re.Name == repoName.ToString())
                            {
                                repoID = re.Id;
                            }
                        }
                        def["authoredBy"] = "{}";
                        def["project"] = "{}";
                        def["url"] = "";
                        def["uri"] = "";
                        def["id"] = "";
                        if (def["queue"]["pool"].HasValues)
                        {
                            def["queue"]["pool"]["id"] = "";
                        }
                        def["_links"] = "{}";
                        def["createdDate"] = "";
                        if (def["variableGroups"] != null)
                        {
                            var variableGroup = def["variableGroups"].HasValues ? def["variableGroups"].ToArray() : new JToken[0];
                            if (variableGroup.Length > 0)
                            {
                                foreach (var groupId in variableGroup)
                                {
                                    groupId["id"] = "$" + variableGroupNameId.Where(x => x.Key == groupId["id"].ToString()).FirstOrDefault().Value + "$";
                                }
                            }
                        }
                        var yamalfilename = def["process"]["yamlFilename"];

                        #region YML PIPELINES OF TYPE AZURE REPOS
                        if (yamalfilename != null && type.ToString().ToLower() == "tfsgit")
                        {
                            count = YmlWithAzureRepos(appConfig, count, templatePath, def, fileName, type);
                        }
                        #endregion

                        #region YML PIPELINE WITH GITHUB
                        else if (yamalfilename != null && type.ToString().ToLower() == "github")
                        {
                            count = YmlWithGitHub(appConfig, count, templatePath, def, fileName, type);
                        }
                        #endregion

                        #region OTHER
                        else if (yamalfilename == null)
                        {
                            count = ClassicPipeline(appConfig, count, templatePath, def, fileName, repoName, type);
                        }
                        #endregion
                    }
                    return count;
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace + "\n");
                AddMessage(appConfig.BuildDefinitionConfig.Id.ErrorId(), ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return 0;
        }
        /// <summary>
        /// Normal Build pipeline, which could be either pointing from Azure Repos or GitHub
        /// </summary>
        /// <param name="appConfig"></param>
        /// <param name="count"></param>
        /// <param name="templatePath"></param>
        /// <param name="def"></param>
        /// <param name="fileName"></param>
        /// <param name="repoName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private int ClassicPipeline(ProjectConfigurations appConfig, int count, string templatePath, JObject def, string fileName, JToken repoName, JToken type)
        {
            try
            {
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
                                    string keyConfig = File.ReadAllText(HostingEnvironment.WebRootPath + "/Templates/EndpointKeyConfig.json");
                                    KeyConfig.Keys keyC = new KeyConfig.Keys();
                                    keyC = JsonConvert.DeserializeObject<KeyConfig.Keys>(keyConfig);
                                    if (keyC != null)
                                    {
                                        foreach (var key in keyC.KeysValue)
                                        {
                                            string keyVal = step[key] != null ? step[key].ToString() : "";
                                            if (!string.IsNullOrEmpty(keyVal))
                                            {
                                                step[key] = "";
                                            }
                                        }
                                        foreach (var key in keyC.KeysValue)
                                        {
                                            string keyVal = step["inputs"][key] != null ? step["inputs"][key].ToString() : "";
                                            if (!string.IsNullOrEmpty(keyVal))
                                            {
                                                step["inputs"][key] = "";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (type.ToString().ToLower() == "github")
                {

                    Guid g = Guid.NewGuid();
                    string randStr = g.ToString().Substring(0, 8);
                    def["repository"]["type"] = "Git";
                    def["repository"]["properties"]["fullName"] = "repository";
                    def["repository"]["properties"]["connectedServiceId"] = "$GitHub_" + randStr + "$";
                    def["repository"]["name"] = "repository";
                    string url = def["repository"]["url"].ToString();
                    if (url != "")
                    {
                        string endPointString = File.ReadAllText(HostingEnvironment.ContentRootPath + "/PreSetting/GitHubEndPoint.json");
                        endPointString = endPointString.Replace("$GitHubURL$", url).Replace("$Name$", "GitHub_" + randStr);

                        if (!Directory.Exists(extractedTemplatePath + appConfig.RepoConfig.Project + "/ServiceEndpoints"))
                        {
                            Directory.CreateDirectory(extractedTemplatePath + appConfig.RepoConfig.Project + "/ServiceEndpoints");
                            File.WriteAllText(extractedTemplatePath + appConfig.RepoConfig.Project + "/ServiceEndpoints/GitHub" + randStr + "-EndPoint.json", endPointString);
                        }
                        else
                        {
                            File.WriteAllText(extractedTemplatePath + appConfig.RepoConfig.Project + "/ServiceEndpoints/GitHub" + randStr + "-EndPoint.json", endPointString);
                        }
                    }
                }
                else if (type.ToString().ToLower() == "git")
                {
                    Guid g = Guid.NewGuid();
                    string randStr = g.ToString().Substring(0, 8);
                    string url = def["repository"]["url"].ToString();
                    string endPointString = File.ReadAllText(HostingEnvironment.ContentRootPath + "/PreSetting/GitHubEndPoint.json");
                    endPointString = endPointString.Replace("$GitHubURL$", url).Replace("$Name$", "GitHub_" + randStr);

                    if (!Directory.Exists(extractedTemplatePath + appConfig.RepoConfig.Project + "/ServiceEndpoints"))
                    {
                        Directory.CreateDirectory(extractedTemplatePath + appConfig.RepoConfig.Project + "/ServiceEndpoints");
                        File.WriteAllText(extractedTemplatePath + appConfig.RepoConfig.Project + "/ServiceEndpoints/GitHub_" + randStr + "-EndPoint.json", endPointString);
                    }
                    else
                    {
                        File.WriteAllText(extractedTemplatePath + appConfig.RepoConfig.Project + "/ServiceEndpoints/GitHub_" + randStr + "-EndPoint.json", endPointString);
                    }
                    def["repository"]["properties"]["connectedServiceId"] = "$GitHub_" + randStr + "$";
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
                count++;
                if (!Directory.Exists(templatePath + "/BuildDefinitions"))
                {
                    Directory.CreateDirectory(templatePath + "/BuildDefinitions");
                    File.WriteAllText(templatePath + "/BuildDefinitions/" + fileName, JsonConvert.SerializeObject(def, Formatting.Indented));
                }
                else
                {
                    File.WriteAllText(templatePath + "/BuildDefinitions/" + fileName, JsonConvert.SerializeObject(def, Formatting.Indented));
                }

                return count;
            }
            catch (Exception ex)
            {
                logger.LogDebug("Exporting normalPipeline \t" + ex.Message + "\n" + ex.StackTrace + "\n");
                AddMessage(appConfig.ReleaseDefinitionConfig.Id.ErrorId(), ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return count;
        }
        /// <summary>
        /// YAML pipeline which is pointing to GitHub
        /// </summary>
        /// <param name="appConfig"></param>
        /// <param name="count"></param>
        /// <param name="templatePath"></param>
        /// <param name="def"></param>
        /// <param name="fileName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private int YmlWithGitHub(ProjectConfigurations appConfig, int count, string templatePath, JObject def, string fileName, JToken type)
        {
            try
            {
                Guid g = Guid.NewGuid();
                string randStr = g.ToString().Substring(0, 8);
                var ymlRepoUrl = def["repository"]["url"].ToString();
                if (!Directory.Exists(extractedTemplatePath + appConfig.BuildDefinitionConfig.Project + "/ImportSourceCode"))
                {
                    Directory.CreateDirectory(extractedTemplatePath + appConfig.BuildDefinitionConfig.Project + "/ImportSourceCode");
                }
                if (type.ToString().ToLower() == "github")
                {
                    string gitHubRepo = def["repository"]["id"].ToString();
                    string[] gitHubIdSplit = gitHubRepo.Split('/');
                    gitHubIdSplit[0] = "$username$";
                    gitHubRepo = string.Join("/", gitHubIdSplit);

                    GitHubRepos.Fork gitHubRepoList = new GitHubRepos.Fork();
                    gitHubRepoList.Repositories = new List<GitHubRepos.Repository>();
                    if (File.Exists(extractedTemplatePath + appConfig.BuildDefinitionConfig.Project + "/ImportSourceCode/GitRepository.json"))
                    {
                        string readrepo = File.ReadAllText(extractedTemplatePath + appConfig.BuildDefinitionConfig.Project + "/ImportSourceCode/GitRepository.json");
                        gitHubRepoList = JsonConvert.DeserializeObject<GitHubRepos.Fork>(readrepo);
                    }
                    GitHubRepos.Repository repoName = new GitHubRepos.Repository
                    {
                        FullName = def["repository"]["id"].ToString(),
                        EndPointName = "GitHub_" + randStr,
                        vcs = "git",
                        vcs_url = "https://github.com/" + def["repository"]["id"].ToString()
                    };
                    gitHubRepoList.Repositories.Add(repoName);

                    File.WriteAllText(extractedTemplatePath + appConfig.BuildDefinitionConfig.Project + "/ImportSourceCode/GitRepository.json", JsonConvert.SerializeObject(gitHubRepoList, Formatting.Indented));

                    def["repository"]["properties"]["apiUrl"] = "https://api.github.com/repos/" + gitHubRepo;
                    def["repository"]["properties"]["branchesUrl"] = "https://api.github.com/repos/" + gitHubRepo + "/branches";
                    def["repository"]["properties"]["cloneUrl"] = "https://github.com/" + gitHubRepo + ".git";
                    def["repository"]["properties"]["fullName"] = "repository";
                    def["repository"]["properties"]["manageUrl"] = "https://github.com/" + gitHubRepo;
                    def["repository"]["properties"]["connectedServiceId"] = "$GitHub_" + randStr + "$";
                    def["repository"]["name"] = gitHubRepo;
                    def["repository"]["url"] = "https://github.com/" + gitHubRepo + ".git";
                    def["repository"]["id"] = gitHubRepo;
                }
                if (ymlRepoUrl != "")
                {
                    string endPointString = File.ReadAllText(HostingEnvironment.ContentRootPath + "/PreSetting/GitHubEndPoint.json");
                    endPointString = endPointString.Replace("$GitHubURL$", ymlRepoUrl).Replace("$Name$", "GitHub_" + randStr);

                    if (!Directory.Exists(extractedTemplatePath + appConfig.BuildDefinitionConfig.Project + "/ServiceEndpoints"))
                    {
                        Directory.CreateDirectory(extractedTemplatePath + appConfig.BuildDefinitionConfig.Project + "/ServiceEndpoints");
                        File.WriteAllText(extractedTemplatePath + appConfig.BuildDefinitionConfig.Project + "/ServiceEndpoints/GitHub_" + randStr + "-EndPoint.json", endPointString);
                    }
                    else
                    {
                        File.WriteAllText(extractedTemplatePath + appConfig.BuildDefinitionConfig.Project + "/ServiceEndpoints/GitHub_" + randStr + "-EndPoint.json", endPointString);
                    }
                }
                count = count + 1;
                if (!Directory.Exists(templatePath + "/BuildDefinitionGitHub"))
                {
                    Directory.CreateDirectory(templatePath + "/BuildDefinitionGitHub");
                    File.WriteAllText(templatePath + "/BuildDefinitionGitHub/" + fileName, JsonConvert.SerializeObject(def, Formatting.Indented));
                }
                else
                {
                    File.WriteAllText(templatePath + "/BuildDefinitionGitHub/" + fileName, JsonConvert.SerializeObject(def, Formatting.Indented));
                }

                return count;
            }
            catch (Exception ex)
            {
                logger.LogDebug("Exporting ymlWithGitHub \t" + ex.Message + "\n" + ex.StackTrace + "\n");
                AddMessage(appConfig.ReleaseDefinitionConfig.Id.ErrorId(), ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return count;
        }
        /// <summary>
        /// YAML pipeline which is pointing to Azure Repos
        /// </summary>
        /// <param name="appConfig"></param>
        /// <param name="count"></param>
        /// <param name="templatePath"></param>
        /// <param name="def"></param>
        /// <param name="fileName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private int YmlWithAzureRepos(ProjectConfigurations appConfig, int count, string templatePath, JObject def, string fileName, JToken type)
        {
            try
            {
                Guid g = Guid.NewGuid();
                string randStr = g.ToString().Substring(0, 8);
                def["triggers"] = new JArray();
                if (type.ToString().ToLower() == "github")
                {
                    def["repository"]["properties"]["fullName"] = "repository";
                    def["repository"]["properties"]["connectedServiceId"] = "$GitHub_" + randStr + "$";
                    def["repository"]["name"] = "repository";
                }
                var ymlRepoUrl = def["repository"]["url"].ToString();
                if (ymlRepoUrl != "")
                {
                    string endPointString = File.ReadAllText(HostingEnvironment.ContentRootPath + "/PreSetting/GitHubEndPoint.json");
                    endPointString = endPointString.Replace("$GitHubURL$", ymlRepoUrl).Replace("$Name$", "GitHub_" + randStr);
                    if (!Directory.Exists(extractedTemplatePath + appConfig.BuildDefinitionConfig.Project + "/ServiceEndpoints"))
                    {
                        Directory.CreateDirectory(extractedTemplatePath + appConfig.BuildDefinitionConfig.Project + "/ServiceEndpoints");
                        File.WriteAllText(extractedTemplatePath + appConfig.BuildDefinitionConfig.Project + "/ServiceEndpoints/GitHub-" + randStr + "-EndPoint.json", endPointString);
                    }
                    else
                    {
                        File.WriteAllText(extractedTemplatePath + appConfig.BuildDefinitionConfig.Project + "/ServiceEndpoints/GitHub-" + randStr + "-EndPoint.json", endPointString);
                    }
                }
                string[] splitYmlRepoUrl = ymlRepoUrl.Split('/');
                if (splitYmlRepoUrl.Length > 0)
                {
                    splitYmlRepoUrl[2] = "$Organization$@dev.azure.com";
                    splitYmlRepoUrl[3] = "$Organization$";
                    splitYmlRepoUrl[4] = "$ProjectName$";
                    ymlRepoUrl = string.Join("/", splitYmlRepoUrl);
                    def["repository"]["url"] = ymlRepoUrl;
                }
                var queueHref = def["queue"]["_links"]["self"]["href"].ToString();
                if (queueHref != "")
                {
                    string[] splitQhref = queueHref.Split('/');
                    if (splitQhref.Length > 0)
                    {
                        splitQhref[3] = "$Organization$";
                        //splitQhref[splitQhref.Length - 1] = "$" + def["queue"]["name"] == null ? "" : def["queue"]["name"].ToString() + "$";
                        def["queue"]["_links"]["self"]["href"] = string.Join("/", splitQhref);
                    }
                    def["queue"]["id"] = "$" + def["queue"]["name"] + "$";
                    def["queue"]["url"] = string.Join("/", splitQhref);
                }
                count = count + 1;
                if (!Directory.Exists(templatePath + "/BuildDefinitions"))
                {
                    Directory.CreateDirectory(templatePath + "/BuildDefinitions");
                    File.WriteAllText(templatePath + "/BuildDefinitions/" + fileName, JsonConvert.SerializeObject(def, Formatting.Indented));
                }
                else
                {
                    File.WriteAllText(templatePath + "/BuildDefinitions/" + fileName, JsonConvert.SerializeObject(def, Formatting.Indented));
                }

                return count;
            }
            catch (Exception ex)
            {
                logger.LogDebug("Exporting ymlWithAzureRepos \t" + ex.Message + "\n" + ex.StackTrace + "\n");
                AddMessage(appConfig.ReleaseDefinitionConfig.Id.ErrorId(), ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return count;
        }
        /// <summary>
        /// Generalizing the release definition method to make it work for All kind of Release definition
        /// </summary>
        /// <param name="appConfig"></param>
        /// <returns></returns>
        public int GeneralizingGetReleaseDefinitions(ProjectConfigurations appConfig)
        {
            try
            {
                BuildandReleaseDefs releaseDefs = new BuildandReleaseDefs(appConfig.ReleaseDefinitionConfig);
                List<JObject> releases = releaseDefs.GetReleaseDefs();
                string rells = JsonConvert.SerializeObject(releases);
                BuildandReleaseDefs agent = new BuildandReleaseDefs(appConfig.AgentQueueConfig);
                Dictionary<string, string> variableGroupNameId = GetVariableGroups(appConfig);
                Dictionary<string, int> queue = agent.GetQueues();
                string templatePath = extractedTemplatePath + appConfig.ReleaseDefinitionConfig.Project;
                int releasecount = 1;
                if (releases.Count > 0)
                {
                    foreach (JObject rel in releases)
                    {
                        var name = rel["name"];
                        rel["id"] = "";
                        rel["url"] = "";
                        rel["_links"] = "{}";
                        rel["createdBy"] = "{}";
                        rel["createdOn"] = "";
                        rel["modifiedBy"] = "{}";
                        rel["modifiedOn"] = "";

                        var variableGroup = rel["variableGroups"].HasValues ? rel["variableGroups"].ToArray() : new JToken[0];
                        if (variableGroup.Length > 0)
                        {
                            foreach (var groupId in variableGroup)
                            {
                                rel["variableGroups"] = new JArray("$" + variableGroupNameId.Where(x => x.Key == groupId.ToString()).FirstOrDefault().Value + "$");
                            }
                        }
                        else
                        {
                            rel["variableGroups"] = new JArray();
                        }
                        var env = rel["environments"];
                        foreach (var e in env)
                        {
                            e["badgeUrl"] = "";
                            var envVariableGroup = e["variableGroups"].HasValues ? e["variableGroups"].ToArray() : new JToken[0];
                            if (envVariableGroup.Length > 0)
                            {
                                foreach (var envgroupId in envVariableGroup)
                                {
                                    e["variableGroups"] = new JArray("$" + variableGroupNameId.Where(x => x.Key == envgroupId.ToString()).FirstOrDefault().Value + "$");
                                }
                            }
                            else
                            {
                                e["variableGroups"] = new JArray();
                            }
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
                                            string keyConfig = File.ReadAllText(HostingEnvironment.WebRootPath + "/Templates/EndpointKeyConfig.json");
                                            KeyConfig.Keys keyC = new KeyConfig.Keys();
                                            keyC = JsonConvert.DeserializeObject<KeyConfig.Keys>(keyConfig);
                                            if (keyC != null)
                                            {
                                                foreach (var key in keyC.KeysValue)
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
                        }

                        var artifact = rel["artifacts"];
                        if (artifact.HasValues)
                        {
                            foreach (var art in artifact)
                            {
                                string buildName = art["definitionReference"]["definition"]["name"].ToString();
                                string type = art["type"].ToString();
                                if (type.ToLower() == "build")
                                {
                                    art["sourceId"] = "$ProjectId$:" + "$" + buildName + "-id$";
                                    art["definitionReference"]["definition"]["id"] = "$" + buildName + "-id$";
                                    art["definitionReference"]["project"]["id"] = "$ProjectId$";
                                    art["definitionReference"]["project"]["name"] = "$ProjectName$";
                                    art["definitionReference"]["artifactSourceDefinitionUrl"] = "{}";
                                }
                                if (type.ToLower() == "azurecontainerrepository")
                                {
                                    art["sourceId"] = "$ProjectId$:" + "$" + buildName + "-id$";
                                    art["definitionReference"]["connection"]["id"] = "";
                                    art["definitionReference"]["definition"]["id"] = "";
                                    art["definitionReference"]["definition"]["name"] = "";
                                    art["definitionReference"]["registryurl"]["id"] = "";
                                    art["definitionReference"]["registryurl"]["name"] = "";
                                    art["definitionReference"]["resourcegroup"]["id"] = "";
                                    art["definitionReference"]["resourcegroup"]["name"] = "";
                                }
                            }
                        }
                        if (!(Directory.Exists(templatePath + "/ReleaseDefinitions")))
                        {
                            Directory.CreateDirectory(templatePath + "/ReleaseDefinitions");
                            File.WriteAllText(templatePath + "/ReleaseDefinitions/" + name + ".json", JsonConvert.SerializeObject(rel, Formatting.Indented));
                        }
                        else
                        {
                            File.WriteAllText(templatePath + "/ReleaseDefinitions/" + name + ".json", JsonConvert.SerializeObject(rel, Formatting.Indented));
                        }
                        releasecount++;
                    }
                }
                return releasecount;
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace + "\n");
                AddMessage(appConfig.ReleaseDefinitionConfig.Id.ErrorId(), ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return 0;
        }
        /// <summary>
        /// Get different kinds of service endpoints and format it into POST json format
        /// </summary>
        /// <param name="appConfig"></param>
        public void GetServiceEndpoints(ProjectConfigurations appConfig)
        {
            try
            {
                ServiceEndPoint serviceEndPoint = new ServiceEndPoint(appConfig.EndpointConfig);
                Parameters.ServiceEndPoint getServiceEndPoint = serviceEndPoint.GetServiceEndPoints();
                if (getServiceEndPoint.Count > 0)
                {
                    foreach (Parameters.Value endpoint in getServiceEndPoint.Value)
                    {
                        switch (endpoint.Authorization.Scheme)
                        {
                            case "OAuth":
                            case "InstallationToken":
                                switch (endpoint.Type)
                                {
                                    case "github":
                                    case "GitHub":
                                        if (endpoint.Authorization.Parameters == null)
                                        {
                                            endpoint.Authorization.Parameters = new Parameters.Parameters
                                            {
                                                AccessToken = "AccessToken"
                                            };
                                        }
                                        else
                                        {
                                            endpoint.Authorization.Parameters.AccessToken = endpoint.Authorization.Parameters.AccessToken ?? "AccessToken";
                                        }
                                        break;
                                }
                                break;
                            case "UsernamePassword":
                                endpoint.Authorization.Parameters.Username = endpoint.Authorization.Parameters.Username ?? "username";
                                endpoint.Authorization.Parameters.Password = endpoint.Authorization.Parameters.Password ?? "password";
                                break;
                            case "ManagedServiceIdentity":
                                if (endpoint.Authorization.Parameters == null)
                                {
                                    endpoint.Authorization.Parameters = new Parameters.Parameters
                                    {
                                        TenantId = Guid.NewGuid().ToString()
                                    };
                                }
                                else
                                {
                                    endpoint.Authorization.Parameters.TenantId = endpoint.Authorization.Parameters.TenantId ?? Guid.NewGuid().ToString();
                                }
                                break;
                            case "ServicePrincipal":
                                switch (endpoint.Type)
                                {
                                    case "devCenter":
                                        endpoint.Authorization.Parameters.ServicePrincipalKey = endpoint.Authorization.Parameters.ServicePrincipalKey ?? "P2ssw0rd@123";
                                        break;
                                    case "azurerm":
                                        endpoint.Authorization.Parameters.Url = null;
                                        endpoint.Authorization.Parameters.ServicePrincipalId = endpoint.Authorization.Parameters.ServicePrincipalId ?? Guid.NewGuid().ToString();
                                        endpoint.Authorization.Parameters.AuthenticationType = endpoint.Authorization.Parameters.AuthenticationType ?? "spnKey";
                                        endpoint.Authorization.Parameters.TenantId = endpoint.Authorization.Parameters.TenantId ?? Guid.NewGuid().ToString();
                                        endpoint.Authorization.Parameters.ServicePrincipalKey = endpoint.Authorization.Parameters.ServicePrincipalKey ?? "spnKey";
                                        break;
                                }
                                break;
                            case "Certificate":
                                switch (endpoint.Type)
                                {
                                    case "dockerhost":
                                        if (endpoint.Authorization.Parameters == null)
                                        {
                                            endpoint.Authorization.Parameters = new Parameters.Parameters();
                                            endpoint.Authorization.Parameters.Cacert = endpoint.Authorization.Parameters.Cacert ?? "cacert";
                                            endpoint.Authorization.Parameters.Cert = endpoint.Authorization.Parameters.Cert ?? "cert";
                                            endpoint.Authorization.Parameters.Key = endpoint.Authorization.Parameters.Key ?? "key";
                                        }
                                        else
                                        {
                                            endpoint.Authorization.Parameters.Cacert = endpoint.Authorization.Parameters.Cacert ?? "cacert";
                                            endpoint.Authorization.Parameters.Cert = endpoint.Authorization.Parameters.Cert ?? "cert";
                                            endpoint.Authorization.Parameters.Key = endpoint.Authorization.Parameters.Key ?? "key";
                                        }
                                        break;

                                    case "azure":
                                        if (endpoint.Authorization.Parameters == null)
                                        {
                                            endpoint.Authorization.Parameters = new Parameters.Parameters
                                            {
                                                Certificate = "certificate"
                                            };
                                        }
                                        else
                                        {
                                            endpoint.Authorization.Parameters.Certificate = endpoint.Authorization.Parameters.Certificate ?? "certificate";
                                        }
                                        break;
                                }
                                break;
                            case "Token":
                                if (endpoint.Authorization.Parameters == null)
                                {
                                    endpoint.Authorization.Parameters = new Parameters.Parameters
                                    {
                                        Apitoken = "apitoken"
                                    };
                                }
                                else
                                {
                                    endpoint.Authorization.Parameters.Apitoken = endpoint.Authorization.Parameters.Apitoken ?? "apitoken";
                                }
                                break;
                            case "None":
                                switch (endpoint.Type)
                                {
                                    case "AzureServiceBus":
                                        if (endpoint.Authorization.Parameters == null)
                                        {
                                            endpoint.Authorization.Parameters = new Parameters.Parameters
                                            {
                                                ServiceBusConnectionString = "connectionstring"
                                            };
                                        }
                                        else
                                        {
                                            endpoint.Authorization.Parameters.ServiceBusConnectionString = endpoint.Authorization.Parameters.ServiceBusConnectionString ?? "connectionstring";
                                        }
                                        break;
                                    case "externalnugetfeed":
                                        if (endpoint.Authorization.Parameters == null)
                                        {
                                            endpoint.Authorization.Parameters = new Parameters.Parameters
                                            {
                                                Nugetkey = "nugetkey"
                                            };
                                        }
                                        else
                                        {
                                            endpoint.Authorization.Parameters.Nugetkey = endpoint.Authorization.Parameters.Nugetkey ?? "nugetkey";
                                        }
                                        break;
                                }
                                break;

                        }
                        string endpointString = JsonConvert.SerializeObject(endpoint);
                        if (!Directory.Exists(extractedTemplatePath + appConfig.EndpointConfig.Project + "/ServiceEndpoints"))
                        {
                            Directory.CreateDirectory(extractedTemplatePath + appConfig.EndpointConfig.Project + "/ServiceEndpoints");
                            File.WriteAllText(extractedTemplatePath + appConfig.EndpointConfig.Project + "/ServiceEndpoints/", JsonConvert.SerializeObject(endpoint, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                        }
                        else
                        {
                            File.WriteAllText(extractedTemplatePath + appConfig.EndpointConfig.Project + "/ServiceEndpoints/" + endpoint.Name + ".json", JsonConvert.SerializeObject(endpoint, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(serviceEndPoint.LastFailureMessage))
                {
                    AddMessage(appConfig.EndpointConfig.Id.ErrorId(), "Error occured while fetchin service endpoints");
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace + "\n");
            }
        }
        /// <summary>
        /// Get All work item names
        /// </summary>
        /// <param name="appConfig"></param>
        /// <returns></returns>
        private string[] GetAllWorkItemsName(ProjectConfigurations appConfig)
        {
            GetWorkItemsCount getWorkItems = new GetWorkItemsCount(appConfig.WorkItemConfig);
            WorkItemNames.Names workItems = getWorkItems.GetAllWorkItemNames();
            List<string> workItemNames = new List<string>();
            if (workItems.Count > 0)
            {
                foreach (var workItem in workItems.Value)
                {
                    workItemNames.Add(workItem.Name);
                }
            }
            return workItemNames.ToArray();
        }

        private Dictionary<string, string> GetVariableGroups(ProjectConfigurations appConfig)
        {
            VariableGroups variableGroups = new VariableGroups(appConfig.VariableGroupConfig);
            GetVariableGroups.Groups groups = variableGroups.GetVariableGroups();
            Dictionary<string, string> varibaleGroupDictionary = new Dictionary<string, string>();
            string templatePath = extractedTemplatePath + appConfig.ReleaseDefinitionConfig.Project;
            if (groups.Count > 0)
            {
                if (!(Directory.Exists(templatePath + "/VariableGroups")))
                {
                    Directory.CreateDirectory(templatePath + "/VariableGroups");
                    File.WriteAllText(templatePath + "/VariableGroups/VariableGroup.json", JsonConvert.SerializeObject(groups, Formatting.Indented));
                }
                else
                {
                    File.WriteAllText(templatePath + "/VariableGroups/VariableGroup.json", JsonConvert.SerializeObject(groups, Formatting.Indented));
                }
                foreach (var vg in groups.Value)
                {
                    if (!varibaleGroupDictionary.ContainsKey(vg.Id))
                    {
                        varibaleGroupDictionary.Add(vg.Id, vg.Name);
                    }
                }
            }
            return varibaleGroupDictionary;
        }
        #endregion END GENERATE ARTIFACTS
    }
}