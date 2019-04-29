using Microsoft.VisualStudio.Services.ExtensionManagement.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using VstsDemoBuilder.Extensions;
using VstsDemoBuilder.Models;
using VstsDemoBuilder.ServiceInterfaces;
using VstsRestAPI;

namespace VstsDemoBuilder.Services
{
    public class ProjectService : IProjectService
    {
        public string ReadFromConfiguration(string key)
        {
            return System.Configuration.ConfigurationManager.AppSettings[key].ToString();
        }

        public Configuration NewConfiguration(string Pat, string accountName, string projectName, string host, string version)
        {
            Configuration obj = new Configuration();
            obj.UriString = host + accountName + "/";
            obj.VersionNumber = version;
            obj.PersonalAccessToken = Pat;
            obj.Project = projectName;
            obj.AccountName = accountName;
            return obj;
        }

        public static void AddMessage(string id, string message, Dictionary<string, string> StatusMessages, object objLock)
        {
            lock (objLock)
            {
                if (id.EndsWith("_Errors"))
                {
                    StatusMessages[id] = (StatusMessages.ContainsKey(id) ? StatusMessages[id] : string.Empty) + message;
                }
                else
                {
                    StatusMessages[id] = message;
                }
            }
        }

        /// <summary>
        /// Installing Extensions
        /// </summary>
        /// <param name="model"></param>
        /// <param name="accountName"></param>
        /// <param name="PAT"></param>
        /// <returns></returns>
        public bool InstallExtensions(Project model, string accountName, string PAT)
        {
            try
            {
                string templatesFolder = HostingEnvironment.MapPath("~") + @"\Templates\";
                string projTemplateFile = string.Format(templatesFolder + @"{0}\Extensions.json", model.SelectedTemplate);
                if (!(System.IO.File.Exists(projTemplateFile)))
                {
                    return false;
                }
                string templateItems = System.IO.File.ReadAllText(projTemplateFile);
                var template = JsonConvert.DeserializeObject<RequiredExtensions.Extension>(templateItems);
                string requiresExtensionNames = string.Empty;

                //Check for existing extensions
                if (template.Extensions.Count > 0)
                {
                    Dictionary<string, bool> dict = new Dictionary<string, bool>();
                    foreach (RequiredExtensions.ExtensionWithLink ext in template.Extensions)
                    {
                        dict.Add(ext.extensionName, false);
                    }
                    var connection = new VssConnection(new Uri(string.Format("https://{0}.visualstudio.com", accountName)), new Microsoft.VisualStudio.Services.OAuth.VssOAuthAccessTokenCredential(PAT));// VssOAuthCredential(PAT));
                    var client = connection.GetClient<ExtensionManagementHttpClient>();
                    var installed = client.GetInstalledExtensionsAsync().Result;
                    var extensions = installed.Where(x => x.Flags == 0).ToList();

                    var trustedFlagExtensions = installed.Where(x => x.Flags == ExtensionFlags.Trusted).ToList();
                    var builtInExtensions = installed.Where(x => x.Flags.ToString() == "BuiltIn, Trusted").ToList();
                    extensions.AddRange(trustedFlagExtensions);
                    extensions.AddRange(builtInExtensions);

                    foreach (var ext in extensions)
                    {
                        foreach (var extension in template.Extensions)
                        {
                            if (extension.extensionName.ToLower() == ext.ExtensionDisplayName.ToLower())
                            {
                                dict[extension.extensionName] = true;
                            }
                        }
                    }
                    var required = dict.Where(x => x.Value == false).ToList();

                    if (required.Count > 0)
                    {
                        Parallel.ForEach(required, async req =>
                        {
                            string publisherName = template.Extensions.Where(x => x.extensionName == req.Key).FirstOrDefault().publisherId;
                            string extensionName = template.Extensions.Where(x => x.extensionName == req.Key).FirstOrDefault().extensionId;
                            try
                            {
                                InstalledExtension extension = null;
                                extension = await client.InstallExtensionByNameAsync(publisherName, extensionName);
                            }
                            catch (OperationCanceledException cancelException)
                            {
                                AddMessage(model.id.ErrorId(), "Error while Installing extensions - operation cancelled: " + cancelException.Message + Environment.NewLine);
                            }
                            catch (Exception exc)
                            {
                                AddMessage(model.id.ErrorId(), "Error while Installing extensions: " + exc.Message);
                            }
                        });
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                //logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.id.ErrorId(), "Error while Installing extensions: " + ex.Message);
                return false;
            }
        }

        private void AddMessage(string v1, string v2)
        {
            throw new NotImplementedException();
        }
    }
}