using NLog;
using System;
using System.Net;
using System.Net.Http;
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
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("User-Agent", Configuration.UserName);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

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
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("User-Agent", Configuration.UserName);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
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
    }
}
