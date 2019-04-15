using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using VstsRestAPI.Viewmodel.Build;


namespace VstsRestAPI.Build
{
    public class BuildDefinition : ApiServiceBase
    {
        public BuildDefinition(IConfiguration configuration) : base(configuration) { }
        private ILog logger = LogManager.GetLogger("ErrorLog");
        /// <summary>
        /// Create Build Definition
        /// </summary>
        /// <param name="json"></param>
        /// <param name="project"></param>
        /// <param name="selectedTemplate"></param>
        /// <returns></returns>
        public string[] CreateBuildDefinition(string json, string project, string selectedTemplate)
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
                    uri = _configuration.UriString + project + "/_apis/build/definitions?api-version=" + _configuration.VersionNumber;
                    var request = new HttpRequestMessage(method, uri) { Content = jsonContent };
                    var response = client.SendAsync(request).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        string buildId = JObject.Parse(result)["id"].ToString();
                        string buildName = JObject.Parse(result)["name"].ToString();
                        return new string[] { buildId, buildName };
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                        this.LastFailureMessage = error;
                        return new string[] { };
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateBuildDefinition" + "\t" + ex.Message + "\t"   + "\n" + ex.StackTrace + "\n");
            }
            return new string[] { };
        }

        /// <summary>
        /// Queue a build after provisioning project
        /// </summary>
        /// <param name="json"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public int QueueBuild(string json, string project)
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var method = new HttpMethod("POST");

                    var request = new HttpRequestMessage(method, project + "/_apis/build/builds?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
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
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "QueueBuild" + "\t" + ex.Message + "\t"   + "\n" + ex.StackTrace + "\n");
            }
            return -1;
        }
    }
}