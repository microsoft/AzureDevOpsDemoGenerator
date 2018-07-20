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
    public partial class ClassificationNodes
    {
        public string LastFailureMessage;
        readonly IConfiguration _configuration;
        readonly string _credentials;

        public ClassificationNodes(IConfiguration configuration)
        {
            _configuration = configuration;
            _credentials = configuration.PersonalAccessToken;//Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _configuration.PersonalAccessToken)));
        }
        /// <summary>
        /// Get Iteration
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public GetNodesResponse.Nodes GetIterations(string projectName)
        {
            GetNodesResponse.Nodes viewModel = new GetNodesResponse.Nodes();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_configuration.UriString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                HttpResponseMessage response = client.GetAsync(string.Format("{0}/_apis/wit/classificationNodes/iterations?$depth=5&api-version=1.0", projectName)).Result;
                if (response.IsSuccessStatusCode)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        viewModel = response.Content.ReadAsAsync<GetNodesResponse.Nodes>().Result;
                    }

                    viewModel.HttpStatusCode = response.StatusCode;
                }
            }

            return viewModel;
        }

        /// <summary>
        /// Create Iterations
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public GetNodeResponse.Node CreateIteration(string projectName, string path)
        {
            CreateUpdateNodeViewModel.Node node = new CreateUpdateNodeViewModel.Node()
            {
                name = path
            };

            GetNodeResponse.Node viewModel = new GetNodeResponse.Node();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                // serialize the fields array into a json string  
                //var patchValue = new StringContent(JsonConvert.SerializeObject(team), Encoding.UTF8, "application/json");
                var jsonContent = new StringContent(JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, _configuration.UriString + string.Format("/{0}/_apis/wit/classificationNodes/iterations?api-version=1.0", projectName)) { Content = jsonContent };
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

            return viewModel;
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
            CreateUpdateNodeViewModel.Node node = new CreateUpdateNodeViewModel.Node()
            {
                id = sourceIterationId
            };

            GetNodeResponse.Node viewModel = new GetNodeResponse.Node();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                var jsonContent = new StringContent(JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, _configuration.UriString + string.Format("/{0}/_apis/wit/classificationNodes/iterations/{1}?api-version=1.0", projectName, targetIteration)) { Content = jsonContent };
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

            return viewModel;
        }

        /// <summary>
        /// Update Iteration Dates- calculating from previous 22 days
        /// </summary>
        /// <param name="ProjectName"></param>
        /// <param name="templateType"></param>
        /// <returns></returns>
        public bool UpdateIterationDates(string ProjectName, string templateType)
        {
            string project = ProjectName;
            DateTime StartDate = DateTime.Today.AddDays(-22);
            DateTime EndDate = DateTime.Today.AddDays(-1);

            Dictionary<string, string[]> sprint_dic = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(templateType) || templateType.ToLower() == TemplateType.Scrum.ToString().ToLower())
            {
                for (int i = 1; i <= 6; i++)
                {
                    sprint_dic.Add("Sprint " + i, new string[] { StartDate.ToShortDateString(), EndDate.ToShortDateString() });
                }
            }
            else
            {
                for (int i = 1; i <= 3; i++)
                {
                    sprint_dic.Add("Iteration " + i, new string[] { StartDate.ToShortDateString(), EndDate.ToShortDateString() });
                }
            }

            foreach (var key in sprint_dic.Keys)
            {
                UpdateIterationDates(project, key, StartDate, EndDate);
                StartDate = EndDate.AddDays(1);
                EndDate = StartDate.AddDays(21);
            }
            return true;
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

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                // serialize the fields array into a json string          
                var patchValue = new StringContent(JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                var method = new HttpMethod("PATCH");

                // send the request
                var request = new HttpRequestMessage(method, _configuration.UriString + project + "/_apis/wit/classificationNodes/iterations/" + path + "?api-version=" + _configuration.VersionNumber) { Content = patchValue };
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

        /// <summary>
        /// Rename Iteration
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="IterationToUpdate"></param>
        /// <returns></returns>
        public bool RenameIteration(string projectName, Dictionary<string, string> IterationToUpdate)
        {
            bool isSuccesfull = false;
            try
            {
                foreach (var key in IterationToUpdate.Keys)
                {
                    CreateUpdateNodeViewModel.Node node = new CreateUpdateNodeViewModel.Node()
                    {
                        name = IterationToUpdate[key]
                    };

                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                        // serialize the fields array into a json string          
                        var patchValue = new StringContent(JsonConvert.SerializeObject(node), Encoding.UTF8, "application/json");
                        var method = new HttpMethod("PATCH");

                        // send the request
                        var request = new HttpRequestMessage(method, _configuration.UriString + projectName + "/_apis/wit/classificationNodes/Iterations/" + key + "?api-version=" + _configuration.VersionNumber) { Content = patchValue };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            isSuccesfull = true;
                        }
                    }
                }
                string project = projectName;
                DateTime StartDate = DateTime.Today.AddDays(-22);
                DateTime EndDate = DateTime.Today.AddDays(-1);

                Dictionary<string, string[]> sprint_dic = new Dictionary<string, string[]>();
                for (int i = 1; i <= IterationToUpdate.Count; i++)
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
            catch (Exception)
            {

            }
            return isSuccesfull;

        }
    }
}
