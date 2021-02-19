using Newtonsoft.Json;
using System.Collections.Generic;

namespace AzureDevOpsAPI.Viewmodel.Importer
{
    public class ImportCardFields
    {
        public class Epic
        {
            public string FieldIdentifier { get; set; }
            public string DisplayFormat { get; set; }
        }

        public class Feature
        {
            public string FieldIdentifier { get; set; }
            public string DisplayFormat { get; set; }
        }

        public class ProductBacklogItem
        {
            public string FieldIdentifier { get; set; }
            public string DisplayFormat { get; set; }
        }
        public class Issue
        {
            public string FieldIdentifier { get; set; }
            public string DisplayFormat { get; set; }
        }
        public class Bug
        {
            public string FieldIdentifier { get; set; }
            public string DisplayFormat { get; set; }
        }
        public class UserStory
        {
            public string FieldIdentifier { get; set; }
            public string DisplayFormat { get; set; }
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
            [JsonProperty("Issue")]
            public IList<Issue> Issue { get; set; }
        }

        public class CardFields
        {
            public Cards Cards { get; set; }
            public string BoardName { get; set; }
        }
    }
}
