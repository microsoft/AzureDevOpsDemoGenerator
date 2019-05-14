using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using VstsRestAPI;
using VstsRestAPI.ProjectsAndTeams;
using VstsRestAPI.Viewmodel.ProjectAndTeams;
using VstsRestAPI.WorkItemAndTracking;

namespace VstsDemoBuilder.Controllers.Apis
{
    [RoutePrefix("api/environment")]
    public class ProjectController : ApiController
    {
        private ITemplateService templateService;
        private IProjectService projectService;
        public delegate string[] ProcessEnvironment(Project model, bool IsAPI);

        public ProjectController()
        {
            templateService = new TemplateService();
            projectService = new ProjectService();
        }

        [HttpPost]
        [Route("create")]
        public HttpResponseMessage create(BulkData model)
        {
            ResponseObject returnObj = new ResponseObject();
            returnObj.templatePath = model.templatePath;
            returnObj.templateName = model.templateName;
            List<User> returnusers = new List<User>();
            try
            {
                List<string> ListOfExistedProjects = new List<string>();
                //check for Organization Name
                if (string.IsNullOrEmpty(model.organizationName))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Provide a valid Account name");
                }
                //Check for AccessToken
                if (string.IsNullOrEmpty(model.accessToken))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, "Token of type Basic must be provided");
                }
                else
                {
                    HttpResponseMessage response = projectService.GetprojectList(model.organizationName, model.accessToken);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return Request.CreateResponse(response.StatusCode);
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
                    foreach (var user in model.users)
                    {
                        //check for Email and Valida project name
                        if (!string.IsNullOrEmpty(user.email) && !string.IsNullOrEmpty(user.ProjectName))
                        {
                            string pattern = @"^(?!_)(?![.])[a-zA-Z0-9!^\-`)(]*[a-zA-Z0-9_!^\.)( ]*[^.\/\\~@#$*%+=[\]{\}'"",:;?<>|](?:[a-zA-Z!)(][a-zA-Z0-9!^\-` )(]+)?$";

                            bool isProjectNameValid = Regex.IsMatch(user.ProjectName, pattern);
                            List<string> restrictedNames = new List<string>() { "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "COM10", "PRN", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LTP", "LTP8", "LTP9", "NUL", "CON", "AUX", "SERVER", "SignalR", "DefaultCollection", "Web", "App_code", "App_Browesers", "App_Data", "App_GlobalResources", "App_LocalResources", "App_Themes", "App_WebResources", "bin", "web.config" };

                            if (!isProjectNameValid)
                            {
                                user.status = "Invalid Project name";
                                return Request.CreateResponse(HttpStatusCode.BadRequest, user);
                            }
                            else if (restrictedNames.ConvertAll(d => d.ToLower()).Contains(user.ProjectName.Trim().ToLower()))
                            {
                                user.status = "Project name must not be a system-reserved name such as PRN, COM1, COM2, COM3, COM4, COM5, COM6, COM7, COM8, COM9, COM10, LPT1, LPT2, LPT3, LPT4, LPT5, LPT6, LPT7, LPT8, LPT9, NUL, CON, AUX, SERVER, SignalR, DefaultCollection, or Web";
                                return Request.CreateResponse(HttpStatusCode.BadRequest, user);
                            }
                            ListOfRequestedProjectNames.Add(user.ProjectName.ToLower());
                        }
                        else
                        {
                            user.status = "EmailId or ProjectName is not found";
                            return Request.CreateResponse(HttpStatusCode.BadRequest, user);
                        }
                    }
                    //check for duplicatte project names from request body
                    bool anyDuplicateProjects = ListOfRequestedProjectNames.GroupBy(n => n).Any(c => c.Count() > 1);
                    if (anyDuplicateProjects)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "ProjectName must be unique");
                    }
                    else
                    {
                        string templateName = string.Empty;

                        if (string.IsNullOrEmpty(model.templateName))
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Template Name should not be empty");
                        }
                        else
                        {
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
                                    if (!templateService.GetTemplateFromPath(model.templatePath, ProjectService.ExtractedTemplate, model.GithubToken))
                                    {
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Failed to load the template from given template path and Check File is public or private, If Private please provide GithubToken in request body");
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "TemplatePath should have .zip extension file name at the end of the url");
                                }
                            }
                            else
                            {
                                string response = templateService.GetTemplate(model.templateName);
                                if (response == "Template Not Found!")
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Template Not Found!");
                                }
                                templateName = model.templateName;
                            }
                        }
                        //check for Extension file from selected template(public or private template)
                        string extensionJsonFile = projectService.GetJsonFilePath(ProjectService.PrivateTemplatePath, templateName, "Extensions.json");//string.Format(templatesFolder + @"{ 0}\Extensions.json", selectedTemplate);
                        if (File.Exists(extensionJsonFile))
                        {
                            //check for Extension installed or not from selected template in selected organization
                            if (projectService.CheckForInstalledExtensions(extensionJsonFile, model.accessToken, model.organizationName))
                            {
                                if (model.IsExtensionRequired)
                                {
                                    Project pmodel = new Project();
                                    pmodel.SelectedTemplate = model.templateName;
                                    pmodel.accessToken = model.accessToken;
                                    pmodel.accountName = model.organizationName;

                                    bool isextensionInstalled = projectService.InstallExtensions(pmodel, model.organizationName, model.accessToken);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Extension is not installed for the selected Template, Please provide IsrequiredExtension: true in the request body");
                                }
                            }
                        }
                        // continue to create project with async delegate method
                        foreach (var user in model.users)
                        {
                            var result = ListOfExistedProjects.ConvertAll(d => d.ToLower()).Contains(user.ProjectName.ToLower());
                            if (result == true)
                            {
                                user.status = user.ProjectName + " is already exist";
                            }
                            else
                            {
                                ProjectService.usercount++;
                                user.TrackId = Guid.NewGuid().ToString().Split('-')[0];
                                user.status = "Project creation is initiated..";
                                Project pmodel = new Project();
                                pmodel.SelectedTemplate = model.templateName;
                                pmodel.accessToken = model.accessToken;
                                pmodel.accountName = model.organizationName;
                                pmodel.ProjectName = user.ProjectName;
                                pmodel.Email = user.email;
                                pmodel.id = user.TrackId;
                                ProcessEnvironment processTask = new ProcessEnvironment(projectService.CreateProjectEnvironment);
                                processTask.BeginInvoke(pmodel, true, new AsyncCallback(EndEnvironmentSetupProcess), processTask);
                            }
                            returnusers.Add(user);
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
            returnObj.users = returnusers;
            return Request.CreateResponse(HttpStatusCode.Accepted, returnObj);
        }

        [HttpGet]
        [Route("CurrentProgress")]
        public HttpResponseMessage GetCurrentProgress(string id)
        {
            var currentProgress = projectService.GetStatusMessage(id);
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
