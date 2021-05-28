using Microsoft.ApplicationInsights;
using NLog;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace AzureDevOpsAPI.WorkItemAndTracking
{
    public class SwimLanes : ApiServiceBase
    {
        private TelemetryClient ai;
        public SwimLanes(IAppConfiguration configuration, TelemetryClient _ai) : base(configuration) { ai = _ai; }
         Logger logger = LogManager.GetLogger("*");
        /// <summary>
        /// Update swim lanes
        /// </summary>
        /// <param name="json"></param>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public bool UpdateSwimLanes(string json, string projectName, string boardType, string teamName)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        var patchValue = new StringContent(json, Encoding.UTF8, "application/json");
                        var method = new HttpMethod("PUT");

                        var request = new HttpRequestMessage(method, projectName + "/" + teamName + "/_apis/work/boards/" + boardType + "/rows?api-version=" + Configuration.VersionNumber) { Content = patchValue };
                        var response = client.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                        else
                        {
                            logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t Update Swimlanes \t" + response.Content.ReadAsStringAsync().Result);
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
    }
}


