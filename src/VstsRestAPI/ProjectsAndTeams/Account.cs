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
    public class Account
    {
        public string lastFailureMessage;
        readonly IConfiguration _configuration;
        readonly string _credentials;

        public Account(IConfiguration configuration)
        {
            _configuration = configuration;
            _credentials = configuration.PersonalAccessToken;//Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _configuration.PersonalAccessToken)));
        }

        /// <summary>
        /// Get Account members
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="AccessToken"></param>
        /// <returns></returns>
        public AccountMembers.Account GetAccountMembers(string accountName, string AccessToken)
        {
            AccountMembers.Account viewModel = new AccountMembers.Account();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(string.Format("https://{0}.vsaex.visualstudio.com/", accountName));
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

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