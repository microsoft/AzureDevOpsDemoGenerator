using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.Repository
{
    public class PullRequestComments
    {
        public class Comment
        {
            public int parentCommentId { get; set; }
            public string content { get; set; }
            public int commentType { get; set; }
        }
        public class reply
        {
            public string content { get; set; }
            public string parentCommentId { get; set; }
            public string commentType { get; set; }
        }

        public class MicrosoftTeamFoundationDiscussionSupportsMarkdown
        {
            public string type { get; set; }
            public int value { get; set; }
        }

        public class Properties
        {
            [JsonProperty(PropertyName = "Microsoft.TeamFoundation.Discussion.SupportsMarkdown")]
            public MicrosoftTeamFoundationDiscussionSupportsMarkdown Microsoft_TeamFoundation_Discussion_SupportsMarkdown { get; set; }
        }
        public class IterationContext
        {
            public int firstComparingIteration { get; set; }
            public int secondComparingIteration { get; set; }
        }
        public class PullRequestThreadContext
        {
            public IterationContext iterationContext { get; set; }
            public int? changeTrackingId { get; set; }
        }
        public class RightFileStart
        {
            public int line { get; set; }
            public int offset { get; set; }
        }

        public class RightFileEnd
        {
            public int line { get; set; }
            public int offset { get; set; }
        }

        public class ThreadContext
        {
            public string filePath { get; set; }
            public RightFileStart rightFileStart { get; set; }
            public RightFileEnd rightFileEnd { get; set; }
        }
        public class Value
        {
            public IList<Comment> comments { get; set; }
            public Properties properties { get; set; }
            public int status { get; set; }
            public PullRequestThreadContext pullRequestThreadContext { get; set; }
            public ThreadContext threadContext { get; set; }
            public IList<reply> Replies { get; set; }

        }

        public class Comments
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }


    }
}
