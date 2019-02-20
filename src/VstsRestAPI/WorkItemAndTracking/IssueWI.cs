using Newtonsoft.Json;
using System;
using VstsRestAPI.Services;

namespace VstsRestAPI.WorkItemAndTracking
{
    public class IssueWI
    {
        private Configuration con = new Configuration();

        // Create Issue Work Items
        public void CreateIssueWI(string credential, string version, string url, string issueName, string description, string projectId, string tag)
        {
            try
            {
                Object[] patchDocument = new Object[3];
                patchDocument[0] = new { op = "add", path = "/fields/System.Title", value = issueName };
                patchDocument[1] = new { op = "add", path = "/fields/System.Description", value = description };
                patchDocument[2] = new { op = "add", path = "/fields/System.Tags", value = tag };


                con.UriString = url;
                con.PersonalAccessToken = credential;
                con.Project = projectId;
                con.VersionNumber = version;
                con.UriParams = "/_apis/wit/workitems/$Issue?api-version=";
                con.RequestBody = JsonConvert.SerializeObject(patchDocument);
                HttpServices httpServices = new HttpServices(con);
                var response = httpServices.PatchBasic();
            }
            catch (Exception)
            {
            }
        }

        // Create Report work items
        public void CreateReportWI(string credential, string version, string url, string websiteUrl, string reportName, string accountName, string templateName, string projectId, string region)
        {
            try
            {
                if (string.IsNullOrEmpty(region))
                {
                    region = "";
                }

                Object[] ReportPatchDocument = new Object[5];

                ReportPatchDocument[0] = new { op = "add", path = "/fields/System.Title", value = reportName };
                ReportPatchDocument[1] = new { op = "add", path = "/fields/CustomAgile.SiteName", value = websiteUrl };
                ReportPatchDocument[2] = new { op = "add", path = "/fields/CustomAgile.AccountName", value = accountName };
                ReportPatchDocument[3] = new { op = "add", path = "/fields/CustomAgile.TemplateName", value = templateName };
                ReportPatchDocument[4] = new { op = "add", path = "/fields/CustomAgile.Region", value = region };

                con.UriString = url;
                con.PersonalAccessToken = credential;
                con.Project = projectId;
                con.VersionNumber = version;
                con.UriParams = "/_apis/wit/workitems/$Analytics?api-version=";
                con.RequestBody = JsonConvert.SerializeObject(ReportPatchDocument);
                HttpServices httpServices = new HttpServices(con);
                var response = httpServices.PatchBasic();
            }
            catch (Exception)
            {

            }
        }
    }
}
