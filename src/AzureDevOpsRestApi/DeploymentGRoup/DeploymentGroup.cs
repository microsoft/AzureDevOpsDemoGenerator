using System;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace AzureDevOpsAPI.DeploymentGRoup
{
    public class DeploymentGroup : ApiServiceBase
    {
        public DeploymentGroup(IAppConfiguration configuration) : base(configuration)
        {
        }

        public bool CreateDeploymentGroup(string json)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    //POST https://dev.azure.com/{organization}/{project}/_apis/distributedtask/deploymentgroups?api-version=4.1-preview.1
                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");
                        var request = new HttpRequestMessage(method, Configuration.UriString + Configuration.Project + "/_apis/distributedtask/deploymentgroups?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                        HttpResponseMessage response = client.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            return true;

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
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
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
    }
}

