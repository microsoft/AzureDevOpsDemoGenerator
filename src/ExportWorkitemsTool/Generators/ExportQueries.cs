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
    public class ExportQueries
    {
        readonly VstsRestAPI.IConfiguration _sourceConfig;
        readonly string _sourceCredentials;

        public ExportQueries(VstsRestAPI.IConfiguration configuration)
        {
            _sourceConfig = configuration;
            _sourceCredentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _sourceConfig.PersonalAccessToken)));
        }
        /// <summary>
        /// Method to get queries jsons under Shared folder path 
        /// </summary>
        /// <param name="project"></param>
        /// <param name="path"></param>
        public void GetQueriesByPath(string project, string path)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_sourceConfig.UriString);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

                    HttpResponseMessage response = client.GetAsync("/DefaultCollection/" + project + "/_apis/wit/queries/Shared%20Queries?$depth=1&api-version=2.2").Result;

                    if (response.IsSuccessStatusCode)
                    {
                        QueryByPathResponse.query QueryResponse = response.Content.ReadAsAsync<QueryByPathResponse.query>().Result;
                        if (QueryResponse.hasChildren)
                        {
                            foreach (var child in QueryResponse.children)
                            {
                                if (!(child.isFolder))
                                {
                                    HttpResponseMessage responseForQuery = client.GetAsync("/DefaultCollection/" + project + "/_apis/wit/queries/" + child.id + "?$expand=wiql&$depth=2&api-version=4.1").Result;

                                    QueryByPathResponse.QueryWithWiql Query = responseForQuery.Content.ReadAsAsync<QueryByPathResponse.QueryWithWiql>().Result;


                                    if (!Directory.Exists(@"Templates\QueriesByPath"))
                                    {
                                        Directory.CreateDirectory(@"Templates\QueriesByPath");
                                    }
                                    string fetchedCardFieldsJSON = JsonConvert.SerializeObject(Query, Formatting.Indented);
                                    System.IO.File.WriteAllText(@"Templates\QueriesByPath\" + Query.name + ".json", fetchedCardFieldsJSON);
                                }
                            }
                        }
                    }
                }
                
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error occured while generating query template: " + ex.Message);
                Console.WriteLine("");
            }

            
        }
    }
}