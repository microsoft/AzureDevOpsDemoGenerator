using System.Collections.Generic;

namespace AzureDevOpsAPI.Viewmodel.Importer
{
    public class ImportBoardRows
    {
        public class Value
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        public class Rows
        {
            public string BoardName { get; set; }
            public IList<Value> value { get; set; }
        }
    }
}
