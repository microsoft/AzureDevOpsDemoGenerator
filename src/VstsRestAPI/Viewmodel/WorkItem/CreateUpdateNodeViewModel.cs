using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.WorkItem
{
    public class CreateUpdateNodeViewModel
    {
        public class Node : BaseViewModel
        {
            public int id { get; set; }
            public string name { get; set; }
            public Attributes attributes { get; set; }
        }

        public class Attributes
        {
            public DateTime startDate { get; set; }
            public DateTime finishDate { get; set; }
        }
    }
}
