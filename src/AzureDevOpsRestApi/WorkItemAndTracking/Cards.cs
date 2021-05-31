using NLog;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using AzureDevOpsAPI.Viewmodel.WorkItem;
using Microsoft.ApplicationInsights;

namespace AzureDevOpsAPI.WorkItemAndTracking
{
    public class Cards : ApiServiceBase
    {
        private TelemetryClient ai;
        public Cards(IAppConfiguration configuration, TelemetryClient _ai) : base(configuration) { ai = _ai; }
         Logger logger = LogManager.GetLogger("*");
        /// <summary>
        /// Update Card fields
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="json"></param>

        public void UpdateCardField(string projectName, string json, string boardType, string teamName)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    json = json.Replace("null", "\"\"");
                    using (var client = GetHttpClient())
                    {
                        StringContent patchValue = new StringContent("");
                        patchValue = new StringContent(json, Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call

                        var method = new HttpMethod("PUT");
                        string boardUrl = Configuration.UriString + projectName + "/" + teamName + "/_apis/work/boards/" + boardType + "/cardsettings?api-version=" + Configuration.VersionNumber;
                        var request = new HttpRequestMessage(method, boardUrl) { Content = patchValue };
                        var response = client.SendAsync(request).Result;
                        
                        if(!response.IsSuccessStatusCode)
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            this.LastFailureMessage = error;
                            retryCount++;
                        }

                        return;
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug("UpdateCardField" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
        }
        /// <summary>
        /// Apply rules to cards
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="json"></param>

        public void ApplyRules(string projectName, string json, string boardType, string teamName)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    json = json.Replace("null", "\"\"");
                    json = json.Replace("$ProjectName$", projectName);
                    CardStylesPatch.ListofCardStyles cardStyles = JsonConvert.DeserializeObject<CardStylesPatch.ListofCardStyles>(json);
                    if (cardStyles.Rules.Message == null)
                    {
                        cardStyles.Rules.Message = "test";
                    }
                    using (var client = GetHttpClient())
                    {
                        var patchValue = new StringContent(JsonConvert.SerializeObject(cardStyles), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
                        var method = new HttpMethod("PATCH");
                        string boardUrl = "https://dev.azure.com/" + Account + "/" + projectName + "/" + teamName + "/_apis/work/boards/" + boardType + "/cardrulesettings?api-version=" + Configuration.VersionNumber;
                        var request = new HttpRequestMessage(method, boardUrl) { Content = patchValue };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            return;
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
                    logger.Debug("ApplyRules" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }

            return;
        }

        /// <summary>
        /// Enable Epic
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="json"></param>
        /// <param name="project"></param>
        public void EnablingEpic(string projectName, string json, string project, string team)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                        var method = new HttpMethod("PATCH");
                        string teamName = projectName + " Team";
                        var request = new HttpRequestMessage(method, project + "/" + teamName + "/_apis/work/teamsettings?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            return;
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
                    logger.Debug("EnablingEpic" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }

            return;
        }
    }
}
