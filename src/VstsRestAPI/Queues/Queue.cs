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
    public class Queue : ApiServiceBase
    {
        public Queue(IConfiguration configuration) : base(configuration) { }

        /// <summary>
        /// Get Agent queue
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> GetQueues()
        {
            Dictionary<string, int> dicQueues = new Dictionary<string, int>();
            QueueModel viewModel = new QueueModel();

            using (var client = GetHttpClient())
            {
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
                    this.LastFailureMessage = error;                    
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

            using (var client = GetHttpClient())
            {
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
                    this.LastFailureMessage = error;
                }
            }

            return viewModel.id;
        }

    }
}
