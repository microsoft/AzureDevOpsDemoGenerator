using NLog;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using AzureDevOpsAPI.Viewmodel.Extractor;

namespace AzureDevOpsAPI.Extractor
{
    public class GetWorkItemsCount : ApiServiceBase
    {

        public GetWorkItemsCount(IAppConfiguration configuration) : base(configuration)
        {

        }
         Logger logger = LogManager.GetLogger("*");
        public class WiMapData
        {
            public string OldId { get; set; }
            public string NewId { get; set; }
            public string WiType { get; set; }
        }

        /// <summary>
        /// method to get list of work items
        /// </summary>
        /// <param name="workItemType"></param>
        /// <returns></returns>
        public WorkItemFetchResponse.WorkItems GetWorkItemsfromSource(string workItemType)
        {
            GetWorkItemsResponse.Results viewModel = new GetWorkItemsResponse.Results();
            WorkItemFetchResponse.WorkItems fetchedWIs;
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    // create wiql object
                    Object wiql = new
                    {
                        query = "Select [State], [Title] ,[Effort]" +
                                "From WorkItems " +
                                "Where [Work Item Type] = '" + workItemType + "' " +
                                "And [System.TeamProject] = '" + Project + "' " +
                                "Order By [Stack Rank] Desc, [Backlog Priority] Desc"
                    };
                    using (var client = GetHttpClient())
                    {
                        var postValue = new StringContent(JsonConvert.SerializeObject(wiql), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call

                        // set the httpmethod to Patch
                        var method = new HttpMethod("POST");

                        // send the request               
                        var request = new HttpRequestMessage(method, Configuration.UriString + "/_apis/wit/wiql?api-version=" + Configuration.VersionNumber) { Content = postValue };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            viewModel = response.Content.ReadAsAsync<GetWorkItemsResponse.Results>().Result;
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            LastFailureMessage = error;
                        }

                        viewModel.HttpStatusCode = response.StatusCode;
                        string workitemIDstoFetch = ""; int wiCtr = 0;
                        foreach (GetWorkItemsResponse.Workitem wi in viewModel.WorkItems)
                        {
                            workitemIDstoFetch = wi.Id + "," + workitemIDstoFetch;
                            wiCtr++;
                        }
                        workitemIDstoFetch = workitemIDstoFetch.TrimEnd(',');
                        fetchedWIs = GetWorkItemsDetailInBatch(workitemIDstoFetch);
                        return fetchedWIs;
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new WorkItemFetchResponse.WorkItems();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new WorkItemFetchResponse.WorkItems();
        }
        /// <summary>
        /// method to get work item data in detail
        /// </summary>
        /// <param name="workitemstoFetch"></param>
        /// <returns></returns>
        public WorkItemFetchResponse.WorkItems GetWorkItemsDetailInBatch(string workitemstoFetch)
        {
            WorkItemFetchResponse.WorkItems viewModel = new WorkItemFetchResponse.WorkItems();
            int retryCount = 0;
            if (!string.IsNullOrEmpty(workitemstoFetch))
            {
                while (retryCount < 5)
                {

                    try
                    {
                        using (var client = GetHttpClient())
                        {
                            HttpResponseMessage response = client.GetAsync(Configuration.UriString + "/_apis/wit/workitems?api-version=" + Configuration.VersionNumber + "&ids=" + workitemstoFetch + "&$expand=relations").Result;
                            if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                viewModel = response.Content.ReadAsAsync<WorkItemFetchResponse.WorkItems>().Result;
                            }
                            else
                            {
                                var errorMessage = response.Content.ReadAsStringAsync();
                                string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                                LastFailureMessage = error;
                                retryCount++;
                            }
                        }
                        return viewModel;
                    }
                    catch (Exception ex)
                    {
                        logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
                        string error = ex.Message;
                        this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                        retryCount++;

                        if (retryCount > 4)
                        {
                            return viewModel;
                        }

                        Thread.Sleep(retryCount * 1000);
                    }
                }
            }
            return new WorkItemFetchResponse.WorkItems();
        }
        /// <summary>
        /// Get All work item names
        /// </summary>
        /// <returns></returns>
        public WorkItemNames.Names GetAllWorkItemNames()
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(string.Format("{0}/{1}/_apis/wit/workitemtypes?api-version={2}", Configuration.UriString, Configuration.Project, Configuration.VersionNumber)).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            WorkItemNames.Names workItemNames = JsonConvert.DeserializeObject<WorkItemNames.Names>(response.Content.ReadAsStringAsync().Result);
                            return workItemNames;
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
                    logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new WorkItemNames.Names(); ;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new WorkItemNames.Names();
        }
    }
}
