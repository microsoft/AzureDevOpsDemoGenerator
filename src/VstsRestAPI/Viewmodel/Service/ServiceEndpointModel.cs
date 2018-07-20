using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.Service
{

    public class Data
    {
    }

    public class CreatedBy
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string uniqueName { get; set; }
    }

    public class Authorization
    {
        public string scheme { get; set; }
    }

    public class ServiceEndpointModel
    {
        public Data data { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string url { get; set; }
        public CreatedBy createdBy { get; set; }
        public Authorization authorization { get; set; }
        public bool isReady { get; set; }
    }

}
