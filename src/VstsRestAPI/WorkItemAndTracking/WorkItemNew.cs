using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using VstsRestAPI.Viewmodel.WorkItem;

namespace VstsRestAPI.WorkItemAndTracking
{
    public partial class WorkItemNew
    {
        public string lastFailureMessage;
        readonly IConfiguration _configuration;
        readonly string _credentials;

        public WorkItemNew(IConfiguration configuration)
        {
            _configuration = configuration;
            _credentials = configuration.PersonalAccessToken;//Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _configuration.PersonalAccessToken)));
        }

        /// <summary>
        /// Method to create the workItems
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public bool CreateWorkItemUsingByPassRules(string projectName, string json)
        {
            List<BatchRequest> batchRequests = JsonConvert.DeserializeObject<List<BatchRequest>>(json);

            foreach (BatchRequest batchRequest in batchRequests)
            {
                string currURI = batchRequest.uri;
                batchRequest.uri = '/' + projectName + currURI;

                JArray newRel = new JArray(2);
                int i = 0;
                foreach (object obj in batchRequest.body)
                {
                    JObject code = JObject.Parse(obj.ToString());
                    i++;

                    //checking if the object has relations key
                    if (code["path"].ToString() == "/relations/-")
                    {
                        JObject hero = (JObject)code["value"];
                        hero["url"] = _configuration.UriString + code["value"]["url"].ToString();

                        batchRequest.body[i - 1] = code;
                    }
                }
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                var newbatchRequest = new StringContent(JsonConvert.SerializeObject(batchRequests), Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");
                // send the request
                var request = new HttpRequestMessage(method, _configuration.UriString + "_apis/wit/$batch?api-version=" + _configuration.VersionNumber) { Content = newbatchRequest };
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    return response.IsSuccessStatusCode;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.lastFailureMessage = error;
                }
            }

            return false;
        }

        /// <summary>
        /// Method to update WorkItems based on id and workItem Type
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ProjectName"></param>
        /// <returns></returns>
        public bool UpdateWorkItemUsingByPassRules(string json, string ProjectName, string currentUser, string jsonSettings)
        {
            string itemType = "";
            string itemTypes = "";
            string UserType = "";
            string JSON_Users = "";
            string JsonType = "";
            string JsonTypes = "";
            List<int> WorkIds = new List<int>();
            var jitems = JObject.Parse(jsonSettings);
            List<string> Tags = new List<string>();
            JArray Tag = jitems["tags"].Value<JArray>();
            foreach (var data in Tag.Values())
            {
                Tags.Add(data.ToString());
            }


            JArray UserList = jitems["users"].Value<JArray>();
            List<string> Users = new List<string>();
            foreach (var data in UserList.Values())
            {
                Users.Add(data.ToString());
            }

            if (!string.IsNullOrEmpty(currentUser)) Users.Add(currentUser);

            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>
            {
                {
                    "Feature",
                    new List<string>() {
                "In Progress",
                "New",
                "Done",
            }
                },
                {
                    "Product Backlog Item",
                    new List<string>() {
                "Approved",
                "Committed",
                "New",
                "Done",
            }
                },
                {
                    "Bug",
                    new List<string>() {
                "Approved",
                "Committed",
                "New",
                "Done",
            }
                },
                {
                    "Task",
                    new List<string>() {
                "In Progress",
                "To Do",
                "Done",
            }
                }
            };
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = new HttpResponseMessage();
                HttpRequestMessage request = new HttpRequestMessage();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                foreach (var type in dic.Keys)
                {
                    var res = GetListOfWorkItems_ByWiql(ProjectName, type);
                    WorkIds.Clear();
                    foreach (var ids in res.workItems)
                    {
                        WorkIds.Add(ids.id);
                    }
                    foreach (var id in WorkIds)
                    {
                        if (type == "Feature" || type == "Product Backlog Item" || type == "Bug" || type == "Task")
                        {
                            itemType = dic[type][new Random().Next(0, dic[type].Count)];
                            itemTypes = Tags[new Random().Next(0, Tags.Count)];
                            UserType = Users[new Random().Next(0, Users.Count)];
                            JsonType = json.Replace("$State$", itemType.ToString());
                            JsonTypes = JsonType.Replace("$Tags$", itemTypes.ToString());
                            JSON_Users = JsonTypes.Replace("$Users$", UserType.ToString());
                        }

                        var jsonContent = new StringContent(JSON_Users, Encoding.UTF8, "application/json-patch+json");
                        var method = new HttpMethod("PATCH");
                        // send the request
                        request = new HttpRequestMessage(method, _configuration.UriString + "_apis/wit/workitems/" + id + "?bypassRules=true&api-version=" + _configuration.VersionNumber) { Content = jsonContent };
                        response = client.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            response.StatusCode = System.Net.HttpStatusCode.OK;
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            this.lastFailureMessage = error;
                        }
                    }
                }
                //return true;
            }
            return false;

        }

        /// <summary>
        /// Method to Get the list of all workItems
        /// </summary>
        /// <param name="project"></param>
        /// <param name="WorkItemType"></param>
        /// <returns></returns>
        public GetWorkItemsResponse.Results GetListOfWorkItems_ByWiql(string project, string WorkItemType)
        {
            GetWorkItemsResponse.Results viewModel = new GetWorkItemsResponse.Results();

            //create wiql object
            Object wiql = new
            {
                query = "Select [Work Item Type],[State], [Title],[Created By] " +
                        "From WorkItems " +
                        "Where [Work Item Type] = '" + WorkItemType + "' " +
                        "And [System.TeamProject] = '" + project + "' " +
                        "And [System.State] = 'New' " +
                        "Order By [State] Asc"
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                var postValue = new StringContent(JsonConvert.SerializeObject(wiql), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call

                // set the httpmethod to Patch
                var method = new HttpMethod("POST");

                // send the request               
                var request = new HttpRequestMessage(method, _configuration.UriString + "_apis/wit/wiql?api-version=" + _configuration.VersionNumber) { Content = postValue };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    viewModel = response.Content.ReadAsAsync<GetWorkItemsResponse.Results>().Result;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.lastFailureMessage = error;
                }
                viewModel.HttpStatusCode = response.StatusCode;
                return viewModel;
            }
        }
    }
}