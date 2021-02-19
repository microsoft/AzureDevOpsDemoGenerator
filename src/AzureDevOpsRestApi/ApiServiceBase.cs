using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AzureDevOpsAPI
{
    public abstract class ApiServiceBase
    {
        public string LastFailureMessage { get; set; }
        protected readonly IAppConfiguration Configuration;
        protected readonly string Credentials;
        protected readonly string Project;
        protected readonly string ProjectId;
        protected readonly string Account;
        protected readonly string Team;
        protected readonly string BaseAddress;
        protected readonly string MediaType;
        protected readonly string Scheme;
        protected readonly string GitCredential;
        protected readonly string UserName;


        public ApiServiceBase(IAppConfiguration configuration)
        {
            Configuration = configuration;
            Credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", Configuration.PersonalAccessToken)));//configuration.PersonalAccessToken;
            Project = configuration.Project;
            Account = configuration.AccountName;
            Team = configuration.Team;
            ProjectId = configuration.ProjectId;

            BaseAddress = configuration.GitBaseAddress;
            MediaType = configuration.MediaType;
            Scheme = configuration.Scheme;
            GitCredential = configuration.GitCredential;
            UserName = configuration.UserName;
        }

        protected HttpClient GetHttpClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(Configuration.UriString)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Credentials);

            return client;
        }
        protected HttpClient GitHubHttpClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(BaseAddress)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaType));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Scheme, GitCredential);
            return client;
        }
    }
}
