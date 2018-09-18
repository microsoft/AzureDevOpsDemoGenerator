using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VstsRestAPI.Viewmodel.Service;

namespace VstsRestAPI.Service
{
    public class ServiceEndPoint : ApiServiceBase
    {
        public ServiceEndPoint(IConfiguration configuration) : base(configuration) { }

        /// <summary>
        /// Create service endpoints
        /// </summary>
        /// <param name="json"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public ServiceEndpointModel CreateServiceEndPoint(string json, string project)
        {
            ServiceEndpointModel viewModel = new ServiceEndpointModel();

            using (var client = GetHttpClient())
            {
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, project + "/_apis/distributedtask/serviceendpoints?api-version=" + _configuration.VersionNumber + "-preview.1") { Content = jsonContent };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    viewModel = response.Content.ReadAsAsync<ServiceEndpointModel>().Result;                    
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                }
            }

            return viewModel;
            
        }
    }
}
