using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class GetReleaseDefResponse
    {
        public class Avatar
        {
            public string Href { get; set; }
        }

        public class Links
        {
            public Avatar Avatar { get; set; }
        }

        public class CreatedBy
        {
            public string DisplayName { get; set; }
            public string Url { get; set; }
            public Links Links { get; set; }
            public string Id { get; set; }
            public string UniqueName { get; set; }
            public string ImageUrl { get; set; }
            public string Descriptor { get; set; }
        }

        public class ModifiedBy
        {
            public string DisplayName { get; set; }
            public string Url { get; set; }
            public string Id { get; set; }
            public string UniqueName { get; set; }
            public string ImageUrl { get; set; }
            public string Descriptor { get; set; }
        }

        public class Properties
        {
        }

        public class Value
        {
            public string Source { get; set; }
            public int Revision { get; set; }
            public object Description { get; set; }
            public DateTime CreatedOn { get; set; }
            public DateTime ModifiedOn { get; set; }
            public bool IsDeleted { get; set; }
            public object VariableGroups { get; set; }
            public string ReleaseNameFormat { get; set; }
            public Properties Properties { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public string Path { get; set; }
            public object ProjectReference { get; set; }
            public string Url { get; set; }
        }

        public class ReleaseDef
        {
            public int Count { get; set; }
            public IList<Value> Value { get; set; }
        }
    }
}
