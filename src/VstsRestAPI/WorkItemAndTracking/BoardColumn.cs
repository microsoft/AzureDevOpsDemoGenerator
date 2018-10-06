using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using VstsRestAPI.Viewmodel.WorkItem;

namespace VstsRestAPI.WorkItemAndTracking
{
    public partial class BoardColumn : ApiServiceBase
    {
        public string rowFieldName;
        public BoardColumn(IConfiguration configuration) : base(configuration) { }

        /// <summary>
        /// Update kanban board colums styles
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool UpdateBoard(string projectName, string fileName, string BoardType)
        {
            string teamName = projectName + " Team";
            List<Scrum.Columns> SColumns = new List<Scrum.Columns>();
            List<Agile.Columns> AColumns = new List<Agile.Columns>();

            if (BoardType == "Backlog%20Items")
            {
                SColumns = JsonConvert.DeserializeObject<List<Scrum.Columns>>(fileName);
            }
            else if (BoardType == "Stories")
            {
                AColumns = JsonConvert.DeserializeObject<List<Agile.Columns>>(fileName);
            }
            GetBoardColumnResponse.ColumnResponse currColumns = new GetBoardColumnResponse.ColumnResponse();
            GetBoardColumnResponseAgile.ColumnResponse currColumnsAgile = new GetBoardColumnResponseAgile.ColumnResponse();
            if (BoardType == "Backlog%20Items")
            {
                currColumns = getBoardColumns(projectName, teamName);
            }
            else if (BoardType == "Stories")
            {
                currColumnsAgile = getBoardColumnsAgile(projectName, teamName);
            }

            if (currColumns.columns == null && currColumnsAgile.columns == null)
            {
                return false;
            }

            string newColID = "";
            string doneColID = "";
            if (BoardType == "Backlog%20Items")
            {
                foreach (GetBoardColumnResponse.Value col in currColumns.columns)
                {
                    if (col.name == "New")
                    {
                        newColID = col.id;
                    }
                    else if (col.name == "Done" || col.name == "Closed")
                    {
                        doneColID = col.id;
                    }
                }
                foreach (Scrum.Columns col in SColumns)
                {
                    if (col.name == "New")
                    {
                        col.id = newColID;
                    }
                    else if (col.name == "Done" || col.name == "Deploy" || col.name == "Closed")
                    {
                        col.id = doneColID;
                    }
                }
            }
            else if (BoardType == "Stories")
            {
                foreach(GetBoardColumnResponseAgile.Value col in currColumnsAgile.columns)
                {
                    if (col.name == "New")
                    {
                        newColID = col.id;
                    }
                    else if (col.name == "Done" || col.name == "Closed")
                    {
                        doneColID = col.id;
                    }
                }
                foreach (Agile.Columns col in AColumns)
                {
                    if (col.name == "New")
                    {
                        col.id = newColID;
                    }
                    else if (col.name == "Done" || col.name == "Deploy" || col.name == "Closed")
                    {
                        col.id = doneColID;
                    }
                }
            }

            using (var client = GetHttpClient())
            {
                StringContent patchValue = new StringContent("");
                if (BoardType == "Backlog%20Items")
                {
                    patchValue = new StringContent(JsonConvert.SerializeObject(SColumns), Encoding.UTF8, "application/json");
                }
                else if (BoardType == "Stories")
                {
                    patchValue = new StringContent(JsonConvert.SerializeObject(AColumns), Encoding.UTF8, "application/json");
                }
                // mediaType needs to be application/json-patch+json for a patch call
                var method = new HttpMethod("PUT");
                //PUT https://dev.azure.com/{organization}/{project}/{team}/_apis/work/boards/{board}/columns?api-version=4.1
                var request = new HttpRequestMessage(method, "/" + projectName + "/" + teamName + "/_apis/work/boards/" + BoardType + "/columns?api-version=4.1") { Content = patchValue };
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
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
            using (var client = GetHttpClient())
            {
                //var patchValue = new StringContent(JsonConvert.SerializeObject(Columns), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
                // var method = new HttpMethod("GET");

                var response = client.GetAsync(_configuration.UriString + "/" + projectName + "/" + teamName + "/_apis/work/boards/Backlog%20Items?api-version=" + _configuration.VersionNumber + "-preview").Result;
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
                    this.LastFailureMessage = error;
                    return new GetBoardColumnResponse.ColumnResponse();
                }
            }
        }
        public GetBoardColumnResponseAgile.ColumnResponse getBoardColumnsAgile(string projectName, string teamName)
        {
            GetBoardColumnResponseAgile.ColumnResponse columns = new GetBoardColumnResponseAgile.ColumnResponse();
            using (var client = GetHttpClient())
            {
                //var patchValue = new StringContent(JsonConvert.SerializeObject(Columns), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
                // var method = new HttpMethod("GET");

                var response = client.GetAsync(_configuration.UriString + "/" + projectName + "/" + teamName + "/_apis/work/boards/Stories?api-version=" + _configuration.VersionNumber + "-preview").Result;
                // var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    columns = response.Content.ReadAsAsync<GetBoardColumnResponseAgile.ColumnResponse>().Result;
                    this.rowFieldName = columns.fields.rowField.referenceName;
                    return columns;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                    return new GetBoardColumnResponseAgile.ColumnResponse();
                }
            }
        }
        /// <summary>
        /// Refresh kanban board
        /// </summary>
        /// <param name="projectName"></param>
        public void RefreshBoard(string projectName)
        {
            using (var client = GetHttpClient())
            {
                //var patchValue = new StringContent(JsonConvert.SerializeObject(Columns), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
                // var method = new HttpMethod("GET");

                var response = client.GetAsync("/" + projectName + "/_backlogs/board/Backlog%20items").Result;
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
