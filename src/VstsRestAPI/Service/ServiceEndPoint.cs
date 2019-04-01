﻿using log4net;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using VstsRestAPI.Viewmodel.Extractor;
using VstsRestAPI.Viewmodel.Service;

namespace VstsRestAPI.Service
{
    public class ServiceEndPoint : ApiServiceBase
    {
        public ServiceEndPoint(IConfiguration configuration) : base(configuration) { }
        private ILog logger = LogManager.GetLogger("ErrorLog");
        /// <summary>
        /// Create service endpoints
        /// </summary>
        /// <param name="json"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public ServiceEndpointModel CreateServiceEndPoint(string json, string project)
        {
            try
            {
                ServiceEndpointModel viewModel = new ServiceEndpointModel();

                using (var client = GetHttpClient())
                {
                    var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var method = new HttpMethod("POST");

                    var request = new HttpRequestMessage(method, project + "/_apis/distributedtask/serviceendpoints?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
                    var response = client.SendAsync(request).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        viewModel = response.Content.ReadAsAsync<ServiceEndpointModel>().Result;
                        return viewModel;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                        LastFailureMessage = error;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateServiceEndPoint" + "\t" + ex.Message + "\t"   + "\n" + ex.StackTrace + "\n");
            }
            return new ServiceEndpointModel();
        }

        public GetServiceEndpoints.ServiceEndPoint GetServiceEndPoints()
        {
            try
            {
                //https://dev.azure.com/exakshay/endpoint/_apis/serviceendpoint/endpoints?api-version=4.1-preview.1
                using (var client = GetHttpClient())
                {
                    var request = string.Format("{0}{1}/_apis/serviceendpoint/endpoints?api-version={2}", _configuration.UriString, Project, _configuration.VersionNumber);
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
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "GetServiceEndPoints" + "\t" + ex.Message + "\t"   + "\n" + ex.StackTrace + "\n");
            }
            return new GetServiceEndpoints.ServiceEndPoint();
        }
    }
}
