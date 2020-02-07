using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.Service
{

    public class Data
    {
    }

    public class CreatedBy
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string UniqueName { get; set; }
    }

    public class Authorization
    {
        public string Scheme { get; set; }
    }

    public class ServiceEndpointModel
    {
        public Data Data { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public CreatedBy CreatedBy { get; set; }
        public Authorization Authorization { get; set; }
        public bool IsReady { get; set; }
    }

}
