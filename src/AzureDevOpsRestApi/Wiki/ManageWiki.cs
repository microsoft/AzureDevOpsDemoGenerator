using NLog;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using AzureDevOpsAPI.Viewmodel.Wiki;

namespace AzureDevOpsAPI.Wiki
{
    public class ManageWiki : ApiServiceBase
    {
        public ManageWiki(IAppConfiguration configuration) : base(configuration) { }
         Logger logger = LogManager.GetLogger("*");
        /// <summary>
        /// Create wiki
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public ProjectwikiResponse.Projectwiki CreateProjectWiki(string jsonString, string projectId)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    ProjectwikiResponse.Projectwiki projectwiki = new ProjectwikiResponse.Projectwiki();
                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

                        var method = new HttpMethod("POST");
                        var request = new HttpRequestMessage(method, Configuration.UriString + "/_apis/wiki/wikis?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                        HttpResponseMessage response = client.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            projectwiki = JsonConvert.DeserializeObject<ProjectwikiResponse.Projectwiki>(result);
                            return projectwiki;
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
                    logger.Debug("CreateProjectWiki" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new ProjectwikiResponse.Projectwiki(); 
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new ProjectwikiResponse.Projectwiki();
        }

        /// <summary>
        /// Add project wiki pages
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="projectName"></param>
        /// <param name="wikiId"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool CreateUpdatePages(string jsonString, string projectName, string wikiId, string path)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    //PUT https://dev.azure.com/{organization}/{project}/_apis/wiki/wikis/{wikiIdentifier}/pages?path={path}&api-version=4.1
                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

                        var method = new HttpMethod("PUT");
                        var request = new HttpRequestMessage(method, projectName + "/_apis/wiki/wikis/" + wikiId + "/pages?path=" + path + "&api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                        HttpResponseMessage response = client.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            return true;
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
                    logger.Debug("CreateUpdatePages" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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

        public bool MovePages(string jsonString, string projectName, string wikiId)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

                        var method = new HttpMethod("POST");
                        var request = new HttpRequestMessage(method, projectName + "/_apis/wiki/wikis/" + wikiId + "/pagemoves?&api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                        HttpResponseMessage response = client.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            return true;
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
                    logger.Debug("MovePages" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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

        public bool DeletePage(string projectName, string wikiId, string path)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        var method = new HttpMethod("DELETE");
                        var request = new HttpRequestMessage(method, projectName + "/_apis/wiki/wikis/" + wikiId + "/pages?path=" + path + "&api-version=" + Configuration.VersionNumber);
                        HttpResponseMessage response = client.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            return true;
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
                    logger.Debug("DeletePage" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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


        public bool CreateCodeWiki(string jsonString)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        var json = new StringContent(jsonString, Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");
                        var request = new HttpRequestMessage(method, string.Format("{0}/{1}/_apis/wiki/wikis?api-version={2}", Configuration.UriString, Project, Configuration.VersionNumber)) { Content = json };
                        HttpResponseMessage response = client.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.Created)
                        {
                            return response.IsSuccessStatusCode;
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
                    logger.Debug("CreateCodeWiki" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
