using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Hosting;
using VstsDemoBuilder.Extensions;
using VstsDemoBuilder.Models;
using VstsDemoBuilder.ServiceInterfaces;
using static VstsDemoBuilder.Models.TemplateSelection;

namespace VstsDemoBuilder.Services
{
    public class TemplateService : ITemplateService
    {

        public List<TemplateDetails> GetAllTemplates()
        {
            var templates = new TemplateSelection.Templates();
            var TemplateDetails = new List<TemplateDetails>();
            try
            {
                Project model = new Project();
                string[] dirTemplates = Directory.GetDirectories(System.Web.Hosting.HostingEnvironment.MapPath("~") + @"\Templates");
                List<string> TemplateNames = new List<string>();
                //Taking all the template folder and adding to list
                foreach (string template in dirTemplates)
                {
                    TemplateNames.Add(Path.GetFileName(template));
                    // Reading Template setting file to check for private templates                   
                }

                if (System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~") + @"\Templates\TemplateSetting.json"))
                {
                    string templateSetting = model.ReadJsonFile(System.Web.Hosting.HostingEnvironment.MapPath("~") + @"\Templates\TemplateSetting.json");
                    templates = JsonConvert.DeserializeObject<TemplateSelection.Templates>(templateSetting);

                    foreach (var templateList in templates.GroupwiseTemplates)
                    {
                        foreach (var template in templateList.Template)
                        {
                            TemplateDetails tmp = new TemplateDetails();

                            tmp.Name = template.Name;
                            tmp.ShortName = template.ShortName;
                            tmp.Tags = template.Tags;
                            tmp.Description = template.Description;
                            //tmp.TemplateFolder = template.TemplateFolder;
                            TemplateDetails.Add(tmp);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t BulkProject \t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return TemplateDetails;
        }

        public List<TemplateDetails> GetTemplatesByTags(string Tags)
        {
            var templates = new TemplateSelection.Templates();
            var Selectedtemplates = new List<TemplateDetails>();
            char delimiter = ',';
            string[] strComponents = Tags.Split(delimiter);
            try
            {
                Project model = new Project();
                string[] dirTemplates = Directory.GetDirectories(System.Web.Hosting.HostingEnvironment.MapPath("~") + @"\Templates");
                List<string> TemplateNames = new List<string>();
                //Taking all the template folder and adding to list
                foreach (string template in dirTemplates)
                {
                    TemplateNames.Add(Path.GetFileName(template));
                    // Reading Template setting file to check for private templates                   
                }

                if (System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~") + @"\Templates\TemplateSetting.json"))
                {
                    string templateSetting = model.ReadJsonFile(System.Web.Hosting.HostingEnvironment.MapPath("~") + @"\Templates\TemplateSetting.json");
                    templates = JsonConvert.DeserializeObject<TemplateSelection.Templates>(templateSetting);

                    foreach (var groupwiseTemplates in templates.GroupwiseTemplates)
                    {
                        foreach (var tmp in groupwiseTemplates.Template)
                        {
                            if (tmp.Tags != null)
                            {
                                foreach (string str in strComponents)
                                {
                                    if (tmp.Tags.Contains(str))
                                    {
                                        TemplateDetails template = new TemplateDetails();

                                        template.Name = tmp.Name;
                                        template.ShortName = tmp.ShortName;
                                        template.Tags = tmp.Tags;
                                        template.Description = tmp.Description;
                                        //template.TemplateFolder = tmp.TemplateFolder;
                                        Selectedtemplates.Add(template);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t BulkProject \t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return Selectedtemplates;
        }

        public string GetTemplate(string TemplateName)
        {
            string templatesPath = HostingEnvironment.MapPath("~") + @"\Templates\";
            string template = string.Empty;

            if (System.IO.File.Exists(templatesPath + Path.GetFileName(TemplateName) + @"\ProjectTemplate.json"))
            {
                Project objP = new Project();
                template = objP.ReadJsonFile(templatesPath + Path.GetFileName(TemplateName) + @"\ProjectTemplate.json");
                return template;
            }
            else
            {
                return "Template Not Found!";
            }
        }

        /// <summary>
        /// Get extracted template path from the given templatepath(url) in request body
        /// </summary>
        /// <param name="TemplateUrl"></param>
        /// <param name="ExtractedTemplate"></param>
        public bool GetTemplateFromPath(string TemplateUrl, string ExtractedTemplate, string GithubToken, string UserID = "", string Password = "")
        {
            bool isvalidFile = false;
            try
            {
                Uri uri = new Uri(TemplateUrl);
                string fileName = Path.GetFileName(TemplateUrl);
                string extension = Path.GetExtension(fileName);

                string templateName = ExtractedTemplate.ToLower().Replace(".zip", "").Trim();
                var path = HostingEnvironment.MapPath("~") + @"\ExtractedZipFile\" + ExtractedTemplate;

                //Downloading template from source of type github
                if (uri.Host == "github.com")
                {
                    var githubToken = GithubToken;
                    var url = TemplateUrl.Replace("github.com/", "raw.githubusercontent.com/").Replace("/blob/master/", "/master/");

                    using (var client = new System.Net.Http.HttpClient())
                    {
                        var credentials = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}:", githubToken);
                        credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(credentials));
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                        var contents = client.GetByteArrayAsync(url).Result;
                        System.IO.File.WriteAllBytes(path, contents);
                    }
                }
                //Downloading file from other source type (ftp or https)
                else
                {
                    WebClient webClient = new WebClient();
                    if (UserID != null && Password != null)
                        webClient.Credentials = new NetworkCredential(UserID, Password);
                    webClient.DownloadFile(TemplateUrl, path);
                    webClient.Dispose();
                }

                isvalidFile = ExtractZipFile(path, templateName);
            }
            catch (Exception ex)
            {
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            finally
            {
                var zippath = HostingEnvironment.MapPath("~") + @"\ExtractedZipFile\" + ExtractedTemplate;
                if (File.Exists(zippath))
                    File.Delete(zippath);
            }
            return isvalidFile;
        }

        public bool ExtractZipFile(string path, string templateName)
        {
            bool isExtracted = false;
            try
            {
                if (File.Exists(path))
                {
                    var Extractedpath = HostingEnvironment.MapPath("~") + @"\PrivateTemplates\" + templateName;
                    System.IO.Compression.ZipFile.ExtractToDirectory(path, Extractedpath);

                    isExtracted = checkTemplateDirectory(Extractedpath);
                    if (isExtracted)
                        ProjectService.PrivateTemplatePath = FindPrivateTemplatePath(Extractedpath);
                }
            }
            catch (Exception ex)
            {
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return isExtracted;

        }

        /// <summary>
        /// Check the valid files from extracted files from zip file in PrivateTemplate Folder 
        /// </summary>
        /// <param name="dir"></param>
        public bool checkTemplateDirectory(string dir)
        {
            string[] filepaths = Directory.GetFiles(dir);
            foreach (var file in filepaths)
            {
                if (Path.GetExtension(Path.GetFileName(file)) != ".json")
                {
                    return false;
                }
            }
            string[] subdirectoryEntries = Directory.GetDirectories(dir);
            foreach (string subdirectory in subdirectoryEntries)
            {
                checkTemplateDirectory(subdirectory);
            }
            return true;
        }

        /// <summary>
        /// Get the private template path from the private template folder 
        /// </summary>
        /// <param name="privateTemplatePath"></param>
        public string FindPrivateTemplatePath(string privateTemplatePath)
        {
            string templatePath = "";
            DirectoryInfo di = new DirectoryInfo(privateTemplatePath);
            FileInfo[] TXTFiles = di.GetFiles("*.json");
            if (TXTFiles.Length > 0)
            {
                templatePath = privateTemplatePath;
            }
            else
            {
                string[] subdirs = Directory.GetDirectories(privateTemplatePath);
                templatePath = FindPrivateTemplatePath(subdirs[0] + @"\");
            }
            return templatePath;
        }

    }
}