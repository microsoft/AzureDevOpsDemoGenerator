using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using VstsRestAPI.Viewmodel.Wiki;

namespace VstsRestAPI.Wiki
{
    public class ManageWiki : ApiServiceBase
    {
        public ManageWiki(IConfiguration configuration) : base(configuration) { }

        /// <summary>
        /// Create wiki
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="projectID"></param>
        /// <returns></returns>
        public ProjectwikiResponse.Projectwiki CreateProjectWiki(string jsonString, string projectID)
        {
            try
            {
                ProjectwikiResponse.Projectwiki projectwiki = new ProjectwikiResponse.Projectwiki();
                using (var client = GetHttpClient())
                {
                    var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

                    var method = new HttpMethod("POST");
                    var request = new HttpRequestMessage(method, "/_apis/wiki/wikis?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
                    HttpResponseMessage response = client.SendAsync(request).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string res = response.Content.ReadAsStringAsync().Result;
                        projectwiki = JsonConvert.DeserializeObject<ProjectwikiResponse.Projectwiki>(res);
                        return projectwiki;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                        this.LastFailureMessage = error;
                    }
                }
            }
            catch (Exception)
            {

            }
            return new ProjectwikiResponse.Projectwiki();
        }

        /// <summary>
        /// Add project wiki pages
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="projectName"></param>
        /// <param name="WikiId"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool CreatePages(string jsonString, string projectName, string WikiId, string path)
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

                    var method = new HttpMethod("PUT");
                    var request = new HttpRequestMessage(method, projectName + "/_apis/wiki/wikis/" + WikiId + "/pages?path=/" + path + "&api-version=" + _configuration.VersionNumber) { Content = jsonContent };
                    HttpResponseMessage response = client.SendAsync(request).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string res = response.Content.ReadAsStringAsync().Result;
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
            catch (Exception)
            {

            }
            return false;
        }
    }
}
