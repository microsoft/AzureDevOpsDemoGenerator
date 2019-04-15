﻿using log4net;
using Microsoft.VisualStudio.Services.ExtensionManagement.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VstsDemoBuilder.Extensions;
using VstsDemoBuilder.Models;
using VstsRestAPI;
using VstsRestAPI.Build;
using VstsRestAPI.DeploymentGRoup;
using VstsRestAPI.Git;
using VstsRestAPI.ProjectsAndTeams;
using VstsRestAPI.QueriesAndWidgets;
using VstsRestAPI.Queues;
using VstsRestAPI.Release;
using VstsRestAPI.Service;
using VstsRestAPI.TestManagement;
using VstsRestAPI.Viewmodel.Extractor;
using VstsRestAPI.Viewmodel.Importer;
using VstsRestAPI.Viewmodel.ProjectAndTeams;
using VstsRestAPI.Viewmodel.QueriesAndWidgets;
using VstsRestAPI.Viewmodel.Repository;
using VstsRestAPI.Viewmodel.Sprint;
using VstsRestAPI.Viewmodel.Wiki;
using VstsRestAPI.Viewmodel.WorkItem;
using VstsRestAPI.Wiki;
using VstsRestAPI.WorkItemAndTracking;

namespace VstsDemoBuilder.Controllers
{
    public class EnvironmentController : Controller
    {

        #region Variables & Properties
        private static readonly object objLock = new object();
        private static Dictionary<string, string> statusMessages;
        private ILog logger = LogManager.GetLogger("ErrorLog");

        private delegate string[] ProcessEnvironment(Project model, string PAT, string accountName);
        public bool isDefaultRepoTodetele = true;
        public string websiteUrl = string.Empty;
        public string templateUsed = string.Empty;
        public string projectName = string.Empty;
        private string extractPath = string.Empty;
        private AccessDetails _accessDetails = new AccessDetails();
        private string logPath = "";
        private string templateVersion = string.Empty;
        private string enableExtractor = "";
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
        #endregion

        #region Manage Status Messages
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

        /// <summary>
        /// Get Template Name to display
        /// </summary>
        /// <param name="TemplateName"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public ContentResult GetTemplate(string TemplateName)
        {
            string templatesPath = Server.MapPath("~") + @"\Templates\";
            string template = string.Empty;

            if (System.IO.File.Exists(templatesPath + Path.GetFileName(TemplateName) + @"\ProjectTemplate.json"))
            {
                Project objP = new Project();
                template = objP.ReadJsonFile(templatesPath + Path.GetFileName(TemplateName) + @"\ProjectTemplate.json");
            }
            return Content(template);
        }

