using Microsoft.VisualStudio.Services.ExtensionManagement.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VstsDemoBuilder.Extensions;
using VstsDemoBuilder.Models;
using VstsDemoBuilder.ServiceInterfaces;
using VstsDemoBuilder.Services;
using VstsRestAPI.Viewmodel.Extractor;
using VstsRestAPI.Viewmodel.ProjectAndTeams;
using VstsRestAPI.WorkItemAndTracking;

namespace VstsDemoBuilder.Controllers
{
    public class EnvironmentController : Controller
    {

        //#region Variables & Properties
        //private static readonly object objLock = new object();
        //private static Dictionary<string, string> statusMessages;
        //private ILog logger = LogManager.GetLogger("ErrorLog");

        private delegate string[] ProcessEnvironment(Project model, bool IsAPI = false);
        //public bool isDefaultRepoTodetele = true;
        //public string websiteUrl = string.Empty;
        //public string templateUsed = string.Empty;
        //public string projectName = string.Empty;
        //private string ProjectService.extractPath = string.Empty;
        //private AccessDetails _accessDetails = new AccessDetails();
        //private string logPath = "";
        //private string templateVersion = string.Empty;
        //private string enableExtractor = "";

        private IProjectService projectService;
        private ITemplateService templateService;
        private IAccountService accountService;
        public EnvironmentController(IProjectService _ProjectService,IAccountService _accountService,ITemplateService _templateService)
        {
            projectService = _ProjectService;
            accountService = _accountService;
            templateService = _templateService;
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
            lock (ProjectService.objLock)
            {
                string message = string.Empty;
                if (ProjectService.StatusMessages.Keys.Count(x => x == id) == 1)
                {
                    message = ProjectService.StatusMessages[id];
                }
                else
                {
                    return "100";
                }

                if (id.EndsWith("_Errors"))
                {
                    projectService.RemoveKey(id);
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
                ProjectService.enableExtractor = Session["EnableExtractor"] != null ? Session["EnableExtractor"].ToString() : string.Empty;
                if (string.IsNullOrEmpty(ProjectService.enableExtractor) || ProjectService.enableExtractor == "false")
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
                AccessDetails _accessDetails = ProjectService.AccessDetails;
                string TemplateSelected = string.Empty;
                if (Session["visited"] != null)
                {
                    Project model = new Project();
                    if (Session["EnableExtractor"] != null)
                    {
                        model.EnableExtractor = Session["EnableExtractor"].ToString();
                        ProjectService.enableExtractor = model.EnableExtractor.ToLower();
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
                        ProfileDetails profile = accountService.GetProfile(_accessDetails);
                        if (profile.displayName != null || profile.emailAddress != null)
                        {
                            Session["User"] = profile.displayName ?? string.Empty;
                            Session["Email"] = profile.emailAddress ?? profile.displayName.ToLower();
                        }
                        if (profile.id != null)
                        {
                            AccountsResponse.AccountList accountList = accountService.GetAccounts(profile.id, _accessDetails);

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
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
                AccessDetails _accessDetails = ProjectService.AccessDetails;
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
                    string accessRequestBody = accountService.GenerateRequestPostData(clientId, code, redirectUrl);
                    _accessDetails = accountService.GetAccessToken(accessRequestBody);
                    //_accessDetails.access_token = "";
                    //_accessDetails.access_token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im9PdmN6NU1fN3AtSGpJS2xGWHo5M3VfVjBabyJ9.eyJuYW1laWQiOiIxZTk1OTNlNC0wM2ViLTY1MjktOWFlNy1lY2M1ZmUyN2QyNWEiLCJzY3AiOiJ2c28uYWdlbnRwb29sc19tYW5hZ2UgdnNvLmJ1aWxkX2V4ZWN1dGUgdnNvLmNvZGVfbWFuYWdlIHZzby5kYXNoYm9hcmRzX21hbmFnZSB2c28uZXh0ZW5zaW9uX21hbmFnZSB2c28uaWRlbnRpdHkgdnNvLnByb2plY3RfbWFuYWdlIHZzby5yZWxlYXNlX21hbmFnZSB2c28uc2VydmljZWVuZHBvaW50X21hbmFnZSB2c28udGVzdF93cml0ZSB2c28ud2lraV93cml0ZSB2c28ud29ya19mdWxsIiwiYXVpIjoiMjZjZDc3NDQtNTBlMS00MWVkLTgzZDgtZDUwNjFhOGM1NDIyIiwiYXBwaWQiOiI1MDE4NzdkMy05YmNjLTRiZTYtYThjZC04MGFkOTk5YTY5NmEiLCJpc3MiOiJhcHAudnNzcHMudmlzdWFsc3R1ZGlvLmNvbSIsImF1ZCI6ImFwcC52c3Nwcy52aXN1YWxzdHVkaW8uY29tIiwibmJmIjoxNTU4MDg4Nzk3LCJleHAiOjE1NTgwOTIzOTd9.sIBT2PvGwebk0rSvpDO7Ogk6z5-cuMQN8ABvrz06Tfktn_J8Sx93U-IAxWBSBVIEsdHNa-JljHgreTPU94wm2vxZMK7wHJ4gAW2G4zC_c6uDPIt81ftRJP3Uxa_6co2X-clCB_6dX4tc5mFot9qTKaAY6sMZJ1EnvyXnEfkQ9nTqI2cc1bIqtyByyr_W8x_j5EaHWghII4s3t1ikuPssaxoIooSekdV7XHrxgNIFN8WtYj7WpfF_t2ZFFFe84FNmxDhDK99ZVDwHnINmMHquiMCVdaMEk5jNddH9slAuxl5Ojku3YfRZOjr-XkBNrQhbjixZMGO0AdNMOyyMsmc6Dg";
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
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
                    ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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

                ProjectService.extractPath = Server.MapPath("~/Templates/" + folder);

                if (Directory.Exists(ProjectService.extractPath))
                {
                    System.IO.File.Delete(Server.MapPath("~/Templates/" + fineName));
                    return Json("Folder already exist. Please rename the folder and upload it.");
                }
                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, ProjectService.extractPath);
                System.IO.File.Delete(zipPath);

                bool settingFile = (System.IO.File.Exists(ProjectService.extractPath + "\\ProjectSettings.json") ? true : false);
                bool projectFile = (System.IO.File.Exists(ProjectService.extractPath + "\\ProjectTemplate.json") ? true : false);

                if (settingFile && projectFile)
                {
                    string projectFileData = System.IO.File.ReadAllText(ProjectService.extractPath + "\\ProjectTemplate.json");
                    ProjectSetting settings = JsonConvert.DeserializeObject<ProjectSetting>(projectFileData);

                    if (!string.IsNullOrEmpty(settings.IsPrivate))
                    {
                        return Json("SUCCESS");
                    }
                    else
                    {
                        Directory.Delete(ProjectService.extractPath, true);
                        return Json("ISPRIVATEERROR");
                    }
                }
                else if (!settingFile && !projectFile)
                {
                    string[] folderName = System.IO.Directory.GetDirectories(ProjectService.extractPath);
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
                                string targetDirectory = ProjectService.extractPath;
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
                                Directory.Delete(ProjectService.extractPath, true);
                                return Json("ISPRIVATEERROR");
                            }
                        }
                    }
                    Directory.Delete(ProjectService.extractPath, true);
                    return Json("PROJECTANDSETTINGNOTFOUND");
                }
                else
                {
                    if (!settingFile)
                    {
                        Directory.Delete(ProjectService.extractPath, true);
                        return Json("SETTINGNOTFOUND");
                    }
                    if (!projectFile)
                    {
                        Directory.Delete(ProjectService.extractPath, true);
                        return Json("PROJECTFILENOTFOUND");
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                Directory.Delete(ProjectService.extractPath, true);
                return Json(ex.Message);
            }
            return Json("0");
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
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
                model.GitHubToken = Session["GitHubToken"].ToString();
                projectService.AddMessage(model.id, string.Empty);
                projectService.AddMessage(model.id.ErrorId(), string.Empty);

                ProcessEnvironment processTask = new ProcessEnvironment(projectService.CreateProjectEnvironment);
                processTask.BeginInvoke(model, false, new AsyncCallback(EndEnvironmentSetupProcess), processTask);
            }
            catch (Exception ex)
            {
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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

                projectService.RemoveKey(strResult[0]);
                if (ProjectService.StatusMessages.Keys.Count(x => x == strResult[0] + "_Errors") == 1)
                {
                    string errorMessages = ProjectService.statusMessages[strResult[0] + "_Errors"];
                    if (errorMessages != "")
                    {
                        //also, log message to file system
                        string logPath = Server.MapPath("~") + @"\Log";
                        string accountName = strResult[1];
                        string fileName = string.Format("{0}_{1}.txt", ProjectService.templateUsed, DateTime.Now.ToString("ddMMMyyyy_HHmmss"));

                        if (!Directory.Exists(logPath))
                        {
                            Directory.CreateDirectory(logPath);
                        }

                        System.IO.File.AppendAllText(Path.Combine(logPath, fileName), errorMessages);

                        //Create ISSUE work item with error details in VSTSProjectgenarator account
                        string patBase64 = System.Configuration.ConfigurationManager.AppSettings["PATBase64"];
                        string url = System.Configuration.ConfigurationManager.AppSettings["URL"];
                        string projectId = System.Configuration.ConfigurationManager.AppSettings["PROJECTID"];
                        string issueName = string.Format("{0}_{1}", ProjectService.templateUsed, DateTime.Now.ToString("ddMMMyyyy_HHmmss"));
                        IssueWI objIssue = new IssueWI();

                        errorMessages = errorMessages + "\t" + "TemplateUsed: " + ProjectService.templateUsed;
                        errorMessages = errorMessages + "\t" + "ProjectCreated : " + ProjectService.projectName;

                        ProjectService.logger.Error(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t  Error: " + errorMessages);

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
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                return Json(new { message = "Error", status = "false" }, JsonRequestBehavior.AllowGet);
            }
        }
    }

}

