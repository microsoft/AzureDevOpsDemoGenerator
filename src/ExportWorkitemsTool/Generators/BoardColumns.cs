using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TemplatesGeneratorTool.ViewModel;

namespace TemplatesGeneratorTool.Generators
{
    public class BoardColumns
    {
        readonly VstsRestAPI.IConfiguration _sourceConfig;
        readonly string _sourceCredentials;

        public BoardColumns(VstsRestAPI.IConfiguration configuration)
        {
            _sourceConfig = configuration;
            _sourceCredentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _sourceConfig.PersonalAccessToken)));
        }

        /// <summary>
        /// method to export BoardColumns json from the source project
        /// </summary>
        /// <param name="project"></param>
        public void ExportBoardColumns(string project)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_sourceConfig.UriString);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

                    HttpResponseMessage response = client.GetAsync(_sourceConfig.UriString + string.Format("/DefaultCollection/{0}/{1}%20Team/_apis/work/boards/Backlog%20items/columns?api-version=2.0-preview", project, project)).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        BoardColumnsResponse columns = Newtonsoft.Json.JsonConvert.DeserializeObject<BoardColumnsResponse>(response.Content.ReadAsStringAsync().Result.ToString());

                        if (!Directory.Exists("Templates"))
                        {
                            Directory.CreateDirectory("Templates");
                        }

                        string fetchedCardFieldsJSON = JsonConvert.SerializeObject(columns.value, Formatting.Indented);
                        System.IO.File.WriteAllText(@"Templates\BoardColumns.json", fetchedCardFieldsJSON);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error occured while generating board columns template: " + ex.Message);
                Console.WriteLine("");
            }
        }
    }
}
        
        