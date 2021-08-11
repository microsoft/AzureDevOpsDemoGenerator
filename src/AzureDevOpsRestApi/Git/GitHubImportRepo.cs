using AzureDevOpsRestApi.Viewmodel.GitHub;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private TelemetryClient ai;
        public GitHubImportRepo(IAppConfiguration configuration, TelemetryClient _ai) : base(configuration)
        {
            ai = _ai;
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
                ai.TrackException(ex);
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
                ai.TrackException(ex);
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
                ai.TrackException(ex);
                logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
            }
            return res;
        }

        public HttpResponseMessage CreateRepo(string createRepoJson, string api)
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
                    var request = new HttpRequestMessage(method, api) { Content = newContent };
                    var response = client.SendAsync(request).Result;
                }
            }
            catch (Exception ex)
            {
                ai.TrackException(ex);
                logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
            }
            return res;
        }

        public HttpResponseMessage ImportRepo(object importRepoObj, string api)
        {
            try
            {
                // https://docs.github.com/en/rest/reference/migrations#start-an-import
                string importRepoJson = JsonConvert.SerializeObject(importRepoObj);
                if (!string.IsNullOrEmpty(api) && !string.IsNullOrEmpty(importRepoJson))
                {
                    using (var client = GitHubHttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                        client.DefaultRequestHeaders.Add("User-Agent", Configuration.UserName);
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        var newContent = new StringContent(importRepoJson, Encoding.UTF8, "application/vnd.github.v3+json");
                        var method = new HttpMethod("PUT");
                        var request = new HttpRequestMessage(method, api) { Content = newContent };
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
                ai.TrackException(ex);
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
                        var request = $"/repos/{Configuration.UserName}/{repoName }/import";
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
                ai.TrackException(ex);
                logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
            }
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);

        }

        public HttpResponseMessage GetRepositoryPublicKey(string repoName, string api)
        {
            HttpResponseMessage res = new HttpResponseMessage();
            try
            {
                if (!string.IsNullOrEmpty(repoName))
                {
                    using (var client = GitHubHttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Add("User-Agent", Configuration.UserName);
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                        // orgs/{org}/actions/secrets/public-key
                        // /repos/{Configuration.UserName}/{repoName}/actions/secrets/public-key

                        //var request = $"repos/{Configuration.UserName}/{repoName}/actions/secrets/public-key";
                        res = client.GetAsync(api).Result;
                        return res;
                    }
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                ai.TrackException(ex);
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\n" + ex.StackTrace + "\n");
            }
            return res;

        }
        public HttpResponseMessage EncryptAndAddSecret(string json, string api)
        {
            HttpResponseMessage res = new HttpResponseMessage();
            try
            {
                //var secretValue = System.Text.Encoding.UTF8.GetBytes(secret.secretValue);
                //var publicKey = Convert.FromBase64String(_publicKey.key);
                //var sealedPublicKeyBox = Sodium.SealedPublicKeyBox.Create(secretValue, publicKey);
                //var encryptedSecret = Convert.ToBase64String(sealedPublicKeyBox);

                using (var client = GitHubHttpClient())
                {
                    var httpMethod = new HttpMethod("PUT");

                    //dynamic obj = new JObject();
                    //obj.encrypted_value = encryptedSecret;
                    //obj.key_id = _publicKey.key_id;
                    //obj.visibility = secret.visibility;
                    //obj.selected_repository_ids = new JArray(secret.selected_repository_ids);
                    //var json = obj.ToString();
                    // https://docs.github.com/en/rest/reference/actions#create-or-update-an-organization-secret
                    // https://docs.github.com/en/rest/reference/actions#create-or-update-a-repository-secret
                    // https://docs.github.com/rest/reference/actions#create-or-update-an-organization-secret
                    // orgs/{org}/actions/secrets/{secret_name}
                    // /repos/{owner}/{repo}/actions/secrets/{secret_name}
                    var jsonContent = new StringContent(json, Encoding.UTF8, "application/vnd.github.v3+json");
                    client.DefaultRequestHeaders.Add("User-Agent", Configuration.UserName);
                    //HttpRequestMessage request = new HttpRequestMessage(httpMethod, string.Format("repos/{0}/{1}/actions/secrets/{2}", Configuration.UserName, repoName, secret.secretName)) { Content = jsonContent };
                    HttpRequestMessage request = new HttpRequestMessage(httpMethod, api) { Content = jsonContent };
                    res = client.SendAsync(request).Result;
                    return res;
                }
            }
            catch (Exception ex)
            {
                ai.TrackException(ex);
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\n" + ex.StackTrace + "\n");
            }
            return res;
        }

        public HttpResponseMessage SetBranchProtectionRule(dynamic _pRule, string repoName)
        {
            HttpResponseMessage res = new HttpResponseMessage();

            try
            {
                using (var client = GitHubHttpClient())
                {
                    // https://docs.github.com/en/rest/reference/repos#update-branch-protection
                    // PUT /repos/{owner}/{repo}/branches/{branch}/protection
                    var httpMethod = new HttpMethod("PUT");
                    client.DefaultRequestHeaders.Add("User-Agent", Configuration.UserName);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.luke-cage-preview+json"));
                    var jsonContent = new StringContent(_pRule.rule.ToString(), Encoding.UTF8, "application/vnd.github.v3+json");
                    HttpRequestMessage request = new HttpRequestMessage(httpMethod, string.Format("repos/{0}/{1}/branches/{2}/protection", Configuration.UserName, repoName, _pRule.branch)) { Content = jsonContent };

                    res = client.SendAsync(request).Result;

                }
            }
            catch (Exception ex)
            {
                ai.TrackException(ex);
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\n" + ex.StackTrace + "\n");
            }
            return res;
        }

        public HttpResponseMessage GetRepository(string user, string repoName)
        {
            try
            {
                // repos/{owner}/{repo}
                if (!string.IsNullOrEmpty(repoName))
                {
                    using (var client = GitHubHttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Add("User-Agent", Configuration.UserName);
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        var request = $"/repos/{user}/{repoName}";
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
                ai.TrackException(ex);
                logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
            }
            return default;
        }

    }
}
