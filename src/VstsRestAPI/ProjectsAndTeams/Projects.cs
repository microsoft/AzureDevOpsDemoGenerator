using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
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
        public ListofProjectsResponse.Projects ListOfProjects()
        {
            ListofProjectsResponse.Projects viewModel = new ListofProjectsResponse.Projects();
            using (var client = GetHttpClient())
            {
                // connect to the REST endpoint            
                HttpResponseMessage response = client.GetAsync("_apis/projects?stateFilter=All&api-version=" + _configuration.VersionNumber).Result;
                // check to see if we have a succesfull respond
                if (response.IsSuccessStatusCode)
                {
                    // set the viewmodel from the content in the response
                    viewModel = response.Content.ReadAsAsync<ListofProjectsResponse.Projects>().Result;
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
                if (response.IsSuccessStatusCode)
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

    }
}