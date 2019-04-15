using log4net;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using VstsRestAPI.Viewmodel.WorkItem;

namespace VstsRestAPI.WorkItemAndTracking
{
    public class Cards : ApiServiceBase
    {
        public Cards(IConfiguration configuration) : base(configuration) { }
        private ILog logger = LogManager.GetLogger("ErrorLog");
        /// <summary>
        /// Update Card fields
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="json"></param>

        public void UpdateCardField(string projectName, string json, string boardType, string teamName)
        {
            try
            {
                json = json.Replace("null", "\"\"");
                using (var client = GetHttpClient())
                {
                    StringContent patchValue = new StringContent("");
                    patchValue = new StringContent(json, Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call

                    var method = new HttpMethod("PUT");
                    string boardURL = _configuration.UriString + projectName + "/" + teamName + "/_apis/work/boards/" + boardType + "/cardsettings?api-version=" + _configuration.VersionNumber;
                    var request = new HttpRequestMessage(method, boardURL) { Content = patchValue };
                    var response = client.SendAsync(request).Result;
                    if (response.IsSuccessStatusCode)
                    {

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
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateNewTeam" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }

        }
        /// <summary>
        /// Apply rules to cards
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="json"></param>

        public void ApplyRules(string projectName, string json, string boardType, string teamName)
        {
            try
            {
                json = json.Replace("null", "\"\"");
                json = json.Replace("$ProjectName$", projectName);
                CardStylesPatch.ListofCardStyles cardStyles = JsonConvert.DeserializeObject<CardStylesPatch.ListofCardStyles>(json);
                if (cardStyles.rules.Message == null)
                {
                    cardStyles.rules.Message = "test";
                }
                using (var client = GetHttpClient())
                {
                    var patchValue = new StringContent(JsonConvert.SerializeObject(cardStyles), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
                    var method = new HttpMethod("PATCH");
                    string boardURL = "https://dev.azure.com/" + Account + "/" + projectName + "/" + teamName + "/_apis/work/boards/" + boardType + "/cardrulesettings?api-version=" + _configuration.VersionNumber;
                    var request = new HttpRequestMessage(method, boardURL) { Content = patchValue };
                    var response = client.SendAsync(request).Result;

                    if (response.IsSuccessStatusCode)
                    {
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
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateNewTeam" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
        }

        /// <summary>
        /// Enable Epic
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="json"></param>
        /// <param name="project"></param>
        public void EnablingEpic(string projectName, string json, string project, string team)
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var method = new HttpMethod("PATCH");
                    string teamName = projectName + " Team";
                    var request = new HttpRequestMessage(method, project + "/" + teamName + "/_apis/work/teamsettings?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
                    var response = client.SendAsync(request).Result;

                    if (response.IsSuccessStatusCode)
                    {

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
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateNewTeam" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
        }
    }
}
