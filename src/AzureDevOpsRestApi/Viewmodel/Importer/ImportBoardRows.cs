using System.Collections.Generic;

namespace AzureDevOpsAPI.Viewmodel.Importer
{
    public class ImportBoardRows
    {
        public class Value
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class Rows
        {
            public string BoardName { get; set; }
            public IList<Value> Value { get; set; }
        }
    }
}
