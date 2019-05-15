using System.Collections.Generic;

namespace VstsRestAPI.Viewmodel.Extractor
{
    public class GetServiceEndpoints
    {
        public class Data
        {
            public string environment { get; set; }
            public string subscriptionId { get; set; }
            public string subscriptionName { get; set; }
            public string scopeLevel { get; set; }
            public string serviceBusQueueName { get; set; }
            public string registrytype { get; set; }
            public string accessExternalGitServer { get; set; }
            public string host { get; set; }
            public string port { get; set; }
            public string privateKey { get; set; }
            public string realmName { get; set; }
            public string acceptUntrustedCerts { get; set; }
            public string authorizationType { get; set; }
        }

        public class Parameters
        {
            public string username { get; set; }
            public string certificate { get; set; }
            public string apitoken { get; set; }            
            public string password { get; set; }
            public string email { get; set; }
            public string registry { get; set; }
            public string url { get; set; }
            public string tenantId { get; set; }
            public string servicePrincipalId { get; set; }
            public string authenticationType { get; set; }
            public string serviceBusConnectionString { get; set; }
            public string servicePrincipalKey { get; set; }
            public string nugetkey { get; set; }
            public string cacert { get; set; }
            public string cert { get; set; }
            public string key { get; set; }
            public string AccessToken { get; set; }
            public string azureTenantId { get; set; }
            public string azureEnvironment { get; set; }
            public string roleBindingName { get; set; }
            public string secretName { get; set; }
            public string serviceAccountName { get; set; }
            public string azureAccessToken { get; set; }
            public string serviceAccountCertificate { get; set; }

        }

        public class Authorization
        {
            public Parameters parameters { get; set; }
            public string scheme { get; set; }
        }

        public class Value
        {
            public Data data { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public string url { get; set; }
            public Authorization authorization { get; set; }
            public bool isShared { get; set; }
            public bool isReady { get; set; }
            public string owner { get; set; }
        }

        public class ServiceEndPoint
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }
    }
}
