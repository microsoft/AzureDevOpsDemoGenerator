using Microsoft.VisualStudio.Services.ExtensionManagement.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
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
        private delegate string[] ProcessEnvironment(Project model);
        private IProjectService projectService;
        private ITemplateService templateService;
        private IAccountService accountService;

        public EnvironmentController(IProjectService _ProjectService, IAccountService _accountService, ITemplateService _templateService)
        {
            projectService = _ProjectService;
            accountService = _accountService;
            templateService = _templateService;
        }

        [HttpGet]
        [AllowAnonymous]
        [SessonTimeout]
        public ContentResult GetCurrentProgress(string id)
        {
            this.ControllerContext.HttpContext.Response.AddHeader("cache-control", "no-cache");
            var currentProgress = GetStatusMessage(id).ToString();
            return Content(currentProgress);
        }

        /// <summary>
        /// Get status message to reply
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [SessonTimeout]
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
        [SessonTimeout]
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
        [SessonTimeout]
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
                        _templates.Groups.Add(group);
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
                AccessDetails _accessDetails = new AccessDetails();
                //AccessDetails _accessDetails = ProjectService.AccessDetails;
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
                    else if (Session["PrivateTemplateName"] != null)
                    {
                        model.TemplateName = Session["PrivateTemplateName"].ToString();
                        TemplateSelected = model.TemplateName;
                    }
                    else
                    {
                        TemplateSelected = System.Configuration.ConfigurationManager.AppSettings["DefaultTemplate"];
                        model.TemplateName = TemplateSelected;
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
                                accList.Add("Select Organization");
                                model.accountsForDropdown = accList;
                                ViewBag.AccDDError = "Could not load your organizations. Please check if the logged in Id contains the Azure DevOps Organizations or change the directory in profile page and try again.";
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
                                if (Session["PrivateTemplateName"] == null)
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
                                                    model.ForkGitHubRepo = template.ForkGitHubRepo.ToString();
                                                    model.templateImage = template.Image ?? "/Templates/TemplateImages/CodeFile.png";
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    model.SelectedTemplate = Session["PrivateTemplateOriginalName"].ToString();
                                    model.Templates.Add(model.SelectedTemplate);
                                    model.selectedTemplateDescription = "<p style='color:red;fontsize:10px'><b>Note</b>: Template will be discarded once the process completes. Please refersh the page to select other templates </p>";
                                    model.selectedTemplateFolder = Session["PrivateTemplateName"].ToString();
                                    model.ForkGitHubRepo = "false";
                                    model.templateImage = "/Templates/TemplateImages/CodeFile.png";
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
        [AllowAnonymous]
        [SessonTimeout]
        public ActionResult PrivateTemplate()
        {
            if (Session["visited"] != null)
            {
                return View();
            }
            else
            {
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
                    if (!string.IsNullOrEmpty(_accessDetails.access_token))
                    {
                        // add your access token here for local debugging                 
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
        [SessonTimeout]
        public ActionResult UploadFiles()
        {
            string[] strResult = new string[2];
            string templateName = string.Empty;
            strResult[0] = templateName;
            strResult[1] = string.Empty;
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    if (!Directory.Exists(Server.MapPath("~") + @"\ExtractedZipFile"))
                    {
                        Directory.CreateDirectory(Server.MapPath("~") + @"\ExtractedZipFile");
                    }
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
                            templateName = fileName.ToLower().Replace(".zip", "").Trim() + "-" + Guid.NewGuid().ToString().Substring(0, 6) + ".zip";

                            if (System.IO.File.Exists(Path.Combine(Server.MapPath("~/ExtractedZipFile/"), templateName)))
                            {
                                System.IO.File.Delete(Path.Combine(Server.MapPath("~/ExtractedZipFile/"), templateName));
                            }
                        }
                        else
                        {
                            fileName = file.FileName;
                            templateName = fileName.ToLower().Replace(".zip", "").Trim() + "-" + Guid.NewGuid().ToString().Substring(0, 6) + ".zip";

                            if (System.IO.File.Exists(Path.Combine(Server.MapPath("~/ExtractedZipFile/"), templateName)))
                            {
                                System.IO.File.Delete(Path.Combine(Server.MapPath("~/ExtractedZipFile/"), templateName));
                            }
                        }

                        // Get the complete folder path and store the file inside it.  
                        fileName = Path.Combine(Server.MapPath("~/ExtractedZipFile/"), templateName);
                        file.SaveAs(fileName);
                    }
                    strResult[0] = templateName;
                    // Returns message that successfully uploaded  
                    return Json(strResult);
                }
                catch (Exception ex)
                {
                    ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    strResult[1] = "Error occurred. Error details: " + ex.Message;
                    return Json(strResult);
                }
            }
            else
            {
                strResult[1] = "No files selected.";
                return Json(strResult);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [SessonTimeout]
        public ActionResult UnzipFile(string fineName)
        {
            PrivateTemplate privateTemplate = new PrivateTemplate();
            string extractPath = string.Empty;
            try
            {
                if (!System.IO.Directory.Exists(Server.MapPath("~") + @"\Logs"))
                {
                    Directory.CreateDirectory(Server.MapPath("~") + @"\Logs");
                }
                if (!Directory.Exists(Server.MapPath("~") + @"\PrivateTemplates"))
                {
                    Directory.CreateDirectory(Server.MapPath("~") + @"\PrivateTemplates");
                }
                //Deleting uploaded zip files present from last one hour
                string extractedZipFile = HostingEnvironment.MapPath("~") + @"ExtractedZipFile\";
                if (Directory.Exists(extractedZipFile))
                {
                    string[] subdirs = Directory.GetFiles(extractedZipFile)
                                   .Select(Path.GetFileName)
                                   .ToArray();
                    foreach (string _file in subdirs)
                    {
                        FileInfo d = new FileInfo(extractedZipFile + _file);
                        if (d.CreationTime < DateTime.Now.AddHours(-1))
                            System.IO.File.Delete(extractedZipFile + _file);
                    }
                }

                string zipPath = Server.MapPath("~/ExtractedZipFile/" + fineName);
                string folder = fineName.Replace(".zip", "");
                privateTemplate.privateTemplateName = folder;

                extractPath = Server.MapPath("~/PrivateTemplates/" + folder);
                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);
                System.IO.File.Delete(zipPath);
                privateTemplate.privateTemplatePath = templateService.FindPrivateTemplatePath(extractPath);

                privateTemplate.responseMessage = templateService.checkSelectedTemplateIsPrivate(privateTemplate.privateTemplatePath);

                bool isExtracted = templateService.checkTemplateDirectory(privateTemplate.privateTemplatePath);
                if (!isExtracted)
                {
                    Directory.Delete(extractPath, true);
                }
            }
            catch (Exception ex)
            {
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                Directory.Delete(extractPath, true);
                return Json(ex.Message);
            }
            return Json(privateTemplate, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get members of the account- Not using now
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [SessonTimeout]
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
        [SessonTimeout]
        public bool StartEnvironmentSetupProcess(Project model)
        {
            try
            {
                if (Session["visited"] != null)
                {
                    Session["PAT"] = model.accessToken;
                    Session["AccountName"] = model.accountName;
                    if (Session["GitHubToken"] != null && Session["GitHubToken"].ToString() != "" && model.GitHubFork)
                    {
                        model.GitHubToken = Session["GitHubToken"].ToString();
                    }
                    if (Session["PrivateTemplateURL"] != null && Session["PrivateTemplateName"] != null)
                    {
                        model.PrivateTemplatePath = Session["PrivateTemplateURL"].ToString();
                        Session["PrivateTemplateURL"] = null;
                        Session["PrivateTemplateName"] = null;
                        Session["PrivateTemplateOriginalName"] = null;
                        Session["templateName"] = System.Configuration.ConfigurationManager.AppSettings["DefaultTemplate"];
                    }
                    projectService.AddMessage(model.id, string.Empty);
                    projectService.AddMessage(model.id.ErrorId(), string.Empty);
                    bool whereIsTemplate = projectService.WhereDoseTemplateBelongTo(model.SelectedTemplate); // checking for private template  existance
                    if (!string.IsNullOrEmpty(model.PrivateTemplatePath) && whereIsTemplate) // if the template path exist and tempalte is present in private fodler
                    {
                        model.IsPrivatePath = true;
                    }
                    ProcessEnvironment processTask = new ProcessEnvironment(projectService.CreateProjectEnvironment);
                    processTask.BeginInvoke(model, new AsyncCallback(EndEnvironmentSetupProcess), processTask);
                }
                else
                {
                    return false;
                }
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
            string templateUsed = string.Empty;
            string ID = string.Empty;
            string accName = string.Empty;
            try
            {
                ProcessEnvironment processTask = (ProcessEnvironment)result.AsyncState;
                string[] strResult = processTask.EndInvoke(result);
                if (strResult != null && strResult.Length > 0)
                {
                    ID = strResult[0];
                    accName = strResult[1];
                    templateUsed = strResult[2];
                    projectService.RemoveKey(ID);
                    if (ProjectService.StatusMessages.Keys.Count(x => x == ID + "_Errors") == 1)
                    {
                        string errorMessages = ProjectService.statusMessages[ID + "_Errors"];
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
            }
            catch (Exception ex)
            {
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            finally
            {
                DeletePrivateTemplate(templateUsed);
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
        [SessonTimeout]
        public JsonResult CheckForInstalledExtensions(string selectedTemplate, string token, string account, string PrivatePath = "")
        {
            try
            {
                bool isTemplateBelongToPrivateFolder = projectService.WhereDoseTemplateBelongTo(selectedTemplate);
                if (!string.IsNullOrEmpty(selectedTemplate) && !string.IsNullOrEmpty(account) && !string.IsNullOrEmpty(token))
                {
                    string accountName = string.Empty;
                    string pat = string.Empty;

                    accountName = account;
                    pat = token;
                    string templatesFolder = string.Empty;
                    string extensionJsonFile = string.Empty;
                    if (isTemplateBelongToPrivateFolder)
                    {
                        templatesFolder = PrivatePath;//Session["PrivateTemplateURL"].ToString();
                        extensionJsonFile = string.Format("{0}\\{1}", templatesFolder, "Extensions.json");
                    }
                    else if (string.IsNullOrEmpty(PrivatePath))
                    {
                        templatesFolder = Server.MapPath("~") + @"\Templates\";
                        extensionJsonFile = string.Format(templatesFolder + @"\{0}\Extensions.json", selectedTemplate);
                    }
                    else
                    {
                        templatesFolder = PrivatePath;
                        extensionJsonFile = string.Format(templatesFolder + @"\Extensions.json");
                    }

                    if (!(System.IO.File.Exists(extensionJsonFile)))
                    {
                        return Json(new { message = "Template not found", status = "false" }, JsonRequestBehavior.AllowGet);
                    }

                    string listedExtension = System.IO.File.ReadAllText(extensionJsonFile);
                    var template = JsonConvert.DeserializeObject<RequiredExtensions.Extension>(listedExtension);

                    template.Extensions.RemoveAll(x => x.extensionName.ToLower() == "analytics");
                    template.Extensions = template.Extensions.OrderBy(y => y.extensionName).ToList();
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
                            if (!dict.ContainsKey(ext.extensionName))
                            {
                                dict.Add(ext.extensionName, false);
                            }
                        }

                        var connection = new VssConnection(new Uri(string.Format("https://{0}.visualstudio.com", accountName)), new Microsoft.VisualStudio.Services.OAuth.VssOAuthAccessTokenCredential(pat));// VssOAuthCredential(PAT));

                        var client = connection.GetClient<ExtensionManagementHttpClient>();
                        var installed = client.GetInstalledExtensionsAsync().Result;
                        var extensions = installed.Where(x => x.Flags == 0 && x.ExtensionDisplayName.ToLower() != "analytics").ToList();

                        var trustedFlagExtensions = installed.Where(x => x.Flags == ExtensionFlags.Trusted && x.ExtensionDisplayName.ToLower() != "analytics").ToList();
                        var builtInExtensions = installed.Where(x => x.Flags.ToString() == "BuiltIn, Trusted" && x.ExtensionDisplayName.ToLower() != "analytics").ToList();

                        extensions.AddRange(trustedFlagExtensions);
                        extensions.AddRange(builtInExtensions);
                        string askld = JsonConvert.SerializeObject(extensions);
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
                    else { requiresExtensionNames = "no extensions required"; return Json(new { message = "no extensions required", status = "false" }, JsonRequestBehavior.AllowGet); }
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

        [AllowAnonymous]
        [SessonTimeout]
        public string CheckSession()
        {
            if (Session["GitHubToken"] != null && Session["GitHubToken"].ToString() != "")
            {
                return Session["GitHubToken"].ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [SessonTimeout]
        public JsonResult UploadPrivateTemplateFromURL(string TemplateURL, string token, string userId, string password, string OldPrivateTemplate = "")
        {
            if (Session["visited"] != null)
            {
                if (!string.IsNullOrEmpty(OldPrivateTemplate))
                {
                    templateService.deletePrivateTemplate(OldPrivateTemplate);
                }
                PrivateTemplate privateTemplate = new PrivateTemplate();
                string templatePath = string.Empty;
                try
                {
                    string templateName = "";
                    string fileName = Path.GetFileName(TemplateURL);
                    string extension = Path.GetExtension(TemplateURL);
                    templateName = fileName.ToLower().Replace(".zip", "").Trim() + "-" + Guid.NewGuid().ToString().Substring(0, 6) + extension.ToLower();
                    privateTemplate.privateTemplateName = templateName.ToLower().Replace(".zip", "").Trim();
                    privateTemplate.privateTemplatePath = templateService.GetTemplateFromPath(TemplateURL, templateName, token, userId, password);

                    if (privateTemplate.privateTemplatePath != "")
                    {
                        privateTemplate.responseMessage = templateService.checkSelectedTemplateIsPrivate(privateTemplate.privateTemplatePath);
                        if (privateTemplate.responseMessage != "SUCCESS")
                        {
                            var templatepath = HostingEnvironment.MapPath("~") + @"\PrivateTemplates\" + templateName.ToLower().Replace(".zip", "").Trim();
                            if (Directory.Exists(templatepath))
                                Directory.Delete(templatepath, true);
                        }
                    }
                    else
                    {
                        privateTemplate.responseMessage = "Unable to download file, please check the provided URL";
                    }

                }
                catch (Exception ex)
                {
                    ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    return Json(new { message = "Error", status = "false" }, JsonRequestBehavior.AllowGet);
                }
                return Json(privateTemplate, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Session Expired", JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        [AllowAnonymous]
        [SessonTimeout]
        public void DeletePrivateTemplate(string TemplateName)
        {
            templateService.deletePrivateTemplate(TemplateName);
        }
    }

}

