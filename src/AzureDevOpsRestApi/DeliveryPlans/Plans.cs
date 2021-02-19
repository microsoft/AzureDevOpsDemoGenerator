using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.DeliveryPlans
{
    public class Plans : ApiServiceBase
    {
        public Plans(IAppConfiguration configuration) : base(configuration) { }

        /// <summary>
        /// Create Delivery plans
        /// </summary>
        /// <param name="json"></param>
        /// <param name="project"></param>
        /// <returns></returns>
		public bool AddDeliveryPlan(string json, string project)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, project + "_apis/work/plans?api-version=3.0-preview.1") { Content = jsonContent };
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
                    this.LastFailureMessage = ex.Message + " ,"+ ex.StackTrace;
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
    }
}