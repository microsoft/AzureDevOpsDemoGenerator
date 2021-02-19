using Newtonsoft.Json;
using NLog;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace AzureDevOpsAPI.Git
{
    public class GitHubImportRepo : ApiServiceBase
    {
        Logger logger = LogManager.GetLogger("*");
        public GitHubImportRepo(IAppConfiguration configuration) : base(configuration)
        {
        }

        public HttpResponseMessage GetUserDetail()
        {
            try
            {
                //https://api.github.com/user
                using (var client = GitHubHttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "demogenapi");
                    HttpResponseMessage response = client.GetAsync("/user").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
            }
            return new HttpResponseMessage();
        }

        public HttpResponseMessage ForkRepo(string repoName)
        {
            HttpResponseMessage res = new HttpResponseMessage();
            try
            {
                using (var client = GitHubHttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("User-Agent", Configuration.UserName);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                    var jsonContent = new StringContent("", Encoding.UTF8, "application/json");
                    var method = new HttpMethod("POST");
                    //repos/octocat/Hello-World/forks
                    var request = new HttpRequestMessage(method, $"repos/{repoName}/forks") { Content = jsonContent };
                    res = client.SendAsync(request).Result;
                }
            }
            catch (Exception ex)
            {
                logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
            }
            return res;
        }
        public HttpResponseMessage ListForks(string repoName)
        {
            HttpResponseMessage res = new HttpResponseMessage();
            try
            {
                using (var client = GitHubHttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", Configuration.UserName);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    var method = new HttpMethod("GET");
                    /// repos /:owner /:repo / forks
                    var request = $"repos/{repoName}/forks";
                    res = client.GetAsync(request).Result;
                }
            }
            catch (Exception ex)
            {
                logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
            }
            return res;
        }

        public HttpResponseMessage CreateRepo(string createRepoJson)
        {
            HttpResponseMessage res = new HttpResponseMessage();
            try
            {
                using (var client = GitHubHttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                    client.DefaultRequestHeaders.Add("User-Agent", Configuration.UserName);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    var newContent = new StringContent(createRepoJson, Encoding.UTF8, "application/vnd.github.v3+json");
                    /// repos /:owner /:repo / forks
                    var method = new HttpMethod("POST");
                    var request = new HttpRequestMessage(method, "user/repos") { Content = newContent };
                    var response = client.SendAsync(request).Result;
                }
            }
            catch (Exception ex)
            {
                logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
            }
            return res;
        }

        public HttpResponseMessage ImportRepo(string repoName, object importRepoObj)
        {
            try
            {
                string importRepoJson = JsonConvert.SerializeObject(importRepoObj);
                if (!string.IsNullOrEmpty(repoName) && !string.IsNullOrEmpty(importRepoJson))
                {
                    using (var client = GitHubHttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                        client.DefaultRequestHeaders.Add("User-Agent", Configuration.UserName);
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        var newContent = new StringContent(importRepoJson, Encoding.UTF8, "application/vnd.github.v3+json");
                        var method = new HttpMethod("PUT");
                        var request = new HttpRequestMessage(method, $"repos/{Configuration.UserName}/{repoName }/import") { Content = newContent };
                        var response = client.SendAsync(request).Result;
                        return response;
                    }
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
            }
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }

        public HttpResponseMessage GetImportStatus(string repoName)
        {
            try
            {
                ///repos/:owner/:repo/import
                if (!string.IsNullOrEmpty(repoName))
                {
                    using (var client = GitHubHttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Add("User-Agent", Configuration.UserName);
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        var request = $"{Configuration.GitBaseAddress}/repos/{Configuration.UserName}/{repoName }/import";
                        var response = client.GetAsync(request).Result;
                        return response;
                    }
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
            }
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);

        }
    }
}
