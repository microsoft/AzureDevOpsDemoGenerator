using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace VstsRestAPI.Services
{
    public class HttpServices
    {
        private Configuration oConfiguration = new Configuration();
        public HttpServices(Configuration config)
        {
            oConfiguration.UriString = config.UriString;
            oConfiguration.Project = config.Project;
            oConfiguration.PersonalAccessToken = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", config.PersonalAccessToken)));//configuration.PersonalAccessToken;

            oConfiguration.UriParams = config.UriParams;
            oConfiguration.RequestBody = config.RequestBody;
            oConfiguration.VersionNumber = config.VersionNumber;

        }

        public HttpResponseMessage PatchBasic()
        {
            HttpResponseMessage oHttpResponseMessage = new HttpResponseMessage();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", oConfiguration.PersonalAccessToken);
                var patchValue = new StringContent(oConfiguration.RequestBody, Encoding.UTF8, "application/json-patch+json"); // mediaType needs to be application/json-patch+json for a patch call
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), oConfiguration.UriString + "/" + oConfiguration.Project + oConfiguration.UriParams + oConfiguration.VersionNumber) { Content = patchValue };
                oHttpResponseMessage = client.SendAsync(request).Result;
            }
            return oHttpResponseMessage;
        }

        public dynamic Post(string json, string uriparams)
        {
            HttpResponseMessage oHttpResponseMessage = new HttpResponseMessage();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", oConfiguration.PersonalAccessToken);
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");
                var request = new HttpRequestMessage(method, oConfiguration.UriString + uriparams) { Content = jsonContent };
                oHttpResponseMessage = client.SendAsync(request).Result;
            }
            return oHttpResponseMessage;
        }
        public HttpResponseMessage Get(string request)
        {
            HttpResponseMessage oHttpResponseMessage = new HttpResponseMessage();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(oConfiguration.UriString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", oConfiguration.PersonalAccessToken);
                oHttpResponseMessage = client.GetAsync(oConfiguration.UriString + request).Result;

            }
            return oHttpResponseMessage;
        }

        public HttpResponseMessage Put()
        {
            HttpResponseMessage oHttpResponseMessage = new HttpResponseMessage();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", oConfiguration.PersonalAccessToken);
                var patchValue = new StringContent(JsonConvert.SerializeObject(oConfiguration.RequestBody), Encoding.UTF8, "application/json-patch+json"); // mediaType needs to be application/json-patch+json for a patch call

                var method = new HttpMethod("PATCH");
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), oConfiguration.UriString + oConfiguration.UriParams) { Content = patchValue };
                oHttpResponseMessage = client.SendAsync(request).Result;

            }
            return oHttpResponseMessage;
        }
    }
}
