using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsDemoBuilder.ServiceInterfaces
{
    public interface ITemplateService
    {
        string GetTemplate(string TemplateName);

        bool GetTemplateFromPath(string TemplateUrl, string ExtractedTemplate, string GithubToken);

        bool checkTemplateDirectory(string dir);

        string FindPrivateTemplatePath(string privateTemplatePath);

    }
}
