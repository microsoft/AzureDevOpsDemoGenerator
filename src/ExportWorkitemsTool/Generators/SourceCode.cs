using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TemplatesGeneratorTool.ViewModel;

namespace TemplatesGeneratorTool.Generators
{
    public class SourceCode
    {
        readonly VstsRestAPI.IConfiguration _sourceConfig;
        readonly string _sourceCredentials;

        public SourceCode(VstsRestAPI.IConfiguration configuration)
        {
            _sourceConfig = configuration;
            _sourceCredentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _sourceConfig.PersonalAccessToken)));
        }

        /// <summary>
        /// method to export source code url jsons
        /// </summary>
        /// <param name="project"></param>
        public void ExportSourceCode(string project)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_sourceConfig.UriString);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

                    HttpResponseMessage response = client.GetAsync(_sourceConfig.UriString + string.Format("/DefaultCollection/{0}/_apis/git/repositories?api-version=1.0", project)).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        RepositoryResponse.Repository repository = Newtonsoft.Json.JsonConvert.DeserializeObject<RepositoryResponse.Repository>(response.Content.ReadAsStringAsync().Result.ToString());
                        if (repository.count > 0)
                        {
                            int count = 1;
                            foreach (RepositoryResponse.Value repo in repository.value)
                            {
                                using (var client1 = new HttpClient())
                                {
                                    client1.BaseAddress = new Uri(_sourceConfig.UriString);
                                    client1.DefaultRequestHeaders.Accept.Clear();
                                    client1.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                    client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

                                    HttpResponseMessage responseCode = client1.GetAsync(_sourceConfig.UriString + string.Format("/DefaultCollection/{0}/_apis/git/repositories/{1}/importRequests?includeAbandoned=true&api-version=3.0-preview", project, repo.id)).Result;
                                    if (responseCode.IsSuccessStatusCode)
                                    {
                                        SourceCodeResponse.Code code = Newtonsoft.Json.JsonConvert.DeserializeObject<SourceCodeResponse.Code>(responseCode.Content.ReadAsStringAsync().Result.ToString());
                                        if (code.value != null)
                                        {
                                            if (!Directory.Exists(@"Templates\sourceCode"))
                                            {
                                                Directory.CreateDirectory(@"Templates\sourceCode");
                                            }
                                            string fetchedSourceCodeJSON = JsonConvert.SerializeObject(code.value, Formatting.Indented);
                                            File.WriteAllText(@"Templates\sourceCode\importSourceCode" + count + ".json", fetchedSourceCodeJSON);
                                            count = count + 1;
                                        }
                                    }
                                }
                                using (var client2 = new HttpClient())
                                {
                                    client2.BaseAddress = new Uri(_sourceConfig.UriString);
                                    client2.DefaultRequestHeaders.Accept.Clear();
                                    client2.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                    client2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

                                    HttpResponseMessage responsePullRequest = client2.GetAsync(_sourceConfig.UriString + string.Format("/DefaultCollection/_apis/git/repositories/{0}/pullRequests?api-version=3.0", repo.id)).Result;
                                    if (responsePullRequest.IsSuccessStatusCode)
                                    {
                                        PullRequestResponse.PullRequest pullRequests = JsonConvert.DeserializeObject<PullRequestResponse.PullRequest>(responsePullRequest.Content.ReadAsStringAsync().Result.ToString());
                                        if (pullRequests.count > 0)
                                        {
                                            foreach (PullRequestResponse.Value request in pullRequests.value)
                                            {
                                                if (!Directory.Exists(@"Templates\PullRequests\" + repo.name))
                                                {
                                                    Directory.CreateDirectory(@"Templates\PullRequests\" + repo.name);
                                                }
                                                string fetchedPullRequestJSON = JsonConvert.SerializeObject(request, Formatting.Indented);
                                                string path = string.Format(@"Templates\PullRequests\{0}\{1}.json", repo.name, request.title);
                                                File.WriteAllText(path, fetchedPullRequestJSON);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured while generating export sourcecode template: " + ex.Message);
                Console.WriteLine("");
            }
        }
    }
}
