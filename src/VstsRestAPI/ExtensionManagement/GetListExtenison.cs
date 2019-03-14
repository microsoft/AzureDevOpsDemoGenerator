using Newtonsoft.Json;
using System.Net.Http;
using VstsRestAPI.Viewmodel.Extractor;

namespace VstsRestAPI.ExtensionManagement
{
    public class GetListExtenison : ApiServiceBase
    {
        public GetListExtenison(IConfiguration configuration) : base(configuration)
        {
        }

        //GET https://extmgmt.dev.azure.com/{organization}/_apis/extensionmanagement/installedextensions?api-version=4.1-preview.1
        public GetExtensions.ExtensionsList GetInstalledExtensions()
        {
            using(var client = GetHttpClient())
            {
                var request = _configuration.UriString + "/_apis/extensionmanagement/installedextensions?api-version" + _configuration.VersionNumber;
                HttpResponseMessage response = client.GetAsync(request).Result;
                if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string res = response.Content.ReadAsStringAsync().Result;
                    GetExtensions.ExtensionsList extensionsList = JsonConvert.DeserializeObject<GetExtensions.ExtensionsList>(res);
                    return extensionsList;
                }
                else{
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                    return new GetExtensions.ExtensionsList();
                }
            }
        }
    }
}
