using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;

namespace VstsRestAPI.TestManagement
{
    public class TestManagement : ApiServiceBase
    {
        public TestManagement(IConfiguration configuration) : base(configuration) { }
        private ILog logger = LogManager.GetLogger("ErrorLog");
        /// <summary>
        /// Create test plans
        /// </summary>
        /// <param name="json"></param>
        /// <param name="project"></param>
        /// <returns></returns>

        public string[] CreateTestPlan(string json, string project)
        {
            try
            {
                string[] testPlan = new string[2];

                using (var client = GetHttpClient())
                {
                    var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var method = new HttpMethod("POST");

                    var request = new HttpRequestMessage(method, _configuration.UriString + project + "/_apis/test/plans?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
                    var response = client.SendAsync(request).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        testPlan[0] = JObject.Parse(result)["id"].ToString();
                        testPlan[1] = JObject.Parse(result)["name"].ToString();
                        return testPlan;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                        LastFailureMessage = error;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateTestPlan" + "\t" + ex.Message + "\t"   + "\n" + ex.StackTrace + "\n");
            }
            return new string[] { };
        }
        /// <summary>
        /// Create test suites
        /// </summary>
        /// <param name="json"></param>
        /// <param name="testPlan"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public string[] CreatTestSuite(string json, string testPlan, string project)
        {
            try
            {
                string[] testSuite = new string[2];
                int parentTestSuite = Convert.ToInt32(testPlan);
                parentTestSuite = parentTestSuite + 1;
                using (var client = GetHttpClient())
                {
                    var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var method = new HttpMethod("POST");

                    var request = new HttpRequestMessage(method, project + "/_apis/test/plans/" + testPlan + "/suites/" + parentTestSuite + "?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
                    var response = client.SendAsync(request).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        testSuite[0] = JObject.Parse(result)["value"].First["id"].ToString();
                        testSuite[1] = JObject.Parse(result)["value"].First["name"].ToString();
                        return testSuite;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                        LastFailureMessage = error;
                    }
                }
                return testSuite;
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateTestPlan" + "\t" + ex.Message + "\t"   + "\n" + ex.StackTrace + "\n");
            }
            return new string[] { };
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
            try
            {
                object json = new { };
                using (var client = GetHttpClient())
                {
                    var jsonContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
                    var method = new HttpMethod("POST");

                    var request = new HttpRequestMessage(method, project + "/_apis/test/plans/" + testPlan + "/suites/" + testSuite + "/testcases/" + testCases + "?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
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
                        LastFailureMessage = error;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "AddTestCasesToSuite" + "\t" + ex.Message + "\t"   + "\n" + ex.StackTrace + "\n");
            }
            return false;
        }
    }
}
