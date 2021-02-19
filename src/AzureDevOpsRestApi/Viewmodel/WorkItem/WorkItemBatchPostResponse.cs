using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.WorkItem
{
    public class WorkItemBatchPostResponse
    {
        public int Count { get; set; }
        [JsonProperty("value")]
        public List<Value> Values { get; set; }

        public class Value
        {
            public int Code { get; set; }
            public Dictionary<string, string> Headers { get; set; }
            public string Body { get; set; }
        }
    }
}
