using System.Net.Http;
using System.Text;

namespace VstsRestAPI.DeploymentGRoup
{
    public class DeploymentGroup : ApiServiceBase
    {
        public DeploymentGroup(IConfiguration configuration) : base(configuration)
        {
        }

        public bool CreateDeploymentGroup(string json)
        {
            //POST https://dev.azure.com/{organization}/{project}/_apis/distributedtask/deploymentgroups?api-version=4.1-preview.1
            using (var client = GetHttpClient())
            {
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");
                var request = new HttpRequestMessage(method, _configuration.UriString + _configuration.Project + "/_apis/distributedtask/deploymentgroups?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
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
                    return false;
                }
            }
        }
    }
}
