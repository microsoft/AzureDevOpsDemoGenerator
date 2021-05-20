using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using AzureDevOpsAPI.Viewmodel.ProjectAndTeams;
using Microsoft.ApplicationInsights;

namespace AzureDevOpsAPI.ProjectsAndTeams
{
    public class Teams : ApiServiceBase
    {
        private TelemetryClient ai;
        public Teams(IAppConfiguration configuration, TelemetryClient _ai) : base(configuration) { ai = _ai; }
        Logger logger = LogManager.GetLogger("*");
        /// <summary>
        /// Create teams
        /// </summary>
        /// <param name="json"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public GetTeamResponse.Team CreateNewTeam(string json, string project)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (HttpClient client = GetHttpClient())
                    {
                        // serialize the fields array into a json string  
                        //var patchValue = new StringContent(JsonConvert.SerializeObject(team), Encoding.UTF8, "application/json");
                        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, client.BaseAddress + "/_apis/projects/" + project + "/teams?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                        var response = client.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            GetTeamResponse.Team viewModel = new GetTeamResponse.Team();
                            viewModel = response.Content.ReadAsAsync<GetTeamResponse.Team>().Result;
                            return viewModel;
                        }
                        else
                        {
                            logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t CreateNewTeam \t" + response.Content.ReadAsStringAsync().Result);
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            LastFailureMessage = error;
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug("CreateNewTeam" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new GetTeamResponse.Team();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new GetTeamResponse.Team();
        }

        /// <summary>
        /// Get Team members
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="teamaName"></param>
        /// <returns></returns>
        public TeamMemberResponse.TeamMembers GetTeamMembers(string projectName, string teamaName)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (HttpClient client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync("_apis/projects/" + projectName + "/teams/" + teamaName + "/members/?api-version=" + Configuration.VersionNumber).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            TeamMemberResponse.TeamMembers viewModel = new TeamMemberResponse.TeamMembers();
                            viewModel = response.Content.ReadAsAsync<TeamMemberResponse.TeamMembers>().Result;
                            return viewModel;
                        }
                        else
                        {
                            logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t GetTeamMembers \t" + response.Content.ReadAsStringAsync().Result);
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
                    logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t GetTeamMembers \t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new TeamMemberResponse.TeamMembers();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new TeamMemberResponse.TeamMembers();
        }

        /// <summary>
        /// Create Area
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="areaName"></param>
        /// <returns></returns>
        public string CreateArea(string projectName, string areaName)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    object node = new { name = areaName };
                    using (HttpClient client = GetHttpClient())
                    {
                        // serialize the fields array into a json string  
                        //var patchValue = new StringContent(JsonConvert.SerializeObject(team), Encoding.UTF8, "application/json");
                        var jsonContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, projectName + "/_apis/wit/classificationNodes/areas?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            string createdAreaName = string.Empty;
                            string result = response.Content.ReadAsStringAsync().Result;
                            JObject jobj = JObject.Parse(result);
                            createdAreaName = jobj["name"].ToString();
                            return createdAreaName;
                        }
                        else
                        {
                            logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t CreateArea \t" + response.Content.ReadAsStringAsync().Result);
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
                    logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t CreateArea \t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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

        /// <summary>
        /// Assign areas for teams
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="teamName"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public bool SetAreaForTeams(string projectName, string teamName, string json)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        var patchValue = new StringContent(json, Encoding.UTF8, "application/json");

                        var method = new HttpMethod("PATCH");

                        var request = new HttpRequestMessage(method, projectName + "/" + teamName + "/_apis/work/teamsettings/teamfieldvalues?api-version=" + Configuration.VersionNumber) { Content = patchValue };
                        var response = client.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                        else
                        {
                            logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t SetAreaForTeams \t" + response.Content.ReadAsStringAsync().Result);
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
                    logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t SetAreaForTeams \t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
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
        /// Get team setting
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public string GetTeamSetting(string projectName)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(projectName + "/_apis/work/teamsettings?api-version=" + Configuration.VersionNumber).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            TeamSettingResponse.TeamSetting viewModel = new TeamSettingResponse.TeamSetting();
                            viewModel = response.Content.ReadAsAsync<TeamSettingResponse.TeamSetting>().Result;
                            return viewModel.BacklogIteration.Id;
                        }
                        else
                        {
                            logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t SetAreaForTeams \t" + response.Content.ReadAsStringAsync().Result);
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t SetAreaForTeams \t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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

        /// <summary>
        /// Set Iteration for team
        /// </summary>
        /// <param name="iterationId"></param>
        /// <param name="projectName"></param>
        /// <param name="teamName"></param>
        /// <returns></returns>
        public bool SetBackLogIterationForTeam(string iterationId, string projectName, string teamName)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    object objJson = new { Backlogiteration = iterationId };
                    using (var client = GetHttpClient())
                    {
                        var postValue = new StringContent(JsonConvert.SerializeObject(objJson), Encoding.UTF8, "application/json");
                        var method = new HttpMethod("PATCH");
                        var request = new HttpRequestMessage(method, projectName + "/" + teamName + "/_apis/work/teamsettings?api-version=" + Configuration.VersionNumber) { Content = postValue };
                        var response = client.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                        else
                        {
                            logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t SetBackLogIterationForTeam \t" + response.Content.ReadAsStringAsync().Result);
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            LastFailureMessage = error;
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t SetBackLogIterationForTeam \t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
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
        /// Get All Iterations
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public TeamIterationsResponse.Iterations GetAllIterations(string projectName)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    TeamIterationsResponse.Iterations viewModel = new TeamIterationsResponse.Iterations();
                    using (var client = GetHttpClient())
                    {
                        //https://dev.azure.com/abcdcentralus/dp-05/_apis/wit/classificationnodes?$depth=1&api-version=5.0-preview.2
                        //HttpResponseMessage response = client.GetAsync(projectName + "/_apis/work/teamsettings/iterations?api-version=" + _configuration.VersionNumber).Result;
                        HttpResponseMessage response = client.GetAsync(projectName + "/_apis/wit/classificationnodes?$depth=1&api-version=5.0-preview.2").Result;
                        if (response.IsSuccessStatusCode)
                        {
                            viewModel = response.Content.ReadAsAsync<TeamIterationsResponse.Iterations>().Result;
                            return viewModel;
                        }
                        else
                        {
                            logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t GetAllIterations \t" + response.Content.ReadAsStringAsync().Result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t GetAllIterations \t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new TeamIterationsResponse.Iterations();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new TeamIterationsResponse.Iterations();
        }

        /// <summary>
        /// Set Iteration for team
        /// </summary>
        /// <param name="iterationId"></param>
        /// <param name="teamName"></param>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public bool SetIterationsForTeam(string iterationId, string teamName, string projectName)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    object objJson = new { id = iterationId };

                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(JsonConvert.SerializeObject(objJson), Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, Configuration.UriString + projectName + "/" + teamName + "/_apis/work/teamsettings/iterations?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
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
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t SetIterationsForTeam \t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
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
        /// Get Team details by Team name
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="teamaName"></param>
        /// <returns></returns>
        public TeamResponse GetTeamByName(string projectName, string teamaName)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    TeamResponse viewModel = new TeamResponse();
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync("_apis/projects/" + projectName + "/teams/" + teamaName + "?api-version=" + Configuration.VersionNumber).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            viewModel = response.Content.ReadAsAsync<TeamResponse>().Result;
                            return viewModel;
                        }
                        else
                        {
                            logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t GetTeamByName \t" + response.Content.ReadAsStringAsync().Result);
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
                    logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t GetTeamByName \t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new TeamResponse();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new TeamResponse();
        }
        /// <summary>
        /// Update team areas
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public bool UpdateTeamsAreas(string projectName, string json)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        var patchValue = new StringContent(json, Encoding.UTF8, "application/json");

                        var method = new HttpMethod("PATCH");

                        var request = new HttpRequestMessage(method, Configuration.UriString + projectName + "/" + projectName + "%20Team/_apis/work/teamsettings/teamfieldvalues?api-version=" + Configuration.VersionNumber) { Content = patchValue };
                        var response = client.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                        else
                        {
                            logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t GetTeamByName \t" + response.Content.ReadAsStringAsync().Result);
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            LastFailureMessage = error;
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t GetTeamByName \t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
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

    }
}