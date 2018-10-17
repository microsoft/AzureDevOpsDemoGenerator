using System.Net.Http;
using VstsRestAPI.Viewmodel.ProjectAndTeams;

namespace VstsRestAPI.ProjectsAndTeams
{
    public class Accounts : ApiServiceBase
    {
        public Accounts(IConfiguration configuration) : base(configuration) { }

        /// <summary>
        /// Get Account members
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public AccountMembers.Account GetAccountMembers(string accountName, string accessToken)
        {
            AccountMembers.Account viewModel = new AccountMembers.Account();
            using (var client = GetHttpClient())
            {
                // connect to the REST endpoint            
                HttpResponseMessage response = client.GetAsync("/_apis/userentitlements?api-version="+_configuration.VersionNumber).Result;

                // check to see if we have a succesfull respond
                if (response.IsSuccessStatusCode)
                {
                    viewModel = response.Content.ReadAsAsync<AccountMembers.Account>().Result;
                }
                return viewModel;
            }
        }
    }
}