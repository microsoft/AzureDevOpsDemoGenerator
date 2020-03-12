using NLog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using AzureDevOpsAPI.Viewmodel.Sprint;
using AzureDevOpsAPI.Viewmodel.WorkItem;
using System.IO;
using AzureDevOpsRestApi.Viewmodel.ProjectAndTeams;

namespace AzureDevOpsAPI.WorkItemAndTracking
{
    public partial class ClassificationNodes : ApiServiceBase
    {
        public ClassificationNodes(IAppConfiguration configuration) : base(configuration) { }
        Logger logger = LogManager.GetLogger("*");
        /// <summary>
        /// Get Iteration
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public GetNodesResponse.Nodes GetIterations(string projectName)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    GetNodesResponse.Nodes viewModel = new GetNodesResponse.Nodes();
                    using (HttpClient client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(string.Format("{0}/_apis/wit/classificationNodes/iterations?$depth=5&api-version=" + Configuration.VersionNumber, projectName)).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                viewModel = response.Content.ReadAsAsync<GetNodesResponse.Nodes>().Result;
                                return viewModel;
                            }
                            else
                            {
                                var errorMessage = response.Content.ReadAsStringAsync();
                                string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                                this.LastFailureMessage = error;
                                retryCount++;
                            }
                            viewModel.HttpStatusCode = response.StatusCode;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new GetNodesResponse.Nodes();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new GetNodesResponse.Nodes();
        }

