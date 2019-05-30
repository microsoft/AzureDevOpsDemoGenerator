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
using VstsDemoBuilder.ServiceInterfaces;
using VstsDemoBuilder.Services;
using static VstsDemoBuilder.Models.TemplateSelection;

namespace VstsDemoBuilder.Controllers.Apis
{
    [RoutePrefix("api/templates")]
    public class TemplateController : ApiController
    {
    
        private ITemplateService templateService;

        public TemplateController()
        {
            templateService = new TemplateService();
        }

        [HttpGet]
        [Route("AllTemplates")]
        public HttpResponseMessage GetTemplates()
        {
            var templates = templateService.GetAllTemplates();
            return Request.CreateResponse(HttpStatusCode.OK, templates);
        }

        [HttpGet]
        [Route("TemplatesByTags")]
        public HttpResponseMessage templatesbyTags(string Tags)
        {
            var templates = templateService.GetTemplatesByTags(Tags);
            return Request.CreateResponse(HttpStatusCode.OK, templates);
        }       


    }
}
