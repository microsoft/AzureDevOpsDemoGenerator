using System.Collections.Generic;

namespace VstsDemoBuilder.ExtractorModels
{
    public class ExtractorAnalysis
    {
        public int teamCount { get; set; }
        public int IterationCount { get; set; }
        public int BuildDefCount { get; set; }
        public int ReleaseDefCount { get; set; }
        public Dictionary<string, int> WorkItemCounts { get; set; }
    }
}