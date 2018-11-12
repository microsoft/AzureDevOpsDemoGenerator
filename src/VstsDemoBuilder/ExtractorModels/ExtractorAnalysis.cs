using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VstsDemoBuilder.ExtractorModels
{
    public class ExtractorAnalysis
    {
        public int teamCount { get; set; }
        public int IterationCount { get; set; }
        public int fetchedEpics { get; set; }
        public int fetchedFeatures { get; set; }
        public int fetchedPBIs { get; set; }
        public int fetchedTasks { get; set; }
        public int fetchedTestCase { get; set; }
        public int fetchedBugs { get; set; }
        public int fetchedUserStories { get; set; }
        public int fetchedTestSuits { get; set; }
        public int fetchedTestPlan { get; set; }
        public int fetchedFeedbackRequest { get; set; }
        public int BuildDefCount { get; set; }
        public int ReleaseDefCount { get; set; }


    }
}