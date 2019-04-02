﻿using log4net;
using Newtonsoft.Json;
using System;
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
        private ILog logger = LogManager.GetLogger("ErrorLog");
        /// <summary>
        /// Update kanban board colums styles
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool UpdateBoard(string projectName, string json, string boardType, string teamName)
        {
            try
            {
                List<Scrum.Columns> scrumColumns = new List<Scrum.Columns>();

                string newColID = "";
                string doneColID = "";
                GetBoardColumnResponse.ColumnResponse currColumns = new GetBoardColumnResponse.ColumnResponse();

                scrumColumns = JsonConvert.DeserializeObject<List<Scrum.Columns>>(json);

                currColumns = GetBoardColumns(projectName, teamName, boardType);
                if (currColumns.columns != null)
                {
                    foreach (GetBoardColumnResponse.Value col in currColumns.columns)
                    {
                        if (col.columnType.ToLower() == "incoming")
                        {
                            newColID = col.id;
                        }
                        else if (col.columnType.ToLower() == "outgoing")
                        {
                            doneColID = col.id;
                        }
                    }
                    foreach (Scrum.Columns col in scrumColumns)
                    {
                        if (col.columnType.ToLower() == "incoming")
                        {
                            col.id = newColID;
                        }
                        else if (col.columnType.ToLower() == "outgoing")
                        {
                            col.id = doneColID;
                        }
                    }
                }
                if (currColumns.columns == null)
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
                    var request = new HttpRequestMessage(method, _configuration.UriString + "/" + projectName + "/" + teamName + "/_apis/work/boards/" + boardType + "/columns?api-version=" + _configuration.VersionNumber) { Content = patchValue };
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
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateNewTeam" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
            try
            {
                GetBoardColumnResponse.ColumnResponse columns = new GetBoardColumnResponse.ColumnResponse();
                using (var client = GetHttpClient())
                {
                    var response = client.GetAsync(_configuration.UriString + "/" + projectName + "/" + teamName + "/_apis/work/boards/" + boardType + "?api-version=" + _configuration.VersionNumber).Result;
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
            catch(Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateNewTeam" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return new GetBoardColumnResponse.ColumnResponse();
        }
        public GetBoardColumnResponseAgile.ColumnResponse GetBoardColumnsAgile(string projectName, string teamName)
        {
            try
            {
                GetBoardColumnResponseAgile.ColumnResponse columns = new GetBoardColumnResponseAgile.ColumnResponse();
                using (var client = GetHttpClient())
                {

                    var response = client.GetAsync(_configuration.UriString + "/" + projectName + "/" + teamName + "/_apis/work/boards/Stories?api-version=" + _configuration.VersionNumber).Result;
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
            catch(Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateNewTeam" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return new GetBoardColumnResponseAgile.ColumnResponse();
        }
    }
}
