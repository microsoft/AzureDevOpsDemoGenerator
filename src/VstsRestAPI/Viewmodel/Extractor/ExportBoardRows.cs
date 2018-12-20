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
            public string Team { get; set; }
            public List<Value> value { get; set; }
        }
        public class TeamRows
        {
            public List<Rows> Rows { get; set; }
        }

    }
}
