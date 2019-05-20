using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace VstsRestAPI
{
    public abstract class ApiServiceBase
    {
        public string LastFailureMessage { get; set; }
        protected readonly IConfiguration _configuration;
        protected readonly string _credentials;
        protected readonly string Project;
        protected readonly string ProjectId;
        protected readonly string Account;
        protected readonly string Team;
        protected readonly string _baseAddress;
        protected readonly string _mediaType;
        protected readonly string _scheme;
        protected readonly string _Gitcredential;
        protected readonly string userName;


        public ApiServiceBase(IConfiguration configuration)
        {
            _configuration = configuration;
            _credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _configuration.PersonalAccessToken)));//configuration.PersonalAccessToken;
            Project = configuration.Project;
            Account = configuration.AccountName;
            Team = configuration.Team;
            ProjectId = configuration.ProjectId;

            _baseAddress = configuration._gitbaseAddress;
            _mediaType = configuration._mediaType;
            _scheme = configuration._scheme;
            _Gitcredential = configuration._gitcredential;
            userName = configuration.userName;
        }

        protected HttpClient GetHttpClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(_configuration.UriString)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

            return client;
        }
        protected HttpClient GitHubHttpClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(_baseAddress)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_mediaType));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_scheme, _Gitcredential);
            return client;
        }
    }
}
