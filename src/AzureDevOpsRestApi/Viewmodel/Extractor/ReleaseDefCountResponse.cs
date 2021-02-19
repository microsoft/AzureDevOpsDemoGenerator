using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class ReleaseDefCountResponse
    {
        public class ReleaseDefinition
        {
            public int Id { get; set; }
        }

        public class Value
        {
            public string Source { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public object VariableGroups { get; set; }
        }

        public class Release
        {
            public int Count { get; set; }
            public IList<Value> Value { get; set; }
        }
    }
}
