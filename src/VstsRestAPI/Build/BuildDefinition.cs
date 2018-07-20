using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VstsRestAPI.Viewmodel;
using VstsRestAPI.Viewmodel.Build;


namespace VstsRestAPI.Build
{
    public class BuildDefinition
    {
        public string lastFailureMessage;
        readonly IConfiguration _configuration;
        readonly string _credentials;

        public BuildDefinition(IConfiguration configuration)
        {
            _configuration = configuration;
            _credentials = configuration.PersonalAccessToken;//Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _configuration.PersonalAccessToken)));
        }
        /// <summary>
        /// Create Build Definition
        /// </summary>
        /// <param name="json"></param>
        /// <param name="project"></param>
        /// <param name="SelectedTemplate"></param>
        /// <returns></returns>
        public string[] CreateBuildDefinition(string json, string project, string SelectedTemplate)
        {
            BuildGetListofBuildDefinitionsResponse.Definitions viewModel = new BuildGetListofBuildDefinitionsResponse.Definitions();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");
                string uri = "";
                if (SelectedTemplate == "SmartHotel360")
                {
                    uri = _configuration.UriString + project + "/_apis/build/definitions?api-version=4.1-preview";
                }
                else
                {
                    uri = _configuration.UriString + project + "/_apis/build/definitions?api-version=" + _configuration.VersionNumber;
                }
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
                    this.lastFailureMessage = error;
                    return new string[] { string.Empty, string.Empty };
                }
            }
            // return -1;
        }

        /// <summary>
        /// Queue a build after provisioning project
        /// </summary>
        /// <param name="json"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public int QueueBuild(string json, string project)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, _configuration.UriString + project + "/_apis/build/builds?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
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
                    this.lastFailureMessage = error;
                    return -1;
                }
            }
        }
    }
}