        /// <summary>
        /// Get groups. based on group selection get template
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public JsonResult GetGroups()
        {
            string groupDetails = "";
            TemplateSelection.Templates templates = new TemplateSelection.Templates();
            string templatesPath = ""; templatesPath = Server.MapPath("~") + @"\Templates\";
            string email = Session["Email"].ToString();
            if (System.IO.File.Exists(templatesPath + "TemplateSetting.json"))
            {
                groupDetails = System.IO.File.ReadAllText(templatesPath + @"\TemplateSetting.json");
                templates = JsonConvert.DeserializeObject<TemplateSelection.Templates>(groupDetails);
                foreach (var Group in templates.GroupwiseTemplates)
                {
                    if (Group.Groups != "Private" && Group.Groups != "PrivateTemp")
                    {
                        foreach (var template in Group.Template)
                        {
                            string templateFolder = template.TemplateFolder;
                            if (!string.IsNullOrEmpty(templateFolder))
                            {
                                DateTime dateTime = System.IO.Directory.GetLastWriteTime(templatesPath + "\\" + templateFolder);
                                template.LastUpdatedDate = dateTime.ToShortDateString();
                            }
                        }
                    }
                }
                enableExtractor = Session["EnableExtractor"] != null ? Session["EnableExtractor"].ToString() : string.Empty;
                if (string.IsNullOrEmpty(enableExtractor) || enableExtractor == "false")
                {
                    TemplateSelection.Templates _templates = new TemplateSelection.Templates();
                    _templates.Groups = new List<string>();
                    foreach (var group in templates.Groups)
                    {
                        if (group.ToLower() != "private")
                        {
                            _templates.Groups.Add(group);
                        }
                    }
                    templates.Groups = _templates.Groups;
                }
            }
            return Json(templates, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Controller Actions
        /// <summary>
        /// View ProjectSetUp
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult CreateProject()
        {
            try
            {
                string TemplateSelected = string.Empty;
                if (Session["visited"] != null)
                {
                    Project model = new Project();
                    if (Session["EnableExtractor"] != null)
                    {
                        model.EnableExtractor = Session["EnableExtractor"].ToString();
                        enableExtractor = model.EnableExtractor.ToLower();
                    }
                    if (Session["templateName"] != null && Session["templateName"].ToString() != "")
                    {
                        model.TemplateName = Session["templateName"].ToString();
                        TemplateSelected = model.TemplateName;
                    }
                    else
                    {
                        TemplateSelected = System.Configuration.ConfigurationManager.AppSettings["DefaultTemplate"];
                    }

                    if (Session["PAT"] != null)
                    {
                        _accessDetails.access_token = Session["PAT"].ToString();
                        ProfileDetails profile = GetProfile(_accessDetails);
                        if (profile.displayName != null || profile.emailAddress != null)
                        {
                            Session["User"] = profile.displayName ?? string.Empty;
                            Session["Email"] = profile.emailAddress ?? profile.displayName.ToLower();
                        }
                        if (profile.id != null)
                        {
                            AccountsResponse.AccountList accountList = GetAccounts(profile.id, _accessDetails);

                            //New Feature Enabling
                            model.accessToken = Session["PAT"].ToString();
                            model.refreshToken = _accessDetails.refresh_token;
                            Session["PAT"] = _accessDetails.access_token;
                            model.MemberID = profile.id;
                            List<string> accList = new List<string>();
                            if (accountList.count > 0)
                            {
                                foreach (var account in accountList.value)
                                {
                                    accList.Add(account.accountName);
                                }
                                accList.Sort();
                                model.accountsForDropdown = accList;
                                model.hasAccount = true;
                            }
                            else
                            {
                                model.accountsForDropdown.Add("Select Organization");
                                ViewBag.AccDDError = "Could not load your organizations. Please change the directory in profile page of Azure DevOps Organization and try again.";
                            }

                            model.Templates = new List<string>();
                            model.accountUsersForDdl = new List<SelectListItem>();
                            TemplateSelection.Templates templates = new TemplateSelection.Templates();
                            string[] dirTemplates = Directory.GetDirectories(Server.MapPath("~") + @"\Templates");

                            //Taking all the template folder and adding to list
                            foreach (string template in dirTemplates)
                            {
                                model.Templates.Add(Path.GetFileName(template));
                            }
                            // Reading Template setting file to check for private templates
                            if (System.IO.File.Exists(Server.MapPath("~") + @"\Templates\TemplateSetting.json"))
                            {
                                string templateSetting = model.ReadJsonFile(Server.MapPath("~") + @"\Templates\TemplateSetting.json");
                                templates = JsonConvert.DeserializeObject<TemplateSelection.Templates>(templateSetting);
                            }
                            //[for direct URLs] if the incoming template name is not null, checking for Template name in Template setting file. 
                            //if exist, will append the template name to Selected template textbox, else will append the SmartHotel360 template
                            if (!string.IsNullOrEmpty(TemplateSelected))
                            {
                                foreach (var grpTemplate in templates.GroupwiseTemplates)
                                {
                                    foreach (var template in grpTemplate.Template)
                                    {
                                        if (template.Name != null)
                                        {
                                            if (template.Name.ToLower() == TemplateSelected.ToLower())
                                            {
                                                model.SelectedTemplate = template.Name;
                                                model.Templates.Add(template.Name);
                                                model.selectedTemplateDescription = template.Description == null ? string.Empty : template.Description;
                                                model.selectedTemplateFolder = template.TemplateFolder == null ? string.Empty : template.TemplateFolder;
                                                model.Message = template.Message == null ? string.Empty : template.Message;
                                            }
                                        }
                                    }
                                }
                            }
                            return View(model);
                        }
                    }
                    return Redirect("../Account/Verify");
                }
                else
                {
                    Session.Clear();
                    return Redirect("../Account/Verify");
                }
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                ViewBag.ErrorMessage = ex.Message;
                return Redirect("../Account/Verify");
            }
        }

        /// <summary>
        /// Call to Create View()
        /// </summary>
        /// <param name="model"></param>
        /// <returns>View()</returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Create(Project model)
        {
            try
            {
                string isCode = Request.QueryString["code"];
                if (isCode == null)
                {
                    return Redirect("../Account/Verify");
                }
                if (Session["visited"] != null)
                {
                    if (Session["templateName"] != null && Session["templateName"].ToString() != "")
                    {
                        model.TemplateName = Session["templateName"].ToString();
                    }
                    string code = Request.QueryString["code"];

                    string redirectUrl = System.Configuration.ConfigurationManager.AppSettings["RedirectUri"];
                    string clientId = System.Configuration.ConfigurationManager.AppSettings["ClientSecret"];
                    string accessRequestBody = GenerateRequestPostData(clientId, code, redirectUrl);
                    _accessDetails = GetAccessToken(accessRequestBody);
                    if (!string.IsNullOrEmpty(_accessDetails.access_token))
                    {
                        // add your access token here for local debugging                 
                        //AccessDetails.access_token = "";
                        model.accessToken = _accessDetails.access_token;
                        Session["PAT"] = _accessDetails.access_token;
                    }
                    return RedirectToAction("createproject", "Environment");
                }
                else
                {
                    Session.Clear();
                    return Redirect("../Account/Verify");
                }
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                return View();
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult UploadFiles()
        {
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {

                        HttpPostedFileBase file = files[i];
                        string fileName;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testFiles = file.FileName.Split(new char[] { '\\' });
                            fileName = testFiles[testFiles.Length - 1];
                            if (System.IO.File.Exists(Path.Combine(Server.MapPath("~/Templates/"), fileName)))
                            {
                                System.IO.File.Delete(Path.Combine(Server.MapPath("~/Templates/"), fileName));
                            }
                        }
                        else
                        {
                            fileName = file.FileName;

                            if (System.IO.File.Exists(Path.Combine(Server.MapPath("~/Templates/"), fileName)))
                            {
                                System.IO.File.Delete(Path.Combine(Server.MapPath("~/Templates/"), fileName));
                            }
                        }

                        // Get the complete folder path and store the file inside it.  
                        fileName = Path.Combine(Server.MapPath("~/Templates/"), fileName);
                        file.SaveAs(fileName);
                    }
                    // Returns message that successfully uploaded  
                    return Json("1");
                }
                catch (Exception ex)
                {
                    logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult UnzipFile(string fineName)
        {
            try
            {
                if (!System.IO.Directory.Exists(Server.MapPath("~") + @"\Logs"))
                {
                    Directory.CreateDirectory(Server.MapPath("~") + @"\Logs");
                }

                string zipPath = Server.MapPath("~/Templates/" + fineName);
                string folder = fineName.Replace(".zip", "");

                extractPath = Server.MapPath("~/Templates/" + folder);

                if (Directory.Exists(extractPath))
                {
                    System.IO.File.Delete(Server.MapPath("~/Templates/" + fineName));
                    return Json("Folder already exist. Please rename the folder and upload it.");
                }
                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);
                System.IO.File.Delete(zipPath);

                bool settingFile = (System.IO.File.Exists(extractPath + "\\ProjectSettings.json") ? true : false);
                bool projectFile = (System.IO.File.Exists(extractPath + "\\ProjectTemplate.json") ? true : false);

                if (settingFile && projectFile)
                {
                    string projectFileData = System.IO.File.ReadAllText(extractPath + "\\ProjectTemplate.json");
                    ProjectSetting settings = JsonConvert.DeserializeObject<ProjectSetting>(projectFileData);

                    if (!string.IsNullOrEmpty(settings.IsPrivate))
                    {
                        return Json("SUCCESS");
                    }
                    else
                    {
                        Directory.Delete(extractPath, true);
                        return Json("ISPRIVATEERROR");
                    }
                }
                else if (!settingFile && !projectFile)
                {
                    string[] folderName = System.IO.Directory.GetDirectories(extractPath);
                    string subDir = "";
                    if (folderName.Length > 0)
                    {
                        subDir = folderName[0];
                    }
                    else
                    {
                        return Json("Could not find required preoject setting and project template file.");
                    }

                    if (subDir != "")
                    {
                        bool settingFile1 = (System.IO.File.Exists(subDir + "\\ProjectSettings.json") ? true : false);
                        bool projectFile1 = (System.IO.File.Exists(subDir + "\\ProjectTemplate.json") ? true : false);
                        if (settingFile1 && projectFile1)
                        {
                            string projectFileData1 = System.IO.File.ReadAllText(subDir + "\\ProjectTemplate.json");
                            ProjectSetting settings1 = JsonConvert.DeserializeObject<ProjectSetting>(projectFileData1);

                            if (!string.IsNullOrEmpty(settings1.IsPrivate))
                            {
                                string sourceDirectory = subDir;
                                string targetDirectory = extractPath;
                                string backupDirectory = System.Web.HttpContext.Current.Server.MapPath("~/TemplateBackUp/");
                                if (!Directory.Exists(backupDirectory))
                                {
                                    Directory.CreateDirectory(backupDirectory);
                                }
                                //Create a tempprary directory
                                string backupDirectoryRandom = backupDirectory + DateTime.Now.ToString("MMMdd_yyyy_HHmmss");

                                if (Directory.Exists(sourceDirectory))
                                {

                                    if (Directory.Exists(targetDirectory))
                                    {

                                        //copy the content of source directory to temp directory
                                        Directory.Move(sourceDirectory, backupDirectoryRandom);

                                        //Delete the target directory
                                        Directory.Delete(targetDirectory);

                                        //Target Directory should not be exist, it will create a new directory
                                        Directory.Move(backupDirectoryRandom, targetDirectory);

                                        DirectoryInfo di = new DirectoryInfo(backupDirectory);

                                        foreach (FileInfo file in di.GetFiles())
                                        {
                                            file.Delete();
                                        }
                                        foreach (DirectoryInfo dir in di.GetDirectories())
                                        {
                                            dir.Delete(true);
                                        }
                                    }
                                }

                                return Json("SUCCESS");
                            }
                            else
                            {
                                Directory.Delete(extractPath, true);
                                return Json("ISPRIVATEERROR");
                            }
                        }
                    }
                    Directory.Delete(extractPath, true);
                    return Json("PROJECTANDSETTINGNOTFOUND");
                }
                else
                {
                    if (!settingFile)
                    {
                        Directory.Delete(extractPath, true);
                        return Json("SETTINGNOTFOUND");
                    }
                    if (!projectFile)
                    {
                        Directory.Delete(extractPath, true);
                        return Json("PROJECTFILENOTFOUND");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                Directory.Delete(extractPath, true);
                return Json(ex.Message);
            }
            return Json("0");
        }


        /// <summary>
        /// Formatting the request for OAuth
        /// </summary>
        /// <param name="appSecret"></param>
        /// <param name="authCode"></param>
        /// <param name="callbackUrl"></param>
        /// <returns></returns>
        public string GenerateRequestPostData(string appSecret, string authCode, string callbackUrl)
        {
            try
            {
                return String.Format("client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={0}&grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer&assertion={1}&redirect_uri={2}",
                            HttpUtility.UrlEncode(appSecret),
                            HttpUtility.UrlEncode(authCode),
                            callbackUrl
                     );
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                ViewBag.ErrorMessage = ex.Message;
            }
            return string.Empty;
        }

        /// <summary>
        /// Generate Access Token
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public AccessDetails GetAccessToken(string body)
        {
            try
            {
                string baseAddress = System.Configuration.ConfigurationManager.AppSettings["BaseAddress"];
                var client = new HttpClient
                {
                    BaseAddress = new Uri(baseAddress)
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "/oauth2/token");

                var requestContent = body;
                request.Content = new StringContent(requestContent, Encoding.UTF8, "application/x-www-form-urlencoded");

                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    AccessDetails details = Newtonsoft.Json.JsonConvert.DeserializeObject<AccessDetails>(result);
                    return details;
                }
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                ViewBag.ErrorMessage = ex.Message;
            }
            return new AccessDetails();
        }

        /// <summary>
        /// Get Profile details
        /// </summary>
        /// <param name="accessDetails"></param>
        /// <returns></returns>
        public ProfileDetails GetProfile(AccessDetails accessDetails)
        {
            ProfileDetails profile = new ProfileDetails();
            using (var client = new HttpClient())
            {
                try
                {
                    string baseAddress = System.Configuration.ConfigurationManager.AppSettings["BaseAddress"];

                    client.BaseAddress = new Uri(baseAddress);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessDetails.access_token);
                    HttpResponseMessage response = client.GetAsync("_apis/profile/profiles/me?api-version=4.1").Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        profile = JsonConvert.DeserializeObject<ProfileDetails>(result);
                        return profile;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t Get Profile :" + errorMessage + "\n");
                    }
                }
                catch (Exception ex)
                {
                    logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                }
                return profile;
            }
        }


        /// <summary>
        /// Refresh access token
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public AccessDetails Refresh_AccessToken(string refreshToken)
        {
            using (var client = new HttpClient())
            {
                string redirectUri = System.Configuration.ConfigurationManager.AppSettings["RedirectUri"];
                string cientSecret = System.Configuration.ConfigurationManager.AppSettings["ClientSecret"];
                string baseAddress = System.Configuration.ConfigurationManager.AppSettings["BaseAddress"];

                var request = new HttpRequestMessage(HttpMethod.Post, baseAddress + "/oauth2/token");
                var requestContent = string.Format(
                    "client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={0}&grant_type=refresh_token&assertion={1}&redirect_uri={2}",
                    HttpUtility.UrlEncode(cientSecret),
                    HttpUtility.UrlEncode(refreshToken), redirectUri
                    );

                request.Content = new StringContent(requestContent, Encoding.UTF8, "application/x-www-form-urlencoded");
                try
                {
                    var response = client.SendAsync(request).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        AccessDetails accesDetails = JsonConvert.DeserializeObject<AccessDetails>(result);
                        return accesDetails;
                    }
                    else
                    {
                        return new AccessDetails();
                    }
                }
                catch (Exception ex)
                {
                    logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    return new AccessDetails();
                }
            }
        }

        /// <summary>
        /// Get list of accounts
        /// </summary>
        /// <param name="memberID"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        public AccountsResponse.AccountList GetAccounts(string memberID, AccessDetails details)
        {
            AccountsResponse.AccountList accounts = new AccountsResponse.AccountList();
            var client = new HttpClient();
            string baseAddress = System.Configuration.ConfigurationManager.AppSettings["BaseAddress"];

            string requestContent = baseAddress + "/_apis/Accounts?memberId=" + memberID + "&api-version=4.1";
            var request = new HttpRequestMessage(HttpMethod.Get, requestContent);
            request.Headers.Add("Authorization", "Bearer " + details.access_token);
            try
            {
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    accounts = JsonConvert.DeserializeObject<Models.AccountsResponse.AccountList>(result);
                    return accounts;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t Get Accounts :" + errorMessage + "\t" + "\n");
                }
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return accounts;
        }

        /// <summary>
        /// Get members of the account- Not using now
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public JsonResult GetMembers(string accountName, string accessToken)
        {
            Project mod = new Project();
            try
            {
                AccountMembers.Account accountMembers = new AccountMembers.Account();
                VstsRestAPI.Configuration _defaultConfiguration = new VstsRestAPI.Configuration() { UriString = "https://" + accountName + ".visualstudio.com/DefaultCollection/", VersionNumber = "2.2", PersonalAccessToken = accessToken };
                VstsRestAPI.ProjectsAndTeams.Accounts objAccount = new VstsRestAPI.ProjectsAndTeams.Accounts(_defaultConfiguration);
                accountMembers = objAccount.GetAccountMembers(accountName, accessToken);
                if (accountMembers.count > 0)
                {
                    foreach (var user in accountMembers.value)
                    {
                        mod.accountUsersForDdl.Add(new SelectListItem
                        {
                            Text = user.member.displayName,
                            Value = user.member.mailAddress
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                return null;
            }
            return Json(mod, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Start the process
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public bool StartEnvironmentSetupProcess(Project model)
        {
            try
            {
                Session["PAT"] = model.accessToken;
                Session["AccountName"] = model.accountName;
                AddMessage(model.id, string.Empty);
                AddMessage(model.id.ErrorId(), string.Empty);

                ProcessEnvironment processTask = new ProcessEnvironment(CreateProjectEnvironment);
                processTask.BeginInvoke(model, model.accessToken, model.accountName, new AsyncCallback(EndEnvironmentSetupProcess), processTask);
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return true;
        }

        /// <summary>
        /// End the process
        /// </summary>
        /// <param name="result"></param>
        public void EndEnvironmentSetupProcess(IAsyncResult result)
        {
            try
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
                        string fileName = string.Format("{0}_{1}.txt", templateUsed, DateTime.Now.ToString("ddMMMyyyy_HHmmss"));

                        if (!Directory.Exists(logPath))
                        {
                            Directory.CreateDirectory(logPath);
                        }

                        System.IO.File.AppendAllText(Path.Combine(logPath, fileName), errorMessages);

                        //Create ISSUE work item with error details in VSTSProjectgenarator account
                        string patBase64 = System.Configuration.ConfigurationManager.AppSettings["PATBase64"];
                        string url = System.Configuration.ConfigurationManager.AppSettings["URL"];
                        string projectId = System.Configuration.ConfigurationManager.AppSettings["PROJECTID"];
                        string issueName = string.Format("{0}_{1}", templateUsed, DateTime.Now.ToString("ddMMMyyyy_HHmmss"));
                        IssueWI objIssue = new IssueWI();

                        errorMessages = errorMessages + "\t" + "TemplateUsed: " + templateUsed;
                        errorMessages = errorMessages + "\t" + "ProjectCreated : " + projectName;

                        logger.Error(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t  Error: " + errorMessages);

                        string logWIT = System.Configuration.ConfigurationManager.AppSettings["LogWIT"];
                        if (logWIT == "true")
                        {
                            objIssue.CreateIssueWI(patBase64, "4.1", url, issueName, errorMessages, projectId, "Demo Generator");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
        }

        /// <summary>
        /// start provisioning project - calls required
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pat"></param>
        /// <param name="accountName"></param>
        /// <returns></returns>
        public string[] CreateProjectEnvironment(Project model, string pat, string accountName)
        {
            logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "Project Name: " + model.ProjectName + "\t Template Selected: " + model.SelectedTemplate + "\t Organization Selected: " + accountName);
            pat = model.accessToken;
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


            string processTemplateId = Default.SCRUM;
            model.Environment = new EnvironmentValues
            {
                serviceEndpoints = new Dictionary<string, string>(),
                repositoryIdList = new Dictionary<string, string>(),
                pullRequests = new Dictionary<string, string>()
            };
            ProjectTemplate template = null;
            ProjectSettings settings = null;
            List<WIMapData> wiMapping = new List<WIMapData>();
            AccountMembers.Account accountMembers = new AccountMembers.Account();
            model.accountUsersForWi = new List<string>();
            websiteUrl = model.websiteUrl;
            templateUsed = model.SelectedTemplate;
            projectName = model.ProjectName;

            string logWIT = System.Configuration.ConfigurationManager.AppSettings["LogWIT"];
            if (logWIT == "true")
            {
                string patBase64 = System.Configuration.ConfigurationManager.AppSettings["PATBase64"];
                string url = System.Configuration.ConfigurationManager.AppSettings["URL"];
                string projectId = System.Configuration.ConfigurationManager.AppSettings["PROJECTID"];
                string reportName = string.Format("{0}", "AzureDevOps_Analytics-DemoGenerator");
                IssueWI objIssue = new IssueWI();
                objIssue.CreateReportWI(patBase64, "1.0", url, websiteUrl, reportName, "", templateUsed, projectId, model.Region);
            }
            //configuration setup
            string _credentials = model.accessToken;
            Configuration _projectCreationVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = projectCreationVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _releaseVersion = new Configuration() { UriString = releaseHost + accountName + "/", VersionNumber = releaseVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _buildVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = buildVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _workItemsVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = workItemsVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _queriesVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = queriesVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _boardVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = boardVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _wikiVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = wikiVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _endPointVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = endPointVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _extensionVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = extensionVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _dashboardVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = dashboardVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _repoVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = repoVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };

            Configuration _getSourceCodeVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = getSourceCodeVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _agentQueueVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = agentQueueVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _testPlanVersion = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = testPlanVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            Configuration _deploymentGroup = new Configuration() { UriString = defaultHost + accountName + "/", VersionNumber = deploymentGroup, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };


            string templatesFolder = Server.MapPath("~") + @"\Templates\";
            string projTemplateFile = string.Format(templatesFolder + @"{0}\ProjectTemplate.json", model.SelectedTemplate);
            string projectSettingsFile = string.Empty;

            //initialize project template and settings
            try
            {
                if (System.IO.File.Exists(projTemplateFile))
                {
                    string templateItems = model.ReadJsonFile(projTemplateFile);
                    template = JsonConvert.DeserializeObject<ProjectTemplate>(templateItems);

                    projectSettingsFile = System.IO.Path.Combine(templatesFolder + model.SelectedTemplate, template.ProjectSettings);
                    if (System.IO.File.Exists(projectSettingsFile))
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
                        }
                    }
                }
                else
                {
                    AddMessage(model.id.ErrorId(), "Project Template not found");
                    StatusMessages[model.id] = "100";
                    return new string[] { model.id, accountName };
                }
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            //create team project
            string jsonProject = model.ReadJsonFile(templatesFolder + "CreateProject.json");
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
                return new string[] { model.id, accountName };
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
                    return new string[] { model.id, accountName };
                }
            }
            watch.Stop();

            //get project id after successfull in VSTS
            model.Environment.ProjectId = objProject.GetProjectIdByName(model.ProjectName);
            model.Environment.ProjectName = model.ProjectName;

            //Install required extensions
            if (model.isExtensionNeeded && model.isAgreeTerms)
            {
                bool isInstalled = InstallExtensions(model, model.accountName, model.accessToken);
                if (isInstalled) { AddMessage(model.id, "Required extensions are installed"); }
            }

            //current user Details
            string teamName = model.ProjectName + " team";
            TeamMemberResponse.TeamMembers teamMembers = GetTeamMembers(model.ProjectName, teamName, _projectCreationVersion, model.id);

            var teamMember = teamMembers.value.FirstOrDefault();
            if (teamMember != null)
            {
                model.Environment.UserUniquename = teamMember.identity.uniqueName;
            }
            if (teamMember != null)
            {
                model.Environment.UserUniqueId = teamMember.identity.id;
            }

            //update board columns and rows
            // Checking for template version
            string projectTemplate = System.IO.File.ReadAllText(System.IO.Path.Combine(templatesFolder + model.SelectedTemplate, "ProjectTemplate.json"));
            if (!string.IsNullOrEmpty(projectTemplate))
            {
                JObject jObject = JsonConvert.DeserializeObject<JObject>(projectTemplate);
                templateVersion = jObject["TemplateVersion"] == null ? string.Empty : jObject["TemplateVersion"].ToString();
            }
            if (templateVersion != "2.0")
            {
                //create teams
                CreateTeams(templatesFolder, model, template.Teams, _projectCreationVersion, model.id, template.TeamArea);

                // for older templates
                string projectSetting = System.IO.File.ReadAllText(System.IO.Path.Combine(templatesFolder + model.SelectedTemplate, "ProjectSettings.json"));
                JObject projectObj = JsonConvert.DeserializeObject<JObject>(projectSetting);
                string processType = projectObj["type"] == null ? string.Empty : projectObj["type"].ToString();
                string boardType = string.Empty;
                if (processType == "" || processType == "Scrum")
                {
                    processType = "Scrum";
                    boardType = "Backlog%20items";
                }
                else
                {
                    boardType = "Stories";
                }
                BoardColumn objBoard = new BoardColumn(_boardVersion);
                string updateSwimLanesJSON = "";
                if (template.BoardRows != null)
                {
                    updateSwimLanesJSON = System.IO.Path.Combine(templatesFolder + model.SelectedTemplate, template.BoardRows);
                    SwimLanes objSwimLanes = new SwimLanes(_boardVersion);
                    if (System.IO.File.Exists(updateSwimLanesJSON))
                    {
                        updateSwimLanesJSON = System.IO.File.ReadAllText(updateSwimLanesJSON);
                        bool isUpdated = objSwimLanes.UpdateSwimLanes(updateSwimLanesJSON, model.ProjectName, boardType, model.ProjectName + " Team");
                    }
                }
                if (template.SetEpic != null)
                {
                    string team = model.ProjectName + " Team";
                    string json = string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, template.SetEpic);
                    if (System.IO.File.Exists(json))
                    {
                        json = model.ReadJsonFile(json);
                        EnableEpic(templatesFolder, model, json, _boardVersion, model.id, team);
                    }
                }

                if (template.BoardColumns != null)
                {
                    string team = model.ProjectName + " Team";
                    string json = string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, template.BoardColumns);
                    if (System.IO.File.Exists(json))
                    {
                        json = model.ReadJsonFile(json);
                        bool success = UpdateBoardColumn(templatesFolder, model, json, _boardVersion, model.id, boardType, team);
                        if (success)
                        {
                            //update Card Fields
                            if (template.CardField != null)
                            {
                                string cardFieldJson = string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, template.CardField);
                                if (System.IO.File.Exists(cardFieldJson))
                                {
                                    cardFieldJson = model.ReadJsonFile(cardFieldJson);
                                    UpdateCardFields(templatesFolder, model, cardFieldJson, _boardVersion, model.id, boardType, model.ProjectName + " Team");
                                }
                            }
                            //Update card styles
                            if (template.CardStyle != null)
                            {
                                string cardStyleJson = string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, template.CardStyle);
                                if (System.IO.File.Exists(cardStyleJson))
                                {
                                    cardStyleJson = model.ReadJsonFile(cardStyleJson);
                                    UpdateCardStyles(templatesFolder, model, cardStyleJson, _boardVersion, model.id, boardType, model.ProjectName + " Team");
                                }
                            }
                            //Enable Epic Backlog
                            AddMessage(model.id, "Board-Column, Swimlanes, Styles updated");
                        }
                    }
                }

                //update sprint dates
                UpdateSprintItems(model, _boardVersion, settings);
                UpdateIterations(model, _boardVersion, templatesFolder, "Iterations.json");
                RenameIterations(model, _boardVersion, settings.renameIterations);
            }
            else
            {
                // for newer version of templates
                string teamsJsonPath = System.IO.Path.Combine(templatesFolder + model.SelectedTemplate, "Teams\\Teams.json");
                if (System.IO.File.Exists(teamsJsonPath))
                {
                    template.Teams = "Teams\\Teams.json";
                    template.TeamArea = "TeamArea.json";
                    CreateTeams(templatesFolder, model, template.Teams, _projectCreationVersion, model.id, template.TeamArea);
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
                        string teamFolderPath = System.IO.Path.Combine(templatesFolder + model.SelectedTemplate, "Teams", jteam["name"].ToString());
                        if (System.IO.Directory.Exists(teamFolderPath))
                        {
                            BoardColumn objBoard = new BoardColumn(_boardVersion);

                            // updating swimlanes for each teams each board(epic, feature, PBI, Stories) 
                            string updateSwimLanesJSON = "";
                            SwimLanes objSwimLanes = new SwimLanes(_boardVersion);
                            template.BoardRows = "BoardRows.json";
                            updateSwimLanesJSON = System.IO.Path.Combine(teamFolderPath, template.BoardRows);
                            if (System.IO.File.Exists(updateSwimLanesJSON))
                            {
                                updateSwimLanesJSON = System.IO.File.ReadAllText(updateSwimLanesJSON);
                                List<ImportBoardRows.Rows> importRows = JsonConvert.DeserializeObject<List<ImportBoardRows.Rows>>(updateSwimLanesJSON);
                                foreach (var board in importRows)
                                {
                                    bool isUpdated = objSwimLanes.UpdateSwimLanes(JsonConvert.SerializeObject(board.value), model.ProjectName, board.BoardName, _teamName);
                                }
                            }

                            // updating team setting for each team
                            string teamSettingJson = "";
                            template.SetEpic = "TeamSetting.json";
                            teamSettingJson = System.IO.Path.Combine(teamFolderPath, template.SetEpic);
                            if (System.IO.File.Exists(teamSettingJson))
                            {
                                teamSettingJson = System.IO.File.ReadAllText(teamSettingJson);
                                EnableEpic(templatesFolder, model, teamSettingJson, _boardVersion, model.id, _teamName);
                            }

                            // updating board columns for each teams each board
                            string teamBoardColumns = "";
                            template.BoardColumns = "BoardColumns.json";
                            teamBoardColumns = System.IO.Path.Combine(teamFolderPath, template.BoardColumns);
                            if (System.IO.File.Exists(teamBoardColumns))
                            {
                                teamBoardColumns = System.IO.File.ReadAllText(teamBoardColumns);
                                List<ImportBoardColumns.ImportBoardCols> importBoardCols = JsonConvert.DeserializeObject<List<ImportBoardColumns.ImportBoardCols>>(teamBoardColumns);
                                foreach (var board in importBoardCols)
                                {
                                    bool success = UpdateBoardColumn(templatesFolder, model, JsonConvert.SerializeObject(board.value, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), _boardVersion, model.id, board.BoardName, _teamName);
                                }
                            }

                            // updating card fields for each team and each board
                            string teamCardFields = "";
                            template.CardField = "CardFields.json";
                            teamCardFields = System.IO.Path.Combine(teamFolderPath, template.CardField);
                            if (System.IO.File.Exists(teamCardFields))
                            {
                                teamCardFields = System.IO.File.ReadAllText(teamCardFields);
                                List<ImportCardFields.CardFields> cardFields = new List<ImportCardFields.CardFields>();
                                cardFields = JsonConvert.DeserializeObject<List<ImportCardFields.CardFields>>(teamCardFields);
                                foreach (var card in cardFields)
                                {
                                    UpdateCardFields(templatesFolder, model, JsonConvert.SerializeObject(card, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), _boardVersion, model.id, card.BoardName, _teamName);
                                }
                            }

                            // updating card styles for each team and each board
                            string teamCardStyle = "";
                            template.CardStyle = "CardStyles.json";
                            teamCardStyle = System.IO.Path.Combine(teamFolderPath, template.CardStyle);
                            if (System.IO.File.Exists(teamCardStyle))
                            {
                                teamCardStyle = System.IO.File.ReadAllText(teamCardStyle);
                                List<CardStyle.Style> cardStyles = new List<CardStyle.Style>();
                                cardStyles = JsonConvert.DeserializeObject<List<CardStyle.Style>>(teamCardStyle);
                                foreach (var cardStyle in cardStyles)
                                {
                                    if (cardStyle.rules.fill != null)
                                    {
                                        UpdateCardStyles(templatesFolder, model, JsonConvert.SerializeObject(cardStyle), _boardVersion, model.id, cardStyle.BoardName, _teamName);
                                    }
                                }
                            }
                        }
                        AddMessage(model.id, "Board-Column, Swimlanes, Styles updated");
                    }
                    UpdateSprintItems(model, _boardVersion, settings);
                    UpdateIterations(model, _boardVersion, templatesFolder, "Iterations.json");
                    RenameIterations(model, _boardVersion, settings.renameIterations);
                }
            }
            //Create Deployment Group
            //CreateDeploymentGroup(templatesFolder, model, _deploymentGroup);

            //create service endpoint
            List<string> listEndPointsJsonPath = new List<string>();
            string serviceEndPointsPath = templatesFolder + model.SelectedTemplate + @"\ServiceEndpoints";
            if (System.IO.Directory.Exists(serviceEndPointsPath))
            {
                System.IO.Directory.GetFiles(serviceEndPointsPath).ToList().ForEach(i => listEndPointsJsonPath.Add(i));
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
            string importSourceCodePath = templatesFolder + model.SelectedTemplate + @"\ImportSourceCode";
            if (System.IO.Directory.Exists(importSourceCodePath))
            {
                System.IO.Directory.GetFiles(importSourceCodePath).ToList().ForEach(i => listImportSourceCodeJsonPaths.Add(i));
            }
            foreach (string importSourceCode in listImportSourceCodeJsonPaths)
            {
                ImportSourceCode(templatesFolder, model, importSourceCode, _repoVersion, model.id, _getSourceCodeVersion);
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
            CreateProjetWiki(templatesFolder, model, _wikiVersion);
            CreateCodeWiki(templatesFolder, model, _wikiVersion);

            List<string> listPullRequestJsonPaths = new List<string>();
            string pullRequestFolder = templatesFolder + model.SelectedTemplate + @"\PullRequests";
            if (System.IO.Directory.Exists(pullRequestFolder))
            {
                System.IO.Directory.GetFiles(pullRequestFolder).ToList().ForEach(i => listPullRequestJsonPaths.Add(i));
            }
            foreach (string pullReq in listPullRequestJsonPaths)
            {
                CreatePullRequest(templatesFolder, model, pullReq, _workItemsVersion);
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
                string featuresFilePath = System.IO.Path.Combine(templatesFolder + model.SelectedTemplate, template.FeaturefromTemplate == null ? string.Empty : template.FeaturefromTemplate);
                string productBackLogPath = System.IO.Path.Combine(templatesFolder + model.SelectedTemplate, template.PBIfromTemplate == null ? string.Empty : template.PBIfromTemplate);
                string taskPath = System.IO.Path.Combine(templatesFolder + model.SelectedTemplate, template.TaskfromTemplate == null ? string.Empty : template.TaskfromTemplate);
                string testCasePath = System.IO.Path.Combine(templatesFolder + model.SelectedTemplate, template.TestCasefromTemplate == null ? string.Empty : template.TestCasefromTemplate);
                string bugPath = System.IO.Path.Combine(templatesFolder + model.SelectedTemplate, template.BugfromTemplate == null ? string.Empty : template.BugfromTemplate);
                string epicPath = System.IO.Path.Combine(templatesFolder + model.SelectedTemplate, template.EpicfromTemplate == null ? string.Empty : template.EpicfromTemplate);
                string userStoriesPath = System.IO.Path.Combine(templatesFolder + model.SelectedTemplate, template.UserStoriesFromTemplate == null ? string.Empty : template.UserStoriesFromTemplate);
                string testPlansPath = string.Empty;
                string testSuitesPath = string.Empty;
                if (model.SelectedTemplate.ToLower() == "myshuttle2")
                {
                    testPlansPath = System.IO.Path.Combine(templatesFolder + model.SelectedTemplate, template.TestPlanfromTemplate);
                    testSuitesPath = System.IO.Path.Combine(templatesFolder + model.SelectedTemplate, template.TestSuitefromTemplate);
                }

                if (model.SelectedTemplate.ToLower() == "myshuttle")
                {
                    testPlansPath = System.IO.Path.Combine(templatesFolder + model.SelectedTemplate, template.TestPlanfromTemplate);
                    testSuitesPath = System.IO.Path.Combine(templatesFolder + model.SelectedTemplate, template.TestSuitefromTemplate);
                }

                if (System.IO.File.Exists(featuresFilePath))
                {
                    workItems.Add("Feature", model.ReadJsonFile(featuresFilePath));
                }

                if (System.IO.File.Exists(productBackLogPath))
                {
                    workItems.Add("Product Backlog Item", model.ReadJsonFile(productBackLogPath));
                }

                if (System.IO.File.Exists(taskPath))
                {
                    workItems.Add("Task", model.ReadJsonFile(taskPath));
                }

                if (System.IO.File.Exists(testCasePath))
                {
                    workItems.Add("Test Case", model.ReadJsonFile(testCasePath));
                }

                if (System.IO.File.Exists(bugPath))
                {
                    workItems.Add("Bug", model.ReadJsonFile(bugPath));
                }

                if (System.IO.File.Exists(userStoriesPath))
                {
                    workItems.Add("User Story", model.ReadJsonFile(userStoriesPath));
                }

                if (System.IO.File.Exists(epicPath))
                {
                    workItems.Add("Epic", model.ReadJsonFile(epicPath));
                }

                if (System.IO.File.Exists(testPlansPath))
                {
                    workItems.Add("Test Plan", model.ReadJsonFile(testPlansPath));
                }

                if (System.IO.File.Exists(testSuitesPath))
                {
                    workItems.Add("Test Suite", model.ReadJsonFile(testSuitesPath));
                }
            }
            //// Modified Work Item import logic
            else
            {
                string _WitPath = Path.Combine(templatesFolder + model.SelectedTemplate + "\\WorkItems");
                if (System.IO.Directory.Exists(_WitPath))
                {
                    string[] workItemFilePaths = System.IO.Directory.GetFiles(_WitPath);
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
                                    workItems.Add(name, model.ReadJsonFile(workItem));
                                }
                            }
                        }
                    }
                }
            }

