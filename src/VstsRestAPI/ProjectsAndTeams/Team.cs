using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using VstsRestAPI.Viewmodel.ProjectAndTeams;

namespace VstsRestAPI.ProjectsAndTeams
{
    public class Team : ApiServiceBase
    {
        public Team(IConfiguration configuration) : base(configuration) { }

        /// <summary>
        /// Create teams
        /// </summary>
        /// <param name="json"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public GetTeamResponse.Team CreateNewTeam(string json, string project)
        {
            GetTeamResponse.Team viewModel = new GetTeamResponse.Team();

            using (var client = GetHttpClient())
            {
                // serialize the fields array into a json string  
                //var patchValue = new StringContent(JsonConvert.SerializeObject(team), Encoding.UTF8, "application/json");
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, client.BaseAddress + "/_apis/projects/" + project + "/teams?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    viewModel = response.Content.ReadAsAsync<GetTeamResponse.Team>().Result;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                }
            }
            return viewModel;
        }

        /// <summary>
        /// Get Team members
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="teamaName"></param>
        /// <returns></returns>
        public TeamMemberResponse.TeamMembers GetTeamMembers(string projectName, string teamaName)
        {
            TeamMemberResponse.TeamMembers viewModel = new TeamMemberResponse.TeamMembers();
            using (var client = GetHttpClient())
            {
                HttpResponseMessage response = client.GetAsync("_apis/projects/" + projectName + "/teams/" + teamaName + "/members/?api-version=" + _configuration.VersionNumber).Result;
                if (response.IsSuccessStatusCode)
                {
                    viewModel = response.Content.ReadAsAsync<TeamMemberResponse.TeamMembers>().Result;
                    return viewModel;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                    return new TeamMemberResponse.TeamMembers();
                }
            }
        }

        /// <summary>
        /// Create Area
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="areaName"></param>
        /// <returns></returns>
        public string CreateArea(string projectName, string areaName)
        {
            string createdAreaName = string.Empty;

            object node = new { name = areaName };

            using (var client = GetHttpClient())
            {
                // serialize the fields array into a json string  
                //var patchValue = new StringContent(JsonConvert.SerializeObject(team), Encoding.UTF8, "application/json");
                var jsonContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, projectName + "/_apis/wit/classificationNodes/areas?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    JObject jobj = JObject.Parse(result);
                    createdAreaName = jobj["name"].ToString();
                    return createdAreaName;
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

        /// <summary>
        /// Assign areas for teams
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="teamName"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public bool SetAreaForTeams(string projectName, string teamName, string json)
        {
            using (var client = GetHttpClient())
            {
                var patchValue = new StringContent(json, Encoding.UTF8, "application/json");

                var method = new HttpMethod("PATCH");

                var request = new HttpRequestMessage(method, projectName + "/" + teamName + "/_apis/work/teamsettings/teamfieldvalues?api-version=" + _configuration.VersionNumber) { Content = patchValue };
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                    return false;
                }
            }
        }

        /// <summary>
        /// Get team setting
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public string GetTeamSetting(string projectName)
        {
            TeamSettingResponse.TeamSetting viewModel = new TeamSettingResponse.TeamSetting();

            using (var client = GetHttpClient())
            {
                HttpResponseMessage response = client.GetAsync(projectName + "/_apis/work/teamsettings?api-version=" + _configuration.VersionNumber).Result;

                if (response.IsSuccessStatusCode)
                {
                    viewModel = response.Content.ReadAsAsync<TeamSettingResponse.TeamSetting>().Result;
                    return viewModel.backlogIteration.id;
                }
                else
                {
                    return string.Empty;
                }

            }
        }

        /// <summary>
        /// Set Iteration for team
        /// </summary>
        /// <param name="iterationId"></param>
        /// <param name="projectName"></param>
        /// <param name="teamName"></param>
        /// <returns></returns>
        public bool SetBackLogIterationForTeam(string iterationId, string projectName, string teamName)
        {
            object objJSON = new { Backlogiteration = iterationId };

            using (var client = GetHttpClient())
            {
                var postValue = new StringContent(JsonConvert.SerializeObject(objJSON), Encoding.UTF8, "application/json");

                var method = new HttpMethod("PATCH");

                var request = new HttpRequestMessage(method, projectName + "/" + teamName + "/_apis/work/teamsettings?api-version=" + _configuration.VersionNumber) { Content = postValue };
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                    return false;
                }
            }
        }

        /// <summary>
        /// Get All Iterations
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public TeamIterationsResponse.Iterations GetAllIterations(string projectName)
        {
            TeamIterationsResponse.Iterations viewModel = new TeamIterationsResponse.Iterations();

            using (var client = GetHttpClient())
            {
                HttpResponseMessage response = client.GetAsync(projectName + "/_apis/work/teamsettings/iterations?api-version=" + _configuration.VersionNumber).Result;

                if (response.IsSuccessStatusCode)
                {
                    viewModel = response.Content.ReadAsAsync<TeamIterationsResponse.Iterations>().Result;
                    return viewModel;
                }
                else
                {
                    return new TeamIterationsResponse.Iterations();
                }

            }
        }

        /// <summary>
        /// Set Iteration for team
        /// </summary>
        /// <param name="IterationId"></param>
        /// <param name="teamName"></param>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public bool SetIterationsForTeam(string IterationId, string teamName, string projectName)
        {
            object objJSON = new { id = IterationId };

            using (var client = GetHttpClient())
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(objJSON), Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, projectName + "/" + teamName + "/_apis/work/teamsettings/iterations?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                    return false;
                }
            }
        }

        /// <summary>
        /// Get Team details by Team name
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="teamaName"></param>
        /// <returns></returns>
        public TeamResponse GetTeamByName(string projectName, string teamaName)
        {
            TeamResponse viewModel = new TeamResponse();
            using (var client = GetHttpClient())
            {
                HttpResponseMessage response = client.GetAsync("_apis/projects/" + projectName + "/teams/" + teamaName + "?api-version=" + _configuration.VersionNumber).Result;
                if (response.IsSuccessStatusCode)
                {
                    viewModel = response.Content.ReadAsAsync<TeamResponse>().Result;
                    return viewModel;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                    return new TeamResponse();
                }
            }
        }
        /// <summary>
        /// Update team areas
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public bool UpdateTeamsAreas(string projectName, string json)
        {
            using (var client = GetHttpClient())
            {
                var patchValue = new StringContent(json, Encoding.UTF8, "application/json");

                var method = new HttpMethod("PATCH");

                var request = new HttpRequestMessage(method, _configuration.UriString + projectName + "/" + projectName + "%20Team/_apis/work/teamsettings/teamfieldvalues?api-version=" + _configuration.VersionNumber) { Content = patchValue };
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                    return false;
                }
            }
        }

    }
}