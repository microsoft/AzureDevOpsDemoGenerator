using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class GetProcessTemplate
    {
        public class Properties
        {
            [JsonProperty(PropertyName = "class")]
            public string Class { get; set; }
            public string ParentProcessTypeId { get; set; }
            public bool IsEnabled { get; set; }
            public string Version { get; set; }
            public bool IsDefault { get; set; }
        }

        public class PTemplate
        {
            public string TypeId { get; set; }
            public object ReferenceName { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public Properties Properties { get; set; }
        }
    }
}
