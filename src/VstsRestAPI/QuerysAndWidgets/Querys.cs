using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using VstsRestAPI.Viewmodel.QuerysAndWidgets;

namespace VstsRestAPI.QuerysAndWidgets
{
    public class Querys : ApiServiceBase
    {
        public Querys(IConfiguration configuration) : base(configuration) { }

        /// <summary>
        /// Get Existing Dashboard by ID
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public string GetDashBoardId(string projectName)
        {
            string dashBoardId = string.Empty;
            using (var client = GetHttpClient())
            {
                HttpResponseMessage response = client.GetAsync(projectName + "/" + projectName + "%20Team/_apis/dashboard/dashboards?api-version=" + _configuration.VersionNumber).Result;
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
                    this.LastFailureMessage = error;
                    return dashBoardId;
                }
            }
        }

        /// <summary>
        /// Create Query in shared query folder
        /// </summary>
        /// <param name="project"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public QueryResponse CreateQuery(string project, string json)
        {
            QueryResponse result = new QueryResponse();
            using (var clientParent = GetHttpClient())
            {
                ////Since we were getting errors like "you do not have access to shared query folder", based on MS team guidence added GET call before POST request
                HttpResponseMessage ResponseParent = clientParent.GetAsync(project + "/_apis/wit/queries?api-version=" + _configuration.VersionNumber).Result;
                Thread.Sleep(2000);
                ////Adding delay to generate Shared Query model in Azure DevOps
                if (ResponseParent.IsSuccessStatusCode && ResponseParent.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    using (var client = GetHttpClient())
                    {
                        //var jsonContent = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
                        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, project + "/_apis/wit/queries/Shared%20Queries/?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
                        var response = client.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            result = response.Content.ReadAsAsync<QueryResponse>().Result;
                            return result;
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            this.LastFailureMessage = error;
                            return new QueryResponse();
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Update existing query which are there in the Current Sprint folder
        /// </summary>
        /// <param name="query"></param>
        /// <param name="project"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public bool UpdateQuery(string query, string project, string json)
        {
            using (var client = GetHttpClient())
            {
                var patchValue = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("PATCH");

                var request = new HttpRequestMessage(method, string.Format("{0}/_apis/wit/queries/{1}?api-version=" + _configuration.VersionNumber, project, query)) { Content = patchValue };
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
        //public bool CreateWidget(string project, string dashBoardId, string json)
        //{
        //    using (var client = GetHttpClient())
        //    {
        //        //var jsonContent = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
        //        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
        //        var method = new HttpMethod("POST");

        //        var request = new HttpRequestMessage(method, project + "/_apis/dashboard/dashboards/" + dashBoardId + "/Widgets?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
        //        var response = client.SendAsync(request).Result;
        //        if (response.IsSuccessStatusCode)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            var errorMessage = response.Content.ReadAsStringAsync();
        //            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
        //            this.LastFailureMessage = error;
        //            return false;
        //        }
        //    }

        //}

        /// <summary>
        /// Get Query by path and Query Name
        /// </summary>
        /// <param name="project"></param>
        /// <param name="queryName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public QueryResponse GetQueryByPathAndName(string project, string queryName, string path)
        {

            using (var client = GetHttpClient())
            {
                HttpResponseMessage response = client.GetAsync(project + "/_apis/wit/queries/" + path + "/" + queryName + "?api-version=" + _configuration.VersionNumber).Result;

                if (response.IsSuccessStatusCode)
                {
                    QueryResponse query = response.Content.ReadAsAsync<QueryResponse>().Result;
                    return query;
                }
            }

            return new QueryResponse();
        }

        /// <summary>
        /// Delete default dashboard since new dasboard will be create with the same name
        /// </summary>
        /// <param name="project"></param>
        /// <param name="dashBoardId"></param>
        /// <returns></returns>
        public bool DeleteDefaultDashboard(string project, string dashBoardId)
        {
            using (var client = GetHttpClient())
            {
                var method = new HttpMethod("DELETE");
                var request = new HttpRequestMessage(method, project + "/" + project + "%20Team/_apis/dashboard/dashboards/" + dashBoardId + "?api-version=" + _configuration.VersionNumber);
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
            using (var client = GetHttpClient())
            {
                //var jsonContent = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, project + "/" + project + "%20Team/_apis/dashboard/dashboards?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
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
                    this.LastFailureMessage = error;
                    return string.Empty;
                }
            }
        }
    }
}
