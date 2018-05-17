using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TemplatesGeneratorTool.Generators
{
    public class ValidateLogin
    {
        readonly VstsRestAPI.IConfiguration _sourceConfig;
        readonly string _sourceCredentials;

        public ValidateLogin(VstsRestAPI.IConfiguration configuration)
        {
            _sourceConfig = configuration;
            _sourceCredentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _sourceConfig.PersonalAccessToken)));
        }

        /// <summary>
        /// method to validate Account name and PAT
        /// </summary>
        /// <returns></returns>

        public bool isValidAccount()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_sourceConfig.UriString);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _sourceCredentials);

                    HttpResponseMessage ResponseDef = client.GetAsync(_sourceConfig.UriString + string.Format("/DefaultCollection/_apis/projects?stateFilter=WellFormed&api-version=1.0")).Result;
                    if (ResponseDef.IsSuccessStatusCode && ResponseDef.StatusCode != System.Net.HttpStatusCode.NonAuthoritativeInformation)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
