using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VstsRestAPI.Viewmodel.ProjectAndTeams;

namespace VstsRestAPI.ProjectsAndTeams
{
    public class Account : ApiServiceBase
    {
        public Account(IConfiguration configuration) : base(configuration) { }

        /// <summary>
        /// Get Account members
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="AccessToken"></param>
        /// <returns></returns>
        public AccountMembers.Account GetAccountMembers(string accountName, string AccessToken)
        {
            AccountMembers.Account viewModel = new AccountMembers.Account();
            using (var client = GetHttpClient())
            {
                // connect to the REST endpoint            
                //HttpResponseMessage response = client.GetAsync("/_apis/memberentitlements?api-version=4.1-preview.1&top=100&skip=0").Result;
                HttpResponseMessage response = client.GetAsync("/_apis/userentitlements?api-version=4.1-preview").Result;

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