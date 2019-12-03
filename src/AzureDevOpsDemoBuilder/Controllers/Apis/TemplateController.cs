using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

using AzureDevOpsDemoBuilder.ServiceInterfaces;
using AzureDevOpsDemoBuilder.Services;
using static AzureDevOpsDemoBuilder.Models.TemplateSelection;

namespace VstsDemoBuilder.Controllers.Apis
{
    [Route("api/templates")]
    public class TemplateController : ControllerBase
    {

        public ITemplateService templateService;
        public IProjectService projectService;
        public TemplateController(ITemplateService _templateService,IProjectService _projectService)
        {
            templateService = _templateService;
            projectService = _projectService;
        }

        [HttpGet]
        [Route("AllTemplates")]
        public List<TemplateDetails> GetTemplates()
        {
            //projectService.TrackFeature("api/templates/Alltemplates");
            return templateService.GetAllTemplates();
        }

        [HttpGet]
        [Route("TemplatesByTags")]
        public List<TemplateDetails> templatesbyTags(string Tags)
        {
            //projectService.TrackFeature("api/templates/TemplateByTags");
            return templateService.GetTemplatesByTags(Tags);
        }


    }
}
