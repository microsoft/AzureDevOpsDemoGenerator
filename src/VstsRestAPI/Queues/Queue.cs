using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using VstsRestAPI.Viewmodel.Queue;

namespace VstsRestAPI.Queues
{
    public class Queue : ApiServiceBase
    {
        public Queue(IConfiguration configuration) : base(configuration) { }
        private ILog logger = LogManager.GetLogger("ErrorLog");
        /// <summary>
        /// Get Agent queue
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> GetQueues()
        {
            try
            {
                Dictionary<string, int> dicQueues = new Dictionary<string, int>();
                QueueModel viewModel = new QueueModel();

                using (var client = GetHttpClient())
                {
                    string req = _configuration.UriString + _configuration.Project + "/_apis/distributedtask/queues?api-version=" + _configuration.VersionNumber;
                    HttpResponseMessage response = client.GetAsync(req).Result;

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
            catch(Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateReleaseDefinition" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return new Dictionary<string, int>();
        }
        /// <summary>
        /// Create Agent Queue by queue name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int CreateQueue(string name)
        {
            try
            {
                AgentQueueModel viewModel = new AgentQueueModel
                {
                    name = name
                };

                using (var client = GetHttpClient())
                {
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(viewModel), Encoding.UTF8, "application/json");
                    var method = new HttpMethod("POST");

                    var request = new HttpRequestMessage(method, _configuration.Project + "/_apis/distributedtask/queues?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
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
            catch(Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateReleaseDefinition" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return 0;
        }
    }
}
