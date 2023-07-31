using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.BranchPolicy
{
    public class BranchPolicy
    {
        public class Policy
        {
            public bool isEnabled { get; set; }
            public bool isBlocking { get; set; }
            public bool isDeleted { get; set; }
            public Settings settings { get; set; }
            public bool isEnterpriseManaged { get; set; }
            public Type type { get; set; }
        }

        public class Scope
        {
            public string refName { get; set; }
            public string matchKind { get; set; }
            public string repositoryId { get; set; }
        }

        public class Settings
        {
            public string buildDefinitionId { get; set; }
            public bool queueOnSourceUpdateOnly { get; set; }
            public bool manualQueueOnly { get; set; }
            public object displayName { get; set; }
            public double validDuration { get; set; }
            public List<Scope> scope { get; set; }
        }

        public class Type
        {
            public string id { get; set; }
            public string url { get; set; }
            public string displayName { get; set; }
        }
    }
}
