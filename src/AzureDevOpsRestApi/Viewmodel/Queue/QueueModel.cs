using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.Queue
{
    public class QueueModel : BaseViewModel
    {
        public int Count { get; set; }
        public AgentQueueModel[] Value { get; set; }
    }

    public class Value
    {
        public int Id { get; set; }
        public string ProjectId { get; set; }
        public string Name { get; set; }
    }

    public class Pool
    {
        public int Id { get; set; }
        public string PoolType { get; set; }
    }

    public class AgentQueueModel
    {
        public int Id { get; set; }
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public string GroupScopeId { get; set; }
        public Pool Pool { get; set; }
    }
}
