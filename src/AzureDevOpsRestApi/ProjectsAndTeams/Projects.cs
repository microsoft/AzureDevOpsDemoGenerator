using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using AzureDevOpsAPI.Extractor;
using AzureDevOpsAPI.Viewmodel.Extractor;
using AzureDevOpsAPI.Viewmodel.ProjectAndTeams;
using Microsoft.ApplicationInsights;

namespace AzureDevOpsAPI.ProjectsAndTeams
{
    public class Projects : ApiServiceBase
    {
        private TelemetryClient ai;
        public Projects(IAppConfiguration configuration, TelemetryClient _ai) : base(configuration) { ai = _ai; }
         Logger logger = LogManager.GetLogger("*");
        /// <summary>
        /// Check for the existance of project
        /// </summary>
        /// <returns></returns>
        public bool IsAccountHasProjects()
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        // connect to the REST endpoint            
                        HttpResponseMessage response = client.GetAsync("_apis/projects?stateFilter=All&api-version=" + Configuration.VersionNumber).Result;
                        // check to see if we have a succesfull respond
                        return response.StatusCode == System.Net.HttpStatusCode.OK;
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return false;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return false;
        }

        /// <summary>
        /// Get List of project
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage GetListOfProjects()
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    ProjectsResponse.ProjectResult viewModel = new ProjectsResponse.ProjectResult();
                    using (var client = GetHttpClient())
                    {
                        // connect to the REST endpoint            
                        HttpResponseMessage response = client.GetAsync(Configuration.UriString + "/_apis/projects?stateFilter=wellFormed&$top=1000&api-version=" + Configuration.VersionNumber).Result;
                        // check to see if we have a succesfull respond
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug(ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new HttpResponseMessage(HttpStatusCode.InternalServerError); 
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Create team project
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public string CreateTeamProject(string json)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, "_apis/projects?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                        var response = client.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.Accepted)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            string projectId = JObject.Parse(result)["id"].ToString();
                            return projectId;
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = string.Empty;
                            if (response.StatusCode.ToString() == "Unauthorized")
                            {
                                error = errorMessage.Result;
                            }
                            else
                            {
                                error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            }
                            LastFailureMessage = error;
                            logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + error + "\n");
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Info(ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return "-1";
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return "-1";
        }

        /// <summary>
        /// Get Project id
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public string GetProjectIdByName(string projectName)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync("_apis/projects/" + projectName + "?includeCapabilities=false&api-version=" + Configuration.VersionNumber).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            string projectId = JObject.Parse(result)["id"].ToString();
                            return projectId;
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            this.LastFailureMessage = error;
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Info(ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return Guid.Empty.ToString();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return Guid.Empty.ToString();
        }

        /// <summary>
        /// Get project to know the status of the project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public string GetProjectStateByName(string project)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync("_apis/projects/" + project + "?includeCapabilities=true&api-version=" + Configuration.VersionNumber).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            string projectStatus = JObject.Parse(result)["state"].ToString();
                            return projectStatus;
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            this.LastFailureMessage = error;
                            retryCount++;
                        }
                    }
                }
                catch (TimeoutException timeout)
                {
                    ai.TrackException(timeout);
                    logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t Time out: " + timeout.Message + "\t" + "\n" + timeout.StackTrace + "\n");
                    LastFailureMessage = timeout.Message + " ," + timeout.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return string.Empty;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
                catch (OperationCanceledException opcan)
                {
                    ai.TrackException(opcan);
                    logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t Operation Cancelled: " + opcan.Message + "\t" + "\n" + opcan.StackTrace + "\n");
                    LastFailureMessage = opcan.Message + " ," + opcan.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return string.Empty;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Info(ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return string.Empty;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return string.Empty;
        }

        public ProjectProperties.Properties GetProjectProperties()
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    ProjectProperties.Properties load = new ProjectProperties.Properties();
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(Configuration.UriString + "/_apis/projects/" + ProjectId + "/properties?api-version=" + Configuration.VersionNumber).Result;
                        if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                        {
                            string res = response.Content.ReadAsStringAsync().Result;
                            load = JsonConvert.DeserializeObject<ProjectProperties.Properties>(res);
                            GetProcessTemplate.PTemplate template = new GetProcessTemplate.PTemplate();

                            string processTypeId = string.Empty;
                            var processType = load.Value.Where(x => x.Name == "System.ProcessTemplateType").FirstOrDefault();
                            if (processType != null)
                            {
                                processTypeId = processType.RefValue;
                            }
                            using (var client1 = GetHttpClient())
                            {
                                HttpResponseMessage response1 = client1.GetAsync(Configuration.UriString + "/_apis/work/processes/" + processTypeId + "?api-version=" + Configuration.VersionNumber).Result;
                                if (response1.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                                {
                                    string templateData = response1.Content.ReadAsStringAsync().Result;
                                    template = JsonConvert.DeserializeObject<GetProcessTemplate.PTemplate>(templateData);
                                    load.TypeClass = template.Properties.Class;
                                    load.Value.Where(x => x.Name == "System.Process Template").FirstOrDefault().RefValue = template.Name;
                                    return load;
                                }
                                else
                                {
                                    var errorMessage = response1.Content.ReadAsStringAsync();
                                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                                    this.LastFailureMessage = error;
                                    return new ProjectProperties.Properties();
                                }
                            }
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            this.LastFailureMessage = error;
                            retryCount++;
                        }

                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Info(ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new ProjectProperties.Properties();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new ProjectProperties.Properties();
        }
    }
}