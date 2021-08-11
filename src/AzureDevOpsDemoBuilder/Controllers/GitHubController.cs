using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using AzureDevOpsDemoBuilder.Models;
using System.Web;
using AzureDevOpsDemoBuilder.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.ApplicationInsights;
using AzureDevOpsAPI.Git;

namespace AzureDevOpsDemoBuilder.Controllers
{
    public class GitHubController : Controller
    {
        public GitHubController(ILogger<GitHubController> _logger, IConfiguration appKeyConfiguration, TelemetryClient _ai)
        {
            AppKeyConfiguration = appKeyConfiguration;
            logger = _logger;
            ai = _ai;
        }

        private GitHubAccessDetails accessDetails = new GitHubAccessDetails();
        public static string state = Guid.NewGuid().ToString().Split('-')[0];

        public IConfiguration AppKeyConfiguration { get; }

        private ILogger<GitHubController> logger;
        private TelemetryClient ai;

        [AllowAnonymous]
        public ActionResult GitOauth()
        {
            //Request User GitHub Identity
            string ClientID = AppKeyConfiguration["GitHubClientId"];
            string ClientSecret = AppKeyConfiguration["GitHubClientSecret"];
            string RedirectUrl = AppKeyConfiguration["GitHubRedirectUrl"];
            string Scope = AppKeyConfiguration["GitHubScope"];
            string url = string.Format("https://github.com/login/oauth/authorize?client_id={0}&scope={1}&redirect_uri={2}&state={3}", ClientID, Scope, RedirectUrl, state);
            return Redirect(url);
        }
        [AllowAnonymous]
        public ActionResult Redirect()
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // Here we get the Code in the Query String, using that we can get access token
                var request = Request;
                string code = HttpContext.Request.Query["code"];
                if (!string.IsNullOrEmpty(code))
                {

                    string reqUrl = FormatRequestUrl(code);
                    // Getting access token, if access token is null, will return to Index page [relogin takes place]
                    GitHubAccessDetails _accessDetails = GetAccessToken(reqUrl);
                    if (_accessDetails.access_token != null)
                    {
                        HttpContext.Session.SetString("GitHubToken", _accessDetails.access_token);
                        ViewBag.Response = _accessDetails.access_token;
                        return RedirectToAction("Status");
                    }
                    else
                    {
                        return RedirectToAction("Issue");
                    }
                }
            }
            catch (Exception ex)
            {
                ai.TrackException(ex);
                logger.LogDebug(ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return RedirectToAction("index", "home");
        }

        public string FormatRequestUrl(string code)
        {
            string ClientID = AppKeyConfiguration["GitHubClientId"];
            string ClientSecret = AppKeyConfiguration["GitHubClientSecret"];
            string RedirectUrl = AppKeyConfiguration["GitHubRedirectUrl"];
            string Scope = AppKeyConfiguration["GitHubScope"];
            string requestUrl = string.Format("?client_id={0}&client_secret={1}&code={2}&redirect_uri={3}&state={4}", ClientID, ClientSecret, code, RedirectUrl, state);
            return requestUrl;
        }

        // Formatting the POST URL
        // Get the access token
        public GitHubAccessDetails GetAccessToken(string body)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    var request = new HttpRequestMessage(HttpMethod.Post, string.Format("https://github.com/login/oauth/access_token/{0}", body));
                    HttpResponseMessage response = client.SendAsync(request).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        accessDetails = JsonConvert.DeserializeObject<GitHubAccessDetails>(response.Content.ReadAsStringAsync().Result);
                        return accessDetails;
                    }
                }
            }
            catch (Exception ex)
            {
                ai.TrackException(ex);
                logger.LogDebug(ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return new GitHubAccessDetails();
        }
        [AllowAnonymous]
        public ActionResult Status()
        {
            return View();
        }
    }
}