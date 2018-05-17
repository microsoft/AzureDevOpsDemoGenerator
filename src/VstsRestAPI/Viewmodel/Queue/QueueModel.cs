using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.Queue
{
    public class QueueModel : BaseViewModel
    {
        public int count { get; set; }
        public AgentQueueModel[] value { get; set; }
    }

    public class Value
    {
        public int id { get; set; }
        public string projectId { get; set; }
        public string name { get; set; }
    }

    public class Pool
    {
        public int id { get; set; }
        public string poolType { get; set; }
    }

    public class AgentQueueModel
    {
        public int id { get; set; }
        public string projectId { get; set; }
        public string name { get; set; }
        public string groupScopeId { get; set; }
        public Pool pool { get; set; }
    }
}
