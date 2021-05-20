using NLog;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using AzureDevOpsRestApi.Viewmodel.Build;
using Microsoft.ApplicationInsights;

namespace AzureDevOpsAPI.Build
{
    public class BuildDefinition : ApiServiceBase
    {
        public BuildDefinition(IAppConfiguration configuration, TelemetryClient _ai) : base(configuration) { ai = _ai; }
        Logger logger = LogManager.GetLogger("*");
        private TelemetryClient ai;

        /// <summary>
        /// Create Build Definition
        /// </summary>
        /// <param name="json"></param>
        /// <param name="project"></param>
        /// <param name="selectedTemplate"></param>
        /// <returns></returns>
        public (string buildId, string buildName) CreateBuildDefinition(string json, string project, string selectedTemplate)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    BuildGetListofBuildDefinitionsResponse.Definitions viewModel = new BuildGetListofBuildDefinitionsResponse.Definitions();
                    using (var client = GetHttpClient())
                    {
                        string uuid = Guid.NewGuid().ToString();
                        uuid = uuid.Substring(0, 8);
                        json = json.Replace("$UUID$", uuid);

                        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        string uri = "";
                        uri = Configuration.UriString + project + "/_apis/build/definitions?api-version=" + Configuration.VersionNumber;
                        var request = new HttpRequestMessage(method, uri) { Content = jsonContent };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            string buildId = JObject.Parse(result)["id"].ToString();
                            string buildName = JObject.Parse(result)["name"].ToString();
                            return (buildId, buildName);
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync().Result;
                            string error = Utility.GeterroMessage(errorMessage.ToString());
                            this.LastFailureMessage = error;
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateBuildDefinition" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");

                    retryCount++;

                    if (retryCount > 4)
                    {
                        return ("error", json);
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return ("error", "");
        }

        /// <summary>
        /// Queue a build after provisioning project
        /// </summary>
        /// <param name="json"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public int QueueBuild(string json, string project)
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

                        var request = new HttpRequestMessage(method, project + "/_apis/build/builds?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                        var response = client.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            int buildId = int.Parse(JObject.Parse(result)["id"].ToString());

                            return buildId;
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
                    logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "QueueBuild" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return -1;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }

            return -1;
        }
    }
}