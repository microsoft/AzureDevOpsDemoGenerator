using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TemplatesGeneratorTool.ViewModel;
using System.IO;
using Newtonsoft.Json;

namespace TemplatesGeneratorTool.Generators
{

   
    public class BuildDefinitions
    {
        readonly VstsRestAPI.IConfiguration _sourceConfig;
        readonly string _sourceCredentials;
        readonly string _accountName;

        public BuildDefinitions(VstsRestAPI.IConfiguration configuration, string accountName)
        {
            _accountName = accountName;
            _sourceConfig = configuration;
            _sourceCredentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _sourceConfig.PersonalAccessToken)));
        }
     
        /// <summary>
        /// Method to export build definitions json from source project
        /// </summary>
        public void ExportBuildDefinitions(string project)
        {
            try
            {
                List<TemplatesGeneratorTool.ViewModel.BuildDefinitions.BuildDefinition> ResultList = new List<TemplatesGeneratorTool.ViewModel.BuildDefinitions.BuildDefinition>();
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_sourceConfig.UriString);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

                    HttpResponseMessage response = client.GetAsync(_sourceConfig.UriString + string.Format("/DefaultCollection/{0}/_apis/build/definitions?api-version=2.0", project)).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        BuildDefinitionResponse.Build Definitions = Newtonsoft.Json.JsonConvert.DeserializeObject<BuildDefinitionResponse.Build>(response.Content.ReadAsStringAsync().Result.ToString());
                        foreach (BuildDefinitionResponse.Value value in Definitions.value)
                        {
                            TemplatesGeneratorTool.ViewModel.BuildDefinitions.BuildDefinition DefinitionResult = new TemplatesGeneratorTool.ViewModel.BuildDefinitions.BuildDefinition();
                            using (var client1 = new HttpClient())
                            {
                                client1.BaseAddress = new Uri(_sourceConfig.UriString);
                                client1.DefaultRequestHeaders.Accept.Clear();
                                client1.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

                                HttpResponseMessage ResponseDef = client1.GetAsync(_sourceConfig.UriString + string.Format("/DefaultCollection/{0}/_apis/build/definitions/{1}?api-version=2.0", project, value.id)).Result;
                                if (response.IsSuccessStatusCode)
                                {
                                    DefinitionResult = Newtonsoft.Json.JsonConvert.DeserializeObject<TemplatesGeneratorTool.ViewModel.BuildDefinitions.BuildDefinition>(ResponseDef.Content.ReadAsStringAsync().Result.ToString());
                                    ResultList.Add(DefinitionResult);
                                }
                            }
                        }
                    }
                }
                if (ResultList.Count > 0)
                {
                    if (!Directory.Exists(@"Templates\BuildDefinitions"))
                    {
                        Directory.CreateDirectory(@"Templates\BuildDefinitions");
                    }
                    int count = 1;
                    foreach (TemplatesGeneratorTool.ViewModel.BuildDefinitions.BuildDefinition def in ResultList)
                    {
                        def.repository.id = "$repositoryId$";
                        def.repository.name = "$repositoryName$";
                        def.repository.url = "";

                        System.IO.File.WriteAllText(@"Templates\BuildDefinitions\BuildDef" + count + ".json", JsonConvert.SerializeObject(def, Formatting.Indented));
                        count = count + 1;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error occured while generating build definition template: " + ex.Message);
                Console.WriteLine("");
            }
        }
    }
}
      