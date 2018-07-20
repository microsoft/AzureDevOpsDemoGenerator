using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatesGeneratorTool.ViewModel
{
    public class GetWorkItemsResponse
    {
        public class Results : BaseViewModel
        {
            public string queryType { get; set; }
            public string queryResultType { get; set; }
            public DateTime asOf { get; set; }
            public Column[] columns { get; set; }
            public Workitem[] workItems { get; set; }
        }

        public class Column
        {
            public string referenceName { get; set; }
            public string name { get; set; }
            public string url { get; set; }
        }

        public class Workitem
        {
            public int id { get; set; }
            public string url { get; set; }
        }
    }
}
