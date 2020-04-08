using NLog;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using AzureDevOpsAPI.Viewmodel.Repository;

namespace AzureDevOpsAPI.Git
{
    public class Repository : ApiServiceBase
    {
        public Repository(IAppConfiguration configuration) : base(configuration) { }
        Logger logger = LogManager.GetLogger("*");
        /// <summary>
        /// Get Source Code from Git Hub
        /// </summary>
        /// <param name="json"></param>
        /// <param name="project"></param>
        /// <param name="repositoryId"></param>
        /// <returns></returns>
        public bool GetSourceCodeFromGitHub(string json, string project, string repositoryId)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, Configuration.UriString + project + "/_apis/git/repositories/" + repositoryId + "/importRequests?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            return response.IsSuccessStatusCode;
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            LastFailureMessage = error;
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("GetSourceCodeFromGitHub" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
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

        /// <summary>
        /// Delete the default repository
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public string GetRepositoryToDelete(string project)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    GetAllRepositoriesResponse.Repositories viewModel = new GetAllRepositoriesResponse.Repositories();
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(project + "/_apis/git/repositories?api-version=" + Configuration.VersionNumber).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            viewModel = response.Content.ReadAsAsync<GetAllRepositoriesResponse.Repositories>().Result;
                            string repository = viewModel.Value.Where(x => x.Name == project).FirstOrDefault().Id;
                            return repository;
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            LastFailureMessage = error;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("GetRepositoryToDelete" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return string.Empty;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return string.Empty;
        }

        /// <summary>
        ///Get Default repository to delete 
        /// </summary>
        /// <param name="repoName"></param>
        /// <returns></returns>
        public string[] GetDefaultRepository(string repoName)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    string[] repo = new string[2];
                    GetAllRepositoriesResponse.Repositories viewModel = new GetAllRepositoriesResponse.Repositories();
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(repoName + "/_apis/git/repositories?api-version=" + Configuration.VersionNumber).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            viewModel = response.Content.ReadAsAsync<GetAllRepositoriesResponse.Repositories>().Result;
                            if (viewModel.Count > 0)
                            {
                                repo[0] = viewModel.Value.FirstOrDefault().Id;
                                repo[1] = viewModel.Value.FirstOrDefault().Name;
                            }
                            return repo;
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
                    logger.Debug("GetDefaultRepository" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n"); logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "GetDefaultRepository" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new string[] { };
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new string[] { };
        }

        /// <summary>
        /// Get list of Repositories
        /// </summary>
        /// <returns></returns>
        public GetAllRepositoriesResponse.Repositories GetAllRepositories()
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    GetAllRepositoriesResponse.Repositories viewModel = new GetAllRepositoriesResponse.Repositories();
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync("/_apis/git/repositories?api-version=" + Configuration.VersionNumber).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            viewModel = response.Content.ReadAsAsync<GetAllRepositoriesResponse.Repositories>().Result;
                            return viewModel;
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
                    logger.Debug("GetAllRepositories" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new GetAllRepositoriesResponse.Repositories();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new GetAllRepositoriesResponse.Repositories();
        }

        /// <summary>
        /// Creates Repository
        /// </summary>
        /// <param name="name"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public string[] CreateRepository(string name, string projectId)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    string[] repository = new string[2];
                    dynamic objJson = new System.Dynamic.ExpandoObject();
                    objJson.name = name;
                    objJson.project = new System.Dynamic.ExpandoObject();
                    objJson.project.id = projectId;
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(objJson);
                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, Configuration.UriString + "/_apis/git/repositories?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            var responseDetails = response.Content.ReadAsStringAsync().Result;
                            JObject objResponse = JObject.Parse(responseDetails);
                            repository[0] = objResponse["id"].ToString();
                            repository[1] = objResponse["name"].ToString();
                            return repository;
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
                    logger.Debug("CreateRepository" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");

                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new string[] { };
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new string[] { };
        }

        /// <summary>
        /// Delete repository
        /// </summary>
        /// <param name="repositoryId"></param>
        /// <returns></returns>
        public bool DeleteRepository(string repositoryId)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        var method = new HttpMethod("DELETE");
                        var request = new HttpRequestMessage(method, Configuration.UriString + Project + "/_apis/git/repositories/" + repositoryId + "?api-version=" + Configuration.VersionNumber);
                        var response = client.SendAsync(request).Result;

                        return response.IsSuccessStatusCode;
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("DeleteRepository" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
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

        /// <summary>
        /// Create Pull Request
        /// </summary>
        /// <param name="json"></param>
        /// <param name="repositoryId"></param>
        /// <returns></returns>
        public (string pullRequestId, string title) CreatePullRequest(string json, string repositoryId)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    string[] pullRequest = new string[2];

                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, Project + "/_apis/git/repositories/" + repositoryId + "/pullRequests?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            var responseDetails = response.Content.ReadAsStringAsync().Result;
                            JObject objResponse = JObject.Parse(responseDetails);
                            string pullRequestId = objResponse["pullRequestId"].ToString();
                            string title = objResponse["title"].ToString();
                            return (pullRequestId, title);
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
                    logger.Debug("CreatePullRequest" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return ("", "");
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return ("", "");
        }

        /// <summary>
        /// Create Comment thread for pull request
        /// </summary>
        /// <param name="repositorId"></param>
        /// <param name="pullRequestId"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public string CreateCommentThread(string repositorId, string pullRequestId, string json)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, Project + "/_apis/git/repositories/" + repositorId + "/pullRequests/" + pullRequestId + "/threads?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            var responseDetails = response.Content.ReadAsStringAsync().Result;
                            JObject objResponse = JObject.Parse(responseDetails);
                            string id = objResponse["id"].ToString();
                            return id;
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
                    logger.Debug("CreateCommentThread" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return string.Empty;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Add Comment thread
        /// </summary>
        /// <param name="repositorId"></param>
        /// <param name="pullRequestId"></param>
        /// <param name="threadId"></param>
        /// <param name="json"></param>
        public void AddCommentToThread(string repositorId, string pullRequestId, string threadId, string json)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, Project + "/_apis/git/repositories/" + repositorId + "/pullRequests/" + pullRequestId + "/threads/" + threadId + "/comments?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            var responseDetails = response.Content.ReadAsStringAsync().Result;
                            JObject objResponse = JObject.Parse(responseDetails);
                        }
                        else
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
                    logger.Debug("AddCommentToThread" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
        }
    }
}