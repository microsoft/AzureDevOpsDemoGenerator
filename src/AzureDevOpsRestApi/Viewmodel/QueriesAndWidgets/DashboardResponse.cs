using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.QueriesAndWidgets
{
    public class DashboardResponse
    {
        public class Dashboard : BaseViewModel
        {
            public Value[] DashboardEntries { get; set; }
        }
        public class Value
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}
