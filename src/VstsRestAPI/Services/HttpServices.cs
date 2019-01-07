using Newtonsoft.Json;
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
            oConfiguration.PersonalAccessToken = config.PersonalAccessToken;
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
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), oConfiguration.UriString + "DefaultCollection/" + oConfiguration.Project + oConfiguration.UriParams + oConfiguration.VersionNumber) { Content = patchValue };
                oHttpResponseMessage = client.SendAsync(request).Result;
            }
            return oHttpResponseMessage;
        }

        public dynamic Post()
        {
            HttpResponseMessage oHttpResponseMessage = new HttpResponseMessage();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", oConfiguration.PersonalAccessToken);
                var patchValue = new StringContent(oConfiguration.RequestBody, Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
                var request = new HttpRequestMessage(new HttpMethod("POST"), oConfiguration.UriString + "DefaultCollection/" + oConfiguration.Project + oConfiguration.UriParams + oConfiguration.VersionNumber) { Content = patchValue };
                oHttpResponseMessage = client.SendAsync(request).Result;

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
