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
    public class ReleaseDefinitions
    {
        readonly VstsRestAPI.IConfiguration _sourceConfig;
        readonly string _sourceCredentials;
        readonly string _accountName;

        public ReleaseDefinitions(VstsRestAPI.IConfiguration configuration, string accountName)
        {
            _accountName = accountName;
            _sourceConfig = configuration;
            _sourceCredentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _sourceConfig.PersonalAccessToken)));
        }
        /// <summary>
        /// method to export release definition jsons from source project
        /// </summary>
        /// <param name="project"></param>
        public void ExportReleaseDefinitions(string project)
        {
            try
            {
                List<TemplatesGeneratorTool.ViewModel.ReleaseDefinitions.ReleaseDefinition> ResultList = new List<TemplatesGeneratorTool.ViewModel.ReleaseDefinitions.ReleaseDefinition>();
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_sourceConfig.UriString);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

                    HttpResponseMessage response = client.GetAsync(string.Format("https://{0}.vsrm.visualstudio.com/DefaultCollection/{1}/_apis/release/definitions?api-version=3.0-preview.1", _accountName, project)).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        ReleaseDefinitionsResponse.Release Definitions = Newtonsoft.Json.JsonConvert.DeserializeObject<ReleaseDefinitionsResponse.Release>(response.Content.ReadAsStringAsync().Result.ToString());
                        foreach (ReleaseDefinitionsResponse.Value value in Definitions.value)
                        {
                            TemplatesGeneratorTool.ViewModel.ReleaseDefinitions.ReleaseDefinition DefinitionResult = new TemplatesGeneratorTool.ViewModel.ReleaseDefinitions.ReleaseDefinition();
                            using (var client1 = new HttpClient())
                            {
                                client1.BaseAddress = new Uri(_sourceConfig.UriString);
                                client1.DefaultRequestHeaders.Accept.Clear();
                                client1.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

                                HttpResponseMessage ResponseDef = client1.GetAsync(string.Format("https://{0}.vsrm.visualstudio.com/DefaultCollection/{1}/_apis/release/definitions/{2}?api-version=3.0-preview.1", _accountName, project, value.id)).Result;
                                if (response.IsSuccessStatusCode)
                                {
                                    DefinitionResult = Newtonsoft.Json.JsonConvert.DeserializeObject<TemplatesGeneratorTool.ViewModel.ReleaseDefinitions.ReleaseDefinition>(ResponseDef.Content.ReadAsStringAsync().Result.ToString());
                                    ResultList.Add(DefinitionResult);
                                }
                            }
                        }
                    }
                }
                if (ResultList.Count > 0)
                {
                    if (!Directory.Exists(@"Templates\ReleaseDefinitions"))
                    {
                        Directory.CreateDirectory(@"Templates\ReleaseDefinitions");
                    }
                    int count = 1;
                    foreach (TemplatesGeneratorTool.ViewModel.ReleaseDefinitions.ReleaseDefinition def in ResultList)
                    {
                        foreach (var environment in def.environments)
                        {
                            environment.queueId = "$Hosted$";
                            environment.owner.displayName = "$OwnerDisplayName$";
                            environment.owner.id = "$OwnerId$";
                            environment.owner.uniqueName = "$OwnerUniqueName$";

                            foreach (var task in environment.deployStep.tasks)
                            {
                                if (task.inputs.ConnectedServiceName != "")
                                {
                                    task.inputs.ConnectedServiceName = string.Format("${0}$", string.Empty);
                                }
                                if (task.inputs.connectedServiceName != "")
                                {
                                    task.inputs.connectedServiceName = string.Format("${0}$", string.Empty);
                                }
                                if (task.inputs.serviceEndpoint != "")
                                {
                                    task.inputs.serviceEndpoint = string.Format("${0}$", string.Empty);
                                }
                                if (task.inputs.ConnectedServiceNameARM != "")
                                {
                                    task.inputs.ConnectedServiceNameARM = string.Format("${0}$", string.Empty);
                                }
                            }
                        }
                        foreach (var artifact in def.artifacts)
                        {
                            artifact.definitionReference.project.id = "$ProjectId$";
                            artifact.definitionReference.project.name = "$ProjectName$";
                            artifact.definitionReference.definition.id = string.Format("${0}-id$", artifact.definitionReference.definition.name);
                            artifact.sourceId = string.Format("$ProjectId$:${0}-id$", artifact.definitionReference.definition.name);
                        }

                        System.IO.File.WriteAllText(@"Templates\ReleaseDefinitions\ReleaseDef" + count + ".json", JsonConvert.SerializeObject(def, Formatting.Indented));
                        count = count + 1;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error occured while generating release definition template: " + ex.Message);
                Console.WriteLine("");
            }
        }
    }
}
