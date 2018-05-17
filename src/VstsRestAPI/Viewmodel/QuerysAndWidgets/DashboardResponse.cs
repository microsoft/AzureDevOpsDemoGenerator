using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.QuerysAndWidgets
{
    public class DashboardResponse
    {
        public class Dashboard : BaseViewModel
        {
            public Value[] dashboardEntries { get; set; }
        }
        public class Value
        {
            public string id { get; set; }
            public string name { get; set; }
        }
    }
}
