using NLog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using AzureDevOpsAPI.Viewmodel.Extractor;
using Microsoft.ApplicationInsights;

namespace AzureDevOpsAPI.Extractor
{
    public class ClassificationNodes : ApiServiceBase
    {
        private TelemetryClient ai;
        public ClassificationNodes(IAppConfiguration configuration, TelemetryClient _ai) : base(configuration) { ai = _ai; }
         Logger logger = LogManager.GetLogger("*");
        // Get Iteration Count
        public GetINumIteration.Iterations GetiterationCount()
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(Configuration.UriString + Configuration.Project + "/_apis/work/teamsettings/iterations?api-version=" + Configuration.VersionNumber).Result;
                        if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            GetINumIteration.Iterations getINum = JsonConvert.DeserializeObject<GetINumIteration.Iterations>(result);
                            return getINum;
                        }
                        else
                        {
                            string errorMessage = response.Content.ReadAsStringAsync().Result;
                            LastFailureMessage = errorMessage;
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug(ex.Message + ex.StackTrace + "\n");
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new GetINumIteration.Iterations();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new GetINumIteration.Iterations();
        }

        // Get Team List to write to file
        public TeamList ExportTeamList(string defaultTeamId)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {

                _ = new TeamList();
                try
                {
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(Configuration.UriString + "/_apis/projects/" + Project + "/teams?api-version=" + Configuration.VersionNumber).Result;
                        if (!response.IsSuccessStatusCode || response.StatusCode != HttpStatusCode.OK)
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            LastFailureMessage = error;
                            retryCount++;
                        }
                        else
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            TeamList teamObj = JsonConvert.DeserializeObject<TeamList>(result);
                            foreach (var team in teamObj.Value)
                            {
                                if (team.Id == defaultTeamId)
                                {
                                    team.IsDefault = "true";
                                }
                                else
                                {
                                    team.IsDefault = "false";
                                }
                            }
                            return teamObj;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug(ex.Message + ex.StackTrace + "\n");
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new TeamList(); ;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new TeamList();
        }

        /// <summary>
        /// GET https://dev.azure.com/fabrikam/Fabrikam/Fabrikam Team/_apis/work/boards/{board}/columns?api-version=4.1
        /// </summary>
        public HttpResponseMessage ExportBoardColums(string boardType)
        {
            var response = new HttpResponseMessage();
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        response = client.GetAsync(string.Format("{0}/{1}/{2}/_apis/work/boards/{3}/columns?api-version={4}", Configuration.UriString, Configuration.Project, Configuration.Team, boardType, Configuration.VersionNumber)).Result;
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug(ex.Message + ex.StackTrace + "\n");
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return response;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return response;
        }

        // Export Board Rows to write file
        //GET https://dev.azure.com/{organization}/{project}/{team}/_apis/work/boards/{board}/rows?api-version=4.1
        public ExportBoardRows.Rows ExportBoardRows(string boardType)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(string.Format("{0}/{1}/{2}/_apis/work/boards/{3}/rows?api-version={4}", Configuration.UriString, Project, Team, boardType, Configuration.VersionNumber)).Result;
                        if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            ExportBoardRows.Rows rows = new ExportBoardRows.Rows();
                            rows = JsonConvert.DeserializeObject<ExportBoardRows.Rows>(result);
                            return rows;
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            LastFailureMessage = error;
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug(ex.Message + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new ExportBoardRows.Rows();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new ExportBoardRows.Rows();
        }

        // Get Team setting
        //GET https://dev.azure.com/{organization}/{project}/{team}/_apis/work/teamsettings?api-version=4.1
        public ExportTeamSetting.Setting ExportTeamSetting()
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                _ = new ExportTeamSetting.Setting();
                try
                {
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(string.Format("{0}/{1}/{2}/_apis/work/teamsettings?api-version={3}", Configuration.UriString, Project, Team, Configuration.VersionNumber)).Result;
                        if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            ExportTeamSetting.Setting setting = JsonConvert.DeserializeObject<ExportTeamSetting.Setting>(result);
                            return setting;
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            LastFailureMessage = error;
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug(ex.Message + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new ExportTeamSetting.Setting(); ;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new ExportTeamSetting.Setting();
        }

        //Get Card Fields
        //GET https://dev.azure.com/{organization}/{project}/{team}/_apis/work/boards/{board}/cardsettings?api-version=4.1
        public HttpResponseMessage ExportCardFields(string boardType)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {

                        HttpResponseMessage response = client.GetAsync(string.Format("{0}/{1}/{2}/_apis/work/boards/{3}/cardsettings?api-version={4}", Configuration.UriString, Project, Team, boardType, Configuration.VersionNumber)).Result;
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug(ex.Message + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return null;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return null;
        }

        //Export Card Styles
        //GET https://dev.azure.com/{organization}/{project}/{team}/_apis/work/boards/{board}/cardrulesettings?api-version=4.1
        public HttpResponseMessage ExportCardStyle(string boardType)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(string.Format("{0}/{1}/{2}/_apis/work/boards/{3}/cardrulesettings?api-version={4}", Configuration.UriString, Project, Team, boardType, Configuration.VersionNumber)).Result;
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug(ex.Message + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return null;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return null;
        }

        // Get Iterations to write file
        //GET https://dev.azure.com/{organization}/{project}/{team}/_apis/work/teamsettings/iterations?api-version=4.1
        public ExportedIterations.Iterations ExportIterationsToSave()
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    ExportIterations.Iterations viewModel = new ExportIterations.Iterations();
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(string.Format("{0}/{1}/_apis/work/teamsettings/iterations?api-version={2}", Configuration.UriString, Project, Configuration.VersionNumber)).Result;
                        if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            viewModel = JsonConvert.DeserializeObject<ExportIterations.Iterations>(result);
                            ExportedIterations.Iterations iterations = new ExportedIterations.Iterations();
                            List<ExportedIterations.Child> listchild = new List<ExportedIterations.Child>();

                            if (viewModel.Count > 0)
                            {
                                foreach (var iteration in viewModel.Value)
                                {
                                    ExportedIterations.Child child = new ExportedIterations.Child();
                                    child.Name = iteration.Name;
                                    child.HasChildren = false;
                                    child.StructureType = "iteration";
                                    listchild.Add(child);
                                }
                                iterations.Children = listchild;
                                iterations.HasChildren = true;
                                iterations.Name = Configuration.Project;
                                iterations.StructureType = "iteration";
                                return iterations;
                            }
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            LastFailureMessage = error;
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ai.TrackException(ex);
                    logger.Debug(ex.Message + ex.StackTrace + "\n");
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return null;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }

            return new ExportedIterations.Iterations();
        }
    }
}
