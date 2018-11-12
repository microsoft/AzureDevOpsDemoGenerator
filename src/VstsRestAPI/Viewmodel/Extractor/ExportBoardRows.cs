using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class ExportBoardRows
    {
        public class Value
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        public class Rows
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }


    }
}
