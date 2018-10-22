using Newtonsoft.Json;
using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class CardFiledsScrum
    {
        public class Bug
        {
            public string fieldIdentifier { get; set; }
            public string displayFormat { get; set; }
            public string displayType { get; set; }
            public string showEmptyFields { get; set; }
        }

        public class ProductBacklogItem
        {
            public string fieldIdentifier { get; set; }
            public string displayFormat { get; set; }
            public string displayType { get; set; }
            public string showEmptyFields { get; set; }
        }

        public class Cards
        {
            public IList<Bug> Bug { get; set; }
            [JsonProperty(PropertyName = "Product Backlog Item")]
            public IList<ProductBacklogItem> ProductBacklogItem { get; set; }
        }

        public class CardField
        {
            public Cards cards { get; set; }
        }
    }
}
