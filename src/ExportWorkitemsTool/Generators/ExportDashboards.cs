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
    public class ExportDashboards
    {
        readonly VstsRestAPI.IConfiguration _sourceConfig;
        readonly string _sourceCredentials;


        public ExportDashboards(VstsRestAPI.IConfiguration configuration)
        {
            _sourceConfig = configuration;
            _sourceCredentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _sourceConfig.PersonalAccessToken)));
        }

        /// <summary>
        /// Method to export Dashboard jsons form source project
        /// </summary>
        /// <param name="project"></param>
        public void GetDashboard(string project)
        {
            try
            {
                string dashBoardId = string.Empty;

                using (var clientOne = new HttpClient())
                {
                    clientOne.BaseAddress = new Uri(_sourceConfig.UriString);
                    clientOne.DefaultRequestHeaders.Accept.Clear();
                    clientOne.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    clientOne.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

                    HttpResponseMessage response = clientOne.GetAsync(project + "/_apis/Dashboard/Dashboards/??api-version=3.0-preview.2").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        DashBoardResponse.Dashboard dashBoard = response.Content.ReadAsAsync<DashBoardResponse.Dashboard>().Result;
                        dashBoardId = dashBoard.dashboardEntries[0].id;
                    }
                }
                if (!(string.IsNullOrEmpty(dashBoardId)))
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(_sourceConfig.UriString);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

                        HttpResponseMessage response = client.GetAsync(project + "/" + project + "%20Team/_apis/Dashboard/Dashboards/" + dashBoardId + "/Widgets/?api-version=3.0-preview.2").Result;

                        if (response.IsSuccessStatusCode)
                        {
                            if (!Directory.Exists(@"Templates\Dashboard"))
                            {
                                Directory.CreateDirectory(@"Templates\Dashboard");
                            }
                            System.IO.File.WriteAllText(@"Templates\Dashboard\" + dashBoardId + ".json", response.Content.ReadAsStringAsync().Result);


                        }
                    }
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine("Error occured while generating Dashboard template: " + ex.Message);
                Console.WriteLine("");
            }
        }
    }
}