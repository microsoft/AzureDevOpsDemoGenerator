using TemplatesGeneratorTool.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Net;


namespace TemplatesGeneratorTool.Generators
{
    public class GenerateWIFromSource
    {
        readonly IConfiguration _sourceConfig;
        readonly string _sourceCredentials;
        readonly string _accountName;

        public GenerateWIFromSource(IConfiguration configuration, string accountName)
        {
            _accountName = accountName;
            _sourceConfig = configuration;
            _sourceCredentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _sourceConfig.PersonalAccessToken)));
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
        public void UpdateWorkItem()
        {
            WorkItemFetchResponse.WorkItems fetchedEpics = getWorkItemsfromSource("Epic");
            WorkItemFetchResponse.WorkItems fetchedFeatures = getWorkItemsfromSource("Feature");
            WorkItemFetchResponse.WorkItems fetchedPBIs = getWorkItemsfromSource("Product Backlog Item");
            WorkItemFetchResponse.WorkItems fetchedTasks = getWorkItemsfromSource("Task");
            WorkItemFetchResponse.WorkItems fetchedTestCase = getWorkItemsfromSource("Test Case");
            WorkItemFetchResponse.WorkItems fetchedBugs = getWorkItemsfromSource("Bug");
            WorkItemFetchResponse.WorkItems fetchedUserStories = getWorkItemsfromSource("User Story");
            WorkItemFetchResponse.WorkItems fetchedTestSuits = getWorkItemsfromSource("Test Suite");
            WorkItemFetchResponse.WorkItems fetchedTestPlan = getWorkItemsfromSource("Test Plan");
            WorkItemFetchResponse.WorkItems fetchedFeedbackRequest = getWorkItemsfromSource("Feedback Request");

            GetBoardRowsResponse.Result fetchedBoardRows = GetBoardRows(_sourceConfig.Project);

            if (!Directory.Exists("Templates"))
            {
                Directory.CreateDirectory("Templates");
            }

            string fetchedPBIsJSON = JsonConvert.SerializeObject(fetchedPBIs, Formatting.Indented);
            System.IO.File.WriteAllText(@"Templates\PBIfromTemplate.json", fetchedPBIsJSON);
            string fetchedTasksJSON = JsonConvert.SerializeObject(fetchedTasks, Formatting.Indented);
            System.IO.File.WriteAllText(@"Templates\TaskfromTemplate.json", fetchedTasksJSON);
            string fetchedTestCasesJSON = JsonConvert.SerializeObject(fetchedTestCase, Formatting.Indented);
            System.IO.File.WriteAllText(@"Templates\TestCasefromTemplate.json", fetchedTestCasesJSON);
            string fetchedBugsJSON = JsonConvert.SerializeObject(fetchedBugs, Formatting.Indented);
            System.IO.File.WriteAllText(@"Templates\BugfromTemplate.json", fetchedBugsJSON);
            string fetchedEpicsJSON = JsonConvert.SerializeObject(fetchedEpics, Formatting.Indented);
            System.IO.File.WriteAllText(@"Templates\EpicfromTemplate.json", fetchedEpicsJSON);
            string fetchedFeaturesJSON = JsonConvert.SerializeObject(fetchedFeatures, Formatting.Indented);
            System.IO.File.WriteAllText(@"Templates\FeaturefromTemplate.json", fetchedFeaturesJSON);
            string fetchedUerStoriesJSON = JsonConvert.SerializeObject(fetchedUserStories, Formatting.Indented);
            System.IO.File.WriteAllText(@"Templates\UerStoriesfromTemplate.json", fetchedUerStoriesJSON);
            string fetchedTestSuitsJSON = JsonConvert.SerializeObject(fetchedTestSuits, Formatting.Indented);
            System.IO.File.WriteAllText(@"Templates\TestSuitesFromTemplate.json", fetchedTestSuitsJSON);
            string fetchedTestPlanJSON = JsonConvert.SerializeObject(fetchedTestPlan, Formatting.Indented);
            System.IO.File.WriteAllText(@"Templates\TestPlanFromTemplate.json", fetchedTestPlanJSON);
            string fetchedFeedbackRequestJSON = JsonConvert.SerializeObject(fetchedFeedbackRequest, Formatting.Indented);
            System.IO.File.WriteAllText(@"Templates\FeedbackRequestFromTemplate.json", fetchedFeedbackRequestJSON);


            string fetchedBoardRowsJSON = JsonConvert.SerializeObject(fetchedBoardRows.value, Formatting.Indented);
            System.IO.File.WriteAllText(@"Templates\BoardRowsFromTemplate.json", fetchedBoardRowsJSON);
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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

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
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

                    // use $expand=all to get all fields
                    //HttpResponseMessage response = client.GetAsync(WorkItemURL + "?$expand=all&api-version=2.2").Result;
                    HttpResponseMessage response = client.GetAsync(_sourceConfig.UriString + "/_apis/wit/workitems?api-version=2.2&ids=" + workitemstoFetch + "&$expand=relations").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        viewModel = response.Content.ReadAsAsync<WorkItemFetchResponse.WorkItems>().Result;
                    }
                    // viewModel.HttpStatusCode = response.StatusCode;
                    if (viewModel.count > 0) { DownloadAttachedFiles(viewModel); }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured while generating work item template: " + ex.Message);
                Console.WriteLine("");
            }
            return viewModel;
        }

        /// <summary>
        /// Method to export Work item attachments
        /// </summary>
        /// <param name="workItems"></param>
        public void DownloadAttachedFiles(WorkItemFetchResponse.WorkItems workItems)
        {
            try
            {
                if (!Directory.Exists(@"Templates\WorkItemAttachments\"))
                {
                    Directory.CreateDirectory(@"Templates\WorkItemAttachments\");
                }
                foreach (var wi in workItems.value)
                {
                    if (wi.relations != null)
                    {
                        foreach (var rel in wi.relations)
                        {
                            if (rel.rel == "AttachedFile")
                            {
                                string remoteUri = rel.url;
                                string fileName = rel.attributes["id"] + rel.attributes["name"];
                                string pathToSave = @"Templates\WorkItemAttachments\" + fileName;

                                using (var client = new HttpClient())
                                {
                                    client.DefaultRequestHeaders.Accept.Clear();
                                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);
                                    HttpResponseMessage response = client.GetAsync(remoteUri + "?api-version=1.0").Result;

                                    if (response.IsSuccessStatusCode)
                                    {
                                        File.WriteAllBytes(pathToSave, response.Content.ReadAsByteArrayAsync().Result);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured while generating work item Attachments: " + ex.Message);
                Console.WriteLine("");
            }


        }

        /// <summary>
        /// method to export board rows (kanban board) from source project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public GetBoardRowsResponse.Result GetBoardRows(string project)
        {
            GetBoardRowsResponse.Result viewModel = new GetBoardRowsResponse.Result();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_sourceConfig.UriString);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

                    HttpResponseMessage response = client.GetAsync(_sourceConfig.UriString + string.Format("/DefaultCollection/{0}/{1}%20Team/_apis/work/boards/Backlog%20items/Rows?api-version=2.0", project, project)).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        viewModel = response.Content.ReadAsAsync<GetBoardRowsResponse.Result>().Result;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured while generating board row template: " + ex.Message);
                Console.WriteLine("");
            }
            return viewModel;
        }
    }
}
