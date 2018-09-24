using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VstsRestAPI.Viewmodel.Extractor;

namespace VstsRestAPI.Extractor
{
    public class GetWorkItemsCount
    {
        readonly IConfiguration _sourceConfig;
        readonly string _sourceCredentials;
        readonly string _accountName;

        public GetWorkItemsCount(IConfiguration configuration)
        {
            _accountName = configuration.AccountName;
            _sourceConfig = configuration;
            _sourceCredentials = configuration.PersonalAccessToken;
        }
        public class WIMapData
        {
            public string oldID { get; set; }
            public string newID { get; set; }
            public string WIType { get; set; }
        }

        /// <summary>
        /// method to get each work item type and save as json
        /// </summary>
        public void GetWorkItemsDetails()
        {
            WorkItemFetchResponse.WorkItems fetchedEpics = getWorkItemsfromSource("Epic");
            //WorkItemFetchResponse.WorkItems fetchedFeatures = getWorkItemsfromSource("Feature");
            //WorkItemFetchResponse.WorkItems fetchedPBIs = getWorkItemsfromSource("Product Backlog Item");
            //WorkItemFetchResponse.WorkItems fetchedTasks = getWorkItemsfromSource("Task");
            //WorkItemFetchResponse.WorkItems fetchedTestCase = getWorkItemsfromSource("Test Case");
            //WorkItemFetchResponse.WorkItems fetchedBugs = getWorkItemsfromSource("Bug");
            //WorkItemFetchResponse.WorkItems fetchedUserStories = getWorkItemsfromSource("User Story");
            //WorkItemFetchResponse.WorkItems fetchedTestSuits = getWorkItemsfromSource("Test Suite");
            //WorkItemFetchResponse.WorkItems fetchedTestPlan = getWorkItemsfromSource("Test Plan");
            //WorkItemFetchResponse.WorkItems fetchedFeedbackRequest = getWorkItemsfromSource("Feedback Request");
        }

        /// <summary>
        /// method to get list of work items
        /// </summary>
        /// <param name="workItemType"></param>
        /// <returns></returns>
        public WorkItemFetchResponse.WorkItems getWorkItemsfromSource(string workItemType)
        {
            GetWorkItemsResponse.Results viewModel = new GetWorkItemsResponse.Results();
            WorkItemFetchResponse.WorkItems fetchedWIs;

            // create wiql object
            Object wiql = new
            {
                query = "Select [State], [Title] ,[Effort]" +
                        "From WorkItems " +
                        "Where [Work Item Type] = '" + workItemType + "'" +
                        "And [System.TeamProject] = '" + _sourceConfig.Project + "' " +
                        "Order By [State] Asc, [Changed Date] Desc"
            };
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _sourceCredentials);

                var postValue = new StringContent(JsonConvert.SerializeObject(wiql), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call

                // set the httpmethod to Patch
                var method = new HttpMethod("POST");

                // send the request               
                var request = new HttpRequestMessage(method, _sourceConfig.UriString + "/_apis/wit/wiql?api-version=2.2") { Content = postValue };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    viewModel = response.Content.ReadAsAsync<GetWorkItemsResponse.Results>().Result;
                }

                viewModel.HttpStatusCode = response.StatusCode;
                string workitemIDstoFetch = ""; int WICtr = 0;
                foreach (GetWorkItemsResponse.Workitem WI in viewModel.workItems)
                {
                    workitemIDstoFetch = WI.id + "," + workitemIDstoFetch;
                    WICtr++;
                }
                Console.WriteLine("Total {0} {1} Work Items read from source", WICtr, workItemType);
                workitemIDstoFetch = workitemIDstoFetch.TrimEnd(',');
                fetchedWIs = GetWorkItemsDetailinBatch(workitemIDstoFetch);

                //update the work items in target if specified
            }

            return fetchedWIs;
        }
        /// <summary>
        /// method to get work item data in detail
        /// </summary>
        /// <param name="workitemstoFetch"></param>
        /// <returns></returns>
        public WorkItemFetchResponse.WorkItems GetWorkItemsDetailinBatch(string workitemstoFetch)
        {
            WorkItemFetchResponse.WorkItems viewModel = new WorkItemFetchResponse.WorkItems();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_sourceConfig.UriString);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _sourceCredentials);

                    // use $expand=all to get all fields
                    //HttpResponseMessage response = client.GetAsync(WorkItemURL + "?$expand=all&api-version=2.2").Result;
                    HttpResponseMessage response = client.GetAsync(_sourceConfig.UriString + "/_apis/wit/workitems?api-version=2.2&ids=" + workitemstoFetch + "&$expand=relations").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        viewModel = response.Content.ReadAsAsync<WorkItemFetchResponse.WorkItems>().Result;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return viewModel;
        }
    }
}
