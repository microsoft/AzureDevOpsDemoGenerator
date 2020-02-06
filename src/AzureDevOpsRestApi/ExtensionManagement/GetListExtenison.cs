using Newtonsoft.Json;
using System.Net.Http;
using AzureDevOpsAPI.Viewmodel.Extractor;
using System;
using NLog;

namespace AzureDevOpsAPI.ExtensionManagement
{
    public class GetListExtenison : ApiServiceBase
    {
        Logger logger = LogManager.GetLogger("*");
        public GetListExtenison(IAppConfiguration configuration) : base(configuration)
        {
        }

        //GET https://extmgmt.dev.azure.com/{organization}/_apis/extensionmanagement/installedextensions?api-version=4.1-preview.1
        public GetExtensions.ExtensionsList GetInstalledExtensions()
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    var request = _configuration.UriString + "/_apis/extensionmanagement/installedextensions?api-version" + _configuration.VersionNumber;
                    HttpResponseMessage response = client.GetAsync(request).Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string res = response.Content.ReadAsStringAsync().Result;
                        GetExtensions.ExtensionsList extensionsList = JsonConvert.DeserializeObject<GetExtensions.ExtensionsList>(res);
                        return extensionsList;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                        this.LastFailureMessage = error;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Debug(ex.Message + ex.StackTrace);
            }
            return new GetExtensions.ExtensionsList();
        }
    }
}
