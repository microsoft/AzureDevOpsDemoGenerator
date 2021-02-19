using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class GetWorkItemsResponse
    {
        public class Results : BaseViewModel
        {
            public string QueryType { get; set; }
            public string QueryResultType { get; set; }
            public DateTime AsOf { get; set; }
            public Column[] Columns { get; set; }
            public Workitem[] WorkItems { get; set; }
        }

        public class Column
        {
            public string ReferenceName { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
        }

        public class Workitem
        {
            public int Id { get; set; }
            public string Url { get; set; }
        }
    }

}
