using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.WorkItem
{
    public class BatchRequest
    {
        public string method { get; set; }
        public Dictionary<string, string> headers { get; set; }
        public object[] body { get; set; }
        public string uri { get; set; }


        public class Value
        {
            public string rel { get; set; }
            public string url { get; set; }
            public Attributes attributes { get; set; }
        }

        public class Attributes
        {
            public string comment { get; set; }
        }
    }

   
}