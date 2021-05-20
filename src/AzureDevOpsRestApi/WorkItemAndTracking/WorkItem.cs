using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using AzureDevOpsAPI.Viewmodel.WorkItem;
using Microsoft.ApplicationInsights;

namespace AzureDevOpsAPI.WorkItemAndTracking
{
    public partial class WorkItem : ApiServiceBase
    {
        private TelemetryClient ai;
        public WorkItem(IAppConfiguration configuration, TelemetryClient _ai) : base(configuration) { ai = _ai; }
         Logger logger = LogManager.GetLogger("*");
        /// <summary>
        /// Method to create the workItems
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public bool CreateWorkItemUsingByPassRules(string projectName, string json)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    List<BatchRequest> batchRequests = JsonConvert.DeserializeObject<List<BatchRequest>>(json);

                    foreach (BatchRequest batchRequest in batchRequests)
                    {
                        string currUri = batchRequest.Uri;
                        batchRequest.Uri = '/' + projectName + currUri;

                        JArray newRel = new JArray(2);
                        int i = 0;
                        foreach (object obj in batchRequest.Body)
                        {
                            JObject code = JObject.Parse(obj.ToString());
                            i++;

                            //checking if the object has relations key
                            if (code["path"].ToString() == "/relations/-")
                            {
                                JObject hero = (JObject)code["value"];
                                hero["url"] = Configuration.UriString + code["value"]["url"].ToString();

                                batchRequest.Body[i - 1] = code;
                            }
                        }
                    }

                    using (var client = GetHttpClient())
                    {
                        var newBatchRequest = new StringContent(JsonConvert.SerializeObject(batchRequests), Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        // send the request
                        var request = new HttpRequestMessage(method, "_apis/wit/$batch?api-version=" + Configuration.VersionNumber) { Content = newBatchRequest };
                        var response = client.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            return response.IsSuccessStatusCode;
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
                    ai.TrackException(ex);
                    logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return false;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return false;
        }

        /// <summary>
        /// Method to update WorkItems based on id and workItem Type
        /// </summary>
        /// <param name="json"></param>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public bool UpdateWorkItemUsingByPassRules(string json, string projectName, string currentUser, string jsonSettings)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    string itemType = "";
                    string itemTypes = "";
                    string userType = "";
                    string jsonUsers = "";
                    string jsonType = "";
                    string jsonTypes = "";
                    List<int> workItemIds = new List<int>();
                    var jsonItems = JObject.Parse(jsonSettings);
                    List<string> tags = new List<string>();
                    JArray tag = jsonItems["tags"].Value<JArray>();
                    foreach (var data in tag.Values())
                    {
                        tags.Add(data.ToString());
                    }


                    JArray userList = jsonItems["users"].Value<JArray>();
                    List<string> users = new List<string>();
                    foreach (var data in userList.Values())
                    {
                        users.Add(data.ToString());
                    }

                    if (!string.IsNullOrEmpty(currentUser))
                    {
                        users.Add(currentUser);
                    }

                    Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>
                    {
                        {"Feature",new List<string>() {"In Progress","New","Done",}},
                        {"Product Backlog Item",new List<string>() {"Approved","Committed","New","Done",}},
                        {"Bug",new List<string>() {"Approved","Committed","New","Done",}},
                        {"Task",new List<string>() {"In Progress","To Do","Done",}}
                    };

                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = new HttpResponseMessage();
                        HttpRequestMessage request = new HttpRequestMessage();
                        foreach (var type in dic.Keys)
                        {
                            var res = GetListOfWorkItems_ByWiql(projectName, type);
                            workItemIds.Clear();
                            foreach (var ids in res.WorkItems)
                            {
                                workItemIds.Add(ids.Id);
                            }
                            foreach (var id in workItemIds)
                            {
                                if (type == "Feature" || type == "Product Backlog Item" || type == "Bug" || type == "Task")
                                {
                                    itemType = dic[type][new Random().Next(0, dic[type].Count)];
                                    itemTypes = tags[new Random().Next(0, tags.Count)];
                                    userType = users[new Random().Next(0, users.Count)];
                                    jsonType = json.Replace("$State$", itemType.ToString());
                                    jsonTypes = jsonType.Replace("$Tags$", itemTypes.ToString());
                                    jsonUsers = jsonTypes.Replace("$Users$", userType.ToString());
                                }

                                var jsonContent = new StringContent(jsonUsers, Encoding.UTF8, "application/json-patch+json");
                                var method = new HttpMethod("PATCH");
                                // send the request
                                request = new HttpRequestMessage(method, Configuration.UriString + "_apis/wit/workitems/" + id + "?bypassRules=true&api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                                response = client.SendAsync(request).Result;
                                if (response.IsSuccessStatusCode)
                                {
                                    response.StatusCode = System.Net.HttpStatusCode.OK;
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
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return false;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return false;

        }

        /// <summary>
        /// Method to Get the list of all workItems
        /// </summary>
        /// <param name="project"></param>
        /// <param name="workItemType"></param>
        /// <returns></returns>
        public GetWorkItemsResponse.Results GetListOfWorkItems_ByWiql(string project, string workItemType)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    GetWorkItemsResponse.Results viewModel = new GetWorkItemsResponse.Results();

                    //create wiql object
                    Object wiql = new
                    {
                        query = "Select [Work Item Type],[State], [Title],[Created By] " +
                                "From WorkItems " +
                                "Where [Work Item Type] = '" + workItemType + "' " +
                                "And [System.TeamProject] = '" + project + "' " +
                                "And [System.State] = 'New' " +
                                "Order By [Stack Rank] Desc, [Backlog Priority] Desc"
                    };

                    using (var client = GetHttpClient())
                    {
                        var postValue = new StringContent(JsonConvert.SerializeObject(wiql), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call

                        // set the httpmethod to Patch
                        var method = new HttpMethod("POST");

                        // send the request               
                        var request = new HttpRequestMessage(method, "_apis/wit/wiql?api-version=" + Configuration.VersionNumber) { Content = postValue };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            viewModel = JsonConvert.DeserializeObject<GetWorkItemsResponse.Results>(response.Content.ReadAsStringAsync().Result);
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            LastFailureMessage = error;
                            retryCount++;
                        }
                        viewModel.HttpStatusCode = response.StatusCode;
                        return viewModel;
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug(ex.Message + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new GetWorkItemsResponse.Results();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new GetWorkItemsResponse.Results();
        }

    }
}