using System;
using System.Net.Http;
using AzureDevOpsAPI.Viewmodel.ProjectAndTeams;
using NLog;

namespace AzureDevOpsAPI.ProjectsAndTeams
{
    public class Accounts : ApiServiceBase
    {
        public Accounts(IAppConfiguration configuration) : base(configuration) { }
        Logger logger = LogManager.GetLogger("*");
        /// <summary>
        /// Get Account members
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public AccountMembers.Account GetAccountMembers(string accountName, string accessToken)
        {
            try
            {
                AccountMembers.Account viewModel = new AccountMembers.Account();
                using (var client = GetHttpClient())
                {
                    // connect to the REST endpoint            
                    HttpResponseMessage response = client.GetAsync("/_apis/userentitlements?api-version=" + _configuration.VersionNumber).Result;

                    // check to see if we have a succesfull respond
                    if (response.IsSuccessStatusCode)
                    {
                        viewModel = response.Content.ReadAsAsync<AccountMembers.Account>().Result;
                    }
                    return viewModel;
                }
            }
            catch (Exception ex)
            {
                logger.Debug("CreateReleaseDefinition" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return new AccountMembers.Account();
        }
    }
}