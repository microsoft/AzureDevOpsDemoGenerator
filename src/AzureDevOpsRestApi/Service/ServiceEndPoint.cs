using NLog;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using AzureDevOpsAPI.Viewmodel.Extractor;
using AzureDevOpsAPI.Viewmodel.Service;
using Microsoft.ApplicationInsights;

namespace AzureDevOpsAPI.Service
{
    public class ServiceEndPoint : ApiServiceBase
    {
        private TelemetryClient ai;
        public ServiceEndPoint(IAppConfiguration configuration, TelemetryClient _ai) : base(configuration) { ai = _ai; }
        Logger logger = LogManager.GetLogger("*");
        /// <summary>
        /// Create service endpoints
        /// </summary>
        /// <param name="json"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public ServiceEndpointModel CreateServiceEndPoint(string json, string project)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    ServiceEndpointModel viewModel = new ServiceEndpointModel();

                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, project + "/_apis/distributedtask/serviceendpoints?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            viewModel = response.Content.ReadAsAsync<ServiceEndpointModel>().Result;

                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            LastFailureMessage = error;
                            retryCount++;
                        }
                        return viewModel;
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug("CreateServiceEndPoint" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new ServiceEndpointModel();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new ServiceEndpointModel();
        }

        public GetServiceEndpoints.ServiceEndPoint GetServiceEndPoints()
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    //https://dev.azure.com/exakshay/endpoint/_apis/serviceendpoint/endpoints?api-version=4.1-preview.1
                    using (var client = GetHttpClient())
                    {
                        var request = string.Format("{0}{1}/_apis/serviceendpoint/endpoints?api-version={2}", Configuration.UriString, Project, Configuration.VersionNumber);
                        HttpResponseMessage response = client.GetAsync(request).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            string res = response.Content.ReadAsStringAsync().Result;
                            GetServiceEndpoints.ServiceEndPoint serviceEndPoint = JsonConvert.DeserializeObject<GetServiceEndpoints.ServiceEndPoint>(res);
                            return serviceEndPoint;
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
                    ai.TrackException(ex);
                    logger.Debug("GetServiceEndPoints" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new GetServiceEndpoints.ServiceEndPoint();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new GetServiceEndpoints.ServiceEndPoint();
        }
    }
}
