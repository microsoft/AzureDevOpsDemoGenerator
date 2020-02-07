using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.QueriesAndWidgets
{
    public class DashBoardeTagResponse
    {
        public class Dashboard
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int Position { get; set; }
            public string ETag { get; set; }
        }
    }
}
