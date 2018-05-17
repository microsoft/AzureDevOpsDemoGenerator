using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.WorkItem
{
    public class GetBoardColumnResponse
    {
        public class ColumnResponse : BaseViewModel
        {

            public int count { get; set; }
            public Value[] columns { get; set; }
            public Fields fields { get; set; }
        }

        public class Value
        {
            public string id { get; set; }
            public string name { get; set; }
            public string itemLimit { get; set; }
            public string isSplit { get; set; }
            public string description { get; set; }
            public string columnType { get; set; }
            public StateMappings stateMappings { get; set; }
        }

        public class Fields
        {
            public Field columnField { get; set; }
            public Field rowField { get; set; }
            public Field doneField { get; set; }
        }

        public class Field
        {
            public string referenceName { get; set; }
            public string url { get; set; }
        }

        public class StateMappings
        {
            [JsonProperty(PropertyName = "Product Backlog Item")]
            public string PBI { get; set; }
            [JsonProperty(PropertyName = "Bug")]
            public string bug { get; set; }
        }
    }
}

