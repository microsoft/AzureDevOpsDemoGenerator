using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI
{
    public class Utility
    {
        public static string GeterroMessage(string exception)
        {
            string message = string.Empty;
            try
            {
                JObject jItems = JObject.Parse(exception); 
                message = jItems["message"].ToString();

                return message;
            }
            catch (Exception)
            {
                return message;
            }
        }
    }

}
