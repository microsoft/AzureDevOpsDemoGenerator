using System;
using System.Collections.Generic;
using System.Text;

namespace AzureDevOpsRestApi.Viewmodel.GitHub
{
    public class ProtectionRule
    {
        public string branch { get; set; }
        public List<dynamic> rules { get; set; }
    }
}
