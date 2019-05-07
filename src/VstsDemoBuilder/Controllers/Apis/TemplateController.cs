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
        [HttpGet]
        [Route("AllTemplates")]
        public HttpResponseMessage GetTemplates()
        {
            var templates = GetAllTemplates();
            return Request.CreateResponse(HttpStatusCode.OK, templates);
        }

        [HttpGet]
        [Route("TemplatesByPlatforms")]
        public HttpResponseMessage templatesbyPlatforms(string platforms)
        {
            var templates = GetTemplatesByPlatforms(platforms);
            return Request.CreateResponse(HttpStatusCode.OK, templates);
        }       

        public TemplateSelection.Templates GetAllTemplates()
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
            catch (Exception)
            {

            }
            return templates;
        }
               
        public List<Template> GetTemplatesByPlatforms(string platforms)
        {
            var templates = new TemplateSelection.Templates();
            var Selectedtemplates = new List<Template>();
            char delimiter = ',';
            string[] strComponents = platforms.Split(delimiter);
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

                                    }
                                }
                            }

                        }
                        groupWiseIndex++;
                    }

                }
            }
            catch (Exception)
            {

            }
            return Selectedtemplates;
        }

    }
}
