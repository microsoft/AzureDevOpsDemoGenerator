using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class RepositoryList
    {
        public class Project
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class Value
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class Repository
        {
            public int Count { get; set; }
            public IList<Value> Value { get; set; }
        }
    }
}
