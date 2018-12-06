using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using VstsRestAPI.Extractor;
using VstsRestAPI.Viewmodel.Extractor;
using VstsRestAPI.Viewmodel.ProjectAndTeams;



namespace VstsRestAPI.ProjectsAndTeams
{
    public class Projects : ApiServiceBase
    {
        public Projects(IConfiguration configuration) : base(configuration) { }

        /// <summary>
        /// Check for the existance of project
        /// </summary>
        /// <returns></returns>
        public bool IsAccountHasProjects()
        {
            using (var client = GetHttpClient())
            {
                // connect to the REST endpoint            
                HttpResponseMessage response = client.GetAsync("_apis/projects?stateFilter=All&api-version=" + _configuration.VersionNumber).Result;
                // check to see if we have a succesfull respond
                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }
            // return false;
        }

        /// <summary>
        /// Get List of project
        /// </summary>
        /// <returns></returns>
        public ProjectsResponse.ProjectResult GetListOfProjects()
        {
            ProjectsResponse.ProjectResult viewModel = new ProjectsResponse.ProjectResult();
            using (var client = GetHttpClient())
            {
                // connect to the REST endpoint            
                HttpResponseMessage response = client.GetAsync(_configuration.UriString + "/_apis/projects?stateFilter=All&api-version=" + _configuration.VersionNumber).Result;
                // check to see if we have a succesfull respond
                if (response.IsSuccessStatusCode)
                {
                    // set the viewmodel from the content in the response
                    viewModel = response.Content.ReadAsAsync<ProjectsResponse.ProjectResult>().Result;
                }
                viewModel.HttpStatusCode = response.StatusCode;
                return viewModel;
            }
        }

        /// <summary>
        /// Create team project
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public string CreateTeamProject(string json)
        {
            using (var client = GetHttpClient())
            {
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, "_apis/projects?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
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
                    this.LastFailureMessage = error;
                    return "-1";
                }
            }
        }

        /// <summary>
        /// Get Project id
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public string GetProjectIdByName(string projectName)
        {
            using (var client = GetHttpClient())
            {
                HttpResponseMessage response = client.GetAsync("_apis/projects/" + projectName + "?includeCapabilities=false&api-version=" + _configuration.VersionNumber).Result;

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
                    return Guid.Empty.ToString();
                }
            }
        }

        /// <summary>
        /// Get project to know the status of the project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public string GetProjectStateByName(string project)
        {
            using (var client = GetHttpClient())
            {
                HttpResponseMessage response = client.GetAsync("_apis/projects/" + project + "?includeCapabilities=true&api-version=" + _configuration.VersionNumber).Result;

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
                    return string.Empty;
                }
            }
        }

        public ProjectProperties.Properties GetProjectProperties()
        {
            try
            {
                ProjectProperties.Properties load = new ProjectProperties.Properties();
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(_configuration.UriString + "/_apis/projects/" + _configuration.Project + "/properties?api-version=" + _configuration.VersionNumber).Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                    {
                        string res = response.Content.ReadAsStringAsync().Result;
                        load = JsonConvert.DeserializeObject<ProjectProperties.Properties>(res);
                        GetProcessTemplate.PTemplate template = new GetProcessTemplate.PTemplate();

                        string processTypeId = string.Empty;
                        var processTypeID = load.value.Where(x => x.name == "System.ProcessTemplateType").FirstOrDefault();
                        if (processTypeID != null)
                        {
                            processTypeId = processTypeID.value;
                        }
                        using (var client1 = GetHttpClient())
                        {
                            HttpResponseMessage response1 = client1.GetAsync(_configuration.UriString + "/_apis/work/processes/" + processTypeId + "?api-version=" + _configuration.VersionNumber).Result;
                            if (response1.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                            {
                                string templateData = response1.Content.ReadAsStringAsync().Result;
                                template = JsonConvert.DeserializeObject<GetProcessTemplate.PTemplate>(templateData);
                                load.TypeClass = template.properties.Class;
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
                    }

                }
            }
            catch (Exception ex)
            {
                this.LastFailureMessage = ex.Message;
            }
            return new ProjectProperties.Properties();
        }

    }
}