using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.WorkItem
{
    public class GetCardFieldResponse
    {

        public class ListofCards
        {
            [JsonProperty("cards")]
            public Cards cards { get; set; }
        }
        public class Cards : BaseViewModel
        {
            [JsonProperty("Bug")]
            public Dictionary<string, string>[] bugs { get; set; }
            [JsonProperty("Product Backlog Item")]
            public Dictionary<string, string>[] pbis { get; set; }
        }

        /*
                    public class Bug
                    {
                    public string fieldIdentifier { get; set; }
                    public string displayFormat { get; set; }
                    public string displayType { get; set; }
                    public string showEmptyFields { get; set; }

                }
                    public class PBI
                    {
                    public string fieldIdentifier { get; set; }
                    public string displayFormat { get; set; }
                    public string displayType { get; set; }
                    public string showEmptyFields { get; set; }
                }

                    public class FieldIdentifier
                    {
                        public string fieldIdentifier { get; set; }
                        public string displayFormat { get; set; }
                        public string displayType { get; set; }
                        public string showEmptyFields { get; set; }
                    }
                    */
    }
}
