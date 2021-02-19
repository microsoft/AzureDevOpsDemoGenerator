﻿using System.Collections.Generic;

namespace AzureDevOpsDemoBuilder.ExtractorModels
{
    public class ExtractorAnalysis
    {
        public int teamCount { get; set; }
        public int IterationCount { get; set; }
        public int BuildDefCount { get; set; }
        public int ReleaseDefCount { get; set; }
        public Dictionary<string, int> WorkItemCounts { get; set; }
        public List<string> ErrorMessages { get; set; }
    }
}