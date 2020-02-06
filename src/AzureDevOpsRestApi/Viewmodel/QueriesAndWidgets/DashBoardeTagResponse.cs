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
            public string id { get; set; }
            public string name { get; set; }
            public int position { get; set; }
            public string eTag { get; set; }
        }
    }
}
