using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.WorkItemAndTracking
{
    public class SwimLanes : ApiServiceBase
    {
        public SwimLanes(IConfiguration configuration) : base(configuration) { }

        /// <summary>
        /// Update swim lanes
        /// </summary>
        /// <param name="json"></param>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public bool UpdateSwimLanes(string json, string projectName)
        {
            string teamName = projectName + " Team";
            if (System.IO.File.Exists(json))
            {
                json = System.IO.File.ReadAllText(json);
                using (var client = GetHttpClient())
                {
                    var patchValue = new StringContent(json, Encoding.UTF8, "application/json"); 
                    var method = new HttpMethod("PUT");

                    var request = new HttpRequestMessage(method, projectName + "/" + teamName + "/_apis/work/boards/Backlog%20items/rows?api-version=" + _configuration.VersionNumber + "-preview") { Content = patchValue };
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
            return false;
            
        }
    }
}
                   
        
      