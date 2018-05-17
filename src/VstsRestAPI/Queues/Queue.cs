using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VstsRestAPI.Viewmodel.Queue;

namespace VstsRestAPI.Queues
{
    public class Queue
    {
        public string lastFailureMessage;
        readonly IConfiguration _configuration;
        readonly string _credentials;

        public Queue(IConfiguration configuration)
        {
            _configuration = configuration;
            _credentials = configuration.PersonalAccessToken;//Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _configuration.PersonalAccessToken)));
        }
        /// <summary>
        /// Get Agent queue
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> GetQueues()
        {
            Dictionary<string, int> dicQueues = new Dictionary<string, int>();
            QueueModel viewModel = new QueueModel();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_configuration.UriString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                HttpResponseMessage response = client.GetAsync(_configuration.Project + "/_apis/distributedtask/queues?api-version=" + _configuration.VersionNumber + "-preview.1").Result;

                if (response.IsSuccessStatusCode)
                {                    
                    viewModel = response.Content.ReadAsAsync<QueueModel>().Result;                    
                    if (viewModel != null && viewModel.value != null)
                    {
                        foreach (AgentQueueModel aq in viewModel.value)
                        {
                            dicQueues[aq.name] = aq.id;
                        }                        
                    }
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.lastFailureMessage = error;                    
                }
            }

            return dicQueues;
        }

        public int CreateQueue(string name)
        {
            AgentQueueModel viewModel = new AgentQueueModel
            {
                name = name
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_configuration.UriString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                var jsonContent = new StringContent(JsonConvert.SerializeObject(viewModel), Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, _configuration.Project + "/_apis/distributedtask/queues?api-version=" + _configuration.VersionNumber + "-preview.1") { Content = jsonContent };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {                    
                    viewModel = response.Content.ReadAsAsync<AgentQueueModel>().Result;                    
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.lastFailureMessage = error;
                }
            }

            return viewModel.id;
        }

    }
}
