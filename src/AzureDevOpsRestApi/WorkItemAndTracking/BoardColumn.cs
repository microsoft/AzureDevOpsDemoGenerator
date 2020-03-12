using NLog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using AzureDevOpsAPI.Viewmodel.WorkItem;

namespace AzureDevOpsAPI.WorkItemAndTracking
{
    public partial class BoardColumn : ApiServiceBase
    {
        public string RowFieldName;
        public BoardColumn(IAppConfiguration configuration) : base(configuration) { }
         Logger logger = LogManager.GetLogger("*");
        /// <summary>
        /// Update kanban board colums styles
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool UpdateBoard(string projectName, string json, string boardType, string teamName)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    List<Scrum.Columns> scrumColumns = new List<Scrum.Columns>();

                    string newColId = "";
                    string doneColId = "";
                    GetBoardColumnResponse.ColumnResponse currColumns = new GetBoardColumnResponse.ColumnResponse();

                    scrumColumns = JsonConvert.DeserializeObject<List<Scrum.Columns>>(json);

                    currColumns = GetBoardColumns(projectName, teamName, boardType);
                    if (currColumns.Columns != null)
                    {
                        foreach (GetBoardColumnResponse.Value col in currColumns.Columns)
                        {
                            if (col.ColumnType.ToLower() == "incoming")
                            {
                                newColId = col.Id;
                            }
                            else if (col.ColumnType.ToLower() == "outgoing")
                            {
                                doneColId = col.Id;
                            }
                        }
                        foreach (Scrum.Columns col in scrumColumns)
                        {
                            if (col.ColumnType.ToLower() == "incoming")
                            {
                                col.Id = newColId;
                            }
                            else if (col.ColumnType.ToLower() == "outgoing")
                            {
                                col.Id = doneColId;
                            }
                        }
                    }
                    if (currColumns.Columns == null)
                    {
                        return false;
                    }
                    using (var client = GetHttpClient())
                    {
                        StringContent patchValue = new StringContent("");
                        string stringSerialize = JsonConvert.SerializeObject(scrumColumns, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        patchValue = new StringContent(stringSerialize, Encoding.UTF8, "application/json");
                        // mediaType needs to be application/json-patch+json for a patch call
                        var method = new HttpMethod("PUT");
                        //PUT https://dev.azure.com/{organization}/{project}/{team}/_apis/work/boards/{board}/columns?api-version=4.1
                        var request = new HttpRequestMessage(method, Configuration.UriString + "/" + projectName + "/" + teamName + "/_apis/work/boards/" + boardType + "/columns?api-version=" + Configuration.VersionNumber) { Content = patchValue };
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
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("UpdateBoard" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
        /// Get kanban board columns
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="teamName"></param>
        /// <returns></returns>
        public GetBoardColumnResponse.ColumnResponse GetBoardColumns(string projectName, string teamName, string boardType)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    GetBoardColumnResponse.ColumnResponse columns = new GetBoardColumnResponse.ColumnResponse();
                    using (var client = GetHttpClient())
                    {
                        var response = client.GetAsync(Configuration.UriString + "/" + projectName + "/" + teamName + "/_apis/work/boards/" + boardType + "?api-version=" + Configuration.VersionNumber).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            columns = response.Content.ReadAsAsync<GetBoardColumnResponse.ColumnResponse>().Result;
                            this.RowFieldName = columns.Fields.RowField.ReferenceName;
                            return columns;
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
                    logger.Debug("GetBoardColumns" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new GetBoardColumnResponse.ColumnResponse();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new GetBoardColumnResponse.ColumnResponse();
        }

        public GetBoardColumnResponseAgile.ColumnResponse GetBoardColumnsAgile(string projectName, string teamName)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    GetBoardColumnResponseAgile.ColumnResponse columns = new GetBoardColumnResponseAgile.ColumnResponse();
                    using (var client = GetHttpClient())
                    {

                        var response = client.GetAsync(Configuration.UriString + "/" + projectName + "/" + teamName + "/_apis/work/boards/Stories?api-version=" + Configuration.VersionNumber).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            columns = response.Content.ReadAsAsync<GetBoardColumnResponseAgile.ColumnResponse>().Result;
                            this.RowFieldName = columns.Fields.RowField.ReferenceName;
                            return columns;
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
                    logger.Debug("GetBoardColumnsAgile" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new GetBoardColumnResponseAgile.ColumnResponse(); 
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new GetBoardColumnResponseAgile.ColumnResponse();
        }
    }
}
