using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using AzureDevOpsAPI.Viewmodel.Extractor;
using AzureDevOpsAPI.Viewmodel.Queue;

namespace AzureDevOpsAPI.Extractor
{
    public class BuildandReleaseDefs : ApiServiceBase
    {
        public BuildandReleaseDefs(IAppConfiguration configuration) : base(configuration) { }
         Logger logger = LogManager.GetLogger("*");
        //https://d2a2v2.visualstudio.com/selenium2/_apis/build/definitions?api-version=4.1
        // Get Build Definition count
        public GetBuildDefResponse.BuildDef GetBuildDefCount()
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(Configuration.UriString + "/" + Project + "/_apis/build/definitions?api-version=" + Configuration.VersionNumber).Result;
                        if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            GetBuildDefResponse.BuildDef getINum = JsonConvert.DeserializeObject<GetBuildDefResponse.BuildDef>(result);
                            return getINum;
                        }
                        else
                        {
                            string errorMessage = response.Content.ReadAsStringAsync().Result;
                            LastFailureMessage = errorMessage;
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new GetBuildDefResponse.BuildDef(); ;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new GetBuildDefResponse.BuildDef();
        }

        //https://d2a2v2.vsrm.visualstudio.com/selenium2/_apis/release/definitions?api-version=4.1-preview.3
        // Get Release Definition count
        public GetReleaseDefResponse.ReleaseDef GetReleaseDefCount()
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {

                        HttpResponseMessage response = client.GetAsync("https://vsrm.dev.azure.com/" + Account + "//" + Project + "/_apis/release/definitions?api-version=4.1-preview.3").Result;
                        if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            GetReleaseDefResponse.ReleaseDef getINum = JsonConvert.DeserializeObject<GetReleaseDefResponse.ReleaseDef>(result);
                            return getINum;
                        }
                        else
                        {
                            string errorMessage = response.Content.ReadAsStringAsync().Result;
                            LastFailureMessage = errorMessage;
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new GetReleaseDefResponse.ReleaseDef(); ;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new GetReleaseDefResponse.ReleaseDef();
        }

        // Get Release Definition count
        public GetReleaseDefResponse.ReleaseDef GetReleaseDef()
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(Project + "/_apis/release/definitions?api-version=4.1-preview.3").Result;
                        if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            GetReleaseDefResponse.ReleaseDef getRelease = JsonConvert.DeserializeObject<GetReleaseDefResponse.ReleaseDef>(result);
                            return getRelease;
                        }
                        else
                        {
                            string errorMessage = response.Content.ReadAsStringAsync().Result;
                            LastFailureMessage = errorMessage;
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                   logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new GetReleaseDefResponse.ReleaseDef();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new GetReleaseDefResponse.ReleaseDef();
        }

        //Export build definitions to write file
        public List<JObject> ExportBuildDefinitions()
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    List<JObject> resultList = new List<JObject>();
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(string.Format("{0}/_apis/build/definitions?api-version=" + Configuration.VersionNumber, Project)).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            BuildDefinitionResponse.Build definitions = Newtonsoft.Json.JsonConvert.DeserializeObject<BuildDefinitionResponse.Build>(response.Content.ReadAsStringAsync().Result.ToString());
                            if (definitions.Count > 0)
                            {
                                foreach (BuildDefinitionResponse.Value value in definitions.Value)
                                {
                                    BuildDefinitions.BuildDefinition definitionResult = new BuildDefinitions.BuildDefinition();
                                    using (var client1 = GetHttpClient())
                                    {
                                        HttpResponseMessage responseDef = client1.GetAsync(string.Format("{0}/_apis/build/definitions/{1}?api-version=" + Configuration.VersionNumber, Project, value.Id)).Result;
                                        if (response.IsSuccessStatusCode)
                                        {
                                            string result = responseDef.Content.ReadAsStringAsync().Result;
                                            JObject o = JObject.Parse(result);
                                            resultList.Add(o);
                                        }
                                    }
                                }
                                return resultList;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new List<JObject>();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new List<JObject>();
        }

        // Get Repository list to create service end point json and import source code json
        public RepositoryList.Repository GetRepoList()
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(Configuration.UriString + "/" + Project + "/_apis/git/repositories?api-version=" + Configuration.VersionNumber).Result;
                        if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            RepositoryList.Repository repository = JsonConvert.DeserializeObject<RepositoryList.Repository>(result);
                            return repository;
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
                    logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new RepositoryList.Repository();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new RepositoryList.Repository();
        }

        // Get Release Definition to write file - Generalizing
        public List<JObject> GetReleaseDefs()
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    List<JObject> jobj = new List<JObject>();
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(Configuration.UriString + "/" + Project + "/_apis/release/definitions?api-version=" + Configuration.VersionNumber).Result;
                        if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            ReleaseDefCountResponse.Release release = new ReleaseDefCountResponse.Release();
                            string res = response.Content.ReadAsStringAsync().Result;
                            release = JsonConvert.DeserializeObject<ReleaseDefCountResponse.Release>(res);
                            if (release.Count > 0)
                            {
                                foreach (var rel in release.Value)
                                {
                                    using (var clients = GetHttpClient())
                                    {
                                        HttpResponseMessage resp = client.GetAsync(Configuration.UriString + "/" + Project + "/_apis/release/definitions/" + rel.Id).Result;
                                        if (resp.IsSuccessStatusCode && resp.StatusCode == System.Net.HttpStatusCode.OK)
                                        {
                                            JObject obj = new JObject();
                                            string relRes = resp.Content.ReadAsStringAsync().Result;
                                            obj = JsonConvert.DeserializeObject<JObject>(relRes);
                                            jobj.Add(obj);
                                        }
                                        else
                                        {
                                            var errorMessage = response.Content.ReadAsStringAsync();
                                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                                            this.LastFailureMessage = error;
                                        }
                                    }
                                }
                                return jobj;
                            }
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
                    logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new List<JObject>(); ;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new List<JObject>();
        }

        // Get Agent Queue to Replace the Queue name in the build definition
        public Dictionary<string, int> GetQueues()
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    Dictionary<string, int> dictionaryQueues = new Dictionary<string, int>();
                    QueueModel viewModel = new QueueModel();

                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(Configuration.Project + "/_apis/distributedtask/queues?api-version=2.0-preview.1").Result;

                        if (response.IsSuccessStatusCode)
                        {
                            viewModel = response.Content.ReadAsAsync<QueueModel>().Result;
                            if (viewModel != null && viewModel.Value != null)
                            {
                                foreach (AgentQueueModel aq in viewModel.Value)
                                {
                                    dictionaryQueues[aq.Name] = aq.Id;
                                }
                            }
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            this.LastFailureMessage = error;
                        }
                    }

                    return dictionaryQueues;
                }
                catch (Exception ex)
                {
                    logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new Dictionary<string, int>();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new Dictionary<string, int>();
        }
    }
}
