using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatesGeneratorTool.ViewModel
{
    public class PullRequestResponse
    {
        public class Reviewer
        {
            public string id { get; set; }
        }
        public class Value
        {
            public string pullRequestId { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public string sourceRefName { get; set; }
            public string targetRefName { get; set; }
            public IList<Reviewer> reviewers { get; set; }
        }
        public class PullRequest
        {
            public IList<Value> value { get; set; }
            public int count { get; set; }
        }


    }
}
