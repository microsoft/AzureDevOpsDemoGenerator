﻿using log4net;
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
        private ILog logger = LogManager.GetLogger("ErrorLog");
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
                    var request = new HttpRequestMessage(method, _configuration.UriString + "/_apis/wiki/wikis?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
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
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateNewTeam" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
        public bool CreateUpdatePages(string jsonString, string projectName, string WikiId, string path)
        {
            try
            {
                //PUT https://dev.azure.com/{organization}/{project}/_apis/wiki/wikis/{wikiIdentifier}/pages?path={path}&api-version=4.1
                using (var client = GetHttpClient())
                {
                    var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

                    var method = new HttpMethod("PUT");
                    var request = new HttpRequestMessage(method, projectName + "/_apis/wiki/wikis/" + WikiId + "/pages?path=" + path + "&api-version=" + _configuration.VersionNumber) { Content = jsonContent };
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
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateNewTeam" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return false;
        }

        public bool MovePages(string jsonString, string projectName, string WikiId)
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

                    var method = new HttpMethod("POST");
                    var request = new HttpRequestMessage(method, projectName + "/_apis/wiki/wikis/" + WikiId + "/pagemoves?&api-version=" + _configuration.VersionNumber) { Content = jsonContent };
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
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateNewTeam" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return false;
        }

        public bool DeletePage(string projectName, string WikiId, string path)
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    var method = new HttpMethod("DELETE");
                    var request = new HttpRequestMessage(method, projectName + "/_apis/wiki/wikis/" + WikiId + "/pages?path=" + path + "&api-version=" + _configuration.VersionNumber);
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
                        return false;
                    }
                }
            }
            catch (Exception ex) 
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateNewTeam" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return false;
        }


        public bool CreateCodeWiki(string jsonString)
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    var json = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var method = new HttpMethod("POST");
                    var request = new HttpRequestMessage(method, string.Format("{0}/{1}/_apis/wiki/wikis?api-version={2}", _configuration.UriString, Project, _configuration.VersionNumber)) { Content = json };
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
                        return false;
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateNewTeam" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return false;
        }
    }
}
