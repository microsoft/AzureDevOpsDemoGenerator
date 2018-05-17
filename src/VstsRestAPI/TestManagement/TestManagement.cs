using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.TestManagement
{
    public class TestManagement
    {
        public string lastFailureMessage;
        readonly IConfiguration _configuration;
        readonly string _credentials;

        public TestManagement(IConfiguration configuration)
        {
            _configuration = configuration;
            _credentials = configuration.PersonalAccessToken;//Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _configuration.PersonalAccessToken)));
        }
        /// <summary>
        /// Create test plans
        /// </summary>
        /// <param name="json"></param>
        /// <param name="project"></param>
        /// <returns></returns>

        public string[] CreateTestPlan(string json, string project)
        {
            string[] testPlan = new string[2];

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_configuration.UriString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, project + "/_apis/test/plans?api-version=1.0") { Content = jsonContent };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    testPlan[0] = JObject.Parse(result)["id"].ToString();
                    testPlan[1] = JObject.Parse(result)["name"].ToString();
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.lastFailureMessage = error;
                }
            }
            return testPlan;
        }
        /// <summary>
        /// Create test suites
        /// </summary>
        /// <param name="json"></param>
        /// <param name="testPlan"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public string[] CreatTestSuite(string json,string testPlan, string project)
        {
            string[] testSuite = new string[2];
            int parentTestSuite = Convert.ToInt32(testPlan);
            parentTestSuite = parentTestSuite + 1;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_configuration.UriString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, project + "/_apis/test/plans/" + testPlan + "/suites/" + parentTestSuite + "?api-version=1.0") { Content = jsonContent };
                var response = client.SendAsync(request).Result; 

                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    testSuite[0] = JObject.Parse(result)["value"].First["id"].ToString();
                    testSuite[1] = JObject.Parse(result)["value"].First["name"].ToString();
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.lastFailureMessage = error;
                }
            }
            return testSuite;
        }
        /// <summary>
        /// Add test cases to test suites
        /// </summary>
        /// <param name="testCases"></param>
        /// <param name="testPlan"></param>
        /// <param name="testSuite"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public bool AddTestCasesToSuite(string testCases, string testPlan, string testSuite, string project)
        {
            object json = new { };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_configuration.UriString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                var jsonContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, project + "/_apis/test/plans/" + testPlan + "/suites/" + testSuite + "/testcases/" + testCases + "?api-version=1.0") { Content = jsonContent };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    return true;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.lastFailureMessage = error;
                    return false;
                }
            }
        }
    }
}
