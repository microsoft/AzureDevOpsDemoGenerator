using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class GetReleaseDefResponse
    {
        public class Avatar
        {
            public string href { get; set; }
        }

        public class Links
        {
            public Avatar avatar { get; set; }
        }

        public class CreatedBy
        {
            public string displayName { get; set; }
            public string url { get; set; }
            public Links _links { get; set; }
            public string id { get; set; }
            public string uniqueName { get; set; }
            public string imageUrl { get; set; }
            public string descriptor { get; set; }
        }

        public class ModifiedBy
        {
            public string displayName { get; set; }
            public string url { get; set; }
            public string id { get; set; }
            public string uniqueName { get; set; }
            public string imageUrl { get; set; }
            public string descriptor { get; set; }
        }

        public class Properties
        {
        }

        public class Value
        {
            public string source { get; set; }
            public int revision { get; set; }
            public object description { get; set; }
            public DateTime createdOn { get; set; }
            public DateTime modifiedOn { get; set; }
            public bool isDeleted { get; set; }
            public object variableGroups { get; set; }
            public string releaseNameFormat { get; set; }
            public Properties properties { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public string path { get; set; }
            public object projectReference { get; set; }
            public string url { get; set; }
        }

        public class ReleaseDef
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }
    }
}