            ImportWorkItems import = new ImportWorkItems(_workItemsVersion, model.Environment.BoardRowFieldName);
            if (System.IO.File.Exists(projectSettingsFile))
            {
                string attchmentFilesFolder = string.Format(templatesFolder + @"{0}\WorkItemAttachments", model.SelectedTemplate);
                if (listPullRequestJsonPaths.Count > 0)
                {
                    if (model.SelectedTemplate == "MyHealthClinic")
                    {
                        wiMapping = import.ImportWorkitems(workItems, model.ProjectName, model.Environment.UserUniquename, model.ReadJsonFile(projectSettingsFile), attchmentFilesFolder, model.Environment.repositoryIdList.ContainsKey("MyHealthClinic") ? model.Environment.repositoryIdList["MyHealthClinic"] : string.Empty, model.Environment.ProjectId, model.Environment.pullRequests, model.UserMethod, model.accountUsersForWi, model.SelectedTemplate);
                    }
                    else if (model.SelectedTemplate == "SmartHotel360")
                    {
                        wiMapping = import.ImportWorkitems(workItems, model.ProjectName, model.Environment.UserUniquename, model.ReadJsonFile(projectSettingsFile), attchmentFilesFolder, model.Environment.repositoryIdList.ContainsKey("PublicWeb") ? model.Environment.repositoryIdList["PublicWeb"] : string.Empty, model.Environment.ProjectId, model.Environment.pullRequests, model.UserMethod, model.accountUsersForWi, model.SelectedTemplate);
                    }
                    else
                    {
                        wiMapping = import.ImportWorkitems(workItems, model.ProjectName, model.Environment.UserUniquename, model.ReadJsonFile(projectSettingsFile), attchmentFilesFolder, model.Environment.repositoryIdList.ContainsKey(model.SelectedTemplate) ? model.Environment.repositoryIdList[model.SelectedTemplate] : string.Empty, model.Environment.ProjectId, model.Environment.pullRequests, model.UserMethod, model.accountUsersForWi, model.SelectedTemplate);
                    }
                }
                else
                {
                    wiMapping = import.ImportWorkitems(workItems, model.ProjectName, model.Environment.UserUniquename, model.ReadJsonFile(projectSettingsFile), attchmentFilesFolder, string.Empty, model.Environment.ProjectId, model.Environment.pullRequests, model.UserMethod, model.accountUsersForWi, model.SelectedTemplate);
                }
                AddMessage(model.id, "Work Items created");
            }
            //Creat TestPlans and TestSuites
            List<string> listTestPlansJsonPaths = new List<string>();
            string testPlansFolder = templatesFolder + model.SelectedTemplate + @"\TestPlans";
            if (Directory.Exists(testPlansFolder))
            {
                Directory.GetFiles(testPlansFolder).ToList().ForEach(i => listTestPlansJsonPaths.Add(i));
            }
            foreach (string testPlan in listTestPlansJsonPaths)
            {
                CreateTestManagement(wiMapping, model, testPlan, templatesFolder, _testPlanVersion);
            }
            if (listTestPlansJsonPaths.Count > 0)
            {
                //AddMessage(model.id, "TestPlans, TestSuites and TestCases created");
            }

