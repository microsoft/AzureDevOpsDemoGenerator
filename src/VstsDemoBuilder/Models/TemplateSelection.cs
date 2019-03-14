using Newtonsoft.Json;
using System.Collections.Generic;

namespace VstsDemoBuilder.Models
{
    public class TemplateSelection
    {
        public class Template
        {
            public string Key { get; set; }
            public string Name { get; set; }
            public string ShortName { get; set; }
            public string Description { get; set; }
            public string[] Tags { get; set; }
            public string Image { get; set; }
            public string TemplateFolder { get; set; }
            public string Message { get; set; }
            public List<string> PreviewImages { get; set; }
            public string Author { get; set; }
            public string LastUpdatedDate { get; set; }
            

        }

        public class GroupwiseTemplate
        {
            public string Groups { get; set; }
            public IList<Template> Template { get; set; }
        }
        public class Templates
        {
            public IList<string> Groups { get; set; }
            public IList<string> PrivateGroups { get; set; }
            public IList<GroupwiseTemplate> GroupwiseTemplates { get; set; }
            public IList<string> privateTemplates { get; set; }
        }
    }
}