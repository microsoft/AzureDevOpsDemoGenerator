using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.WorkItem
{
    class WorkItemPatch
    {
        public class Field
        {
            public string Op { get; set; }
            public string Path { get; set; }
            public object Value { get; set; }
        }

        public class Value
        {
            public string Rel { get; set; }
            public string Url { get; set; }
            public Attributes Attributes { get; set; }
        }

        public class Attributes
        {
            public string Comment { get; set; }
            public string Name { get; set; }
        }
    }
}
