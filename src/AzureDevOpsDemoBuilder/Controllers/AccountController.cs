using AzureDevOpsDemoBuilder.Models;
using AzureDevOpsDemoBuilder.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace AzureDevOpsDemoBuilder.Controllers
{

    public class AccountController : Controller
    {
        private readonly AccessDetails accessDetails = new AccessDetails();
        private TemplateSelection.Templates templates = new TemplateSelection.Templates();
        //private IProjectService projectService;
        private IWebHostEnvironment HostingEnvironment;
        private ILogger<AccountController> logger;
        private IAccountService _accountService;
        public IConfiguration AppKeyConfiguration { get; }
        public AccountController(IAccountService accountService, IConfiguration configuration, IWebHostEnvironment _webHostEnvironment, ILogger<AccountController> _logger)
        {
            _accountService = accountService;
            AppKeyConfiguration = configuration;
            HostingEnvironment = _webHostEnvironment;
            logger = _logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Unsupported_browser()
        {
            return View();
        }

        /// <summary>
        /// Verify View
        /// </summary>
        /// <param name="model"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Verify(LoginModel model, string id)
        {
            HttpContext.Session.Clear();
            // check to enable extractor
            if (string.IsNullOrEmpty(model.EnableExtractor) || model.EnableExtractor.ToLower() == "false")
            {
                model.EnableExtractor = AppKeyConfiguration["EnableExtractor"];
            }
            if (!string.IsNullOrEmpty(model.EnableExtractor))
            {
                HttpContext.Session.SetString("EnableExtractor", model.EnableExtractor);
            }
            var browser = Request.Headers["User-Agent"].ToString();
            if (browser.Contains("InternetExplorer"))
            {
                return RedirectToAction("Unsupported_browser", "Account");
            }
            try
            {
                if (!string.IsNullOrEmpty(model.name))
                {
                    if (System.IO.File.Exists(HostingEnvironment.WebRootPath + "/Templates/TemplateSetting.json"))
                    {
                        string privateTemplatesJson = System.IO.File.ReadAllText(HostingEnvironment.WebRootPath + "/Templates/TemplateSetting.json");
                        templates = Newtonsoft.Json.JsonConvert.DeserializeObject<TemplateSelection.Templates>(privateTemplatesJson);
                        if (templates != null)
                        {
                            bool flag = false;
                            foreach (var grpTemplate in templates.GroupwiseTemplates)
                            {
                                foreach (var template in grpTemplate.Template)
                                {
                                    if (template.ShortName != null && template.ShortName.ToLower() == model.name.ToLower())
                                    {
                                        flag = true;
                                        HttpContext.Session.SetString("templateName", template.Name);
                                    }
                                }
                            }
                            if (flag == false)
                            {
                                HttpContext.Session.SetString("templateName", null);
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(model.Event))
                {
                    string eventsTemplate = HostingEnvironment.WebRootPath + "/Templates/Events.json";
                    if (System.IO.File.Exists(eventsTemplate))
                    {
                        string eventContent = System.IO.File.ReadAllText(eventsTemplate);
                        var jItems = JObject.Parse(eventContent);
                        if (jItems[model.Event] != null)
                        {
                            model.Event = jItems[model.Event].ToString();
                        }
                        else
                        {
                            model.Event = string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(JsonConvert.SerializeObject(ex, Formatting.Indented) + Environment.NewLine);
            }
            //return RedirectToAction("../account/verify");
            return View(model);
        }

        /// <summary>
        /// Get Account at the end of project provision
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public string GetAccountName()
        {
            if (HttpContext.Session.GetString("AccountName") != null)
            {
                string accountName = HttpContext.Session.GetString("AccountName").ToString();
                return accountName;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Index view which calls VSTS OAuth
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index()
        {
            try
            {
                HttpContext.Session.SetString("visited", "1");
                string url = "https://app.vssps.visualstudio.com/oauth2/authorize?client_id={0}&response_type=Assertion&state=User1&scope={1}&redirect_uri={2}";
                string redirectUrl = AppKeyConfiguration["RedirectUri"];
                string clientId = AppKeyConfiguration["ClientId"];
                string AppScope = AppKeyConfiguration["appScope"];
                url = string.Format(url, clientId, AppScope, redirectUrl);
                return Redirect(url);
            }
            catch (Exception ex)
            {
                logger.LogDebug(JsonConvert.SerializeObject(ex, Formatting.Indented) + Environment.NewLine);
            }
            return RedirectToAction("verify");
        }

        /// <summary>
        /// Sign out
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult SignOut()
        {
            HttpContext.Session.Clear();
            return Redirect("https://app.vssps.visualstudio.com/_signout");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult SessionOutReturn()
        {
            return View();
        }
    }

}
