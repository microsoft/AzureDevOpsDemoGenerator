using System;
using System.Collections.Generic;
using System.Text;

namespace AzureDevOpsRestApi.Git
{
    public class ImportRepoResponse
    {
        public class Import
        {
            public string vcs { get; set; }
            public string use_lfs { get; set; }
            public string vcs_url { get; set; }
            public string status { get; set; }
            public string status_text { get; set; }
            public bool has_large_files { get; set; }
            public int large_files_size { get; set; }
            public int large_files_count { get; set; }
            public int authors_count { get; set; }
            public int percent { get; set; }
            public int commit_count { get; set; }
            public string url { get; set; }
            public string html_url { get; set; }
            public string authors_url { get; set; }
            public string repository_url { get; set; }
        }


    }
}
