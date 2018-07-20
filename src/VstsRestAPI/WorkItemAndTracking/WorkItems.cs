using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VstsRestAPI.Viewmodel.WorkItem;

namespace VstsRestAPI.WorkItemAndTracking
{
    public class WorkItems
    {

        public string LastFailureMessage;
        readonly IConfiguration _configuration;
        readonly string _credentials;

        public WorkItems(IConfiguration configuration)
        {
            _configuration = configuration;
            _credentials = configuration.PersonalAccessToken;//Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _configuration.PersonalAccessToken)));
        }

        /// <summary>
        /// Create Work items bypassing all rules
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public WorkItemPatchResponse.WorkItem CreateWorkItemUsingByPassRules(string json)
        {
            WorkItemPatchResponse.WorkItem viewModel = new WorkItemPatchResponse.WorkItem();
            WorkItemPatch.Field[] fields = new WorkItemPatch.Field[3];

            
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                // serialize the fields array into a json string          
                var patchValue = new StringContent(JsonConvert.SerializeObject(fields), Encoding.UTF8, "application/json-patch+json"); // mediaType needs to be application/json-patch+json for a patch call

                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json-patch+json");
                // set the httpmethod to Patch
                var method = new HttpMethod("PATCH");

                // send the request
                var request = new HttpRequestMessage(method, _configuration.UriString + "TestProject" + "/_apis/wit/workitems/$" + "User Story" + "?bypassRules=true&api-version=2.2") { Content = jsonContent };
                var response = client.SendAsync(request).Result;

                var me = response.ToString();

                if (response.IsSuccessStatusCode)
                {
                    viewModel = response.Content.ReadAsAsync<WorkItemPatchResponse.WorkItem>().Result;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                }

                viewModel.HttpStatusCode = response.StatusCode;
                return viewModel;
            }
        }

        /// <summary>
        /// Add work items links
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public WorkItemPatchResponse.WorkItem AddLink(string json)
        {
            WorkItemPatchResponse.WorkItem viewModel = new WorkItemPatchResponse.WorkItem();
            //string json = System.IO.File.ReadAllText(Server.MapPath("~") + @"\JSON\WorkItemLink.json");
            
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                // serialize the fields array into a json string          
                //var patchValue = new StringContent(JsonConvert.SerializeObject(fields), Encoding.UTF8, "application/json-patch+json"); // mediaType needs to be application/json-patch+json for a patch call
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json-patch+json");
                // set the httpmethod to Patch
                var method = new HttpMethod("PATCH");

                // send the request
                var request = new HttpRequestMessage(method, _configuration.UriString + "TestProject" + "/_apis/wit/workitems/$User Story?api-version=2.2") { Content = jsonContent };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    viewModel = response.Content.ReadAsAsync<WorkItemPatchResponse.WorkItem>().Result;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                }

                viewModel.HttpStatusCode = response.StatusCode;

                return viewModel;
            }
        }

    }
}