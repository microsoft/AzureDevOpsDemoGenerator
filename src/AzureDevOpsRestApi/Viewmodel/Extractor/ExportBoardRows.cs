using System.Collections.Generic;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class ExportBoardRows
    {
        public class Value
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class Rows
        {
            public string BoardName { get; set; }
            public List<Value> Value { get; set; }
        }
    }
}
