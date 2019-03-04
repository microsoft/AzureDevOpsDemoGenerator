using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class GetExtensions
    {
        public class Value
        {
            public string extensionId { get; set; }
            public string extensionName { get; set; }
            public string publisherId { get; set; }
            public string publisherName { get; set; }
            public string flags { get; set; }
        }

        public class ExtensionsList
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }
    }
}
