using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatesGeneratorTool.ViewModel
{
    public class ServiceEndPointsResponse
    {
      
        public class Authorization
        {
            public string scheme { get; set; }
            public parameter parameters = new parameter { subscriptionId = "" };
        }
        public class Value
        {
            public string id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public string url { get; set; }
            public string subscriptionId = "";
            public Authorization authorization { get; set; }
            public bool isReady { get; set; }
        }
        public class parameter
        {
            public string subscriptionId = "";
            public string username { get; set; }
            public string password { get; set; }
        
        }
        public class Service
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }
    }
}

