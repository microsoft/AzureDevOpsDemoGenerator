using Microsoft.VisualStudio.Services.ExtensionManagement.WebApi;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Mvc;
using VstsDemoBuilder.Extensions;
using VstsDemoBuilder.ExtractorModels;
using VstsDemoBuilder.Models;
using VstsRestAPI.Extractor;
using VstsRestAPI.Viewmodel.Extractor;
using LaunchDarkly.Client;
using System.Web.Hosting;

namespace VstsDemoBuilder.Controllers
{

    public class ExtractorController : Controller
    {
        AccessDetails accessDetails = new AccessDetails();
        EnvironmentController con = new EnvironmentController();
        private static object objLock = new object();
        private static Dictionary<string, string> statusMessages;
        delegate string[] ProcessEnvironment(Project model);
        ExtractorAnalysis analysis = new ExtractorAnalysis();

        LdClient ldClient = new LdClient("sdk-36af231d-d756-445a-b539-97752bbba254");
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

        [AllowAnonymous]
        public ActionResult Index(ProjectList.ProjectCount model)
        {
            string email = Session["Email"].ToString();
            User user = LaunchDarkly.Client.User.WithKey(email.ToLower());
            bool showFeature = ldClient.BoolVariation("extractor", user, false);
            if (showFeature)
            {
                string pat = "";
                if (Session["PAT"] != null)
                {
                    pat = Session["PAT"].ToString();
                }
                if (string.IsNullOrEmpty(pat))
                {
                    return Redirect("../Account/Verify");
                }
                else
                {
                    accessDetails.access_token = pat;
                    ProfileDetails Profile = con.GetProfile(accessDetails);
                    Session["User"] = Profile.displayName;
                    Session["PAT"] = pat;
                    Accounts.AccountList accountList = con.GetAccounts(Profile.id, accessDetails);
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
            else
            {
                return RedirectToAction("NotFound", "Extractor");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ContentResult GetCurrentProgress(string id)
        {
            this.ControllerContext.HttpContext.Response.AddHeader("cache-control", "no-cache");
            var currentProgress = GetStatusMessage(id).ToString();
            return Content(currentProgress);
        }
        /// <summary>
        /// Get status message to resplay
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        [AllowAnonymous]
        public JsonResult GetprojectList(ProjectList.Authentication auth)
        {
            ProjectList.ProjectCount load = new ProjectList.ProjectCount();
            string accname = auth.accname;
            string _credentials = auth.pat;
            string URL = "https://" + accname + ".visualstudio.com/DefaultCollection/";

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("appication/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);
                    HttpResponseMessage response = client.GetAsync("/_apis/projects?stateFilter=WellFormed&api-version=1.0").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string res = response.Content.ReadAsStringAsync().Result;
                        load = JsonConvert.DeserializeObject<ProjectList.ProjectCount>(res);
                        if (load.count == 0)
                        {
                            load.errmsg = "No projects found!";
                        }
                    }
                    else
                    {
                        var res = response.Content.ReadAsStringAsync().Result;
                    }
                }
            }
            catch (Exception ex)
            {
                load.errmsg = ex.Message.ToString();
                string message = ex.Message.ToString();
            }
            return Json(load, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult GetProjectPropertirs(string accname, string project, string _credentials)
        {
            string URL = "https://" + accname + ".visualstudio.com/DefaultCollection/";
            ProjectProperties.Properties load = new ProjectProperties.Properties();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("appication/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);
                    HttpResponseMessage response = client.GetAsync("/_apis/projects/" + project + "/properties?api-version=4.1-preview.1").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string res = response.Content.ReadAsStringAsync().Result;
                        load = JsonConvert.DeserializeObject<ProjectProperties.Properties>(res);
                        return Json(load, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var res = response.Content.ReadAsStringAsync().Result;
                        return Json(res, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        public bool startExtractionProcess(Project model)
        {
            //ProcessEnvironment processTask = new ProcessEnvironment(CreateProjectEnvironment);
            //processTask.BeginInvoke(model, model.accessToken, model.accountName, model.ProjectName, new AsyncCallback(EndEnvironmentSetupProcess), processTask);
            return true;
        }
        public void EndEnvironmentSetupProcess(IAsyncResult result)
        {
            ProcessEnvironment processTask = (ProcessEnvironment)result.AsyncState;
            string[] strResult = processTask.EndInvoke(result);
        }
        [AllowAnonymous]
        public JsonResult CreateProjectEnvironment(Project model)
        {
            VstsRestAPI.Configuration config = new VstsRestAPI.Configuration() { UriString = "https://" + model.accountName + ".visualstudio.com:", PersonalAccessToken = model.accessToken, Project = model.ProjectName, AccountName = model.accountName };
            analysis.teamCount = GetTeamListtoSave(model.ProjectName, model.accountName, model.accessToken);
            analysis.IterationCount = GetiterationCount(config);
            GetWorkItemDetails(config);
            GetiterationCount(config);
            GetbuildDefinitoin(config);
            GetReleaseDefinitoin(config);
            return Json(analysis, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public int GetTeamListtoSave(string projectName, string AccountName, string pat)
        {
            Teams.TeamList teamObj = new Teams.TeamList();
            SrcTeamsList _team = new SrcTeamsList();
            string URL = "https://" + AccountName + ".visualstudio.com/";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(URL);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", pat);
                HttpResponseMessage response = client.GetAsync("_apis/projects/" + projectName + "/teams?api-version=2.2").Result;
                if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string res = response.Content.ReadAsStringAsync().Result;
                    teamObj = JsonConvert.DeserializeObject<Teams.TeamList>(res);
                    return teamObj.count;
                }
            }
            return 0;
        }

        public int GetiterationCount(VstsRestAPI.Configuration con)
        {
            ClassificationNodes nodes = new ClassificationNodes(con);
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
                    return 0;
                }
                return 0;
            }

        }

        public void GetWorkItemDetails(VstsRestAPI.Configuration con)
        {
            GetWorkItemsCount itemsCount = new GetWorkItemsCount(con);
            WorkItemFetchResponse.WorkItems fetchedEpics = itemsCount.getWorkItemsfromSource("Epic");
            analysis.fetchedEpics = fetchedEpics.count;
            WorkItemFetchResponse.WorkItems fetchedFeatures = itemsCount.getWorkItemsfromSource("Feature");
            analysis.fetchedFeatures = fetchedFeatures.count;

            WorkItemFetchResponse.WorkItems fetchedPBIs = itemsCount.getWorkItemsfromSource("Product Backlog Item");
            analysis.fetchedPBIs = fetchedPBIs.count;

            WorkItemFetchResponse.WorkItems fetchedTasks = itemsCount.getWorkItemsfromSource("Task");
            analysis.fetchedTasks = fetchedTasks.count;

            WorkItemFetchResponse.WorkItems fetchedTestCase = itemsCount.getWorkItemsfromSource("Test Case");
            analysis.fetchedTestCase = fetchedTestCase.count;

            WorkItemFetchResponse.WorkItems fetchedBugs = itemsCount.getWorkItemsfromSource("Bug");
            analysis.fetchedBugs = fetchedBugs.count;

            WorkItemFetchResponse.WorkItems fetchedUserStories = itemsCount.getWorkItemsfromSource("User Story");
            analysis.fetchedUserStories = fetchedUserStories.count;

            WorkItemFetchResponse.WorkItems fetchedTestSuits = itemsCount.getWorkItemsfromSource("Test Suite");
            analysis.fetchedTestSuits = fetchedTestSuits.count;

            WorkItemFetchResponse.WorkItems fetchedTestPlan = itemsCount.getWorkItemsfromSource("Test Plan");
            analysis.fetchedTestPlan = fetchedTestPlan.count;

            WorkItemFetchResponse.WorkItems fetchedFeedbackRequest = itemsCount.getWorkItemsfromSource("Feedback Request");
            analysis.fetchedFeedbackRequest = fetchedFeedbackRequest.count;
        }

        public void GetbuildDefinitoin(VstsRestAPI.Configuration con)
        {
            GetBuildandReleaseDefs buildandReleaseDefs = new GetBuildandReleaseDefs(con);
            GetBuildDefResponse.BuildDef buildDef = new GetBuildDefResponse.BuildDef();
            buildDef = buildandReleaseDefs.GetBuildDefCount();
            if (buildDef.count > 0)
            {
                analysis.BuildDefCount = buildDef.count;
            }
            else
            {
                analysis.BuildDefCount = 0;
            }
        }

        public void GetReleaseDefinitoin(VstsRestAPI.Configuration con)
        {
            GetBuildandReleaseDefs buildandReleaseDefs = new GetBuildandReleaseDefs(con);
            GetReleaseDefResponse.ReleaseDef releaseDef = new GetReleaseDefResponse.ReleaseDef();
            releaseDef = buildandReleaseDefs.GetReleaseDefCount();
            if (releaseDef.count > 0)
            {
                analysis.ReleaseDefCount = releaseDef.count;
            }
            else
            {
                analysis.ReleaseDefCount = 0;
            }
        }
        [AllowAnonymous]
        public JsonResult CheckForInstalledExtensions(string AccountName, string PAT)
        {
            try
            {
                List<ProjectList.AccountExtension> exa = new List<ProjectList.AccountExtension>();
                var connection = new VssConnection(new Uri(string.Format("https://{0}.visualstudio.com", AccountName)), new VssOAuthAccessTokenCredential(PAT));
                var client = connection.GetClient<ExtensionManagementHttpClient>();
                var installed = client.GetInstalledExtensionsAsync().Result;
                var extensions = installed.Where(x => x.Flags == 0).ToList();

                foreach (var extension in extensions)
                {
                    ProjectList.AccountExtension accountExtension = new ProjectList.AccountExtension();

                    accountExtension.ExtensionId = extension.ExtensionName;
                    accountExtension.PublisherId = extension.PublisherName;
                    accountExtension.name = extension.ExtensionDisplayName;
                    exa.Add(accountExtension);
                }

                // System.IO.File.WriteAllText(Server.MapPath("\\Templates\\Extension.json"), JsonConvert.SerializeObject(extensions, Formatting.Indented));
                return Json(exa, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }
            return null;

        }

        [HttpPost]
        [AllowAnonymous]
        public bool StartEnvironmentSetupProcess(Project model)
        {
            Location.IPHostGenerator IpCon = new Location.IPHostGenerator();
            string IP = IpCon.GetVisitorDetails();
            string Region = IpCon.GetLocation(IP);
            model.Region = Region;
            AddMessage(model.id, string.Empty);
            AddMessage(model.id.ErrorId(), string.Empty);
            System.Web.HttpContext.Current.Session["Project"] = model.ProjectName;

            ProcessEnvironment processTask = new ProcessEnvironment(GenerateTemplateArifacts);
            processTask.BeginInvoke(model, new AsyncCallback(EndEnvironmentSetupProcess), processTask);
            return true;
        }
        [AllowAnonymous]
        public string[] GenerateTemplateArifacts(Project model)
        {
            VstsRestAPI.Configuration config = new VstsRestAPI.Configuration() { UriString = "https://" + model.accountName + ".visualstudio.com:", PersonalAccessToken = model.accessToken, Project = model.ProjectName, AccountName = model.accountName };
            bool isTeam = GetTeamList(config);
            if (isTeam)
            {
                AddMessage(model.id, "Teams Definition");
            }

            bool isIteration = GetIterations(config);
            if (isIteration)
            {
                AddMessage(model.id, "Iterations Definition");
            }
            string projectSetting = "";
            projectSetting = System.IO.File.ReadAllText(Server.MapPath("~") + @"PreSetting\ProjectSettings.json");
            projectSetting = projectSetting.Replace("$type$", model.ProcessTemplate);
            System.IO.File.WriteAllText(Server.MapPath("~") + @"ExtractedTemplate\" + model.ProjectName + "\\ProjectSettings.json", projectSetting);

            string ProjectTemplate = "";
            ProjectTemplate = System.IO.File.ReadAllText(Server.MapPath("~") + @"PreSetting\ProjectTemplate.json");
            System.IO.File.WriteAllText(Server.MapPath("~") + @"ExtractedTemplate\" + model.ProjectName + "\\ProjectTemplate.json", ProjectTemplate);

            string TeamArea = "";
            TeamArea = System.IO.File.ReadAllText(Server.MapPath("~") + @"PreSetting\TeamArea.json");
            System.IO.File.WriteAllText(Server.MapPath("~") + @"ExtractedTemplate\" + model.ProjectName + "\\TeamArea.json", TeamArea);
            AddMessage(model.id, "Team Areas Definition");

            GetWorkItems(config);
            AddMessage(model.id, "Work Items Definition");

            int count = GetBuildDef(config);
            if (count > 1)
                AddMessage(model.id, "Build Definition");

            int relCount = GetReleaseDef(config);
            if (relCount > 1)
            {
                AddMessage(model.id, "Release Definition");
            }

            GetRepositoryList(config);
            AddMessage(model.id, "Repository and Service Endpoint Definition");


            string startPath = Path.Combine(Server.MapPath("~") + @"ExtractedTemplate\", model.ProjectName);
            string zipPath = Path.Combine(Server.MapPath("~") + @"ExtractedTemplate\", model.ProjectName + ".zip");
            if (System.IO.File.Exists(zipPath))
            {
                System.IO.File.Delete(zipPath);
            }
            ZipFile.CreateFromDirectory(startPath, zipPath);
            Directory.Delete(Path.Combine(Server.MapPath("~") + @"ExtractedTemplate\", model.ProjectName), true);
            StatusMessages[model.id] = "100";
            return new string[] { model.id, "" };
        }
        public bool GetTeamList(VstsRestAPI.Configuration con)
        {
            ClassificationNodes nodes = new ClassificationNodes(con);
            SrcTeamsList _team = new SrcTeamsList();

            _team = nodes.GetTeamList();

            string fetchedJson = JsonConvert.SerializeObject(_team, Formatting.Indented);
            if (fetchedJson != "")
            {
                fetchedJson = fetchedJson.Remove(0, 14);
                fetchedJson = fetchedJson.Remove(fetchedJson.Length - 1);
                if (!Directory.Exists(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project))
                    Directory.CreateDirectory(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project);
                System.IO.File.WriteAllText(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project + "\\Teams.json", fetchedJson);
                return true;
            }
            else
            {
                string error = nodes.LastFailureMessage;
                return false;
            }
        }
        public bool GetIterations(VstsRestAPI.Configuration con)
        {
            try
            {
                ClassificationNodes nodes = new ClassificationNodes(con);
                ItearationList.Iterations viewModel = new ItearationList.Iterations();
                viewModel = nodes.GetIterations();
                string fetchedJson = JsonConvert.SerializeObject(viewModel, Formatting.Indented);
                if (fetchedJson != "")
                {
                    if (!Directory.Exists(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project))
                        Directory.CreateDirectory(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project);
                    System.IO.File.WriteAllText(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project + "\\Iterations.json", fetchedJson);
                    return true;
                }
                else
                {
                    string error = nodes.LastFailureMessage;
                    return false;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }
        public void GetWorkItems(VstsRestAPI.Configuration con)
        {
            if (!Directory.Exists(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project))
                Directory.CreateDirectory(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project);

            GetWorkItemsCount itemsCount = new GetWorkItemsCount(con);
            WorkItemFetchResponse.WorkItems fetchedEpics = itemsCount.getWorkItemsfromSource("Epic");
            string EpicJson = JsonConvert.SerializeObject(fetchedEpics, Formatting.Indented);
            if (EpicJson != null || EpicJson != "")
                System.IO.File.WriteAllText(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project + "\\EpicfromTemplate.json", EpicJson);

            WorkItemFetchResponse.WorkItems fetchedFeatures = itemsCount.getWorkItemsfromSource("Feature");
            string FeatureJson = JsonConvert.SerializeObject(fetchedFeatures, Formatting.Indented);
            if (FeatureJson != null || FeatureJson != "")
                System.IO.File.WriteAllText(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project + "\\FeaturefromTemplate.json", FeatureJson);

            WorkItemFetchResponse.WorkItems fetchedPBIs = itemsCount.getWorkItemsfromSource("Product Backlog Item");
            string PBIJson = JsonConvert.SerializeObject(fetchedPBIs, Formatting.Indented);
            if (PBIJson != null || PBIJson != "")
                System.IO.File.WriteAllText(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project + "\\PBIfromTemplate.json", PBIJson);

            WorkItemFetchResponse.WorkItems fetchedTasks = itemsCount.getWorkItemsfromSource("Task");
            string TaskJson = JsonConvert.SerializeObject(fetchedTasks, Formatting.Indented);
            if (TaskJson != null || TaskJson != "")
                System.IO.File.WriteAllText(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project + "\\TaskfromTemplate.json", TaskJson);

            WorkItemFetchResponse.WorkItems fetchedTestCase = itemsCount.getWorkItemsfromSource("Test Case");
            string TestCasesJson = JsonConvert.SerializeObject(fetchedTestCase, Formatting.Indented);
            if (TestCasesJson != null || TestCasesJson != "")
                System.IO.File.WriteAllText(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project + "\\TestCasefromTemplate.json", TestCasesJson);

            WorkItemFetchResponse.WorkItems fetchedBugs = itemsCount.getWorkItemsfromSource("Bug");
            string BugJson = JsonConvert.SerializeObject(fetchedBugs, Formatting.Indented);
            if (BugJson != null || BugJson != "")
                System.IO.File.WriteAllText(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project + "\\BugfromTemplate.json", BugJson);

            WorkItemFetchResponse.WorkItems fetchedUserStories = itemsCount.getWorkItemsfromSource("User Story");
            string UserStoryJson = JsonConvert.SerializeObject(fetchedUserStories, Formatting.Indented);
            if (UserStoryJson != null || UserStoryJson != "")
                System.IO.File.WriteAllText(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project + "\\UserStoriesfromTemplate.json", UserStoryJson);
        }

        public void GetRepositoryList(VstsRestAPI.Configuration con)
        {
            GetBuildandReleaseDefs buildandReleaseDefs = new GetBuildandReleaseDefs(con);
            RepositoryList.Repository repos = buildandReleaseDefs.GetRepoList();
            if (repos.count > 0)
            {
                foreach (var repo in repos.value)
                {
                    string Host = "https://dev.azure.com/" + con.AccountName + "/" + con.Project;
                    string SourceCodeJson = System.IO.File.ReadAllText(Server.MapPath("~") + @"PreSetting\ImportSourceCode.json");
                    SourceCodeJson = SourceCodeJson.Replace("$Host$", Host).Replace("$Repo$", repo.name);
                    string EndPointJson = System.IO.File.ReadAllText(Server.MapPath("~") + @"PreSetting\ServiceEndPoint.json");
                    EndPointJson = EndPointJson.Replace("$Host$", Host).Replace("$Repo$", repo.name);
                    if (!Directory.Exists(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project + "\\ImportSourceCode"))
                    {
                        Directory.CreateDirectory(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project + "\\ImportSourceCode");
                        System.IO.File.WriteAllText(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project + "\\ImportSourceCode\\" + repo.name + ".json", SourceCodeJson);
                    }
                    else
                    {
                        System.IO.File.WriteAllText(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project + "\\ImportSourceCode\\" + repo.name + ".json", SourceCodeJson);
                    }
                    if (!Directory.Exists(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project + "\\ServiceEndpoints"))
                    {
                        Directory.CreateDirectory(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project + "\\ServiceEndpoints");
                        System.IO.File.WriteAllText(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project + "\\ServiceEndpoints\\" + repo.name + "-code.json", EndPointJson);
                    }
                    else
                    {
                        System.IO.File.WriteAllText(Server.MapPath("~") + @"ExtractedTemplate\" + con.Project + "\\ServiceEndpoints\\" + repo.name + "-code.json", EndPointJson);
                    }

                }
            }
        }

        public int GetBuildDef(VstsRestAPI.Configuration con)
        {
            GetBuildandReleaseDefs buildandReleaseDefs = new GetBuildandReleaseDefs(con);
            List<JObject> builds = buildandReleaseDefs.ExportBuildDefinitions();
            RepositoryList.Repository repo = buildandReleaseDefs.GetRepoList();
            if (builds.Count > 0)
            {
                int count = 1;
                //creating ImportCode Json file
                string TemplatePath = Server.MapPath("~") + @"ExtractedTemplate\" + con.Project;
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
                    def["repository"]["id"] = "$" + repoName + "$";
                    def["repository"]["url"] = "";
                    def["authoredBy"] = "{}";
                    def["authoredBy"] = "{}";
                    def["project"] = "{}";
                    def["url"] = "";
                    def["uri"] = "";
                    def["id"] = "";
                    def["queue"]["id"] = "";
                    def["queue"]["pool"]["id"] = "";
                    def["_links"] = "{}";
                    def["createdDate"] = "";

                    if (!(Directory.Exists(TemplatePath + "\\BuildDefinitions")))
                    {
                        Directory.CreateDirectory(TemplatePath + "\\BuildDefinitions");
                        System.IO.File.WriteAllText(TemplatePath + "\\BuildDefinitions\\BuildDef" + count + ".json", JsonConvert.SerializeObject(def, Formatting.Indented));
                    }
                    else
                    {
                        System.IO.File.WriteAllText(TemplatePath + "\\BuildDefinitions\\BuildDef" + count + ".json", JsonConvert.SerializeObject(def, Formatting.Indented));
                    }


                    count = count + 1;
                }
                return count;
            }
            return 1;
        }

        public int GetReleaseDef(VstsRestAPI.Configuration con)
        {
            GetBuildandReleaseDefs releaseDefs = new GetBuildandReleaseDefs(con);
            List<JObject> releases = releaseDefs.GetReleaseDefs();
            Dictionary<string, int> queue = releaseDefs.GetQueues();
            string TemplatePath = Server.MapPath("~") + @"ExtractedTemplate\" + con.Project;
            if (releases.Count > 0)
            {
                int count = 1;
                foreach (JObject rel in releases)
                {
                    ReleaseDefResponse.Response responseObj = new ReleaseDefResponse.Response();
                    responseObj = JsonConvert.DeserializeObject<ReleaseDefResponse.Response>(rel.ToString());
                    foreach (var ownerObj in responseObj.environments)
                    {
                        ownerObj.owner.id = "$OwnerId$";
                        ownerObj.owner.displayName = "$OwnerDisplayName$";
                        ownerObj.owner.uniqueName = "$OwnerUniqueName$";
                        if (ownerObj.deployPhases.Count > 0)
                        {
                            foreach (var depPhase in ownerObj.deployPhases)
                            {
                                string queuename = "";
                                if (queue != null)
                                {
                                    if (queue.Count > 0)
                                    {
                                        var agenetName = queue.Where(x => x.Value.ToString() == depPhase.deploymentInput.queueId).FirstOrDefault();
                                        if (agenetName.Key != null)
                                        {
                                            queuename = agenetName.Key.ToString();
                                        }
                                    }
                                }
                                if (queuename != "")
                                {
                                    depPhase.deploymentInput.queueId = "$" + queuename + "$";
                                }
                                else
                                {
                                    depPhase.deploymentInput.queueId = "";
                                }
                                if (depPhase.workflowTasks.Count > 0)
                                {
                                    foreach (var workFlow in depPhase.workflowTasks)
                                    {
                                        workFlow.inputs.ConnectedServiceName = "";
                                        workFlow.inputs.ConnectedServiceNameARM = "";
                                        workFlow.inputs.deploymentGroupEndpoint = "";
                                    }
                                }
                            }
                        }
                    }
                    if (responseObj.artifacts.Count > 0)
                    {
                        foreach (var artifact in responseObj.artifacts)
                        {
                            string buildName = artifact.definitionReference.definition.name;

                            artifact.sourceId = "$ProjectId$:" + "$" + buildName + "-id$";
                            artifact.definitionReference.definition.id = "$" + buildName + "-id$";

                            artifact.definitionReference.project.id = "$ProjectId$";
                            artifact.definitionReference.project.name = "$ProjectName$";
                        }
                        if (!(Directory.Exists(TemplatePath + "\\ReleaseDefinitions")))
                        {
                            Directory.CreateDirectory(TemplatePath + "\\ReleaseDefinitions");
                            System.IO.File.WriteAllText(TemplatePath + "\\ReleaseDefinitions\\ReleaseDef" + count + ".json", JsonConvert.SerializeObject(responseObj, Formatting.Indented));
                        }
                        else
                        {
                            System.IO.File.WriteAllText(TemplatePath + "\\BuildDefinitions\\ReleaseDef" + count + ".json", JsonConvert.SerializeObject(responseObj, Formatting.Indented));
                        }
                    }
                }
                return count;
            }
            return 1;
        }
        [AllowAnonymous]
        private void RemoveFolder()
        {
            string projectName = Session["Project"].ToString();
            if (projectName != "")
            {
                System.IO.File.Delete(Server.MapPath("~") + @"ExtractedTemplate\" + projectName);
                System.IO.File.Delete(Server.MapPath("~") + @"ExtractedTemplate\" + projectName + ".zip");
            }

        }
    }
}
public class TeamData
{
    public string name { get; set; }
    public string description { get; set; }
}