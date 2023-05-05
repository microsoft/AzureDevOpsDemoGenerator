using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.Plans
{
   public class APlan
    {
        public class CardSettings
        {
            public Fields fields { get; set; }
        }

        public class Clause
        {
            public string fieldName { get; set; }
            public string logicalOperator { get; set; }
            public string @operator { get; set; }
            public string value { get; set; }
        }

        public class CoreField
        {
            public string referenceName { get; set; }
            public string displayName { get; set; }
            public string fieldType { get; set; }
            public bool isIdentity { get; set; }
        }

        public class Criterion
        {
            public string fieldName { get; set; }
            public string logicalOperator { get; set; }
            public string @operator { get; set; }
            public string value { get; set; }
        }

        public class Fields
        {
            public bool showId { get; set; }
            public bool showAssignedTo { get; set; }
            public string assignedToDisplayFormat { get; set; }
            public bool showState { get; set; }
            public bool showTags { get; set; }
            public bool showParent { get; set; }
            public bool showEmptyFields { get; set; }
            public bool showChildRollup { get; set; }
            public object additionalFields { get; set; }
            public List<CoreField> coreFields { get; set; }
        }

        public class Marker
        {
            public DateTime date { get; set; }
            public string label { get; set; }
            public string color { get; set; }
        }

        public class Properties
        {
            public List<TeamBacklogMapping> teamBacklogMappings { get; set; }
            public List<Criterion> criteria { get; set; }
            public CardSettings cardSettings { get; set; }
            public List<Marker> markers { get; set; }
            public List<StyleSetting> styleSettings { get; set; }
            public List<TagStyleSetting> tagStyleSettings { get; set; }
        }

        public class Root
        {
            public string id { get; set; }
            public int revision { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public Properties properties { get; set; }
        }

        public class Settings
        {
            [JsonProperty("background-color")]
            public string backgroundcolor { get; set; }

            [JsonProperty("title-color")]
            public string titlecolor { get; set; }
        }

        public class StyleSetting
        {
            public string name { get; set; }
            public string isEnabled { get; set; }
            public string filter { get; set; }
            public List<Clause> clauses { get; set; }
            public Settings settings { get; set; }
        }

        public class TagStyleSetting
        {
            public string name { get; set; }
            public string isEnabled { get; set; }
            public Settings settings { get; set; }
        }

        public class TeamBacklogMapping
        {
            public string teamId { get; set; }
            public string categoryReferenceName { get; set; }
        }
    }
}
