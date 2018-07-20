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
    public class IssueWI
    {
        /// <summary>
        /// Create Issue Work Items
        /// </summary>
        /// <param name="Credential"></param>
        /// <param name="version"></param>
        /// <param name="URL"></param>
        /// <param name="issuenName"></param>
        /// <param name="description"></param>
        /// <param name="projectId"></param>
        public void CreateIssueWI(string Credential, string version, string URL, string issuenName, string description, string projectId)
        {
            try
            {
                Object[] patchDocument = new Object[2];
                patchDocument[0] = new { op = "add", path = "/fields/System.Title", value = issuenName };
                patchDocument[1] = new { op = "add", path = "/fields/System.Description", value = description };

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Credential);

                    // serialize the fields array into a json string          
                    var patchValue = new StringContent(JsonConvert.SerializeObject(patchDocument), Encoding.UTF8, "application/json-patch+json"); // mediaType needs to be application/json-patch+json for a patch call

                    var method = new HttpMethod("PATCH");
                    var request = new HttpRequestMessage(method, URL + "DefaultCollection/" + projectId + "/_apis/wit/workitems/$Issue?api-version=" + version) { Content = patchValue };
                    var response = client.SendAsync(request).Result;
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Create Report work items
        /// </summary>
        /// <param name="Credential"></param>
        /// <param name="version"></param>
        /// <param name="URL"></param>
        /// <param name="websiteUrl"></param>
        /// <param name="ReportName"></param>
        /// <param name="AccountName"></param>
        /// <param name="TemplateName"></param>
        /// <param name="projectId"></param>
        /// <param name="Region"></param>
        public void CreateReportWI(string Credential, string version, string URL, string websiteUrl, string ReportName, string AccountName, string TemplateName, string projectId, string Region)
        {
            try
            {
                if (string.IsNullOrEmpty(Region))
                {
                    Region = "";
                }

                Object[] patchDocument = new Object[5];

                patchDocument[0] = new { op = "add", path = "/fields/System.Title", value = ReportName };
                patchDocument[1] = new { op = "add", path = "/fields/CustomAgile.SiteName", value = websiteUrl };
                patchDocument[2] = new { op = "add", path = "/fields/CustomAgile.AccountName", value = AccountName };
                patchDocument[3] = new { op = "add", path = "/fields/CustomAgile.TemplateName", value = TemplateName };
                patchDocument[4] = new { op = "add", path = "/fields/CustomAgile.Region", value = Region };

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Credential);

                    // serialize the fields array into a json string          
                    var patchValue = new StringContent(JsonConvert.SerializeObject(patchDocument), Encoding.UTF8, "application/json-patch+json"); // mediaType needs to be application/json-patch+json for a patch call

                    var method = new HttpMethod("PATCH");
                    var request = new HttpRequestMessage(method, URL + "DefaultCollection/" + projectId + "/_apis/wit/workitems/$Analytics?api-version=" + version) { Content = patchValue };
                    var response = client.SendAsync(request).Result;
                }
            }
            catch (Exception)
            {

            }
        }


    }
}
