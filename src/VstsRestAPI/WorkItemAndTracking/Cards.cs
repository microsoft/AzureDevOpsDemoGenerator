using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VstsRestAPI.Viewmodel.WorkItem;

namespace VstsRestAPI.WorkItemAndTracking
{
    public class Cards
    {
        public string LastFailureMessage;
        readonly IConfiguration _configuration;
        readonly string _credentials;

        public Cards(IConfiguration configuration)
        {
            _configuration = configuration;
            _credentials = configuration.PersonalAccessToken;//Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _configuration.PersonalAccessToken)));
        }
        /// <summary>
        /// Update Card fields
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="Json"></param>

        public void UpdateCardField(string projectName, string Json)
        {
            GetCardFieldResponse.ListofCards cardfield = JsonConvert.DeserializeObject<GetCardFieldResponse.ListofCards>(Json);

            if (cardfield.cards.Message == null)
            {
                cardfield.cards.Message = "test";
            }
            //List<ColumnPost> Columns = JsonConvert.DeserializeObject<List<ColumnPost>>(fileName);
            GetCardFieldResponse.ListofCards viewModel = new GetCardFieldResponse.ListofCards();
            string teamName = projectName + " Team";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                // serialize the fields array into a json string
                var patchValue = new StringContent(JsonConvert.SerializeObject(cardfield), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
                var method = new HttpMethod("PUT");
                // GetBoards getAgileBoards = new GetBoards(_configuration);
                string boardURL = _configuration.UriString + "/" + projectName + "/" + teamName + "/_apis/work/boards/Backlog%20items/cardsettings?api-version=2.0-preview";
                //Console.WriteLine("Board URL is {0}", boardURL);
                var request = new HttpRequestMessage(method, boardURL) { Content = patchValue };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    viewModel = response.Content.ReadAsAsync<GetCardFieldResponse.ListofCards>().Result;

                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                }

                // viewModel.HttpStatusCode = response.StatusCode;

                //return viewModel;

            }
        }
        /// <summary>
        /// Apply rules to cards
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="Json"></param>

        public void ApplyRules(string projectName, string Json)
        {
            //CardStylesPatch.ListofCardStyles cardStyles1 = JsonConvert.DeserializeObject<CardStylesPatch.ListofCardStyles>(File.ReadAllText(""));

            CardStylesPatch.ListofCardStyles cardStyles = JsonConvert.DeserializeObject<CardStylesPatch.ListofCardStyles>(Json);
            if (cardStyles.rules.Message == null)
            {
                cardStyles.rules.Message = "test";
            }

            string teamName = projectName + " Team";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                // serialize the fields array into a json string
                var patchValue = new StringContent(JsonConvert.SerializeObject(cardStyles), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
                var method = new HttpMethod("PATCH");
                // GetBoards getAgileBoards = new GetBoards(_configuration);
                string boardURL = _configuration.UriString + "/" + projectName + "/" + teamName + "/_apis/work/boards/Backlog%20items/cardrulesettings?api-version=" + _configuration.VersionNumber + "-preview";
                //Console.WriteLine("Board URL i s {0}", boardURL);
                var request = new HttpRequestMessage(method, boardURL) { Content = patchValue };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    // viewModel = response.Content.ReadAsAsync<GetCardFieldResponse.ListofCards>().Result;

                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                }
            }
        }

        /// <summary>
        /// Enable Epic
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="Json"></param>
        /// <param name="project"></param>
        public void EnablingEpic(string projectName, string Json, string project)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

                var jsonContent = new StringContent(Json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("PATCH");
                string teamName = projectName + " Team";
                var request = new HttpRequestMessage(method, _configuration.UriString + project + "/" + teamName + "/_apis/work/teamsettings?api-version=3.0-preview") { Content = jsonContent };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                }
            }
        }
    }
}
