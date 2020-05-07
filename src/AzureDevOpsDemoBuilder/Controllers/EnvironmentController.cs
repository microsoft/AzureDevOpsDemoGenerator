using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AzureDevOpsDemoBuilder.Extensions;
using AzureDevOpsDemoBuilder.Models;
using AzureDevOpsDemoBuilder.ServiceInterfaces;
using AzureDevOpsDemoBuilder.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AzureDevOpsAPI.Viewmodel.ProjectAndTeams;
using AzureDevOpsAPI.WorkItemAndTracking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.ExtensionManagement.WebApi;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AzureDevOpsDemoBuilder.Controllers
{
    public class EnvironmentController : Controller
    {
        private delegate string[] ProcessEnvironment(Project model);
        private IProjectService projectService;

        public IConfiguration AppKeyConfiguration { get; }

        private ITemplateService templateService;
        private IAccountService accountService;
        private IWebHostEnvironment HostingEnvironment;
        private ILogger<EnvironmentController> logger;

        public EnvironmentController(IProjectService _ProjectService, IConfiguration configuration,
            IAccountService _accountService, ITemplateService _templateService, IWebHostEnvironment hostEnvironment, ILogger<EnvironmentController> _logger)
        {
            projectService = _ProjectService;
            AppKeyConfiguration = configuration;
            accountService = _accountService;
            templateService = _templateService;
            HostingEnvironment = hostEnvironment;
            logger = _logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public ContentResult GetCurrentProgress(string id)
        {
            this.ControllerContext.HttpContext.Response.Headers.Add("cache-control", "no-cache");
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
            string templatesPath = HostingEnvironment.WebRootPath + "/Templates/";
            string template = string.Empty;

            if (System.IO.File.Exists(templatesPath + Path.GetFileName(TemplateName) + "/ProjectTemplate.json"))
            {
                Project objP = new Project();
                template = objP.ReadJsonFile(templatesPath + Path.GetFileName(TemplateName) + "/ProjectTemplate.json");
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
            string templatesPath = ""; templatesPath = HostingEnvironment.WebRootPath + "/Templates/";
            string email = HttpContext.Session.GetString("Email");
            if (System.IO.File.Exists(templatesPath + "TemplateSetting.json"))
            {
                groupDetails = System.IO.File.ReadAllText(templatesPath + "/TemplateSetting.json");
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
                ProjectService.enableExtractor = HttpContext.Session.GetString("EnableExtractor") != null ? HttpContext.Session.GetString("EnableExtractor") : string.Empty;
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
            return Json(templates);
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
                if (HttpContext.Session.GetString("visited") != null)
                {
                    //logger.LogInformation("visited");
                    Project model = new Project();
                    if (HttpContext.Session.GetString("EnableExtractor") != null)
                    {
                        model.EnableExtractor = HttpContext.Session.GetString("EnableExtractor");
                        ProjectService.enableExtractor = model.EnableExtractor.ToLower();
                    }
                    if (HttpContext.Session.GetString("templateName") != null && HttpContext.Session.GetString("templateName") != "")
                    {
                        model.TemplateName = HttpContext.Session.GetString("templateName");
                        TemplateSelected = model.TemplateName;
                    }
                    else
                    {
                        TemplateSelected = AppKeyConfiguration["DefaultTemplate"];
                    }

                    if (HttpContext.Session.GetString("PAT") != null)
                    {
                        //logger.LogInformation("PAT is not null");
                        _accessDetails.access_token = HttpContext.Session.GetString("PAT");
                        ProfileDetails profile = accountService.GetProfile(_accessDetails);
                        if (profile.displayName != null || profile.emailAddress != null)
                        {
                            ViewData.Add("User", profile.displayName ?? string.Empty);
                            ViewData.Add("Email", profile.emailAddress ?? profile.displayName.ToLower());
                        }
                        if (profile.id != null)
                        {
                            AccountsResponse.AccountList accountList = accountService.GetAccounts(profile.id, _accessDetails);

                            //New Feature Enabling
                            model.AccessToken = HttpContext.Session.GetString("PAT");
                            model.RefreshToken = _accessDetails.refresh_token;
                            HttpContext.Session.SetString("PAT", _accessDetails.access_token);
                            model.MemberID = profile.id;
                            List<string> accList = new List<string>();
                            if (accountList.Count > 0)
                            {
                                foreach (var account in accountList.Value)
                                {
                                    accList.Add(account.AccountName);
                                }
                                accList.Sort();
                                model.AccountsForDropdown = accList;
                                model.HasAccount = true;
                            }
                            else
                            {
                                accList.Add("Select Organization");
                                model.AccountsForDropdown = accList;
                                ViewBag.AccDDError = "Could not load your organizations. Please check if the logged in Id contains the Azure DevOps Organizations or change the directory in profile page and try again.";
                            }

                            model.Templates = new List<string>();
                            model.AccountUsersForDdl = new List<SelectListItem>();
                            TemplateSelection.Templates templates = new TemplateSelection.Templates();
                            string[] dirTemplates = Directory.GetDirectories(HostingEnvironment.WebRootPath + "/Templates");

                            //Taking all the template folder and adding to list
                            foreach (string template in dirTemplates)
                            {
                                model.Templates.Add(Path.GetFileName(template));
                            }
                            // Reading Template setting file to check for private templates
                            if (System.IO.File.Exists(HostingEnvironment.WebRootPath + "/Templates/TemplateSetting.json"))
                            {
                                string templateSetting = model.ReadJsonFile(HostingEnvironment.WebRootPath + "/Templates/TemplateSetting.json");
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
                                                model.SelectedTemplateDescription = template.Description == null ? string.Empty : template.Description;
                                                model.SelectedTemplateFolder = template.TemplateFolder == null ? string.Empty : template.TemplateFolder;
                                                model.Message = template.Message == null ? string.Empty : template.Message;
                                                model.ForkGitHubRepo = template.ForkGitHubRepo.ToString();
                                                model.TemplateImage = template.Image ?? "/Templates/TemplateImages/CodeFile.png";
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
                    HttpContext.Session.Clear();
                    return Redirect("../Account/Verify");
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                return Redirect("../Account/Verify");
            }
        }
        [AllowAnonymous]
        public ActionResult PrivateTemplate()
        {
            if (HttpContext.Session.GetString("visited") != null)
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
                string isCode = HttpContext.Request.Query["code"].ToString();
                if (isCode == null)
                {
                    return Redirect("../Account/Verify");
                }
                if (HttpContext.Session.GetString("visited") != null)
                {
                    if (HttpContext.Session.GetString("templateName") != null && HttpContext.Session.GetString("templateName") != "")
                    {
                        model.TemplateName = HttpContext.Session.GetString("templateName");
                    }
                    string code = HttpContext.Request.Query["code"].ToString();

                    string redirectUrl = AppKeyConfiguration["RedirectUri"];
                    string clientId = AppKeyConfiguration["ClientSecret"];
                    string accessRequestBody = accountService.GenerateRequestPostData(clientId, code, redirectUrl);
                    _accessDetails = accountService.GetAccessToken(accessRequestBody);
                    if (!string.IsNullOrEmpty(_accessDetails.access_token))
                    {
                        // add your access token here for local debugging                 
                        model.AccessToken = _accessDetails.access_token;
                        HttpContext.Session.SetString("PAT", _accessDetails.access_token);
                    }
                    return RedirectToAction("createproject", "Environment");
                }
                else
                {
                    HttpContext.Session.Clear();
                    return Redirect("../Account/Verify");
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                return View();
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult UploadFiles()
        {
            string[] strResult = new string[2];
            string templateName = string.Empty;
            strResult[0] = templateName;
            strResult[1] = string.Empty;
            // Checking no of files injected in Request object  
            if (Request.Form.Files.Count > 0)
            {
                try
                {
                    if (!Directory.Exists(HostingEnvironment.ContentRootPath + "/ExtractedZipFile"))
                    {
                        Directory.CreateDirectory(HostingEnvironment.ContentRootPath + "/ExtractedZipFile");
                    }
                    //  Get all files from Request object  
                    IFormFileCollection files = Request.Form.Files;
                    for (int i = 0; i < files.Count; i++)
                    {

                        var file = files[i];
                        string fileName;

                        // Checking for Internet Explorer  
                        if (Request.Headers["User-Agent"].ToString().ToUpper() == "IE" || Request.Headers["User-Agent"].ToString().ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testFiles = file.FileName.Split(new char[] { '\\' });
                            fileName = testFiles[testFiles.Length - 1];
                            templateName = fileName.ToLower().Replace(".zip", "").Trim() + "-" + Guid.NewGuid().ToString().Substring(0, 6) + ".zip";

                            if (System.IO.File.Exists(Path.Combine(HostingEnvironment.ContentRootPath + "/ExtractedZipFile/", templateName)))
                            {
                                System.IO.File.Delete(Path.Combine(HostingEnvironment.ContentRootPath + "/ExtractedZipFile/", templateName));
                            }
                        }
                        else
                        {
                            fileName = file.FileName;
                            templateName = fileName.ToLower().Replace(".zip", "").Trim() + "-" + Guid.NewGuid().ToString().Substring(0, 6) + ".zip";

                            if (System.IO.File.Exists(Path.Combine(HostingEnvironment.ContentRootPath + "/ExtractedZipFile/", templateName)))
                            {
                                System.IO.File.Delete(Path.Combine(HostingEnvironment.ContentRootPath + ("/ExtractedZipFile/"), templateName));
                            }
                        }

                        // Get the complete folder path and store the file inside it.  
                        fileName = Path.Combine(HostingEnvironment.ContentRootPath + "/ExtractedZipFile/", templateName);

                        using (var stream = new FileStream(fileName, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                    }
                    strResult[0] = templateName;
                    // Returns message that successfully uploaded  
                    return Json(strResult);
                }
                catch (Exception ex)
                {
                    logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
        public ActionResult UnzipFile(string fineName)
        {
            PrivateTemplate privateTemplate = new PrivateTemplate();
            string extractPath = string.Empty;
            try
            {
                if (!Directory.Exists(HostingEnvironment.ContentRootPath + "/PrivateTemplates"))
                {
                    Directory.CreateDirectory(HostingEnvironment.ContentRootPath + "/PrivateTemplates");
                }
                //Deleting uploaded zip files present from last one hour
                string extractedZipFile = HostingEnvironment.ContentRootPath + "/ExtractedZipFile/";
                if (!Directory.Exists(extractedZipFile))
                {
                    Directory.CreateDirectory(extractedZipFile);
                }
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

                string zipPath = HostingEnvironment.ContentRootPath + "/ExtractedZipFile/" + fineName;
                string folder = fineName.Replace(".zip", "");
                privateTemplate.PrivateTemplateName = folder;

                extractPath = HostingEnvironment.ContentRootPath + "/ExtractedZipFile/" + folder;
                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);
                System.IO.File.Delete(zipPath);
                privateTemplate.PrivateTemplatePath = templateService.FindPrivateTemplatePath(extractPath);

                privateTemplate.ResponseMessage = templateService.checkSelectedTemplateIsPrivate(privateTemplate.PrivateTemplatePath);

                bool isExtracted = templateService.checkTemplateDirectory(privateTemplate.PrivateTemplatePath);
                if (!isExtracted)
                {
                    Directory.Delete(extractPath, true);
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                Directory.Delete(extractPath, true);
                return Json(ex.Message);
            }
            return Json(privateTemplate);
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
                AzureDevOpsAPI.AppConfiguration _defaultConfiguration = new AzureDevOpsAPI.AppConfiguration() { UriString = "https://" + accountName + ".visualstudio.com/DefaultCollection/", VersionNumber = "2.2", PersonalAccessToken = accessToken };
                AzureDevOpsAPI.ProjectsAndTeams.Accounts objAccount = new AzureDevOpsAPI.ProjectsAndTeams.Accounts(_defaultConfiguration);
                accountMembers = objAccount.GetAccountMembers(accountName, accessToken);
                if (accountMembers.Count > 0)
                {
                    foreach (var user in accountMembers.Value)
                    {
                        mod.AccountUsersForDdl.Add(new SelectListItem
                        {
                            Text = user.Member.DisplayName,
                            Value = user.Member.MailAddress
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                return null;
            }
            return Json(mod);
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
                HttpContext.Session.SetString("PAT", model.AccessToken);
                HttpContext.Session.SetString("AccountName", model.AccountName);
                HttpContext.Session.SetString("trackId", model.Id);
                HttpContext.Session.SetString("template", model.SelectedTemplate);
                if (HttpContext.Session.GetString("GitHubToken") != null && HttpContext.Session.GetString("GitHubToken") != "" && model.GitHubFork)
                {
                    model.GitHubToken = HttpContext.Session.GetString("GitHubToken");
                }
                projectService.AddMessage(model.Id, string.Empty);
                projectService.AddMessage(model.Id.ErrorId(), string.Empty);
                if (!string.IsNullOrEmpty(model.PrivateTemplatePath))
                {
                    model.IsPrivatePath = true;
                }
                ProcessEnvironment processTask = new ProcessEnvironment(projectService.CreateProjectEnvironment);
                var workTask = Task.Run(() => processTask.Invoke(model));
                workTask.ContinueWith((antecedent) =>
                {
                    EndEnvironmentSetupProcess(workTask, model);
                });
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return true;
        }

        /// <summary>
        /// End the process
        /// </summary>
        /// <param name="result"></param>
        public string EndEnvironmentSetupProcess(IAsyncResult result, Project model)
        {
            string templateUsed = string.Empty;
            string ID = string.Empty;
            string accName = string.Empty;
            try
            {
                ProcessEnvironment processTask = (ProcessEnvironment)result.AsyncState;
                //string[] strResult = processTask.EndInvoke(result);
                projectService.RemoveKey(model.Id);
                if (ProjectService.StatusMessages.Keys.Count(x => x == model.Id + "_Errors") == 1)
                {
                    string errorMessages = ProjectService.statusMessages[model.Id + "_Errors"];
                    if (errorMessages != "")
                    {
                        //also, log message to file system
                        string logPath = HostingEnvironment.WebRootPath + "/log";
                        string fileName = string.Format("{0}_{1}.txt", templateUsed, DateTime.Now.ToString("ddMMMyyyy_HHmmss"));

                        if (!Directory.Exists(logPath))
                        {
                            Directory.CreateDirectory(logPath);
                        }

                        System.IO.File.AppendAllText(Path.Combine(logPath, fileName), errorMessages);

                        //Create ISSUE work item with error details in VSTSProjectgenarator account
                        string patBase64 = AppKeyConfiguration["PATBase64"];
                        string url = AppKeyConfiguration["URL"];
                        string projectId = AppKeyConfiguration["PROJECTID"];
                        string issueName = string.Format("{0}_{1}", templateUsed, DateTime.Now.ToString("ddMMMyyyy_HHmmss"));
                        IssueWi objIssue = new IssueWi();

                        errorMessages = errorMessages + "\t" + "TemplateUsed: " + templateUsed;
                        errorMessages = errorMessages + "\t" + "ProjectCreated : " + ProjectService.projectName;

                        logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t  Error: " + errorMessages);

                        string logWIT = AppKeyConfiguration["LogWIT"];
                        if (logWIT == "true")
                        {
                            objIssue.CreateIssueWi(patBase64, "4.1", url, issueName, errorMessages, projectId, "Demo Generator");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            finally
            {
                DeletePrivateTemplate(templateUsed);
            }
            return "";
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
        public JsonResult CheckForInstalledExtensions(string selectedTemplate, string token, string account, string PrivatePath = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(selectedTemplate) && !string.IsNullOrEmpty(account) && !string.IsNullOrEmpty(token))
                {
                    string accountName = string.Empty;
                    string pat = string.Empty;

                    accountName = account;
                    pat = token;
                    string templatesFolder = string.Empty;
                    string extensionJsonFile = string.Empty;
                    if (string.IsNullOrEmpty(PrivatePath))
                    {
                        templatesFolder = HostingEnvironment.WebRootPath + "/Templates/";
                        extensionJsonFile = string.Format(templatesFolder + "/{0}/Extensions.json", selectedTemplate);
                    }
                    else
                    {
                        templatesFolder = PrivatePath;
                        extensionJsonFile = string.Format(templatesFolder + "/Extensions.json");
                    }



                    if (!(System.IO.File.Exists(extensionJsonFile)))
                    {
                        return Json(new { message = "Template not found", status = "false" });
                    }

                    string listedExtension = System.IO.File.ReadAllText(extensionJsonFile);
                    var template = JsonConvert.DeserializeObject<RequiredExtensions.Extension>(listedExtension);

                    template.Extensions.RemoveAll(x => x.ExtensionName.ToLower() == "analytics");
                    template.Extensions = template.Extensions.OrderBy(y => y.ExtensionName).ToList();
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
                            if (!dict.ContainsKey(ext.ExtensionName))
                            {
                                dict.Add(ext.ExtensionName, false);
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
                                if (extension.ExtensionName.ToLower() == ext.ExtensionDisplayName.ToLower() && extension.ExtensionId.ToLower() == ext.ExtensionName.ToLower())
                                {
                                    dict[extension.ExtensionName] = true;
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

                                    string link = "<i class='fas fa-check-circle'></i> " + template.Extensions.Where(x => x.ExtensionName == ins.Key).FirstOrDefault().Link;
                                    string lincense = "";
                                    requiresExtensionNames = requiresExtensionNames + link + lincense + "<br/><br/>";
                                }
                            }
                            foreach (var req in required)
                            {
                                string publisher = template.Extensions.Where(x => x.ExtensionName == req.Key).FirstOrDefault().PublisherName;
                                if (publisher == "Microsoft")
                                {
                                    string link = "<i class='fa fa-times-circle'></i> " + template.Extensions.Where(x => x.ExtensionName == req.Key).FirstOrDefault().Link;

                                    string lincense = "";
                                    if (!string.IsNullOrEmpty(template.Extensions.Where(x => x.ExtensionName == req.Key).FirstOrDefault().License))
                                    {
                                        lincense = " - " + template.Extensions.Where(x => x.ExtensionName == req.Key).FirstOrDefault().License;
                                    }
                                    requiredMicrosoftExt = requiredMicrosoftExt + link + lincense + "<br/>";
                                }
                                else
                                {
                                    string link = "<i class='fa fa-times-circle'></i> " + template.Extensions.Where(x => x.ExtensionName == req.Key).FirstOrDefault().Link;
                                    string lincense = "";
                                    if (!string.IsNullOrEmpty(template.Extensions.Where(x => x.ExtensionName == req.Key).FirstOrDefault().License))
                                    {
                                        lincense = " - " + template.Extensions.Where(x => x.ExtensionName == req.Key).FirstOrDefault().License;
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
                            return Json(new { message = finalExtensionString, status = "false" });
                        }
                        else
                        {
                            var installedExtensions = dict.Where(x => x.Value == true).ToList();
                            if (installedExtensions.Count > 0)
                            {
                                requiresExtensionNames = "All required extensions are installed/enabled in your Azure DevOps Organization.<br/><br/><b>";
                                foreach (var ins in installedExtensions)
                                {
                                    string link = "<i class='fas fa-check-circle'></i> " + template.Extensions.Where(x => x.ExtensionName == ins.Key).FirstOrDefault().Link;
                                    string lincense = "";
                                    requiresExtensionNames = requiresExtensionNames + link + lincense + "<br/>";
                                }
                                return Json(new { message = requiresExtensionNames, status = "true" });
                            }
                        }

                    }
                    else { requiresExtensionNames = "no extensions required"; return Json(new { message = "no extensions required", status = "false" }); }
                    return Json(new { message = requiresExtensionNames, status = "false" });
                }
                else
                {
                    return Json(new { message = "Error", status = "false" });
                }

            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                return Json(new { message = "Error", status = "false" });
            }
        }

        [AllowAnonymous]
        public string CheckSession()
        {
            if (HttpContext.Session.GetString("GitHubToken") != null && HttpContext.Session.GetString("GitHubToken") != "")
            {
                return HttpContext.Session.GetString("GitHubToken");
            }
            else
            {
                return string.Empty;
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult UploadPrivateTemplateFromURL(string TemplateURL, string token, string userId, string password, string OldPrivateTemplate = "")
        {
            PrivateTemplate privateTemplate = new PrivateTemplate();
            if (HttpContext.Session.GetString("visited") != null)
            {
                if (!string.IsNullOrEmpty(OldPrivateTemplate))
                {
                    templateService.deletePrivateTemplate(OldPrivateTemplate);
                }
                string templatePath = string.Empty;
                try
                {
                    string templateName = "";
                    string fileName = Path.GetFileName(TemplateURL);
                    string extension = Path.GetExtension(TemplateURL);
                    templateName = fileName.ToLower().Replace(".zip", "").Trim() + "-" + Guid.NewGuid().ToString().Substring(0, 6) + extension.ToLower();
                    privateTemplate.PrivateTemplateName = templateName.ToLower().Replace(".zip", "").Trim();
                    privateTemplate.PrivateTemplatePath = templateService.GetTemplateFromPath(TemplateURL, templateName, token, userId, password);

                    if (privateTemplate.PrivateTemplatePath != "")
                    {
                        privateTemplate.ResponseMessage = templateService.checkSelectedTemplateIsPrivate(privateTemplate.PrivateTemplatePath);
                        if (privateTemplate.ResponseMessage != "SUCCESS")
                        {
                            var templatepath = HostingEnvironment.ContentRootPath + "/PrivateTemplates/" + templateName.ToLower().Replace(".zip", "").Trim();
                            if (Directory.Exists(templatepath))
                                Directory.Delete(templatepath, true);
                        }
                    }
                    else
                    {
                        privateTemplate.ResponseMessage = "Unable to download file, please check the provided URL";
                    }

                }
                catch (Exception ex)
                {
                    logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    return Json(new { message = "Error", status = "false" });
                }

            }
            return Json(privateTemplate);
        }

        [HttpPost]
        [AllowAnonymous]
        public void DeletePrivateTemplate(string TemplateName)
        {
            templateService.deletePrivateTemplate(TemplateName);
        }
    }

}

