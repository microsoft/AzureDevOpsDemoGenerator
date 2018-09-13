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
    public class WorkItems :ApiServiceBase
    {
        public WorkItems(IConfiguration configuration) : base(configuration) { }

        /// <summary>
        /// Create Work items bypassing all rules
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public WorkItemPatchResponse.WorkItem CreateWorkItemUsingByPassRules(string json)
        {
            WorkItemPatchResponse.WorkItem viewModel = new WorkItemPatchResponse.WorkItem();
            WorkItemPatch.Field[] fields = new WorkItemPatch.Field[3];

            
            using (var client = GetHttpClient())
            {
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
            
            using (var client = GetHttpClient())
            {
                // serialize the fields array into a json string          
                //var patchValue = new StringContent(JsonConvert.SerializeObject(fields), Encoding.UTF8, "application/json-patch+json"); // mediaType needs to be application/json-patch+json for a patch call
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json-patch+json");
                // set the httpmethod to Patch
                var method = new HttpMethod("PATCH");

                // send the request
                var request = new HttpRequestMessage(method, "TestProject" + "/_apis/wit/workitems/$User Story?api-version=2.2") { Content = jsonContent };
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