using Newtonsoft.Json;
using System.Collections.Generic;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class KeyConfig
    {
        public class Keys
        {
            [JsonProperty(PropertyName = "keys")]
            public List<string> KeysValue { get; set; }
        }
    }
}
