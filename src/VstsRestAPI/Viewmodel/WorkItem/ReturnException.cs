using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.WorkItem
{
    class ReturnException
    {
        public string id { get; set; }
        public string innerException { get; set; }
        public string message { get; set; }
        public string typeName { get; set; }
        public string typeKey { get; set; }
        public string errorCode { get; set; }
        public string eventId { get; set; }
    }
}