            //create build Definition
            string buildDefinitionsPath = templatesFolder + model.SelectedTemplate + @"\BuildDefinitions";
            model.BuildDefinitions = new List<BuildDef>();
            if (Directory.Exists(buildDefinitionsPath))
            {
                Directory.GetFiles(buildDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.BuildDefinitions.Add(new Models.BuildDef() { FilePath = i }));
            }
            bool isBuild = CreateBuildDefinition(templatesFolder, model, _buildVersion, model.id);
            if (isBuild)
            {
                AddMessage(model.id, "Build definition created");
            }

            //Queue a Build
            string buildJson = string.Format(templatesFolder + @"{0}\QueueBuild.json", model.SelectedTemplate);
            if (System.IO.File.Exists(buildJson))
            {
                QueueABuild(model, buildJson, _buildVersion);
            }

            //create release Definition
            string releaseDefinitionsPath = templatesFolder + model.SelectedTemplate + @"\ReleaseDefinitions";
            model.ReleaseDefinitions = new List<ReleaseDef>();
            if (Directory.Exists(releaseDefinitionsPath))
            {
                Directory.GetFiles(releaseDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.ReleaseDefinitions.Add(new Models.ReleaseDef() { FilePath = i }));
            }
            bool isReleased = CreateReleaseDefinition(templatesFolder, model, _releaseVersion, model.id, teamMembers);
            if (isReleased)
            {
                AddMessage(model.id, "Release definition created");
            }

