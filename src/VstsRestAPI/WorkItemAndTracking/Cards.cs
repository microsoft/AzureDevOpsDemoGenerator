using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using VstsRestAPI.Viewmodel.WorkItem;

namespace VstsRestAPI.WorkItemAndTracking
{
    public class Cards : ApiServiceBase
    {
        public Cards(IConfiguration configuration) : base(configuration) { }

        /// <summary>
        /// Update Card fields
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="json"></param>

        public void UpdateCardField(string projectName, string json, string boardType)
        {
            json = json.Replace("null", "\"\"");
            GetCardFieldResponse.ListofCards cardField = new GetCardFieldResponse.ListofCards();
            GetCardFieldResponseAgile.ListofCards agileCardField = new GetCardFieldResponseAgile.ListofCards();

            //if (boardType == "Backlog%20items")
            //{
            //    cardField = JsonConvert.DeserializeObject<GetCardFieldResponse.ListofCards>(json);
            //    if (cardField.cards.Message == null)
            //    {
            //        cardField.cards.Message = "test";
            //    }
            //}
            //else if (boardType == "Stories")
            //{
            //    agileCardField = JsonConvert.DeserializeObject<GetCardFieldResponseAgile.ListofCards>(json);
            //    if (agileCardField.cards.Message == null)
            //    {
            //        agileCardField.cards.Message = "test";
            //    }
            //}

            string teamName = projectName + " Team";
            using (var client = GetHttpClient())
            {
                StringContent patchValue = new StringContent("");
                //json = JsonConvert.SerializeObject(cardField);
                if (boardType == "Backlog%20items")
                {
                    patchValue = new StringContent(json, Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
                }
                else if (boardType == "Stories")
                {
                    patchValue = new StringContent(json, Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
                }
                var method = new HttpMethod("PUT");
                string boardURL = _configuration.UriString  + projectName + "/" + teamName + "/_apis/work/boards/" + boardType + "/cardsettings?api-version=" + _configuration.VersionNumber;
                var request = new HttpRequestMessage(method, boardURL) { Content = patchValue };
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
        /// <summary>
        /// Apply rules to cards
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="json"></param>

        public void ApplyRules(string projectName, string json, string boardType)
        {
            json = json.Replace("null", "\"\"");
            json = json.Replace("$ProjectName$", projectName);
            CardStylesPatch.ListofCardStyles cardStyles = JsonConvert.DeserializeObject<CardStylesPatch.ListofCardStyles>(json);
            if (cardStyles.rules.Message == null)
            {
                cardStyles.rules.Message = "test";
            }

            string teamName = projectName + " Team";

            using (var client = GetHttpClient())
            {
                var patchValue = new StringContent(JsonConvert.SerializeObject(cardStyles), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
                var method = new HttpMethod("PATCH");
                string boardURL = "https://dev.azure.com/" + Account + "/" + projectName + "/" + teamName + "/_apis/work/boards/" + boardType + "/cardrulesettings?api-version=" + _configuration.VersionNumber;
                var request = new HttpRequestMessage(method, boardURL) { Content = patchValue };
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

        /// <summary>
        /// Enable Epic
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="json"></param>
        /// <param name="project"></param>
        public void EnablingEpic(string projectName, string json, string project, string team)
        {
            using (var client = GetHttpClient())
            {
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("PATCH");
                string teamName = projectName + " Team";
                var request = new HttpRequestMessage(method, project + "/" + teamName + "/_apis/work/teamsettings?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
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
