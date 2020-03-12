using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.ReleaseDefinition
{
    public class ReleaseDefinitionsResponse
    {
        public class Release
        {
            public int Count { get; set; }
            public Value[] Value { get; set; }
        }
        public class Value
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
