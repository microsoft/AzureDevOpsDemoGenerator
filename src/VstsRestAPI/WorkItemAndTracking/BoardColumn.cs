using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VstsRestAPI.Viewmodel.WorkItem;

namespace VstsRestAPI.WorkItemAndTracking
{
    public partial class BoardColumn
    {
        public string rowFieldName;
        public string lastFailureMessage;
        readonly IConfiguration _configuration;
        readonly string _credentials;

        public BoardColumn(IConfiguration configuration)
        {
            _configuration = configuration;
            _credentials = configuration.PersonalAccessToken;//Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _configuration.PersonalAccessToken)));
        }

        /// <summary>
        /// Update kanban board colums styles
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool UpdateBoard(string projectName, string fileName)
        {
            string teamName = projectName + " Team";
            List<ColumnPost> Columns = JsonConvert.DeserializeObject<List<ColumnPost>>(fileName);

            GetBoardColumnResponse.ColumnResponse currColumns = getBoardColumns(projectName, teamName);
            if (currColumns.columns == null) return false;

            string newColID = "";
            string doneColID = "";
            foreach (GetBoardColumnResponse.Value col in currColumns.columns)
            {
                if (col.name == "New")
                {
                    newColID = col.id;
                }
                else if (col.name == "Done")
                {
                    doneColID = col.id;
                }
            }
            foreach (ColumnPost col in Columns)
            {
                if (col.name == "New")
                {
                    col.id = newColID;

                }
                else if (col.name == "Done")
                {
                    col.id = doneColID;
                }
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                var patchValue = new StringContent(JsonConvert.SerializeObject(Columns), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
                var method = new HttpMethod("PUT");

                var request = new HttpRequestMessage(method, _configuration.UriString + "/" + projectName + "/" + teamName + "/_apis/work/boards/Backlog%20items/columns?api-version=" + _configuration.VersionNumber + "-preview") { Content = patchValue };
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.lastFailureMessage = error;
                    return false;
                }
            }
        }

        /// <summary>
        /// Get kanban board columns
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="teamName"></param>
        /// <returns></returns>
        public GetBoardColumnResponse.ColumnResponse getBoardColumns(string projectName, string teamName)
        {
            GetBoardColumnResponse.ColumnResponse columns = new GetBoardColumnResponse.ColumnResponse();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                //var patchValue = new StringContent(JsonConvert.SerializeObject(Columns), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
                // var method = new HttpMethod("GET");

                var response = client.GetAsync(_configuration.UriString + "/" + projectName + "/" + teamName + "/_apis/work/boards/Backlog%20items?api-version=" + _configuration.VersionNumber + "-preview").Result;
                // var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    columns = response.Content.ReadAsAsync<GetBoardColumnResponse.ColumnResponse>().Result;
                    this.rowFieldName = columns.fields.rowField.referenceName;
                    return columns;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.lastFailureMessage = error;
                    return new GetBoardColumnResponse.ColumnResponse();
                }
            }
        }
        /// <summary>
        /// Refresh kanban board
        /// </summary>
        /// <param name="projectName"></param>
        public void RefreshBoard(string projectName)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                //var patchValue = new StringContent(JsonConvert.SerializeObject(Columns), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
                // var method = new HttpMethod("GET");

                var response = client.GetAsync(_configuration.UriString + "/" + projectName + "/_backlogs/board/Backlog%20items").Result;
                // var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {

                }
                else
                {

                }
            }
        }
    }
}
