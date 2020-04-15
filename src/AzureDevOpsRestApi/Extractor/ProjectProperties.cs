using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Extractor
{
    public class ProjectProperties
    {
        public class Value
        {
            public string Name { get; set; }
            [JsonProperty(PropertyName = "value")]
            public string RefValue { get; set; }
        }

        public class Properties
        {
            public int Count { get; set; }
            public IList<Value> Value { get; set; }

            public string TypeClass { get; set; }
        }
        

    }
}
