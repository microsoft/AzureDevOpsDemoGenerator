using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.Http;
using VstsDemoBuilder.Models;
using VstsDemoBuilder.ServiceInterfaces;
using VstsDemoBuilder.Services;
using VstsRestAPI.Viewmodel.ProjectAndTeams;
using VstsRestAPI.WorkItemAndTracking;

namespace VstsDemoBuilder.Controllers.Apis
{
    [RoutePrefix("api/environment")]
    public class ProjectController : ApiController
    {
        private ITemplateService templateService;
        private IProjectService projectService;
        public delegate string[] ProcessEnvironment(Project model);


        public ProjectController()
        {
            templateService = new TemplateService();
            projectService = new ProjectService();
        }

        [HttpPost]
        [Route("create")]
        public HttpResponseMessage create(MultiProjects model)
        {
            ProjectResponse returnObj = new ProjectResponse();
            returnObj.templatePath = model.templatePath;
            returnObj.templateName = model.templateName;
            List<RequestedProject> returnProjects = new List<RequestedProject>();
            try
            {

                string ReadErrorMessages = System.IO.File.ReadAllText(string.Format(HostingEnvironment.MapPath("~") + @"\JSON\" + @"{0}", "ErrorMessages.json"));
                var Messages = JsonConvert.DeserializeObject<Messages>(ReadErrorMessages);
                var errormessages = Messages.ErrorMessages;
                List<string> ListOfExistedProjects = new List<string>();
                //check for Organization Name
                if (string.IsNullOrEmpty(model.organizationName))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, errormessages.AccountMessages.InvalidAccountName); //"Provide a valid Account name"
                }
                //Check for AccessToken
                if (string.IsNullOrEmpty(model.accessToken))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, errormessages.AccountMessages.InvalidAccessToken ); //"Token of type Basic must be provided"
                }
                else
                {
                    HttpResponseMessage response = projectService.GetprojectList(model.organizationName, model.accessToken);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, errormessages.AccountMessages.CheckaccountDetails);
                    }
                    else
                    {
                        var projectResult = response.Content.ReadAsAsync<ProjectsResponse.ProjectResult>().Result;
                        foreach (var project in projectResult.value)
                        {
                            ListOfExistedProjects.Add(project.name); // insert list of existing projects in selected organiszation to dummy list
                        }
                    }
                }
                if (model.users.Count > 0)
                {
                    List<string> ListOfRequestedProjectNames = new List<string>();
                    foreach (var project in model.users)
                    {
                        //check for Email and Validate project name
                        if (!string.IsNullOrEmpty(project.email) && !string.IsNullOrEmpty(project.projectName))
                        {
                            string pattern = @"^(?!_)(?![.])[a-zA-Z0-9!^\-`)(]*[a-zA-Z0-9_!^\.)( ]*[^.\/\\~@#$*%+=[\]{\}'"",:;?<>|](?:[a-zA-Z!)(][a-zA-Z0-9!^\-` )(]+)?$";

                            bool isProjectNameValid = Regex.IsMatch(project.projectName, pattern);
                            List<string> restrictedNames = new List<string>() { "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "COM10", "PRN", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LTP", "LTP8", "LTP9", "NUL", "CON", "AUX", "SERVER", "SignalR", "DefaultCollection", "Web", "App_code", "App_Browesers", "App_Data", "App_GlobalResources", "App_LocalResources", "App_Themes", "App_WebResources", "bin", "web.config" };

                            if (!isProjectNameValid)
                            {
                                project.status = errormessages.ProjectMessages.InvalidProjectName; //"Invalid Project name";
                                return Request.CreateResponse(HttpStatusCode.BadRequest, project);
                            }
                            else if (restrictedNames.ConvertAll(d => d.ToLower()).Contains(project.projectName.Trim().ToLower()))
                            {
                                project.status = errormessages.ProjectMessages.ProjectNameWithReservedKeyword;//"Project name must not be a system-reserved name such as PRN, COM1, COM2, COM3, COM4, COM5, COM6, COM7, COM8, COM9, COM10, LPT1, LPT2, LPT3, LPT4, LPT5, LPT6, LPT7, LPT8, LPT9, NUL, CON, AUX, SERVER, SignalR, DefaultCollection, or Web";
                                return Request.CreateResponse(HttpStatusCode.BadRequest, project);
                            }
                            ListOfRequestedProjectNames.Add(project.projectName.ToLower());
                        }
                        else
                        {
                            project.status = errormessages.ProjectMessages.ProjectNameOrEmailID;//"EmailId or ProjectName is not found";
                            return Request.CreateResponse(HttpStatusCode.BadRequest, project);
                        }
                    }
                    //check for duplicatte project names from request body
                    bool anyDuplicateProjects = ListOfRequestedProjectNames.GroupBy(n => n).Any(c => c.Count() > 1);
                    if (anyDuplicateProjects)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, errormessages.ProjectMessages.DuplicateProject); //"ProjectName must be unique"
                    }
                    else
                    {
                        string templateName = string.Empty;
                        bool isPrivate = false;
                        if (string.IsNullOrEmpty(model.templateName) && string.IsNullOrEmpty(model.templatePath))
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, errormessages.TemplateMessages.TemplateNameOrTemplatePath); //"Please provide templateName or templatePath(GitHub)"
                        }
                        else
                        {
                            ProjectService.PrivateTemplatePath = "";
                            //check for Private template path provided in request body
                            if (!string.IsNullOrEmpty(model.templatePath))
                            {
                                string fileName = Path.GetFileName(model.templatePath);
                                string extension = Path.GetExtension(model.templatePath);
                                if (extension.ToLower() == ".zip")
                                {
                                    ProjectService.ExtractedTemplate = fileName.ToLower().Replace(".zip", "").Trim() + "-" + Guid.NewGuid().ToString().Substring(0, 6) + extension.ToLower();
                                    templateName = ProjectService.ExtractedTemplate;
                                    model.templateName = ProjectService.ExtractedTemplate.ToLower().Replace(".zip", "").Trim();
                                    //Get template  by extarcted the template from TemplatePath and returning boolean value for Valid template
                                    bool IsDownloadableTemplate = templateService.GetTemplateFromPath(model.templatePath, ProjectService.ExtractedTemplate, model.gitHubToken, model.userId, model.password);
                                    if (!IsDownloadableTemplate)
                                    {
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, errormessages.TemplateMessages.FailedTemplate);//"Failed to load the template from given template path. Check the repository URL and the file name.  If the repository is private then make sure that you have provided a GitHub token(PAT) in the request body"
                                    }
                                    else
                                    {
                                        isPrivate = true;
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, errormessages.TemplateMessages.PrivateTemplateFileExtension);//"TemplatePath should have .zip extension file name at the end of the url"
                                }
                            }
                            else
                            {
                                string response = templateService.GetTemplate(model.templateName);
                                if (response == "Template Not Found!")
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, errormessages.TemplateMessages.TemplateNotFound);
                                }
                                templateName = model.templateName;
                            }
                        }
                        //check for Extension file from selected template(public or private template)
                        string extensionJsonFile = projectService.GetJsonFilePath(isPrivate, ProjectService.PrivateTemplatePath, templateName, "Extensions.json");//string.Format(templatesFolder + @"{ 0}\Extensions.json", selectedTemplate);
                        if (File.Exists(extensionJsonFile))
                        {
                            //check for Extension installed or not from selected template in selected organization
                            if (projectService.CheckForInstalledExtensions(extensionJsonFile, model.accessToken, model.organizationName))
                            {
                                if (model.installExtensions)
                                {
                                    Project pmodel = new Project();
                                    pmodel.SelectedTemplate = model.templateName;
                                    pmodel.accessToken = model.accessToken;
                                    pmodel.accountName = model.organizationName;

                                    bool isextensionInstalled = projectService.InstallExtensions(pmodel, model.organizationName, model.accessToken);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, errormessages.ProjectMessages.ExtensionNotInstalled); //"Extension is not installed for the selected Template, Please provide IsExtensionRequired: true in the request body"
                                }
                            }
                        }
                        // continue to create project with async delegate method
                        foreach (var project in model.users)
                        {
                            var result = ListOfExistedProjects.ConvertAll(d => d.ToLower()).Contains(project.projectName.ToLower());
                            if (result == true)
                            {
                                project.status = project.projectName + " is already exist";
                            }
                            else
                            {
                                ProjectService.usercount++;
                                project.trackId = Guid.NewGuid().ToString().Split('-')[0];
                                project.status = "Project creation is initiated..";
                                Project pmodel = new Project();
                                pmodel.SelectedTemplate = model.templateName;
                                pmodel.accessToken = model.accessToken;
                                pmodel.accountName = model.organizationName;
                                pmodel.ProjectName = project.projectName;
                                pmodel.Email = project.email;
                                pmodel.id = project.trackId;
                                pmodel.IsApi = true;
                                if (model.templatePath != "")
                                    pmodel.IsPrivatePath = true;
                                ProcessEnvironment processTask = new ProcessEnvironment(projectService.CreateProjectEnvironment);
                                processTask.BeginInvoke(pmodel, new AsyncCallback(EndEnvironmentSetupProcess), processTask);
                            }
                            returnProjects.Add(project);
                        }
                        if (!string.IsNullOrEmpty(model.templatePath) && ProjectService.usercount == 0)
                        {
                            var templatepath = HostingEnvironment.MapPath("~") + @"\PrivateTemplates\" + ProjectService.ExtractedTemplate.ToLower().Replace(".zip", "").Trim();
                            if (Directory.Exists(templatepath))
                                Directory.Delete(templatepath, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t BulkProject \t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            returnObj.users = returnProjects;
            return Request.CreateResponse(HttpStatusCode.Accepted, returnObj);
        }

        [HttpGet]
        [Route("GetCurrentProgress")]
        public HttpResponseMessage GetCurrentProgress(string TrackId)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons

            var currentProgress = projectService.GetStatusMessage(TrackId);
            return Request.CreateResponse(HttpStatusCode.OK, currentProgress["status"]);
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
                        string logPath = HostingEnvironment.MapPath("~") + @"\Log";
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

                        errorMessages = errorMessages + Environment.NewLine + "TemplateUsed: " + ProjectService.templateUsed;
                        errorMessages = errorMessages + Environment.NewLine + "ProjectCreated : " + ProjectService.projectName;

                        ProjectService.logger.Error(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t  Error: " + errorMessages);

                        string logWIT = System.Configuration.ConfigurationManager.AppSettings["LogWIT"];
                        if (logWIT == "true")
                        {
                            objIssue.CreateIssueWI(patBase64, "4.1", url, issueName, errorMessages, projectId, "Demo Generator");
                        }
                    }
                }
                ProjectService.usercount--;
            }
            catch (Exception ex)
            {
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            finally
            {
                if (ProjectService.usercount == 0 && !string.IsNullOrEmpty(ProjectService.ExtractedTemplate))
                {
                    var templatepath = HostingEnvironment.MapPath("~") + @"\PrivateTemplates\" + ProjectService.ExtractedTemplate.ToLower().Replace(".zip", "").Trim();
                    if (Directory.Exists(templatepath))
                        Directory.Delete(templatepath, true);

                }
            }
        }
    }
}


