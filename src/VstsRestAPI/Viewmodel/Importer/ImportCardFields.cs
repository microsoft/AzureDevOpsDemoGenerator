using Newtonsoft.Json;
using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.Importer
{
    public class ImportCardFields
    {
        public class Epic
        {
            public string fieldIdentifier { get; set; }
            public string displayFormat { get; set; }
        }

        public class Feature
        {
            public string fieldIdentifier { get; set; }
            public string displayFormat { get; set; }
        }

        public class ProductBacklogItem
        {
            public string fieldIdentifier { get; set; }
            public string displayFormat { get; set; }
        }

        public class Bug
        {
            public string fieldIdentifier { get; set; }
            public string displayFormat { get; set; }
        }
        public class UserStory
        {
            public string fieldIdentifier { get; set; }
            public string displayFormat { get; set; }
        }

        public class Cards
        {
            [JsonProperty("Epic")]
            public IList<Epic> Epic { get; set; }

            [JsonProperty("Feature")]
            public IList<Feature> Feature { get; set; }

            [JsonProperty("Product Backlog Item")]
            public IList<ProductBacklogItem> ProductBacklogItem { get; set; }
            [JsonProperty("User Story")]
            public IList<UserStory> UserStory { get; set; }

            [JsonProperty("Bug")]
            public IList<Bug> Bug { get; set; }
        }

        public class CardFields
        {
            public Cards cards { get; set; }
            public string BoardName { get; set; }
        }
    }
}
