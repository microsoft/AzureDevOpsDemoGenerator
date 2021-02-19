using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class GetINumIteration
    {
        public class Attributes
        {
            public DateTime? StartDate { get; set; }
            public DateTime? FinishDate { get; set; }
            public string TimeFrame { get; set; }
        }

        public class Value
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Path { get; set; }
            public Attributes Attributes { get; set; }
            public string Url { get; set; }
        }

        public class Iterations
        {
            public int Count { get; set; }
            public IList<Value> Value { get; set; }
        }


    }
}
