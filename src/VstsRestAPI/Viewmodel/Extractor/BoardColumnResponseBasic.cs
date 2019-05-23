using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class BoardColumnResponseBasic
    {
        public class StateMappings
        {
            [JsonProperty(PropertyName = "Issue")]
            public string feature { get; set; }
            [JsonProperty(PropertyName = "Epic")]
            public string epic { get; set; }
        }

        public class Value
        {
            public string name { get; set; }
            public int itemLimit { get; set; }
            public StateMappings stateMappings { get; set; }
            public string columnType { get; set; }
            public bool? isSplit { get; set; }
            public string description { get; set; }
        }

        public class ColumnResponse
        {
            public string BoardName { get; set; }
            public List<Value> value { get; set; }
        }
    }

}
