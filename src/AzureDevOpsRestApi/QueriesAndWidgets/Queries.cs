using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using AzureDevOpsAPI.Viewmodel.Extractor;
using AzureDevOpsAPI.Viewmodel.QueriesAndWidgets;

namespace AzureDevOpsAPI.QueriesAndWidgets
{
    public class Queries : ApiServiceBase
    {
        public Queries(IAppConfiguration configuration) : base(configuration) { }
        Logger logger = LogManager.GetLogger("*");

        /// <summary>
        /// Get Existing Dashboard by ID
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public string GetDashBoardId(string projectName)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    string dashBoardId = string.Empty;
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(projectName + "/" + projectName + "%20Team/_apis/dashboard/dashboards?api-version=" + Configuration.VersionNumber).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            DashboardResponse.Dashboard dashBoard = response.Content.ReadAsAsync<DashboardResponse.Dashboard>().Result;
                            if (dashBoard.DashboardEntries.Length >= 0)
                            {
                                dashBoardId = dashBoard.DashboardEntries[0].Id;
                            }
                            return dashBoardId;
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
                    logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
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
        /// Create Query in shared query folder
        /// </summary>
        /// <param name="project"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public QueryResponse CreateQuery(string project, string json)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    QueryResponse result = new QueryResponse();
                    using (var clientParent = GetHttpClient())
                    {
                        //Since we were getting errors like "you do not have access to shared query folder", based on MS team guidence added GET call before POST request
                        //Adding delay to generate Shared Query model in Azure DevOps
                        HttpResponseMessage responseParent = clientParent.GetAsync(project + "/_apis/wit/queries?api-version=" + Configuration.VersionNumber).Result;
                        Thread.Sleep(2000);
                        if (responseParent.IsSuccessStatusCode && responseParent.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            using (var client = GetHttpClient())
                            {
                                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                                var method = new HttpMethod("POST");

                                var request = new HttpRequestMessage(method, project + "/_apis/wit/queries/Shared%20Queries?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
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
                                    retryCount++;
                                }
                            }
                        }
                    }
                    return result;
                }
                catch (OperationCanceledException opr)
                {
                    logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t OperationCanceledException: " + opr.Message + "\n" + opr.StackTrace + "\n");
                    LastFailureMessage = opr.Message + " ," + opr.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new QueryResponse();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
                catch (Exception ex)
                {
                    logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new QueryResponse();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new QueryResponse();
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
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        var patchValue = new StringContent(json, Encoding.UTF8, "application/json");
                        var method = new HttpMethod("PATCH");

                        var request = new HttpRequestMessage(method, string.Format("{0}/_apis/wit/queries/{1}?api-version=" + Configuration.VersionNumber, project, query)) { Content = patchValue };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                        else
                        {
                            string msg = response.Content.ReadAsStringAsync().Result;
                            logger.Debug(msg + "\n");
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
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
        /// Get Query by path and Query Name
        /// </summary>
        /// <param name="project"></param>
        /// <param name="queryName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public QueryResponse GetQueryByPathAndName(string project, string queryName, string path)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(project + "/_apis/wit/queries/" + path + "/" + queryName + "?api-version=" + Configuration.VersionNumber).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            QueryResponse query = response.Content.ReadAsAsync<QueryResponse>().Result;
                            return query;
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
                    logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new QueryResponse();
                    }

                    Thread.Sleep(retryCount * 1000);
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
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    if (dashBoardId != "")
                    {
                        using (var client = GetHttpClient())
                        {
                            var method = new HttpMethod("DELETE");
                            var request = new HttpRequestMessage(method, project + "/" + project + "%20Team/_apis/dashboard/dashboards/" + dashBoardId + "?api-version=" + Configuration.VersionNumber);
                            var response = client.SendAsync(request).Result;

                            if (response.IsSuccessStatusCode)
                            {
                                return true;
                            }
                            else
                            {
                                dynamic responseForInvalidStatusCode = response.Content.ReadAsAsync<dynamic>();
                                Newtonsoft.Json.Linq.JContainer msg = responseForInvalidStatusCode.Result;
                                retryCount++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
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
        /// Create new dashboard
        /// </summary>
        /// <param name="project"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public string CreateNewDashBoard(string project, string json)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        //var jsonContent = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
                        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, project + "/" + project + "%20Team/_apis/dashboard/dashboards?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
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
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
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

        public GetQueries.Queries GetQueriesWiql()
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    //https://dev.azure.com/balajida/sss12/_apis/wit/queries?$expand=wiql&$depth=2&api-version=4.1
                    using (var client = GetHttpClient())
                    {
                        string request = string.Format("{0}{1}/_apis/wit/queries?$expand=wiql&$depth=2&{2}", Configuration.UriString, Project, Configuration.VersionNumber);
                        HttpResponseMessage response = client.GetAsync(request).Result;
                        if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string res = response.Content.ReadAsStringAsync().Result;
                            GetQueries.Queries getQueries = JsonConvert.DeserializeObject<GetQueries.Queries>(res);
                            return getQueries;
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            LastFailureMessage = error;
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new GetQueries.Queries();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new GetQueries.Queries();
        }
    }
}
