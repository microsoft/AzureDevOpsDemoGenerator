using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace AzureDevOpsAPI.TestManagement
{
    public class TestManagement : ApiServiceBase
    {
        public TestManagement(IAppConfiguration configuration) : base(configuration) { }
         Logger logger = LogManager.GetLogger("*");
        /// <summary>
        /// Create test plans
        /// </summary>
        /// <param name="json"></param>
        /// <param name="project"></param>
        /// <returns></returns>

        public string[] CreateTestPlan(string json, string project)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    string[] testPlan = new string[2];

                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, Configuration.UriString + project + "/_apis/test/plans?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
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
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("CreateTestPlan" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new string[] { };
                    }

                    Thread.Sleep(retryCount * 1000);
                }
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
            int retryCount = 0;
            while (retryCount < 5)
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

                        var request = new HttpRequestMessage(method, project + "/_apis/test/plans/" + testPlan + "/suites/" + parentTestSuite + "?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            dynamic resSerialize = JsonConvert.DeserializeObject<dynamic>(result);
                            if (resSerialize.count > 0)
                            {
                                testSuite[0] = JObject.Parse(result)["value"].First["id"].ToString();
                                testSuite[1] = JObject.Parse(result)["value"].First["name"].ToString();
                            }
                            return testSuite;
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            LastFailureMessage = error;
                            retryCount++;
                        }
                    }
                    return testSuite;
                }
                catch (Exception ex)
                {
                    logger.Debug("CreatTestSuite" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new string[] { };
                    }

                    Thread.Sleep(retryCount * 1000);
                }
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
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    object json = new { };
                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, project + "/_apis/test/plans/" + testPlan + "/suites/" + testSuite + "/testcases/" + testCases + "?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
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
                            retryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("AddTestCasesToSuite" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
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
