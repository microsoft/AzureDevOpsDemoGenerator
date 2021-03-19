using System.Collections.Generic;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class GetServiceEndpoints
    {
        public class Data
        {
            public string Environment { get; set; }
            public string SubscriptionId { get; set; }
            public string SubscriptionName { get; set; }
            public string ScopeLevel { get; set; }
            public string ServiceBusQueueName { get; set; }
            public string Registrytype { get; set; }
            public string AccessExternalGitServer { get; set; }
            public string Host { get; set; }
            public string Port { get; set; }
            public string PrivateKey { get; set; }
            public string RealmName { get; set; }
            public string AcceptUntrustedCerts { get; set; }
            public string AuthorizationType { get; set; }
            public string managementGroupId { get; set; }
            public string managementGroupName { get; set; }
        }

        public class Parameters
        {
            public string Username { get; set; }
            public string Certificate { get; set; }
            public string Apitoken { get; set; }            
            public string Password { get; set; }
            public string Email { get; set; }
            public string Registry { get; set; }
            public string Url { get; set; }
            public string TenantId { get; set; }
            public string ServicePrincipalId { get; set; }
            public string AuthenticationType { get; set; }
            public string ServiceBusConnectionString { get; set; }
            public string ServicePrincipalKey { get; set; }
            public string Nugetkey { get; set; }
            public string Cacert { get; set; }
            public string Cert { get; set; }
            public string Key { get; set; }
            public string AccessToken { get; set; }
            public string AzureTenantId { get; set; }
            public string AzureEnvironment { get; set; }
            public string RoleBindingName { get; set; }
            public string SecretName { get; set; }
            public string ServiceAccountName { get; set; }
            public string AzureAccessToken { get; set; }
            public string ServiceAccountCertificate { get; set; }

        }

        public class Authorization
        {
            public Parameters Parameters { get; set; }
            public string Scheme { get; set; }
        }

        public class Value
        {
            public Data Data { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Url { get; set; }
            public Authorization Authorization { get; set; }
            public bool IsShared { get; set; }
            public bool IsReady { get; set; }
            public string Owner { get; set; }
        }

        public class ServiceEndPoint
        {
            public int Count { get; set; }
            public IList<Value> Value { get; set; }
        }
    }
}
