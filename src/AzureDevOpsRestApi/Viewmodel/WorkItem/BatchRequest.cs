using System.Collections.Generic;

namespace AzureDevOpsAPI.Viewmodel.WorkItem
{
    public class BatchRequest
    {
        public string Method { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public object[] Body { get; set; }
        public string Uri { get; set; }


        public class Value
        {
            public string Rel { get; set; }
            public string Url { get; set; }
            public Attributes Attributes { get; set; }
        }

        public class Attributes
        {
            public string Comment { get; set; }
        }
    }

   
}