using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TemplatesGeneratorTool.ViewModel;
using Newtonsoft.Json;

namespace TemplatesGeneratorTool.Generators
{
    public class CardFieldsAndCardStyles
    {
        readonly VstsRestAPI.IConfiguration _sourceConfig;
        readonly string _sourceCredentials;

        public CardFieldsAndCardStyles(VstsRestAPI.IConfiguration configuration)
        {
            _sourceConfig = configuration;
            _sourceCredentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _sourceConfig.PersonalAccessToken)));
        }


        /// <summary>
        /// Method to export cardfield json from source project
        /// </summary>
        /// <param name="project"></param>
        public void GetCardFields(string project)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_sourceConfig.UriString);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

                    HttpResponseMessage response = client.GetAsync(_sourceConfig.UriString + string.Format("/DefaultCollection/{0}/_apis/work/boards/Backlog%20items/cardsettings?api-version=2.0-preview.1", project)).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        CardFieldResponse.CardFields CardFields = Newtonsoft.Json.JsonConvert.DeserializeObject<CardFieldResponse.CardFields>(response.Content.ReadAsStringAsync().Result.ToString());

                        if (!Directory.Exists("Templates"))
                        {
                            Directory.CreateDirectory("Templates");
                        }

                        string fetchedCardFieldsJSON = JsonConvert.SerializeObject(CardFields, Formatting.Indented);
                        System.IO.File.WriteAllText(@"Templates\UpdateCardFields.json", fetchedCardFieldsJSON);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error occured while generating card field template: " + ex.Message);
                Console.WriteLine("");
            }
        }
        /// <summary>
        /// Method to export cardstyles json from source project
        /// </summary>
        /// <param name="project"></param>
        public void GetCardStyles(string project)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_sourceConfig.UriString);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

                    HttpResponseMessage response = client.GetAsync(_sourceConfig.UriString + string.Format("/DefaultCollection/{0}/_apis/work/boards/Backlog%20items/cardrulesettings?api-version=2.0-preview.1", project)).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        CardStyleResponse.CardStyles CardStyles = Newtonsoft.Json.JsonConvert.DeserializeObject<CardStyleResponse.CardStyles>(response.Content.ReadAsStringAsync().Result.ToString());

                        if (!Directory.Exists("Templates"))
                        {
                            Directory.CreateDirectory("Templates");
                        }
                        string fetchedCardStylesJSON = JsonConvert.SerializeObject(CardStyles, Formatting.Indented);
                        System.IO.File.WriteAllText(@"Templates\UpdateCardStyles.json", fetchedCardStylesJSON);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error occured while generating card style template: " + ex.Message);
                Console.WriteLine("");
            }
        }
    }
}