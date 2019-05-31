﻿using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Web.Mvc;
using VstsDemoBuilder.Models;
using VstsDemoBuilder.ServiceInterfaces;

namespace VstsDemoBuilder.Controllers
{

    public class AccountController : Controller
    {
        private readonly AccessDetails accessDetails = new AccessDetails();
        private TemplateSelection.Templates templates = new TemplateSelection.Templates();
        private ILog logger = LogManager.GetLogger("ErrorLog");
        private IProjectService projectService;

        public AccountController(IProjectService _projectService)
        {
            projectService = _projectService;
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
            Session.Clear();
            // check to enable extractor
            //model.EnableExtractor = "true";
            if (!string.IsNullOrEmpty(model.EnableExtractor))
            {
                Session["EnableExtractor"] = model.EnableExtractor;
            }
            var browser = Request.Browser.Type;
            if (browser.Contains("InternetExplorer"))
            {
                return RedirectToAction("Unsupported_browser", "Account");
            }
            try
            {
                if (!string.IsNullOrEmpty(model.name))
                {
                    if (System.IO.File.Exists(Server.MapPath("~") + @"\Templates\TemplateSetting.json"))
                    {
                        string privateTemplatesJson = System.IO.File.ReadAllText(Server.MapPath("~") + @"\Templates\TemplateSetting.json");
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
                                        Session["templateName"] = template.Name;
                                    }
                                }
                            }
                            if (flag == false)
                            {
                                Session["templateName"] = null;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(model.Event))
                {
                    string eventsTemplate = Server.MapPath("~") + @"\Templates\Events.json";
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
                logger.Debug(JsonConvert.SerializeObject(ex, Formatting.Indented) + Environment.NewLine);
            }
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
            if (Session["AccountName"] != null)
            {
                string accountName = Session["AccountName"].ToString();
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
                Session["visited"] = "1";
                string url = "https://app.vssps.visualstudio.com/oauth2/authorize?client_id={0}&response_type=Assertion&state=User1&scope={1}&redirect_uri={2}";
                string redirectUrl = System.Configuration.ConfigurationManager.AppSettings["RedirectUri"];
                string clientId = System.Configuration.ConfigurationManager.AppSettings["ClientId"];
                string AppScope = System.Configuration.ConfigurationManager.AppSettings["appScope"];
                url = string.Format(url, clientId, AppScope, redirectUrl);
                return Redirect(url);
            }
            catch (Exception ex)
            {
                logger.Debug(JsonConvert.SerializeObject(ex, Formatting.Indented) + Environment.NewLine);
            }
            return RedirectToAction("../shared/error");
        }

        /// <summary>
        /// Sign out
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult SignOut()
        {
            Session.Clear();
            return Redirect("https://app.vssps.visualstudio.com/_signout");
        }
    }
}
