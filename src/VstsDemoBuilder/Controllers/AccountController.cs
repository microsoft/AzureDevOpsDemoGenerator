using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using VstsDemoBuilder.Models;
using VstsRestAPI;
using VstsRestAPI.ProjectsAndTeams;
using VstsDemoBuilder.Extensions;

namespace VstsDemoBuilder.Controllers
{

    public class AccountController : Controller
    {
        AccessDetails AccessDetails = new AccessDetails();
        TemplateSetting privateTemplates = new TemplateSetting();

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
            var browser = Request.Browser.Type;
            if (browser.Contains("InternetExplorer"))
            {
                return RedirectToAction("Unsupported_browser", "Account");
            }
            Session.Clear();
            try
            {
                if (!string.IsNullOrEmpty(model.name) && !string.IsNullOrEmpty(model.templateid))
                {
                    if (System.IO.File.Exists(Server.MapPath("~") + @"\Templates\TemplateSetting.json"))
                    {
                        string privateTemplatesJson = System.IO.File.ReadAllText(Server.MapPath("~") + @"\Templates\TemplateSetting.json");
                        privateTemplates = Newtonsoft.Json.JsonConvert.DeserializeObject<TemplateSetting>(privateTemplatesJson);
                        if (privateTemplates != null)
                        {
                            bool flag = false;
                            foreach (var template in privateTemplates.privateTemplateKeys)
                            {
                                if (template.key != "" && template.value != "")
                                {
                                    if (template.key == model.templateid && template.value.ToLower() == model.name.ToLower())
                                    {
                                        flag = true;
                                        Session["templateName"] = model.name;
                                        Session["templateId"] = model.templateid;
                                    }
                                }
                            }
                            if (flag == false)
                            {
                                Session["templateName"] = null;
                                Session["templateId"] = null;
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
            catch { }
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
            Session["visited"] = "1";

            //testing
            string url = "https://app.vssps.visualstudio.com/oauth2/authorize?client_id={0}&response_type=Assertion&state=User1&scope=vso.agentpools_manage%20vso.build_execute%20vso.code_manage%20vso.dashboards_manage%20vso.extension_manage%20vso.identity%20vso.project_manage%20vso.release_manage%20vso.serviceendpoint_manage%20vso.test_write%20vso.wiki_write%20vso.work_full&redirect_uri={1}";

            string redirectUrl = System.Configuration.ConfigurationManager.AppSettings["RedirectUri"];
            string clientId = System.Configuration.ConfigurationManager.AppSettings["ClientId"];
            url = string.Format(url, clientId, redirectUrl);
            return Redirect(url);
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