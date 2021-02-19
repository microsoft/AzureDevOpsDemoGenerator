using NLog;
using System;
using System.Net.Http;
using System.Threading;
using AzureDevOpsAPI.Viewmodel.ProjectAndTeams;

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
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    AccountMembers.Account viewModel = new AccountMembers.Account();
                    using (var client = GetHttpClient())
                    {
                        // connect to the REST endpoint            
                        HttpResponseMessage response = client.GetAsync("/_apis/userentitlements?api-version=" + Configuration.VersionNumber).Result;

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
                    this.LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new AccountMembers.Account(); 
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new AccountMembers.Account();
        }
    }
}