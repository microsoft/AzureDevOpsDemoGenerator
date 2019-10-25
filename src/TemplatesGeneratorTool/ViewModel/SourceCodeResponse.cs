using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatesGeneratorTool.ViewModel
{
    public class SourceCodeResponse
    {
        public class GitSource
        {
            public string url { get; set; }
        }

        public class Parameters
        {
            public GitSource gitSource { get; set; }
        }
        public class Value
        {
            public Parameters parameters { get; set; }
            public string serviceEndpointId = "";
            public bool deleteServiceEndpointAfterImportIsDone = true;
        }

        public class Code
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }
    }
}