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
    public class Teams
    {
        readonly VstsRestAPI.IConfiguration _sourceConfig;
        readonly string _sourceCredentials;

        public Teams(VstsRestAPI.IConfiguration configuration)
        {
            _sourceConfig = configuration;
            _sourceCredentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _sourceConfig.PersonalAccessToken)));
        }

        /// <summary>
        /// method to export teams jsons from source projects
        /// </summary>
        /// <param name="project"></param>
        public void ExportTeams(string project)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_sourceConfig.UriString);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

                    HttpResponseMessage response = client.GetAsync(_sourceConfig.UriString + string.Format("/DefaultCollection/_apis/projects/{0}/teams?api-version=2.2", project)).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        TeamsResponse.Team Teams = Newtonsoft.Json.JsonConvert.DeserializeObject<TeamsResponse.Team>(response.Content.ReadAsStringAsync().Result.ToString());

                        if (!Directory.Exists("Templates"))
                        {
                            Directory.CreateDirectory("Templates");
                        }

                        string fetchedTeamsJSON = JsonConvert.SerializeObject(Teams.value, Formatting.Indented);
                        System.IO.File.WriteAllText(@"Templates\Teams.json", fetchedTeamsJSON);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured while generating teams template: " + ex.Message);
                Console.WriteLine("");
            }
        }
    }
}
        
    