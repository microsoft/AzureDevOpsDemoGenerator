using Newtonsoft.Json;
using System.Collections.Generic;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class KeyConfig
    {
        public class keys
        {
            [JsonProperty(PropertyName = "keys")]
            public List<string> KeysValue { get; set; }
        }
    }
}