            //Create query and widgets
            List<string> listDashboardQueriesPath = new List<string>();
            string dashboardQueriesPath = templatesFolder + model.SelectedTemplate + @"\Dashboard\Queries";
            string dashboardPath = templatesFolder + model.SelectedTemplate + @"\Dashboard";

            if (Directory.Exists(dashboardQueriesPath))
            {
                Directory.GetFiles(dashboardQueriesPath).ToList().ForEach(i => listDashboardQueriesPath.Add(i));
            }
            if (Directory.Exists(dashboardPath))
            {
                CreateQueryAndWidgets(templatesFolder, model, listDashboardQueriesPath, _queriesVersion, _dashboardVersion, _releaseVersion, _projectCreationVersion, _boardVersion);
                AddMessage(model.id, "Queries, Widgets and Charts created");
            }
            string _checkIsPrivate = System.IO.File.ReadAllText(Server.MapPath("~") + @"Templates\" + model.SelectedTemplate + "\\ProjectTemplate.json");
            if (_checkIsPrivate != "")
            {
                ProjectSetting setting = new ProjectSetting();
                setting = JsonConvert.DeserializeObject<ProjectSetting>(_checkIsPrivate);
                if (setting.IsPrivate == "true")
                {
                    Directory.Delete(Path.Combine(templatesFolder, model.SelectedTemplate), true);
                }
            }
            StatusMessages[model.id] = "100";
            return new string[] { model.id, accountName };
        }
        #endregion

