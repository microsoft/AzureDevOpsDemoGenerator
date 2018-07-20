using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VstsRestAPI.Viewmodel.QuerysAndWidgets;

namespace VstsRestAPI.QuerysAndWidgets
{
    public class Querys
    {
        public string lastFailureMessage;
        readonly IConfiguration _configuration;
        readonly string _credentials;

        public Querys(IConfiguration configuration)
        {
            _configuration = configuration;
            _credentials = configuration.PersonalAccessToken;//Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _configuration.PersonalAccessToken)));
        }

        /// <summary>
        /// Get Dashboard by ID
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public string GetDashBoardId(string projectName)
        {
            string dashBoardId = string.Empty;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_configuration.UriString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                HttpResponseMessage response = client.GetAsync(projectName + "/" + projectName + "%20Team/_apis/dashboard/dashboards?api-version=4.1-preview.2").Result;
                if (response.IsSuccessStatusCode)
                {
                    DashboardResponse.Dashboard dashBoard = response.Content.ReadAsAsync<DashboardResponse.Dashboard>().Result;
                    dashBoardId = dashBoard.dashboardEntries[0].id;
                    return dashBoardId;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.lastFailureMessage = error;
                    return dashBoardId;
                }
            }
        }

        /// <summary>
        /// Get Dashboard eTag
        /// </summary>
        /// <param name="dashboardId"></param>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public string GetDashboardeTag(string dashboardId, string projectName)
        {
            string dashBoardeTag = string.Empty;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_configuration.UriString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                HttpResponseMessage response = client.GetAsync(projectName + "/" + projectName + "%20Team/_apis/Dashboard/Dashboards/" + dashboardId + "?api-version=" + _configuration.VersionNumber + "-preview.2").Result;
                if (response.IsSuccessStatusCode)
                {
                    DashBoardeTagResponse.Dashboard dashBoard = response.Content.ReadAsAsync<DashBoardeTagResponse.Dashboard>().Result;
                    dashBoardeTag = dashBoard.eTag.ToString();
                    return dashBoardeTag;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.lastFailureMessage = error;
                    return dashBoardeTag;
                }
            }
        }

        /// <summary>
        /// Create Query
        /// </summary>
        /// <param name="project"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public QueryResponse CreateQuery(string project, string json)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_configuration.UriString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                //var jsonContent = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, project + "/_apis/wit/queries/Shared%20Queries/?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    QueryResponse result = response.Content.ReadAsAsync<QueryResponse>().Result;
                    return result;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.lastFailureMessage = error;
                    return new QueryResponse();

                }
            }
        }

        /// <summary>
        /// Update existing query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="project"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public bool UpdateQuery(string query, string project, string json)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                var patchValue = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("PATCH");

                var request = new HttpRequestMessage(method, _configuration.UriString + string.Format("{0}/_apis/wit/queries/{1}?api-version=2.2", project, query)) { Content = patchValue };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Create widget for dashboard
        /// </summary>
        /// <param name="project"></param>
        /// <param name="dashBoardId"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public bool CreateWidget(string project, string dashBoardId, string json)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_configuration.UriString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                //var jsonContent = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, project + "/_apis/dashboard/dashboards/" + dashBoardId + "/Widgets?api-version=" + _configuration.VersionNumber + "-preview.2") { Content = jsonContent };
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.lastFailureMessage = error;
                    return false;
                }
            }

        }

        /// <summary>
        /// Get Query by path
        /// </summary>
        /// <param name="project"></param>
        /// <param name="queryName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public QueryResponse GetQueryByPathAndName(string project, string queryName, string path)
        {

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_configuration.UriString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                HttpResponseMessage response = client.GetAsync(project + "/_apis/wit/queries/" + path + "/" + queryName + "?api-version=2.2").Result;

                if (response.IsSuccessStatusCode)
                {
                    QueryResponse query = response.Content.ReadAsAsync<QueryResponse>().Result;
                    return query;
                }
            }

            return new QueryResponse();
        }

        /// <summary>
        /// Delete default dashboard
        /// </summary>
        /// <param name="project"></param>
        /// <param name="dashBoardId"></param>
        /// <returns></returns>
        public bool DeleteDefaultDashboard(string project, string dashBoardId)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                var method = new HttpMethod("DELETE");
                var request = new HttpRequestMessage(method, _configuration.UriString + project + "/" + project + "%20Team/_apis/dashboard/dashboards/" + dashBoardId + "?api-version=" + _configuration.VersionNumber + "-preview.2");
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    dynamic responseForInvalidStatusCode = response.Content.ReadAsAsync<dynamic>();
                    Newtonsoft.Json.Linq.JContainer msg = responseForInvalidStatusCode.Result;
                    return false;
                }
            }
        }

        /// <summary>
        /// Create new dashboard
        /// </summary>
        /// <param name="project"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public string CreateNewDashBoard(string project, string json)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_configuration.UriString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                //var jsonContent = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, project + "/" + project + "%20Team/_apis/dashboard/dashboards?api-version=" + _configuration.VersionNumber + "-preview.2") { Content = jsonContent };
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    var details = response.Content.ReadAsStringAsync().Result;
                    string dashBoardId = JObject.Parse(details)["id"].ToString();
                    return dashBoardId;

                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.lastFailureMessage = error;
                    return string.Empty;
                }
            }
        }
    }
}