        /// <summary>
        /// Create Iterations
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public GetNodeResponse.Node CreateIteration(string projectName, string path)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    CreateUpdateNodeViewModel.Node node = new CreateUpdateNodeViewModel.Node()
                    {
                        Name = path
                    };
                    GetNodeResponse.Node viewModel = new GetNodeResponse.Node();
                    using (HttpClient client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, Configuration.UriString + "/" + projectName + "/_apis/wit/classificationNodes/iterations?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            viewModel = response.Content.ReadAsAsync<GetNodeResponse.Node>().Result;
                            return viewModel;
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            retryCount++;
                        }
                        viewModel.HttpStatusCode = response.StatusCode;
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new GetNodeResponse.Node();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new GetNodeResponse.Node();
        }

        /// <summary>
        /// movie Iteration to parent
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="targetIteration"></param>
        /// <param name="sourceIterationId"></param>
        /// <returns></returns>
        public GetNodeResponse.Node MoveIteration(string projectName, string targetIteration, int sourceIterationId)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    CreateUpdateNodeViewModel.Node node = new CreateUpdateNodeViewModel.Node()
                    {
                        Id = sourceIterationId
                    };
                    GetNodeResponse.Node viewModel = new GetNodeResponse.Node();

                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, string.Format("/{0}/_apis/wit/classificationNodes/iterations/{1}?api-version=" + Configuration.VersionNumber, projectName, targetIteration)) { Content = jsonContent };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            viewModel = response.Content.ReadAsAsync<GetNodeResponse.Node>().Result;
                            return viewModel;
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            retryCount++;
                        }

                        viewModel.HttpStatusCode = response.StatusCode;
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new GetNodeResponse.Node();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new GetNodeResponse.Node();
        }

        /// <summary>
        /// Update Iteration Dates- calculating from previous 22 days
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="templateType"></param>
        /// <returns></returns>
        public bool UpdateIterationDates(string projectName, string templateType, string selectedTemplateName, string teamIterationMapJson)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    // team iteration changes
                    if (File.Exists(teamIterationMapJson))
                    {
                        teamIterationMapJson = File.ReadAllText(teamIterationMapJson);
                        if (!string.IsNullOrEmpty(teamIterationMapJson))
                        {
                            string project = projectName;
                            DateTime startDate = DateTime.Today;
                            DateTime endDate = DateTime.Today.AddDays(12);

                            TeamIterations.Map iterationMaps = new TeamIterations.Map();
                            iterationMaps = JsonConvert.DeserializeObject<TeamIterations.Map>(teamIterationMapJson);
                            if (iterationMaps.TeamIterationMap.Count > 0)
                            {
                                int i = 0;
                                foreach (var iterationTeam in iterationMaps.TeamIterationMap)
                                {
                                    if (i % 2 == 1)
                                    {
                                        startDate = DateTime.Today;
                                        endDate = DateTime.Today.AddDays(18);
                                    }
                                    foreach (var iteration in iterationTeam.Iterations)
                                    {

                                        Dictionary<string, string[]> sprint_dictionary = new Dictionary<string, string[]>();
                                        sprint_dictionary.Add(iteration, new string[] { startDate.ToShortDateString(), endDate.ToShortDateString() });
                                        foreach (var key in sprint_dictionary.Keys)
                                        {
                                            UpdateIterationDates(project, key, startDate, endDate);
                                            if (i % 2 == 1)
                                            {
                                                startDate = endDate.AddDays(1);
                                                endDate = startDate.AddDays(18);
                                            }
                                            else
                                            {
                                                startDate = endDate.AddDays(1);
                                                endDate = startDate.AddDays(12);
                                            }
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                    }
                    else
                    {
                        string project = projectName;
                        DateTime startDate = DateTime.Today.AddDays(-22);
                        DateTime endDate = DateTime.Today.AddDays(-1);

                        Dictionary<string, string[]> sprint_dictionary = new Dictionary<string, string[]>();

                        if (string.IsNullOrWhiteSpace(templateType) || templateType.ToLower() == TemplateType.Scrum.ToString().ToLower())
                        {
                            for (int i = 1; i <= 6; i++)
                            {
                                sprint_dictionary.Add("Sprint " + i, new string[] { startDate.ToShortDateString(), endDate.ToShortDateString() });
                            }
                        }
                        else if (string.IsNullOrWhiteSpace(templateType) || templateType.ToLower() == TemplateType.Basic.ToString().ToLower())
                        {
                            for (int i = 1; i <= 1; i++)
                            {
                                sprint_dictionary.Add("Sprint " + i, new string[] { startDate.ToShortDateString(), endDate.ToShortDateString() });
                            }
                        }
                        else
                        {
                            for (int i = 1; i <= 3; i++)
                            {
                                sprint_dictionary.Add("Iteration " + i, new string[] { startDate.ToShortDateString(), endDate.ToShortDateString() });
                            }
                        }

                        foreach (var key in sprint_dictionary.Keys)
                        {
                            UpdateIterationDates(project, key, startDate, endDate);
                            startDate = endDate.AddDays(1);
                            endDate = startDate.AddDays(21);
                        }
                    }
                    return true;
                }
                catch (OperationCanceledException opr)
                {
                    logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t OperationCanceledException: " + opr.Message + "\n" + opr.StackTrace + "\n");
                    LastFailureMessage = opr.Message + " ," + opr.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return false;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
                catch (Exception ex)
                {
                    logger.Debug("\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
        /// Update Iteration Dates
        /// </summary>
        /// <param name="project"></param>
        /// <param name="path"></param>
        /// <param name="startDate"></param>
        /// <param name="finishDate"></param>
        /// <returns></returns>
        public GetNodeResponse.Node UpdateIterationDates(string project, string path, DateTime startDate, DateTime finishDate)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    CreateUpdateNodeViewModel.Node node = new CreateUpdateNodeViewModel.Node()
                    {
                        //name = path,
                        Attributes = new CreateUpdateNodeViewModel.Attributes()
                        {
                            StartDate = startDate,
                            FinishDate = finishDate
                        }
                    };

                    GetNodeResponse.Node viewModel = new GetNodeResponse.Node();

                    using (var client = GetHttpClient())
                    {
                        // serialize the fields array into a json string          
                        var patchValue = new StringContent(JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                        var method = new HttpMethod("PATCH");

                        // send the request
                        var request = new HttpRequestMessage(method, project + "/_apis/wit/classificationNodes/iterations/" + path + "?api-version=" + Configuration.VersionNumber) { Content = patchValue };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            viewModel = response.Content.ReadAsAsync<GetNodeResponse.Node>().Result;
                            viewModel.Message = "success";
                        }
                        else
                        {
                            dynamic responseForInvalidStatusCode = response.Content.ReadAsAsync<dynamic>();
                            Newtonsoft.Json.Linq.JContainer msg = responseForInvalidStatusCode.Result;
                            viewModel.Message = msg.ToString();

                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            this.LastFailureMessage = error;
                            retryCount++;
                        }

                        viewModel.HttpStatusCode = response.StatusCode;

                        return viewModel;
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new GetNodeResponse.Node();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new GetNodeResponse.Node();
        }

        /// <summary>
        /// Rename Iteration
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="iterationToUpdate"></param>
        /// <returns></returns>
        public bool RenameIteration(string projectName, Dictionary<string, string> iterationToUpdate)
        {
            bool isSuccessful = false;
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    foreach (var key in iterationToUpdate.Keys)
                    {
                        CreateUpdateNodeViewModel.Node node = new CreateUpdateNodeViewModel.Node()
                        {
                            Name = iterationToUpdate[key]
                        };

                        using (var client = GetHttpClient())
                        {
                            // serialize the fields array into a json string          
                            var patchValue = new StringContent(JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                            var method = new HttpMethod("PATCH");

                            // send the request
                            var request = new HttpRequestMessage(method, projectName + "/_apis/wit/classificationNodes/Iterations/" + key + "?api-version=" + Configuration.VersionNumber) { Content = patchValue };
                            var response = client.SendAsync(request).Result;

                            if (response.IsSuccessStatusCode)
                            {
                                isSuccessful = true;
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
                    string project = projectName;
                    DateTime startDate = DateTime.Today.AddDays(-22);
                    DateTime endDate = DateTime.Today.AddDays(-1);

                    Dictionary<string, string[]> sprintDic = new Dictionary<string, string[]>();
                    for (int i = 1; i <= iterationToUpdate.Count; i++)
                    {
                        sprintDic.Add("Sprint " + i, new string[] { startDate.ToShortDateString(), endDate.ToShortDateString() });
                    }
                    foreach (var key in sprintDic.Keys)
                    {
                        UpdateIterationDates(project, key, startDate, endDate);
                        startDate = endDate.AddDays(1);
                        endDate = startDate.AddDays(21);
                    }

                    return isSuccessful;
                }
                catch (Exception ex)
                {
                    logger.Debug("\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return isSuccessful;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return isSuccessful;
        }

        public SprintResponse.Sprints GetSprints(string project)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    SprintResponse.Sprints sprints = new SprintResponse.Sprints();
                    using (var client = GetHttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(project + "/" + project + "%20Team/_apis/work/teamsettings/iterations?api-version=" + Configuration.VersionNumber).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            sprints = JsonConvert.DeserializeObject<SprintResponse.Sprints>(result);
                            return sprints;
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
                    logger.Debug("\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new SprintResponse.Sprints();
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new SprintResponse.Sprints();
        }
    }
}