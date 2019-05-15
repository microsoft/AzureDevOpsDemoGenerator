using Common.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VstsDemoBuilder.Extensions;
using VstsDemoBuilder.Models;
using static VstsDemoBuilder.Models.TemplateSelection;

namespace VstsDemoBuilder.Controllers.Apis
{
    [RoutePrefix("api/templates")]
    public class TemplateController : ApiController
    {
        private ILog logger = LogManager.GetLogger("ErrorLog");

        [HttpGet]
        [Route("AllTemplates")]
        public HttpResponseMessage GetTemplates()
        {
            var templates = GetAllTemplates();
            return Request.CreateResponse(HttpStatusCode.OK, templates);
        }

        [HttpGet]
        [Route("TemplatesByTags")]
        public HttpResponseMessage templatesbyTags(string Tags)
        {
            var templates = GetTemplatesByTags(Tags);
            return Request.CreateResponse(HttpStatusCode.OK, templates);
        }       

        public Templates GetAllTemplates()
        {
            var templates = new TemplateSelection.Templates();
            try
            {
                Project model = new Project();
                string[] dirTemplates = Directory.GetDirectories(System.Web.Hosting.HostingEnvironment.MapPath("~") + @"\Templates");
                List<string> TemplateNames = new List<string>();
                //Taking all the template folder and adding to list
                foreach (string template in dirTemplates)
                {
                    TemplateNames.Add(Path.GetFileName(template));
                    // Reading Template setting file to check for private templates                   
                }

                if (System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~") + @"\Templates\TemplateSetting.json"))
                {
                    string templateSetting = model.ReadJsonFile(System.Web.Hosting.HostingEnvironment.MapPath("~") + @"\Templates\TemplateSetting.json");
                    templates = JsonConvert.DeserializeObject<TemplateSelection.Templates>(templateSetting);

                }
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t BulkProject \t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return templates;
        }
               
        public List<Template> GetTemplatesByTags(string Tags)
        {
            var templates = new TemplateSelection.Templates();
            var Selectedtemplates = new List<Template>();
            char delimiter = ',';
            string[] strComponents = Tags.Split(delimiter);
            try
            {
                Project model = new Project();
                string[] dirTemplates = Directory.GetDirectories(System.Web.Hosting.HostingEnvironment.MapPath("~") + @"\Templates");
                List<string> TemplateNames = new List<string>();
                //Taking all the template folder and adding to list
                foreach (string template in dirTemplates)
                {
                    TemplateNames.Add(Path.GetFileName(template));
                    // Reading Template setting file to check for private templates                   
                }

                if (System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~") + @"\Templates\TemplateSetting.json"))
                {
                    string templateSetting = model.ReadJsonFile(System.Web.Hosting.HostingEnvironment.MapPath("~") + @"\Templates\TemplateSetting.json");
                    templates = JsonConvert.DeserializeObject<TemplateSelection.Templates>(templateSetting);
                    //Selectedtemplates.Groups = templates.Groups;
                    //Selectedtemplates.GroupwiseTemplates = new List<TemplateSelection.GroupwiseTemplate>();
                    int groupWiseIndex = 0;

                    foreach (var groupwiseTemplates in templates.GroupwiseTemplates)
                    {
                        //Selectedtemplates.GroupwiseTemplates.Add(new TemplateSelection.GroupwiseTemplate()
                        //{
                        //    Groups = groupwiseTemplates.Groups,
                        //    Template = new List<TemplateSelection.Template>()
                        //});
                        foreach (var tmp in groupwiseTemplates.Template)
                        {
                            if (tmp.Tags != null)
                            {
                                foreach (string str in strComponents)
                                {
                                    if (tmp.Tags.Contains(str))
                                    {
                                        Selectedtemplates.Add(tmp);
                                        break;
                                    }
                                }
                            }

                        }
                        groupWiseIndex++;
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t BulkProject \t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return Selectedtemplates;
        }

    }
}
