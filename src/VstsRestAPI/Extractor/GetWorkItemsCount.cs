using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using VstsRestAPI.Viewmodel.Extractor;

namespace VstsRestAPI.Extractor
{
    public class GetWorkItemsCount : ApiServiceBase
    {

        public GetWorkItemsCount(IConfiguration configuration) : base(configuration)
        {

        }
        public class WIMapData
        {
            public string oldID { get; set; }
            public string newID { get; set; }
            public string WIType { get; set; }
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
            try
            {
                // create wiql object
                Object wiql = new
                {
                    query = "Select [State], [Title] ,[Effort]" +
                            "From WorkItems " +
                            "Where [Work Item Type] = '" + workItemType + "'" +
                            "And [System.TeamProject] = '" + Project + "' " +
                            "Order By [Stack Rank] Desc, [Backlog Priority] Desc"
                };
                using (var client = GetHttpClient())
                {
                    var postValue = new StringContent(JsonConvert.SerializeObject(wiql), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call

                    // set the httpmethod to Patch
                    var method = new HttpMethod("POST");

                    // send the request               
                    var request = new HttpRequestMessage(method, _configuration.UriString + "/_apis/wit/wiql?api-version=4.1") { Content = postValue };
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
                    string workitemIDstoFetch = ""; int WICtr = 0;
                    foreach (GetWorkItemsResponse.Workitem WI in viewModel.workItems)
                    {
                        workitemIDstoFetch = WI.id + "," + workitemIDstoFetch;
                        WICtr++;
                    }
                    workitemIDstoFetch = workitemIDstoFetch.TrimEnd(',');
                    fetchedWIs = GetWorkItemsDetailInBatch(workitemIDstoFetch);
                    return fetchedWIs;
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                LastFailureMessage = error;
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
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(_configuration.UriString + "/_apis/wit/workitems?api-version=4.1&ids=" + workitemstoFetch + "&$expand=relations").Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        viewModel = response.Content.ReadAsAsync<WorkItemFetchResponse.WorkItems>().Result;
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
                string error = ex.Message;
                LastFailureMessage = error;
            }
            return viewModel;
        }
    }
}
