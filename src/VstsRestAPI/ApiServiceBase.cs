﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI
{
    public abstract class ApiServiceBase
    {
        public string LastFailureMessage { get; set; }
        protected readonly IConfiguration _configuration;
        protected readonly string _credentials;

        public ApiServiceBase(IConfiguration configuration)
        {
            _configuration = configuration;
            _credentials = configuration.PersonalAccessToken;//Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _configuration.PersonalAccessToken)));
        }

        protected HttpRequestMessage GetHttpRequest(string uri)
        {
            return this.GetHttpRequest("GET", uri);
        }

        protected HttpRequestMessage GetHttpRequest(string method, string uri)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(_configuration.UriString);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials);

            var httpMethod = new HttpMethod(method);
            var request = new HttpRequestMessage(httpMethod, uri);
            //_configuration.Project + "/_apis/distributedtask/queues?api-version=" + _configuration.VersionNumber + "-preview.1") { Content = jsonContent };

            return request;
        }
    }
}