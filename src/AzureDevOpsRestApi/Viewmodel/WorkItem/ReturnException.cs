using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.WorkItem
{
    class ReturnException
    {
        public string Id { get; set; }
        public string InnerException { get; set; }
        public string Message { get; set; }
        public string TypeName { get; set; }
        public string TypeKey { get; set; }
        public string ErrorCode { get; set; }
        public string EventId { get; set; }
    }
}
