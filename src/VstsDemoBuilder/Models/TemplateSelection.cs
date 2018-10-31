using System.Collections.Generic;

namespace VstsDemoBuilder.Models
{
    public class TemplateSelection
    {
        public class Template
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string[] tags { get; set; }
            public string[] icon { get; set; }
        }

        public class GroupwiseTemplate
        {
            public string Groups { get; set; }
            public IList<Template> Template { get; set; }
        }

        public class PrivateTemplateKey
        {
            public string key { get; set; }
            public string value { get; set; }
           
        }

        public class Templates
        {
            public IList<string> Groups { get; set; }
            public IList<string> PrivateGroups { get; set; }
            public IList<GroupwiseTemplate> GroupwiseTemplates { get; set; }
            public IList<string> privateTemplates { get; set; }
            public IList<PrivateTemplateKey> privateTemplateKeys { get; set; }
        }


    }
}