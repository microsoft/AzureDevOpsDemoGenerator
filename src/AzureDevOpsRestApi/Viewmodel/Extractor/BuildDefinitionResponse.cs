using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class BuildDefinitionResponse
    {
        public class Build
        {
            public int Count { get; set; }
            public Value[] Value { get; set; }
        }
        public class Value
        {
            public int Id { get; set; }
        }
    }
}
