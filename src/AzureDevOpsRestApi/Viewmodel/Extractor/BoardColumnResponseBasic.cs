using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class BoardColumnResponseBasic
    {
        public class StateMappings
        {
            [JsonProperty(PropertyName = "Issue")]
            public string Feature { get; set; }
            [JsonProperty(PropertyName = "Epic")]
            public string Epic { get; set; }
        }

        public class Value
        {
            public string Name { get; set; }
            public int ItemLimit { get; set; }
            public StateMappings StateMappings { get; set; }
            public string ColumnType { get; set; }
            public bool? IsSplit { get; set; }
            public string Description { get; set; }
        }

        public class ColumnResponse
        {
            public string BoardName { get; set; }
            public List<Value> Value { get; set; }
        }
    }

}
