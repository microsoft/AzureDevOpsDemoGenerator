using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using VstsRestAPI.Viewmodel.Sprint;
using VstsRestAPI.Viewmodel.WorkItem;

namespace VstsRestAPI.WorkItemAndTracking
{
    public partial class ClassificationNodes : ApiServiceBase
    {
        public ClassificationNodes(IConfiguration configuration) : base(configuration) { }
        private ILog logger = LogManager.GetLogger("ErrorLog");
        /// <summary>
        /// Get Iteration
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public GetNodesResponse.Nodes GetIterations(string projectName)
        {
            try
            {
                GetNodesResponse.Nodes viewModel = new GetNodesResponse.Nodes();
                using (HttpClient client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(string.Format("{0}/_apis/wit/classificationNodes/iterations?$depth=5&api-version=" + _configuration.VersionNumber, projectName)).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            viewModel = response.Content.ReadAsAsync<GetNodesResponse.Nodes>().Result;
                            return viewModel;
                        }
                        viewModel.HttpStatusCode = response.StatusCode;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateNewTeam" + "\t" + ex.Message + "\t"   + "\n" + ex.StackTrace + "\n");
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
            try
            {
                CreateUpdateNodeViewModel.Node node = new CreateUpdateNodeViewModel.Node()
                {
                    name = path
                };
                GetNodeResponse.Node viewModel = new GetNodeResponse.Node();
                using (HttpClient client = GetHttpClient())
                {
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                    var method = new HttpMethod("POST");

                    var request = new HttpRequestMessage(method, _configuration.UriString + "/" + projectName + "/_apis/wit/classificationNodes/iterations?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
                    var response = client.SendAsync(request).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        viewModel = response.Content.ReadAsAsync<GetNodeResponse.Node>().Result;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                    }
                    viewModel.HttpStatusCode = response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t"   + "\n" + ex.StackTrace + "\n");
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
            try
            {
                CreateUpdateNodeViewModel.Node node = new CreateUpdateNodeViewModel.Node()
                {
                    id = sourceIterationId
                };
                GetNodeResponse.Node viewModel = new GetNodeResponse.Node();

                using (var client = GetHttpClient())
                {
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                    var method = new HttpMethod("POST");

                    var request = new HttpRequestMessage(method, string.Format("/{0}/_apis/wit/classificationNodes/iterations/{1}?api-version=" + _configuration.VersionNumber, projectName, targetIteration)) { Content = jsonContent };
                    var response = client.SendAsync(request).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        viewModel = response.Content.ReadAsAsync<GetNodeResponse.Node>().Result;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                    }

                    viewModel.HttpStatusCode = response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t"   + "\n" + ex.StackTrace + "\n");
            }
            return new GetNodeResponse.Node();
        }

        /// <summary>
        /// Update Iteration Dates- calculating from previous 22 days
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="templateType"></param>
        /// <returns></returns>
        public bool UpdateIterationDates(string projectName, string templateType)
        {
            try
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
                return true;
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t"   + "\n" + ex.StackTrace + "\n");
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
            try
            {
                CreateUpdateNodeViewModel.Node node = new CreateUpdateNodeViewModel.Node()
                {
                    //name = path,
                    attributes = new CreateUpdateNodeViewModel.Attributes()
                    {
                        startDate = startDate,
                        finishDate = finishDate
                    }
                };

                GetNodeResponse.Node viewModel = new GetNodeResponse.Node();

                using (var client = GetHttpClient())
                {
                    // serialize the fields array into a json string          
                    var patchValue = new StringContent(JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                    var method = new HttpMethod("PATCH");

                    // send the request
                    var request = new HttpRequestMessage(method, project + "/_apis/wit/classificationNodes/iterations/" + path + "?api-version=" + _configuration.VersionNumber) { Content = patchValue };
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
                    }

                    viewModel.HttpStatusCode = response.StatusCode;

                    return viewModel;
                }
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t"   + "\n" + ex.StackTrace + "\n");
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
            try
            {
                foreach (var key in iterationToUpdate.Keys)
                {
                    CreateUpdateNodeViewModel.Node node = new CreateUpdateNodeViewModel.Node()
                    {
                        name = iterationToUpdate[key]
                    };

                    using (var client = GetHttpClient())
                    {
                        // serialize the fields array into a json string          
                        var patchValue = new StringContent(JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                        var method = new HttpMethod("PATCH");

                        // send the request
                        var request = new HttpRequestMessage(method, projectName + "/_apis/wit/classificationNodes/Iterations/" + key + "?api-version=" + _configuration.VersionNumber) { Content = patchValue };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            isSuccessful = true;
                        }
                    }
                }
                string project = projectName;
                DateTime StartDate = DateTime.Today.AddDays(-22);
                DateTime EndDate = DateTime.Today.AddDays(-1);

                Dictionary<string, string[]> sprint_dic = new Dictionary<string, string[]>();
                for (int i = 1; i <= iterationToUpdate.Count; i++)
                {
                    sprint_dic.Add("Sprint " + i, new string[] { StartDate.ToShortDateString(), EndDate.ToShortDateString() });
                }
                foreach (var key in sprint_dic.Keys)
                {
                    UpdateIterationDates(project, key, StartDate, EndDate);
                    StartDate = EndDate.AddDays(1);
                    EndDate = StartDate.AddDays(21);
                }

            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t"   + "\n" + ex.StackTrace + "\n");
            }
            return isSuccessful;
        }

        public SprintResponse.Sprints GetSprints(string project)
        {
            try
            {
                SprintResponse.Sprints sprints = new SprintResponse.Sprints();
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(project + "/" + project + "%20Team/_apis/work/teamsettings/iterations?api-version=" + _configuration.VersionNumber).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        sprints = JsonConvert.DeserializeObject<SprintResponse.Sprints>(result);
                        return sprints;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t"   + "\n" + ex.StackTrace + "\n");
            }
            return new SprintResponse.Sprints();
        }
    }
}
