using NLog;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using AzureDevOpsAPI.Viewmodel.Extractor;

namespace AzureDevOpsAPI.Extractor
{
    public class VariableGroups : ApiServiceBase
    {
        Logger logger = LogManager.GetLogger("*");

        public VariableGroups(IAppConfiguration configuration) : base(configuration)
        {
        }
        
        public GetVariableGroups.Groups GetVariableGroups()
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        string req = string.Format("https://dev.azure.com/{0}/{1}/_apis/distributedtask/variablegroups?api-version={2}", Configuration.AccountName, Configuration.Project, Configuration.VersionNumber);
                        var response = client.GetAsync(req).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            string res = response.Content.ReadAsStringAsync().Result;
                            GetVariableGroups.Groups ress = JsonConvert.DeserializeObject<GetVariableGroups.Groups>(res);
                            return ress;
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
                        return new GetVariableGroups.Groups();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new GetVariableGroups.Groups();
        }

        public GetVariableGroups.VariableGroupsCreateResponse PostVariableGroups(string json)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                        string req = string.Format("https://dev.azure.com/{0}/{1}/_apis/distributedtask/variablegroups?api-version={2}", Configuration.AccountName, Configuration.Project, Configuration.VersionNumber);

                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, req) { Content = jsonContent };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            string res1 = response.Content.ReadAsStringAsync().Result;
                            GetVariableGroups.VariableGroupsCreateResponse ress = JsonConvert.DeserializeObject<GetVariableGroups.VariableGroupsCreateResponse>(res1);
                            return ress;
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
                        return new GetVariableGroups.VariableGroupsCreateResponse();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new GetVariableGroups.VariableGroupsCreateResponse();
        }
    }
}
