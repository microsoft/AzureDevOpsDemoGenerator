using System.Collections.Generic;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{

    public class Value
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IsDefault { get; set; }
    }

    public class TeamList
    {
        public IList<Value> Value { get; set; }
    }
}
