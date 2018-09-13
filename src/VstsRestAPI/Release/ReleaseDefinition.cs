using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VstsRestAPI.Viewmodel;
using VstsRestAPI.Viewmodel.ReleaseDefinition;

namespace VstsRestAPI.Release
{
    public class ReleaseDefinition : ApiServiceBase
    {
        public ReleaseDefinition(IConfiguration configuration) : base(configuration) { }

        /// <summary>
        /// Create Release Definition
        /// </summary>
        /// <param name="json"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public string[] CreateReleaseDefinition(string json, string project)
        {
            string[] releaseDef = new string[2];
            using (var client = GetHttpClient())
            {
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, project + "/_apis/release/definitions?api-version=4.0-preview.3") { Content = jsonContent };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    releaseDef[0] = JObject.Parse(result)["id"].ToString();
                    releaseDef[1] = JObject.Parse(result)["name"].ToString();

                    return releaseDef;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                    return releaseDef;
                }
            }

        }
        public bool CreateRelease(string json, string project)
        {
            using (var client = GetHttpClient())
            {
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, project + "_apis/release/releases?api-version=" + _configuration.VersionNumber + "-preview.2") { Content = jsonContent };
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
                    return false;
                }
            }
        }
        public int[] GetEnvironmentIdsByName(string project, string definitionName, string environment1, string environment2)
        {
            int[] EnvironmentIds = new int[2];
            try
            {
                string requestURL = string.Empty;
                using (var client = GetHttpClient())
                {
                    requestURL = string.Format("{0}/_apis/release/definitions?api-version=3.0-preview.1", project);
                    HttpResponseMessage response = client.GetAsync(requestURL).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        ReleaseDefinitionsResponse.Release Definitions = Newtonsoft.Json.JsonConvert.DeserializeObject<ReleaseDefinitionsResponse.Release>(response.Content.ReadAsStringAsync().Result.ToString());

                        int requiredDefinitionId = Definitions.value.Where(x => x.name == definitionName).FirstOrDefault().id;
                        using (var client1 = new HttpClient())
                        {
                            client1.BaseAddress = new Uri(_configuration.UriString);
                            client1.DefaultRequestHeaders.Accept.Clear();
                            client1.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                            client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                            requestURL = string.Format("{0}/_apis/release/definitions/{1}?api-version=3.0-preview.1", project, requiredDefinitionId);
                            HttpResponseMessage ResponseDef = client1.GetAsync(requestURL).Result;
                            if (response.IsSuccessStatusCode)
                            {
                                ReleaseDefinitions.ReleaseDefinition DefinitionResult = Newtonsoft.Json.JsonConvert.DeserializeObject<ReleaseDefinitions.ReleaseDefinition>(ResponseDef.Content.ReadAsStringAsync().Result.ToString());
                                EnvironmentIds[0] = DefinitionResult.environments.Where(x => x.name == environment1).FirstOrDefault().id;
                                EnvironmentIds[1] = DefinitionResult.environments.Where(x => x.name == environment2).FirstOrDefault().id;
                                return EnvironmentIds;
                            }
                        }
                    }
                }
            }
            catch (Exception) { return EnvironmentIds; }
            return EnvironmentIds;
        }
    }
}   