using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.WorkItem
{
    public class CreateUpdateNodeViewModel
    {
        public class Node : BaseViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Attributes Attributes { get; set; }
        }

        public class Attributes
        {
            public DateTime StartDate { get; set; }
            public DateTime FinishDate { get; set; }
        }
    }
}
