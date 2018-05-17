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
    public class PullRequests
    {

        readonly VstsRestAPI.IConfiguration _sourceConfig;
        readonly string _sourceCredentials;

        public PullRequests(VstsRestAPI.IConfiguration configuration)
        {
            _sourceConfig = configuration;
            _sourceCredentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _sourceConfig.PersonalAccessToken)));
        }
        /// <summary>
        /// method to export PullRequest jsons from source project
        /// </summary>
        /// <param name="project"></param>
        public void ExportPullRequests(string project)
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
                            foreach (var value in repository.value)
                            {
                                using (var clinetOne = new HttpClient())
                                {
                                    clinetOne.BaseAddress = new Uri(_sourceConfig.UriString);
                                    clinetOne.DefaultRequestHeaders.Accept.Clear();
                                    clinetOne.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                    clinetOne.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

                                    HttpResponseMessage pullRequestResponse = clinetOne.GetAsync(_sourceConfig.UriString + string.Format("/DefaultCollection/_apis/git/repositories/{0}/pullRequests?api-version=3.0", value.id)).Result;

                                    if(pullRequestResponse.IsSuccessStatusCode)
                                    {
                                        PullRequestResponse.PullRequest pullRequests = JsonConvert.DeserializeObject<PullRequestResponse.PullRequest>(pullRequestResponse.Content.ReadAsStringAsync().Result.ToString());
                                        int count = 1;
                                        if (pullRequests.count > 0)
                                        {
                                            foreach (var pullReq in pullRequests.value)
                                            {
                                                if (!Directory.Exists(@"Templates\PullRequests"))
                                                {
                                                    Directory.CreateDirectory(@"Templates\PullRequests");
                                                }
                                                foreach(var reviewer in pullReq.reviewers) { reviewer.id = "$reviewer$"; }

                                                string pullRequestJSON = JsonConvert.SerializeObject(pullReq, Formatting.Indented);
                                                string path = string.Format(@"Templates\PullRequests\{0}", value.name);
                                                if (!Directory.Exists(path))
                                                {
                                                    Directory.CreateDirectory(path);
                                                }
                                                System.IO.File.WriteAllText(path + @"\PullRequest" + count + ".json", pullRequestJSON);
                                                count = count + 1;
                                                using (var clientTwo = new HttpClient())
                                                {
                                                    clientTwo.BaseAddress = new Uri(_sourceConfig.UriString);
                                                    clientTwo.DefaultRequestHeaders.Accept.Clear();
                                                    clientTwo.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                                    clientTwo.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

                                                    HttpResponseMessage pullRequestComments = clientTwo.GetAsync(_sourceConfig.UriString + string.Format("/DefaultCollection/_apis/git/repositories/{0}/pullRequests/{1}/threads?api-version=3.0", value.id, pullReq.pullRequestId)).Result;

                                                    if(pullRequestComments.IsSuccessStatusCode)
                                                    {
                                                        if (!Directory.Exists(@"Templates\PullRequestComments"))
                                                        {
                                                            Directory.CreateDirectory(@"Templates\PullRequestComments");
                                                        }
                                                        PullRequestCommentResponse.Comments pullReqComments = JsonConvert.DeserializeObject<PullRequestCommentResponse.Comments>(pullRequestComments.Content.ReadAsStringAsync().Result.ToString());
                                                        
                                                        if (pullReqComments.count > 0)
                                                        {
                                                            string commentJSON = JsonConvert.SerializeObject(pullReqComments.value, Formatting.Indented);
                                                            string commentPath = string.Format(@"Templates\PullRequestComments\{0}", pullReq.title);
                                                            if (!Directory.Exists(commentPath))
                                                            {
                                                                Directory.CreateDirectory(commentPath);
                                                            }
                                                            System.IO.File.WriteAllText(commentPath + @"\Comment.json", commentJSON);
                                                            
                                                        }
                                                    }
                                                }
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
                Console.WriteLine("Error occured while generating pull request template: " + ex.Message);
                Console.WriteLine("");
            }
        }

    }
}
  