using NLog;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using AzureDevOpsAPI.Viewmodel.ReleaseDefinition;

namespace AzureDevOpsAPI.Release
{
    public class ReleaseDefinition : ApiServiceBase
    {
        public ReleaseDefinition(IAppConfiguration configuration) : base(configuration) { }
        Logger logger = LogManager.GetLogger("*");
        /// <summary>
        /// Create Release Definition
        /// </summary>
        /// <param name="json"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public (string releaseDefId, string releaseDefName) CreateReleaseDefinition(string json, string project)
        {
            string error = "";
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, project + "/_apis/release/definitions?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            return (JObject.Parse(result)["id"].ToString(), JObject.Parse(result)["name"].ToString());
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            this.LastFailureMessage = error;
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    error = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateReleaseDefinition" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n";
                    logger.Debug(error);

                    retryCount++;

                    if (retryCount > 4)
                    {
                        return ("eror", error);
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return ("eror", error);
        }
        public bool CreateRelease(string json, string project)
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var method = new HttpMethod("POST");

                    var request = new HttpRequestMessage(method, project + "_apis/release/releases?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
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
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateRelease" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return false;
        }
        public int[] GetEnvironmentIdsByName(string project, string definitionName, string environment1, string environment2)
        {
            int[] environmentIds = new int[2];
            try
            {
                string requestURL = string.Empty;
                using (var client = GetHttpClient())
                {
                    requestURL = string.Format("{0}/_apis/release/definitions?api-version=" + Configuration.VersionNumber, project);
                    HttpResponseMessage response = client.GetAsync(requestURL).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        ReleaseDefinitionsResponse.Release Definitions = Newtonsoft.Json.JsonConvert.DeserializeObject<ReleaseDefinitionsResponse.Release>(response.Content.ReadAsStringAsync().Result.ToString());

                        int requiredDefinitionId = Definitions.Value.Where(x => x.Name == definitionName).FirstOrDefault().Id;
                        using (var client1 = GetHttpClient())
                        {
                            requestURL = string.Format("{0}/_apis/release/definitions/{1}?api-version=" + Configuration.VersionNumber, project, requiredDefinitionId);
                            HttpResponseMessage ResponseDef = client1.GetAsync(requestURL).Result;
                            if (response.IsSuccessStatusCode)
                            {
                                ReleaseDefinitions.ReleaseDefinition DefinitionResult = Newtonsoft.Json.JsonConvert.DeserializeObject<ReleaseDefinitions.ReleaseDefinition>(ResponseDef.Content.ReadAsStringAsync().Result.ToString());
                                environmentIds[0] = DefinitionResult.Environments.Where(x => x.Name == environment1).FirstOrDefault().Id;
                                environmentIds[1] = DefinitionResult.Environments.Where(x => x.Name == environment2).FirstOrDefault().Id;
                                return environmentIds;
                            }
                            else
                            {
                                var errorMessage = response.Content.ReadAsStringAsync();
                                string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                                this.LastFailureMessage = error;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "GetEnvironmentIdsByName" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return environmentIds;
        }
    }
}
