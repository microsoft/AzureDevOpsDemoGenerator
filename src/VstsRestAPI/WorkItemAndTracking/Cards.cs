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
            GetCardFieldResponse.ListofCards cardfield = new GetCardFieldResponse.ListofCards();
            GetCardFieldResponseAgile.ListofCards agilecardfield = new GetCardFieldResponseAgile.ListofCards();

            if (boardType == "Backlog%20items")
            {
                cardfield = JsonConvert.DeserializeObject<GetCardFieldResponse.ListofCards>(json);
                if (cardfield.cards.Message == null)
                {
                    cardfield.cards.Message = "test";
                }
            }
            else if (boardType == "Stories")
            {
                agilecardfield = JsonConvert.DeserializeObject<GetCardFieldResponseAgile.ListofCards>(json);
                if (agilecardfield.cards.Message == null)
                {
                    agilecardfield.cards.Message = "test";
                }
            }

            string teamName = projectName + " Team";
            using (var client = GetHttpClient())
            {
                StringContent patchValue = new StringContent("");
                if (boardType == "Backlog%20items")
                {
                    patchValue = new StringContent(JsonConvert.SerializeObject(cardfield), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
                }
                else if (boardType == "Stories")
                {
                    patchValue = new StringContent(JsonConvert.SerializeObject(agilecardfield), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
                }
                var method = new HttpMethod("PUT");
                string boardURL = "https://dev.azure.com/" + Account + "/" + projectName + "/" + teamName + "/_apis/work/boards/" + boardType + "/cardsettings?api-version=" + _configuration.VersionNumber;
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
                //PATCH https://dev.azure.com/{organization}/{project}/{team}/_apis/work/boards/{board}/cardrulesettings?api-version=4.1
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
        public void EnablingEpic(string projectName, string json, string project)
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
