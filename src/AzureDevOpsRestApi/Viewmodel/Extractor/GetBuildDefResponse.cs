using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class GetBuildDefResponse
    {
        public class Project
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
            public string State { get; set; }
            public int Revision { get; set; }
            public string Visibility { get; set; }
        }

        public class Value
        {
            public Project Project { get; set; }
        }

        public class BuildDef
        {
            public int Count { get; set; }
            public IList<Value> Value { get; set; }
        }
    }
}
