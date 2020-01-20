using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class GetExtensions
    {
        public class Value
        {
            public string ExtensionId { get; set; }
            public string ExtensionName { get; set; }
            public string PublisherId { get; set; }
            public string PublisherName { get; set; }
            public string Flags { get; set; }
        }

        public class ExtensionsList
        {
            public int Count { get; set; }
            public IList<Value> Value { get; set; }
        }
    }
}
