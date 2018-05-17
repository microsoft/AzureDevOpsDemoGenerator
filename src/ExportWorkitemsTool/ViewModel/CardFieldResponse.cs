using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatesGeneratorTool.ViewModel
{
    public class CardFieldResponse
    {
        public class Bug
        {
            public string fieldIdentifier = "";
            public string displayFormat = "";
            public string displayType = "";
            public string showEmptyFields = "";
        }
        public class ProductBacklogItem
        {
            public string fieldIdentifier = "";
            public string displayFormat = "";
            public string displayType = "";
            public string showEmptyFields = "";
        }
        public class Cards
        {
            public IList<Bug> Bug { get; set; }
            [JsonProperty(PropertyName = "Product Backlog Item")]
            public IList<ProductBacklogItem> ProductBacklogItem { get; set; }
        }
        public class CardFields
        {
            public Cards cards { get; set; }
        }
    }
}
