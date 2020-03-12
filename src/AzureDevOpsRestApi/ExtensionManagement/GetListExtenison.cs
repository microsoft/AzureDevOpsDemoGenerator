using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using AzureDevOpsAPI.Viewmodel.Extractor;
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
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        var request = Configuration.UriString + "/_apis/extensionmanagement/installedextensions?api-version" + Configuration.VersionNumber;
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
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug(ex.Message + ex.StackTrace);
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new GetExtensions.ExtensionsList(); ;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new GetExtensions.ExtensionsList();
        }
    }
}
