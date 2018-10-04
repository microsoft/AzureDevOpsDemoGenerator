using Newtonsoft.Json;
using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class CardFiledsAgile
    {
        public class Bug
        {
            public string fieldIdentifier { get; set; }
            public string displayFormat { get; set; }
            public string displayType { get; set; }
            public string showEmptyFields { get; set; }
        }

        public class UserStory
        {
            public string fieldIdentifier { get; set; }
            public string displayFormat { get; set; }
            public string displayType { get; set; }
            public string showEmptyFields { get; set; }
        }

        public class Cards
        {
            public IList<Bug> Bug { get; set; }
            [JsonProperty(PropertyName = "User Story")]
            public IList<UserStory> ProductBacklogItem { get; set; }
        }

        public class CardField
        {
            public Cards cards { get; set; }
        }
    }
}
