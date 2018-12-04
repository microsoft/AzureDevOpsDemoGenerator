using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using VstsRestAPI.Viewmodel.Extractor;
using VstsRestAPI.Viewmodel.Queue;

namespace VstsRestAPI.Extractor
{
    public class GetBuildandReleaseDefs : ApiServiceBase
    {
        public GetBuildandReleaseDefs(IConfiguration configuration) : base(configuration) { }

        //https://d2a2v2.visualstudio.com/selenium2/_apis/build/definitions?api-version=4.1
        // Get Build Definition count
        public GetBuildDefResponse.BuildDef GetBuildDefCount()
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(_configuration.UriString + "/" + Project + "/_apis/build/definitions?api-version=" + _configuration.VersionNumber).Result;
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
                        return new GetBuildDefResponse.BuildDef();
                    }
                }
            }
            catch (Exception)
            {

            }
            return new GetBuildDefResponse.BuildDef();
        }

        //https://d2a2v2.vsrm.visualstudio.com/selenium2/_apis/release/definitions?api-version=4.1-preview.3
        // Get Release Definition count
        public GetReleaseDefResponse.ReleaseDef GetReleaseDefCount()
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
                        return new GetReleaseDefResponse.ReleaseDef();
                    }
                }
            }
            catch (Exception)
            {

            }
            return new GetReleaseDefResponse.ReleaseDef();
        }

        // Get Release Definition count
        public GetReleaseDefResponse.ReleaseDef GetReleaseDef()
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
                        return new GetReleaseDefResponse.ReleaseDef();
                    }
                }
            }
            catch (Exception)
            {

            }
            return new GetReleaseDefResponse.ReleaseDef();
        }

        //Export build definitions to write file
        public List<JObject> ExportBuildDefinitions()
        {
            try
            {
                List<JObject> resultList = new List<JObject>();
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(string.Format("{0}/_apis/build/definitions?api-version=" + _configuration.VersionNumber, Project)).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        BuildDefinitionResponse.Build Definitions = Newtonsoft.Json.JsonConvert.DeserializeObject<BuildDefinitionResponse.Build>(response.Content.ReadAsStringAsync().Result.ToString());
                        if (Definitions.count > 0)
                        {
                            foreach (BuildDefinitionResponse.Value value in Definitions.value)
                            {
                                BuildDefinitions.BuildDefinition DefinitionResult = new BuildDefinitions.BuildDefinition();
                                using (var client1 = GetHttpClient())
                                {
                                    HttpResponseMessage ResponseDef = client1.GetAsync(string.Format("{0}/_apis/build/definitions/{1}?api-version=" + _configuration.VersionNumber, Project, value.id)).Result;
                                    if (response.IsSuccessStatusCode)
                                    {
                                        string result = ResponseDef.Content.ReadAsStringAsync().Result;
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
            catch (Exception)
            {

            }
            return new List<JObject>();
        }

        // Get Repository list to create service end point json and import source code json
        public RepositoryList.Repository GetRepoList()
        {
            using (var client = GetHttpClient())
            {
                HttpResponseMessage response = client.GetAsync(_configuration.UriString + "/" + Project + "/_apis/git/repositories?api-version=" + _configuration.VersionNumber).Result;
                if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    RepositoryList.Repository repository = JsonConvert.DeserializeObject<RepositoryList.Repository>(result);
                    return repository;
                }
            }
            return new RepositoryList.Repository();
        }

        // Get Release Definition to write file - Generalizing
        public List<JObject> GetReleaseDefs()
        {
            List<JObject> jobj = new List<JObject>();
            using (var client = GetHttpClient())
            {
                HttpResponseMessage response = client.GetAsync("/" + Project + "/_apis/release/definitions?api-version=" + _configuration.VersionNumber).Result;
                if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ReleaseDefCountResponse.Release release = new ReleaseDefCountResponse.Release();
                    string res = response.Content.ReadAsStringAsync().Result;
                    release = JsonConvert.DeserializeObject<ReleaseDefCountResponse.Release>(res);
                    if (release.count > 0)
                    {
                        foreach (var rel in release.value)
                        {
                            using (var clients = GetHttpClient())
                            {
                                HttpResponseMessage resp = client.GetAsync("/" + Project + "/_apis/release/definitions/" + rel.id).Result;
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
                }
                return new List<JObject>();
            }
        }

        // Get Agent Queue to Replace the Queue name in the build definition
        public Dictionary<string, int> GetQueues()
        {
            Dictionary<string, int> dictionaryQueues = new Dictionary<string, int>();
            QueueModel viewModel = new QueueModel();

            using (var client = GetHttpClient())
            {
                HttpResponseMessage response = client.GetAsync(_configuration.Project + "/_apis/distributedtask/queues?api-version=2.0-preview.1").Result;

                if (response.IsSuccessStatusCode)
                {
                    viewModel = response.Content.ReadAsAsync<QueueModel>().Result;
                    if (viewModel != null && viewModel.value != null)
                    {
                        foreach (AgentQueueModel aq in viewModel.value)
                        {
                            dictionaryQueues[aq.name] = aq.id;
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

    }
}
