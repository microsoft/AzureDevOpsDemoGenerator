using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

using VstsDemoBuilder.ServiceInterfaces;
using VstsDemoBuilder.Services;
using static VstsDemoBuilder.Models.TemplateSelection;

namespace VstsDemoBuilder.Controllers.Apis
{
    [Route("api/templates")]
    public class TemplateController : Controller
    {

        public ITemplateService templateService;
        public ProjectService projectService;


        [HttpGet]
        [Route("AllTemplates")]
        public List<TemplateDetails> GetTemplates()
        {
            projectService.TrackFeature("api/templates/Alltemplates");
            return templateService.GetAllTemplates();
        }

        [HttpGet]
        [Route("TemplatesByTags")]
        public List<TemplateDetails> templatesbyTags(string Tags)
        {
            projectService.TrackFeature("api/templates/TemplateByTags");
            return templateService.GetTemplatesByTags(Tags);
        }


    }
}
