using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace VstsRestAPI.WorkItemAndTracking
{
    public class IssueWI
    {
        /// <summary>
        /// Create Issue Work Items
        /// </summary>
        /// <param name="credential"></param>
        /// <param name="version"></param>
        /// <param name="url"></param>
        /// <param name="issuenName"></param>
        /// <param name="description"></param>
        /// <param name="projectId"></param>
        public void CreateIssueWI(string credential, string version, string url, string issuenName, string description, string projectId)
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
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credential);

                    // serialize the fields array into a json string          
                    var patchValue = new StringContent(JsonConvert.SerializeObject(patchDocument), Encoding.UTF8, "application/json-patch+json"); // mediaType needs to be application/json-patch+json for a patch call

                    var method = new HttpMethod("PATCH");
                    var request = new HttpRequestMessage(method, url + "DefaultCollection/" + projectId + "/_apis/wit/workitems/$Issue?api-version=" + version) { Content = patchValue };
                    var response = client.SendAsync(request).Result;

                    string res = response.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Create Report work items
        /// </summary>
        /// <param name="credential"></param>
        /// <param name="version"></param>
        /// <param name="url"></param>
        /// <param name="websiteUrl"></param>
        /// <param name="reportName"></param>
        /// <param name="accountName"></param>
        /// <param name="templateName"></param>
        /// <param name="projectId"></param>
        /// <param name="region"></param>
        public void CreateReportWI(string credential, string version, string url, string websiteUrl, string reportName, string accountName, string templateName, string projectId, string region)
        {
            try
            {
                if (string.IsNullOrEmpty(region))
                {
                    region = "";
                }

                Object[] patchDocument = new Object[5];

                patchDocument[0] = new { op = "add", path = "/fields/System.Title", value = reportName };
                patchDocument[1] = new { op = "add", path = "/fields/CustomAgile.SiteName", value = websiteUrl };
                patchDocument[2] = new { op = "add", path = "/fields/CustomAgile.AccountName", value = accountName };
                patchDocument[3] = new { op = "add", path = "/fields/CustomAgile.TemplateName", value = templateName };
                patchDocument[4] = new { op = "add", path = "/fields/CustomAgile.Region", value = region };

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credential);

                    // serialize the fields array into a json string          
                    var patchValue = new StringContent(JsonConvert.SerializeObject(patchDocument), Encoding.UTF8, "application/json-patch+json"); // mediaType needs to be application/json-patch+json for a patch call

                    var method = new HttpMethod("PATCH");
                    var request = new HttpRequestMessage(method, url + "DefaultCollection/" + projectId + "/_apis/wit/workitems/$Analytics?api-version=" + version) { Content = patchValue };
                    var response = client.SendAsync(request).Result;
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
