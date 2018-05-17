using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatesGeneratorTool
{
    public class Configuration : IConfiguration
    {
        public string UriString { get; set; }
        public string PersonalAccessToken { get; set; }
        public string Project { get; set; }
        public string Team { get; set; }
        public string MoveToProject { get; set; }
        public string Query { get; set; }
        public string Identity { get; set; }
        public string WorkItemIds { get; set; }
        public string WorkItemId { get; set; }
        public string ProcessId { get; set; }
        public string PickListId { get; set; }
        public string QueryId { get; set; }
        public string FilePath { get; set; }
        public string GitRepositoryId { get; set; }
    }
}
