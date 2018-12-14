using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.Extractor
{

    public class Value
    {
        public string name { get; set; }
        public string description { get; set; }
    }

    public class TeamList
    {
        public IList<Value> value { get; set; }
    }
}
