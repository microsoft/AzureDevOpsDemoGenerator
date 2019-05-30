using System.Net;
using System.Net.Http;
using System.Text;

namespace VstsRestAPI.Git
{
    public class GitHubImportRepo : ApiServiceBase
    {
        public GitHubImportRepo(IConfiguration configuration) : base(configuration)
        {
        }

        public HttpResponseMessage GetUserDetail()
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
            return new HttpResponseMessage();
        }

        public HttpResponseMessage ForkRepo(string repoName)
        {
            HttpResponseMessage res = new HttpResponseMessage();
            using (var client = GitHubHttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", _configuration.userName);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                var jsonContent = new StringContent("", Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");
                //repos/octocat/Hello-World/forks
                var request = new HttpRequestMessage(method, $"repos/{repoName}/forks") { Content = jsonContent };
                res = client.SendAsync(request).Result;
            }
            return res;
        }
        public HttpResponseMessage ListForks(string repoName)
        {
            HttpResponseMessage res = new HttpResponseMessage();
            using (var client = GitHubHttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", _configuration.userName);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var method = new HttpMethod("GET");
                /// repos /:owner /:repo / forks
                var request = $"repos/{repoName}/forks";
                res = client.GetAsync(request).Result;
            }
            return res;
        }
    }
}
