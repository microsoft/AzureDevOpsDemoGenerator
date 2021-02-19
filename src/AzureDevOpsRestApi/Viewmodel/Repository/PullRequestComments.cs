using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.Repository
{
    public class PullRequestComments
    {
        public class Comment
        {
            public int ParentCommentId { get; set; }
            public string Content { get; set; }
            public string CommentType { get; set; }
        }
        public class Reply
        {
            public string Content { get; set; }
            public string ParentCommentId { get; set; }
            public string CommentType { get; set; }
        }

        public class MicrosoftTeamFoundationDiscussionSupportsMarkdown
        {
            public string Type { get; set; }
            public int Value { get; set; }
        }

        public class Properties
        {
            [JsonProperty(PropertyName = "Microsoft.TeamFoundation.Discussion.SupportsMarkdown")]
            public MicrosoftTeamFoundationDiscussionSupportsMarkdown MicrosoftTeamFoundationDiscussionSupportsMarkdown { get; set; }
        }
        public class IterationContext
        {
            public int FirstComparingIteration { get; set; }
            public int SecondComparingIteration { get; set; }
        }
        public class PullRequestThreadContext
        {
            public IterationContext IterationContext { get; set; }
            public int? ChangeTrackingId { get; set; }
        }
        public class RightFileStart
        {
            public int Line { get; set; }
            public int Offset { get; set; }
        }

        public class RightFileEnd
        {
            public int Line { get; set; }
            public int Offset { get; set; }
        }

        public class ThreadContext
        {
            public string FilePath { get; set; }
            public RightFileStart RightFileStart { get; set; }
            public RightFileEnd RightFileEnd { get; set; }
        }
        public class Value
        {
            public IList<Comment> Comments { get; set; }
            public Properties Properties { get; set; }
            public PullRequestThreadContext PullRequestThreadContext { get; set; }
            public ThreadContext ThreadContext { get; set; }
            public IList<Reply> Replies { get; set; }

        }

        public class Comments
        {
            public int Count { get; set; }
            public IList<Value> Value { get; set; }
        }


    }
}
