using log4net;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using VstsRestAPI.Viewmodel.Extractor;

namespace VstsRestAPI.Extractor
{
    public class VariableGroups : ApiServiceBase
    {
        public VariableGroups(IConfiguration configuration) : base(configuration)
        {
        }
        private ILog logger = LogManager.GetLogger("ErrorLog");
        public GetVariableGroups.Groups GetVariableGroups()
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    string req = string.Format("https://dev.azure.com/{0}/{1}/_apis/distributedtask/variablegroups?api-version={2}", _configuration.AccountName, _configuration.Project, _configuration.VersionNumber);
                    var response = client.GetAsync(req).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string res = response.Content.ReadAsStringAsync().Result;
                        GetVariableGroups.Groups ress = JsonConvert.DeserializeObject<GetVariableGroups.Groups>(res);
                        return ress;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\n" + ex.StackTrace + "\n");
            }
            return new GetVariableGroups.Groups();
        }

        public GetVariableGroups.VariableGroupsCreateResponse PostVariableGroups(string json)
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                    string req = string.Format("https://dev.azure.com/{0}/{1}/_apis/distributedtask/variablegroups?api-version={2}", _configuration.AccountName, _configuration.Project, _configuration.VersionNumber);

                    var method = new HttpMethod("POST");

                    var request = new HttpRequestMessage(method, req) { Content = jsonContent };
                    var response = client.SendAsync(request).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string res1 = response.Content.ReadAsStringAsync().Result;
                        GetVariableGroups.VariableGroupsCreateResponse ress = JsonConvert.DeserializeObject<GetVariableGroups.VariableGroupsCreateResponse>(res1);
                        return ress;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return new GetVariableGroups.VariableGroupsCreateResponse();
        }
    }
}
