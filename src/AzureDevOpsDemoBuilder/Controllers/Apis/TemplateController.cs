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

        private ITemplateService templateService;

        public TemplateController()
        {
        }

        [HttpGet]
        [Route("AllTemplates")]
        public List<TemplateDetails> GetTemplates()
        {
            ProjectService.TrackFeature("api/templates/Alltemplates");
            return templateService.GetAllTemplates();
        }

        [HttpGet]
        [Route("TemplatesByTags")]
        public List<TemplateDetails> templatesbyTags(string Tags)
        {
            ProjectService.TrackFeature("api/templates/TemplateByTags");
            return templateService.GetTemplatesByTags(Tags);
        }


    }
}