        #region Project Setup Operations
        /// <summary>
        /// Create Teams
        /// </summary>
        /// <param name="templatesFolder"></param>
        /// <param name="model"></param>
        /// <param name="teamsJSON"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="id"></param>
        /// <param name="teamAreaJSON"></param>
        private void CreateTeams(string templatesFolder, Project model, string teamsJSON, VstsRestAPI.Configuration _projectConfig, string id, string teamAreaJSON)
        {
            try
            {
                string jsonTeams = string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, teamsJSON);
                if (System.IO.File.Exists(jsonTeams))
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
                        string isDefault = jTeam["isDefault"] != null ? jTeam["isDefault"].ToString() : string.Empty;
                        if (isDefault == "false" || isDefault == "")
                        {
                            GetTeamResponse.Team teamResponse = objTeam.CreateNewTeam(jTeam.ToString(), model.ProjectName);
                            if (!(string.IsNullOrEmpty(teamResponse.id)))
                            {
                                string areaName = objTeam.CreateArea(model.ProjectName, teamResponse.name);
                                string updateAreaJSON = string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, teamAreaJSON);
                                if (System.IO.File.Exists(updateAreaJSON))
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
                                        bool isIterationUpdated = objTeam.SetIterationsForTeam(iteration.id, teamResponse.name, model.ProjectName);
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
                            string updateAreaJSON = string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, "UpdateTeamArea.json");
                            if (System.IO.File.Exists(updateAreaJSON))
                            {
                                updateAreaJSON = model.ReadJsonFile(updateAreaJSON);
                                updateAreaJSON = updateAreaJSON.Replace("$ProjectName$", model.ProjectName);
                                bool isUpdated = objTeam.UpdateTeamsAreas(model.ProjectName, updateAreaJSON);
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
        /// <param name="templatesFolder"></param>
        /// <param name="model"></param>
        /// <param name="workItemJSON"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="id"></param>
        private void CreateWorkItems(string templatesFolder, Project model, string workItemJSON, VstsRestAPI.Configuration _defaultConfiguration, string id)
        {
            try
            {
                string jsonWorkItems = string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, workItemJSON);
                if (System.IO.File.Exists(jsonWorkItems))
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
        /// <param name="templatesFolder"></param>
        /// <param name="model"></param>
        /// <param name="BoardColumnsJSON"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool UpdateBoardColumn(string templatesFolder, Project model, string BoardColumnsJSON, VstsRestAPI.Configuration _BoardConfig, string id, string BoardType, string team)
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
        /// <param name="templatesFolder"></param>
        /// <param name="model"></param>
        /// <param name="json"></param>
        /// <param name="_configuration"></param>
        /// <param name="id"></param>
        private void UpdateCardFields(string templatesFolder, Project model, string json, Configuration _configuration, string id, string boardType, string team)
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
        /// <param name="templatesFolder"></param>
        /// <param name="model"></param>
        /// <param name="json"></param>
        /// <param name="_configuration"></param>
        /// <param name="id"></param>
        private void UpdateCardStyles(string templatesFolder, Project model, string json, Configuration _configuration, string id, string boardType, string team)
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
        /// <param name="templatesFolder"></param>
        /// <param name="model"></param>
        /// <param name="json"></param>
        /// <param name="_config3_0"></param>
        /// <param name="id"></param>
        private void EnableEpic(string templatesFolder, Project model, string json, VstsRestAPI.Configuration _boardVersion, string id, string team)
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
        /// <param name="templatesFolder"></param>
        /// <param name="model"></param>
        /// <param name="workItemUpdateJSON"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="id"></param>
        /// <param name="currentUser"></param>
        /// <param name="projectSettingsJSON"></param>
        private void UpdateWorkItems(string templatesFolder, Project model, string workItemUpdateJSON, VstsRestAPI.Configuration _defaultConfiguration, string id, string currentUser, string projectSettingsJSON)
        {
            try
            {
                string jsonWorkItemsUpdate = string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, workItemUpdateJSON);
                string jsonProjectSettings = string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, projectSettingsJSON);
                if (System.IO.File.Exists(jsonWorkItemsUpdate))
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
        /// <param name="templatesFolder"></param>
        /// <param name="iterationsJSON"></param>
        private void UpdateIterations(Project model, VstsRestAPI.Configuration _boardConfig, string templatesFolder, string iterationsJSON)
        {
            try
            {
                string jsonIterations = string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, iterationsJSON);
                if (System.IO.File.Exists(jsonIterations))
                {
                    iterationsJSON = model.ReadJsonFile(jsonIterations);
                    ClassificationNodes objClassification = new ClassificationNodes(_boardConfig);

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
        private void CreateIterationNode(Project model, ClassificationNodes objClassification, GetNodesResponse.Child child, GetNodesResponse.Nodes currentIterations)
        {
            string[] defaultSprints = new string[] { "Sprint 1", "Sprint 2", "Sprint 3", "Sprint 4", "Sprint 5", "Sprint 6", };
            if (defaultSprints.Contains(child.name))
            {
                var nd = (currentIterations.hasChildren) ? currentIterations.children.FirstOrDefault(i => i.name == child.name) : null;
                if (nd != null)
                {
                    child.id = nd.id;
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
        private void MoveIterationNode(Project model, ClassificationNodes objClassification, GetNodesResponse.Child child)
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
                ClassificationNodes objClassification = new ClassificationNodes(_boardConfig);
                bool classificationNodesResult = objClassification.UpdateIterationDates(model.ProjectName, settings.type);

                if (!(string.IsNullOrEmpty(objClassification.LastFailureMessage)))
                {
                    AddMessage(model.id.ErrorId(), "Error while updating sprint items: " + objClassification.LastFailureMessage + Environment.NewLine);
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
                    ClassificationNodes objClassification = new ClassificationNodes(_defaultConfiguration);
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
        /// <param name="templatesFolder"></param>
        /// <param name="model"></param>
        /// <param name="sourceCodeJSON"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="importSourceConfiguration"></param>
        /// <param name="id"></param>
        private void ImportSourceCode(string templatesFolder, Project model, string sourceCodeJSON, VstsRestAPI.Configuration _repo, string id, VstsRestAPI.Configuration _retSourceCodeVersion)
        {

            try
            {
                string[] repositoryDetail = new string[2];
                if (System.IO.File.Exists(sourceCodeJSON))
                {
                    Repository objRepository = new Repository(_repo);
                    string repositoryName = Path.GetFileName(sourceCodeJSON).Replace(".json", "");
                    if (model.ProjectName.ToLower() == repositoryName.ToLower())
                    {
                        repositoryDetail = objRepository.GetDefaultRepository(model.ProjectName);
                        isDefaultRepoTodetele = false;
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
        /// <param name="templatesFolder"></param>
        /// <param name="model"></param>
        /// <param name="pullRequestJsonPath"></param>
        /// <param name="_configuration3_0"></param>
        private void CreatePullRequest(string templatesFolder, Project model, string pullRequestJsonPath, VstsRestAPI.Configuration _workItemConfig)
        {
            try
            {
                if (System.IO.File.Exists(pullRequestJsonPath))
                {
                    string commentFile = Path.GetFileName(pullRequestJsonPath);
                    string repositoryId = string.Empty;
                    if (model.SelectedTemplate == "MyHealthClinic") { repositoryId = model.Environment.repositoryIdList["MyHealthClinic"]; }
                    if (model.SelectedTemplate == "SmartHotel360") { repositoryId = model.Environment.repositoryIdList["PublicWeb"]; }
                    else { repositoryId = model.Environment.repositoryIdList[model.SelectedTemplate]; }

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
                            commentFile = string.Format(templatesFolder + @"{0}\PullRequests\Comments\{1}", model.SelectedTemplate, commentFile);
                            if (System.IO.File.Exists(commentFile))
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
        private void CreateServiceEndPoint(Project model, List<string> jsonPaths, VstsRestAPI.Configuration _endpointConfig)
        {
            try
            {
                string serviceEndPointId = string.Empty;
                foreach (string jsonPath in jsonPaths)
                {
                    string jsonCreateService = jsonPath;
                    if (System.IO.File.Exists(jsonCreateService))
                    {
                        string username = System.Configuration.ConfigurationManager.AppSettings["UserID"];
                        string password = System.Configuration.ConfigurationManager.AppSettings["Password"];
                        string extractPath = Server.MapPath("~/Templates/" + model.SelectedTemplate);
                        string projectFileData = System.IO.File.ReadAllText(extractPath + "\\ProjectTemplate.json");
                        ProjectSetting settings = JsonConvert.DeserializeObject<ProjectSetting>(projectFileData);
                        ServiceEndPoint objService = new ServiceEndPoint(_endpointConfig);

                        string gitUserName = System.Configuration.ConfigurationManager.AppSettings["GitUserName"];
                        string gitUserPassword = System.Configuration.ConfigurationManager.AppSettings["GitUserPassword"];


                        if (!string.IsNullOrEmpty(settings.IsPrivate))
                        {
                            jsonCreateService = model.ReadJsonFile(jsonCreateService);
                            jsonCreateService = jsonCreateService.Replace("$ProjectName$", model.ProjectName);
                            jsonCreateService = jsonCreateService.Replace("$username$", model.Email).Replace("$password$", model.accessToken);
                        }
                        else
                        {
                            jsonCreateService = model.ReadJsonFile(jsonCreateService);
                            jsonCreateService = jsonCreateService.Replace("$ProjectName$", model.ProjectName);
                            jsonCreateService = jsonCreateService.Replace("$username$", username).Replace("$password$", password).Replace("$GitUserName$", gitUserName).Replace("$GitUserPassword$", gitUserPassword);
                        }
                        if (model.SelectedTemplate.ToLower() == "bikesharing360")
                        {
                            string bikeSharing360username = System.Configuration.ConfigurationManager.AppSettings["UserID"];
                            string bikeSharing360password = System.Configuration.ConfigurationManager.AppSettings["BikeSharing360Password"];
                            jsonCreateService = jsonCreateService.Replace("$BikeSharing360username$", bikeSharing360username).Replace("$BikeSharing360password$", bikeSharing360password);
                        }
                        else if (model.SelectedTemplate.ToLower() == "contososhuttle")
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
                            var url = model.Parameters.ContainsKey("OctopusURL") ? model.Parameters["OctopusURL"] : string.Empty;
                            var apiKey = model.Parameters.ContainsKey("APIkey") ? model.Parameters["APIkey"] : string.Empty;
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
        /// <param name="templateFolder"></param>
        /// <param name="_defaultConfiguration"></param>
        private void CreateTestManagement(List<WIMapData> wiMapping, Project model, string testPlanJson, string templateFolder, VstsRestAPI.Configuration _testPlanVersion)
        {
            try
            {
                if (System.IO.File.Exists(testPlanJson))
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
                        string testSuiteJson = string.Format(templateFolder + @"{0}\TestPlans\TestSuites\{1}", model.SelectedTemplate, fileName);
                        if (System.IO.File.Exists(testSuiteJson))
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
                                    if (testSuiteResponse.Length > 0)
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
        /// <param name="templatesFolder"></param>
        /// <param name="model"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool CreateBuildDefinition(string templatesFolder, Project model, VstsRestAPI.Configuration _buildConfig, string id)
        {
            bool flag = false;
            try
            {
                foreach (BuildDef buildDef in model.BuildDefinitions)
                {
                    if (System.IO.File.Exists(buildDef.FilePath))
                    {
                        BuildDefinition objBuild = new BuildDefinition(_buildConfig);
                        string jsonBuildDefinition = model.ReadJsonFile(buildDef.FilePath);
                        jsonBuildDefinition = jsonBuildDefinition.Replace("$ProjectName$", model.Environment.ProjectName)
                                             .Replace("$ProjectId$", model.Environment.ProjectId);
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
                if (System.IO.File.Exists(jsonQueueABuild))
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
        /// <param name="templatesFolder"></param>
        /// <param name="model"></param>
        /// <param name="_releaseConfiguration"></param>
        /// <param name="_config3_0"></param>
        /// <param name="id"></param>
        /// <param name="teamMembers"></param>
        /// <returns></returns>
        private bool CreateReleaseDefinition(string templatesFolder, Project model, VstsRestAPI.Configuration _releaseConfiguration, string id, TeamMemberResponse.TeamMembers teamMembers)
        {
            bool flag = false;
            try
            {
                var teamMember = teamMembers.value.FirstOrDefault();
                foreach (ReleaseDef relDef in model.ReleaseDefinitions)
                {
                    if (System.IO.File.Exists(relDef.FilePath))
                    {
                        ReleaseDefinition objRelease = new ReleaseDefinition(_releaseConfiguration);
                        string jsonReleaseDefinition = model.ReadJsonFile(relDef.FilePath);
                        jsonReleaseDefinition = jsonReleaseDefinition.Replace("$ProjectName$", model.Environment.ProjectName)
                                             .Replace("$ProjectId$", model.Environment.ProjectId)
                                             .Replace("$OwnerUniqueName$", teamMember.identity.uniqueName)
                                             .Replace("$OwnerId$", teamMember.identity.id)
                                  .Replace("$OwnerDisplayName$", teamMember.identity.displayName);

                        //Adding randon UUID to website name
                        string uuid = Guid.NewGuid().ToString();
                        uuid = uuid.Substring(0, 8);
                        jsonReleaseDefinition = jsonReleaseDefinition.Replace("$UUID$", uuid).Replace("$RandomNumber$", uuid).Replace("$AccountName$", model.accountName); ;

                        foreach (BuildDef objBuildDef in model.BuildDefinitions)
                        {
                            //update build ids
                            string placeHolder = string.Format("${0}-id$", objBuildDef.Name);
                            jsonReleaseDefinition = jsonReleaseDefinition.Replace(placeHolder, objBuildDef.Id);

                            //update agent queue ids
                            foreach (string queue in model.Environment.AgentQueues.Keys)
                            {
                                placeHolder = string.Format("${0}$", queue);
                                jsonReleaseDefinition = jsonReleaseDefinition.Replace(placeHolder, model.Environment.AgentQueues[queue].ToString());
                            }

                            //update endpoint ids
                            foreach (string endpoint in model.Environment.serviceEndpoints.Keys)
                            {
                                placeHolder = string.Format("${0}$", endpoint);
                                jsonReleaseDefinition = jsonReleaseDefinition.Replace(placeHolder, model.Environment.serviceEndpoints[endpoint]);
                            }
                        }
                        string[] releaseDef = objRelease.CreateReleaseDefinition(jsonReleaseDefinition, model.ProjectName);
                        if (releaseDef.Length > 0)
                        {
                            relDef.Id = releaseDef[0];
                            relDef.Name = releaseDef[1];
                        }
                        if (!string.IsNullOrEmpty(relDef.Name))
                        {
                            objRelease.LastFailureMessage = string.Empty;
                        }
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
        /// <param name="templatesFolder"></param>
        /// <param name="model"></param>
        /// <param name="listQueries"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="_configuration2"></param>
        /// <param name="_configuration3"></param>
        /// <param name="releaseConfig"></param>
        public void CreateQueryAndWidgets(string templatesFolder, Project model, List<string> listQueries, VstsRestAPI.Configuration _queriesVersion, VstsRestAPI.Configuration _dashboardVersion, VstsRestAPI.Configuration _releaseConfig, VstsRestAPI.Configuration _projectConfig, VstsRestAPI.Configuration _boardConfig)
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
                string dashBoardTemplate = string.Format(templatesFolder + @"{0}\Dashboard\Dashboard.json", model.SelectedTemplate);
                if (System.IO.File.Exists(dashBoardTemplate))
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
                    if (model.SelectedTemplate == "MyHealthClinic" || model.SelectedTemplate == "PartsUnlimited" || model.SelectedTemplate == "PartsUnlimited-agile")
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


                            if (model.SelectedTemplate == "MyHealthClinic")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$ReleaseDefId$", model.ReleaseDefinitions.Where(x => x.Name == "MyHealthClinicE2E").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "MyHealthClinicE2E").FirstOrDefault().Id : string.Empty).
                                             Replace("$ActiveBugs$", queryResults.Where(x => x.name == "Active Bugs_WI").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Active Bugs_WI").FirstOrDefault().id : string.Empty).
                                             Replace("$MyHealthClinicE2E$", model.BuildDefinitions.Where(x => x.Name == "MyHealthClinicE2E").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "MyHealthClinicE2E").FirstOrDefault().Id : string.Empty).
                                                 Replace("$RepositoryId$", model.Environment.repositoryIdList.Any(i => i.Key.ToLower().Contains("myhealthclinic")) ? model.Environment.repositoryIdList.Where(x => x.Key.ToLower() == "myhealthclinic").FirstOrDefault().Value : string.Empty);
                            }
                            if (model.SelectedTemplate == "PartsUnlimited" || model.SelectedTemplate == "PartsUnlimited-agile")
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
                    if (model.SelectedTemplate == "MyShuttleDocker")
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
                                  .Replace("$Test Plan$", queryResults.Where(x => x.name == "Test Plans").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Test Plans").FirstOrDefault().id : string.Empty)
                                  .Replace("$Test Cases$", queryResults.Where(x => x.name == "Test Cases").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Test Cases").FirstOrDefault().id : string.Empty)
                                  .Replace("$Feature$", queryResults.Where(x => x.name == "Feature").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Feature").FirstOrDefault().id : string.Empty)
                                  .Replace("$Tasks$", queryResults.Where(x => x.name == "Tasks").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Tasks").FirstOrDefault().id : string.Empty)
                                         .Replace("$RepoMyShuttleDocker$", model.Environment.repositoryIdList.Where(x => x.Key == "MyShuttleDocker").FirstOrDefault().ToString() != "" ? model.Environment.repositoryIdList.Where(x => x.Key == "MyShuttleDocker").FirstOrDefault().Value : string.Empty);


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
                    if (model.SelectedTemplate.ToLower() == "docker" || model.SelectedTemplate.ToLower() == "php" || model.SelectedTemplate.ToLower() == "sonarqube" || model.SelectedTemplate.ToLower() == "github" || model.SelectedTemplate.ToLower() == "whitesource bolt" || model.SelectedTemplate == "DeploymentGroups" || model.SelectedTemplate == "Octopus")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);
                            dashBoardTemplate = dashBoardTemplate.Replace("$Task$", queryResults.Where(x => x.name == "Tasks").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Tasks").FirstOrDefault().id : string.Empty)
                                         .Replace("$AllWorkItems$", queryResults.Where(x => x.name == "All Work Items").FirstOrDefault() != null ? queryResults.Where(x => x.name == "All Work Items").FirstOrDefault().id : string.Empty)
                                         .Replace("$Feature$", queryResults.Where(x => x.name == "Feature").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Feature").FirstOrDefault().id : string.Empty)
                                         .Replace("$Projectid$", model.Environment.ProjectId)
                                         .Replace("$Epic$", queryResults.Where(x => x.name == "Epics").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Epics").FirstOrDefault().id : string.Empty);

                            if (model.SelectedTemplate.ToLower() == "docker")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$BuildDocker$", model.BuildDefinitions.Where(x => x.Name == "MHCDocker.build").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "MHCDocker.build").FirstOrDefault().Id : string.Empty)
                                .Replace("$ReleaseDocker$", model.ReleaseDefinitions.Where(x => x.Name == "MHCDocker.release").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "MHCDocker.release").FirstOrDefault().Id : string.Empty)
                                  .Replace("$PBI$", queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault().id : string.Empty);
                            }
                            else if (model.SelectedTemplate.ToLower() == "php")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$buildPHP$", model.BuildDefinitions.Where(x => x.Name == "PHP").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "PHP").FirstOrDefault().Id : string.Empty)
                        .Replace("$releasePHP$", model.ReleaseDefinitions.Where(x => x.Name == "PHP").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "PHP").FirstOrDefault().Id : string.Empty)
                                 .Replace("$PBI$", queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault().id : string.Empty);
                            }
                            else if (model.SelectedTemplate.ToLower() == "sonarqube")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$BuildSonarQube$", model.BuildDefinitions.Where(x => x.Name == "SonarQube").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "SonarQube").FirstOrDefault().Id : string.Empty)
                                .Replace("$PBI$", queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault().id : string.Empty);

                            }
                            else if (model.SelectedTemplate.ToLower() == "github")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$PBI$", queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault().id : string.Empty)
                                             .Replace("$buildGitHub$", model.BuildDefinitions.Where(x => x.Name == "GitHub").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "GitHub").FirstOrDefault().Id : string.Empty)
                                             .Replace("$Hosted$", model.Environment.AgentQueues["Hosted"].ToString())
                                             .Replace("$releaseGitHub$", model.ReleaseDefinitions.Where(x => x.Name == "GitHub").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "GitHub").FirstOrDefault().Id : string.Empty);

                            }
                            else if (model.SelectedTemplate.ToLower() == "whitesource bolt")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$PBI$", queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.name == "Product Backlog Items").FirstOrDefault().id : string.Empty)
                                          .Replace("$buildWhiteSource$", model.BuildDefinitions.Where(x => x.Name == "WhiteSourceBolt").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "WhiteSourceBolt").FirstOrDefault().Id : string.Empty);
                            }

                            else if (model.SelectedTemplate == "DeploymentGroups")
                            {
                                QueryResponse WorkInProgress = objQuery.GetQueryByPathAndName(model.ProjectName, "Work in Progress_WI", "Shared%20Queries");
                                dashBoardTemplate = dashBoardTemplate.Replace("$WorkinProgress$", WorkInProgress.id);
                            }

                            else if (model.SelectedTemplate == "Octopus")
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

                    if (model.SelectedTemplate.ToLower() == "smarthotel360")
                    {
                        if (isDashboardDeleted)
                        {
                            string startdate = DateTime.Now.ToString("yyyy-MM-dd");
                            VstsRestAPI.ProjectsAndTeams.Teams objTeam = new VstsRestAPI.ProjectsAndTeams.Teams(_projectConfig);
                            TeamResponse defaultTeam = objTeam.GetTeamByName(model.ProjectName, model.ProjectName + " team");
                            ClassificationNodes objnodes = new ClassificationNodes(_boardConfig);
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
                                .Replace("$PublicWebSiteCD$", model.ReleaseDefinitions.Where(x => x.Name == "PublicWebSiteCD").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "PublicWebSiteCD").FirstOrDefault().Id : string.Empty);

                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);

                        }
                    }
                    if (model.SelectedTemplate.ToLower() == "contososhuttle")
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
                //Update WorkInProgress ,UnfinishedWork Queries,Test Cases,Blocked Tasks queries.
                string updateQueryString = string.Empty;

                updateQueryString = "SELECT [System.Id],[System.Title],[Microsoft.VSTS.Common.BacklogPriority],[System.AssignedTo],[System.State],[Microsoft.VSTS.Scheduling.RemainingWork],[Microsoft.VSTS.CMMI.Blocked],[System.WorkItemType] FROM workitemLinks WHERE ([Source].[System.TeamProject] = @project AND [Source].[System.IterationPath] UNDER '$Project$\\Sprint 2' AND ([Source].[System.WorkItemType] IN GROUP 'Microsoft.RequirementCategory' OR [Source].[System.WorkItemType] IN GROUP 'Microsoft.TaskCategory' ) AND [Source].[System.State] <> 'Removed' AND [Source].[System.State] <> 'Done') AND ([System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Forward')  AND ([Target].[System.WorkItemType] IN GROUP 'Microsoft.TaskCategory' AND [Target].[System.State] <> 'Done' AND [Target].[System.State] <> 'Removed') ORDER BY [Microsoft.VSTS.Common.BacklogPriority],[Microsoft.VSTS.Scheduling.Effort], [Microsoft.VSTS.Scheduling.RemainingWork],[System.Id] MODE (Recursive)";
                dynamic queryObject = new System.Dynamic.ExpandoObject();
                updateQueryString = updateQueryString.Replace("$Project$", model.Environment.ProjectName);
                queryObject.wiql = updateQueryString;
                bool isUpdated = objQuery.UpdateQuery("Shared%20Queries/Current%20Sprint/Unfinished Work", model.Environment.ProjectName, Newtonsoft.Json.JsonConvert.SerializeObject(queryObject));

                updateQueryString = "SELECT [System.Id],[System.WorkItemType],[System.Title],[System.AssignedTo],[System.State],[Microsoft.VSTS.Scheduling.RemainingWork] FROM workitems WHERE [System.TeamProject] = @project AND [System.IterationPath] UNDER '$Project$\\Sprint 2' AND [System.WorkItemType] IN GROUP 'Microsoft.TaskCategory' AND [System.State] = 'In Progress' ORDER BY [System.AssignedTo],[Microsoft.VSTS.Common.BacklogPriority],[System.Id]";
                updateQueryString = updateQueryString.Replace("$Project$", model.Environment.ProjectName);
                queryObject.wiql = updateQueryString;
                isUpdated = objQuery.UpdateQuery("Shared%20Queries/Current%20Sprint/Work in Progress", model.Environment.ProjectName, Newtonsoft.Json.JsonConvert.SerializeObject(queryObject));


                updateQueryString = "SELECT [System.Id],[System.WorkItemType],[System.Title],[System.State],[Microsoft.VSTS.Common.Priority] FROM workitems WHERE [System.TeamProject] = @project AND [System.IterationPath] UNDER @currentIteration AND [System.WorkItemType] IN GROUP 'Microsoft.TestCaseCategory' ORDER BY [Microsoft.VSTS.Common.Priority],[System.Id] ";
                queryObject.wiql = updateQueryString;
                isUpdated = objQuery.UpdateQuery("Shared%20Queries/Current%20Sprint/Test Cases", model.Environment.ProjectName, Newtonsoft.Json.JsonConvert.SerializeObject(queryObject));

                updateQueryString = "SELECT [System.Id],[System.WorkItemType],[System.Title],[Microsoft.VSTS.Common.BacklogPriority],[System.AssignedTo],[System.State],[Microsoft.VSTS.CMMI.Blocked] FROM workitems WHERE [System.TeamProject] = @project AND [System.IterationPath] UNDER @currentIteration AND [System.WorkItemType] IN GROUP 'Microsoft.TaskCategory' AND [Microsoft.VSTS.CMMI.Blocked] = 'Yes' AND [System.State] <> 'Removed' ORDER BY [Microsoft.VSTS.Common.BacklogPriority], [System.Id]";
                queryObject.wiql = updateQueryString;
                isUpdated = objQuery.UpdateQuery("Shared%20Queries/Current%20Sprint/Blocked Tasks", model.Environment.ProjectName, Newtonsoft.Json.JsonConvert.SerializeObject(queryObject));

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

        /// <summary>
        /// Checking for Extenison in the account
        /// </summary>
        /// <param name="selectedTemplate"></param>
        /// <param name="token"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public JsonResult CheckForInstalledExtensions(string selectedTemplate, string token, string account)
        {
            try
            {
                if (!string.IsNullOrEmpty(selectedTemplate) && !string.IsNullOrEmpty(account) && !string.IsNullOrEmpty(token))
                {
                    string accountName = string.Empty;
                    string pat = string.Empty;

                    accountName = account;
                    pat = token;
                    string templatesFolder = Server.MapPath("~") + @"\Templates\";
                    string extensionJsonFile = string.Format(templatesFolder + @"{0}\Extensions.json", selectedTemplate);
                    if (!(System.IO.File.Exists(extensionJsonFile)))
                    {
                        return Json(new { message = "Template not found", status = "false" }, JsonRequestBehavior.AllowGet);
                    }

                    string listedExtension = System.IO.File.ReadAllText(extensionJsonFile);
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

                        var connection = new VssConnection(new Uri(string.Format("https://{0}.visualstudio.com", accountName)), new Microsoft.VisualStudio.Services.OAuth.VssOAuthAccessTokenCredential(pat));// VssOAuthCredential(PAT));

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
                            requiresExtensionNames = "<p style='color:red'>One or more extension(s) is not installed/enabled in your Azure DevOps Organization.</p><p> You will need to install and enable them in order to proceed. If you agree with the terms below, the required extensions will be installed automatically for the selected organization when the project is provisioned, otherwise install them manually and try refreshing the page </p>";
                            var installedExtensions = dict.Where(x => x.Value == true).ToList();
                            if (installedExtensions.Count > 0)
                            {
                                foreach (var ins in installedExtensions)
                                {

                                    string link = "<i class='fas fa-check-circle'></i> " + template.Extensions.Where(x => x.extensionName == ins.Key).FirstOrDefault().link;
                                    string lincense = "";
                                    requiresExtensionNames = requiresExtensionNames + link + lincense + "<br/><br/>";
                                }
                            }
                            foreach (var req in required)
                            {
                                string publisher = template.Extensions.Where(x => x.extensionName == req.Key).FirstOrDefault().publisherName;
                                if (publisher == "Microsoft")
                                {
                                    string link = "<i class='fa fa-times-circle'></i> " + template.Extensions.Where(x => x.extensionName == req.Key).FirstOrDefault().link;

                                    string lincense = "";
                                    if (!string.IsNullOrEmpty(template.Extensions.Where(x => x.extensionName == req.Key).FirstOrDefault().License))
                                    {
                                        lincense = " - " + template.Extensions.Where(x => x.extensionName == req.Key).FirstOrDefault().License;
                                    }
                                    requiredMicrosoftExt = requiredMicrosoftExt + link + lincense + "<br/>";
                                }
                                else
                                {
                                    string link = "<i class='fa fa-times-circle'></i> " + template.Extensions.Where(x => x.extensionName == req.Key).FirstOrDefault().link;
                                    string lincense = "";
                                    if (!string.IsNullOrEmpty(template.Extensions.Where(x => x.extensionName == req.Key).FirstOrDefault().License))
                                    {
                                        lincense = " - " + template.Extensions.Where(x => x.extensionName == req.Key).FirstOrDefault().License;
                                    }
                                    requiredThirdPartyExt = requiredThirdPartyExt + link + lincense + "<br/>";
                                }
                            }
                            if (!string.IsNullOrEmpty(requiredMicrosoftExt))
                            {
                                requiredMicrosoftExt = requiredMicrosoftExt + "<br/><div id='agreeTerms'><label style='font-weight: 400; text-align: justify; padding-left: 5px;'><input type = 'checkbox' class='terms' id = 'agreeTermsConditions' placeholder='microsoft' /> &nbsp; By checking the box I agree, and also on behalf of all users in the organization, that our use of the extension(s) is governed by the  <a href = 'https://go.microsoft.com/fwlink/?LinkID=266231' target = '_blank'> Microsoft Online Services Terms </a> and <a href = 'https://go.microsoft.com/fwlink/?LinkId=131004&clcid=0x409' target = '_blank'> Microsoft Online Services Privacy Statement</a></label></div>";
                            }
                            if (!string.IsNullOrEmpty(requiredThirdPartyExt))
                            {
                                requiredThirdPartyExt = requiredThirdPartyExt + "<br/><div id='ThirdPartyAgreeTerms'><label style = 'font-weight: 400; text-align: justify; padding-left: 5px;'><input type = 'checkbox' class='terms' id = 'ThirdPartyagreeTermsConditions' placeholder='thirdparty' /> &nbsp; The extension(s) are offered to you for your use by a third party, not Microsoft.  The extension(s) is licensed separately according to its corresponding License Terms.  By continuing and installing those extensions, you also agree to those License Terms.</label></div>";
                            }
                            finalExtensionString = requiresExtensionNames + requiredMicrosoftExt + requiredThirdPartyExt;
                            return Json(new { message = finalExtensionString, status = "false" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var installedExtensions = dict.Where(x => x.Value == true).ToList();
                            if (installedExtensions.Count > 0)
                            {
                                requiresExtensionNames = "All required extensions are installed/enabled in your Azure DevOps Organization.<br/><br/><b>";
                                foreach (var ins in installedExtensions)
                                {
                                    string link = "<i class='fas fa-check-circle'></i> " + template.Extensions.Where(x => x.extensionName == ins.Key).FirstOrDefault().link;
                                    string lincense = "";
                                    requiresExtensionNames = requiresExtensionNames + link + lincense + "<br/>";
                                }
                                return Json(new { message = requiresExtensionNames, status = "true" }, JsonRequestBehavior.AllowGet);
                            }
                        }

                    }
                    else { requiresExtensionNames = "no extensions required"; return Json(requiresExtensionNames, JsonRequestBehavior.AllowGet); }
                    return Json(new { message = requiresExtensionNames, status = "false" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { message = "Error", status = "false" }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                return Json(new { message = "Error", status = "false" }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Installing Extensions
        /// </summary>
        /// <param name="model"></param>
        /// <param name="accountName"></param>
        /// <param name="PAT"></param>
        /// <returns></returns>
        public bool InstallExtensions(Project model, string accountName, string PAT)
        {
            try
            {
                string templatesFolder = Server.MapPath("~") + @"\Templates\";
                string projTemplateFile = string.Format(templatesFolder + @"{0}\Extensions.json", model.SelectedTemplate);
                if (!(System.IO.File.Exists(projTemplateFile)))
                {
                    return false;
                }
                string templateItems = System.IO.File.ReadAllText(projTemplateFile);
                var template = JsonConvert.DeserializeObject<RequiredExtensions.Extension>(templateItems);
                string requiresExtensionNames = string.Empty;

                //Check for existing extensions
                if (template.Extensions.Count > 0)
                {
                    Dictionary<string, bool> dict = new Dictionary<string, bool>();
                    foreach (RequiredExtensions.ExtensionWithLink ext in template.Extensions)
                    {
                        dict.Add(ext.extensionName, false);
                    }
                    var connection = new VssConnection(new Uri(string.Format("https://{0}.visualstudio.com", accountName)), new Microsoft.VisualStudio.Services.OAuth.VssOAuthAccessTokenCredential(PAT));// VssOAuthCredential(PAT));
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
        /// Get Session data
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public JsonResult CheckSession()
        {
            List<string> listSession = new List<string>();
            if (Session["templateName"] != null)
            {
                listSession.Add(Session["templateName"].ToString());
            }

            if (Session["templateId"] != null)
            {
                listSession.Add(Session["templateId"].ToString());
            }

            if (Session["Message"] != null)
            {
                listSession.Add(Session["Message"].ToString());
            }
            return Json(listSession, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// WIKI set up operations 
        /// Project as Wiki and Code as Wiki
        /// </summary>
        /// <param name="templatesFolder"></param>
        /// <param name="model"></param>
        /// <param name="_wikiConfiguration"></param>
        public void CreateProjetWiki(string templatesFolder, Project model, Configuration _wikiConfiguration)
        {
            try
            {
                ManageWiki manageWiki = new ManageWiki(_wikiConfiguration);
                string projectWikiFolderPath = templatesFolder + model.SelectedTemplate + "\\Wiki\\ProjectWiki";
                if (Directory.Exists(projectWikiFolderPath))
                {
                    string createWiki = string.Format(templatesFolder + "\\CreateWiki.json"); // check is path
                    if (System.IO.File.Exists(createWiki))
                    {
                        string jsonString = System.IO.File.ReadAllText(createWiki);
                        jsonString = jsonString.Replace("$ProjectID$", model.Environment.ProjectId)
                            .Replace("$Name$", model.Environment.ProjectName);
                        ProjectwikiResponse.Projectwiki projectWikiResponse = manageWiki.CreateProjectWiki(jsonString, model.Environment.ProjectId);
                        string[] subDirectories = Directory.GetDirectories(projectWikiFolderPath);
                        foreach (var dir in subDirectories)
                        {
                            //dirName==parentName//
                            string[] dirSplit = dir.Split('\\');
                            string dirName = dirSplit[dirSplit.Length - 1];
                            string sampleContent = System.IO.File.ReadAllText(templatesFolder + "\\SampleContent.json");
                            sampleContent = sampleContent.Replace("$Content$", "Sample wiki content");
                            bool isPage = manageWiki.CreateUpdatePages(sampleContent, model.Environment.ProjectName, projectWikiResponse.id, dirName);//check is created

                            if (isPage)
                            {
                                string[] getFiles = System.IO.Directory.GetFiles(dir);
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
                                                string movePages = System.IO.File.ReadAllText(templatesFolder + "\\MovePages.json");
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
        public void CreateCodeWiki(string templatesFolder, Project model, VstsRestAPI.Configuration _wikiConfiguration)
        {
            try
            {
                string wikiFolder = templatesFolder + model.SelectedTemplate + "\\Wiki";
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
                            foreach (string repository in model.Environment.repositoryIdList.Keys)
                            {
                                string placeHolder = string.Format("${0}$", repository);
                                json = json.Replace(placeHolder, model.Environment.repositoryIdList[repository])
                                    .Replace("$Name$", name).Replace("$ProjectID$", model.Environment.ProjectId);
                            }
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
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.id.ErrorId(), "Error while creating wiki: " + ex.Message);
            }
        }

        public void CreateDeploymentGroup(string templateFolder, Project model, Configuration _deploymentGroup)
        {
            string path = templateFolder + model.SelectedTemplate + "\\DeploymentGroups\\CreateDeploymentGroup.json";
            if (System.IO.File.Exists(path))
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
                string templatesPath = ""; templatesPath = Server.MapPath("~") + @"\Templates\";
                if (System.IO.File.Exists(templatesPath + "TemplateSetting.json"))
                {
                    groupDetails = System.IO.File.ReadAllText(templatesPath + @"\TemplateSetting.json");
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
        #endregion
    }
}

