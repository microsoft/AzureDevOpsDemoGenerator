using AzureDevOpsAPI.Viewmodel.Extractor;
using AzureDevOpsDemoBuilder.Extensions;
using AzureDevOpsDemoBuilder.Models;
using AzureDevOpsDemoBuilder.ServiceInterfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using static AzureDevOpsDemoBuilder.Models.TemplateSelection;

namespace AzureDevOpsDemoBuilder.Services
{
    public class TemplateService : ITemplateService
    {
        private IWebHostEnvironment HostingEnvironment;
        private ILogger<TemplateService> logger;

        public TemplateService(IWebHostEnvironment _host, ILogger<TemplateService> _logger)
        {
            HostingEnvironment = _host;
            logger = _logger;
        }

        public List<TemplateDetails> GetAllTemplates()
        {
            var templates = new TemplateSelection.Templates();
            var TemplateDetails = new List<TemplateDetails>();
            try
            {
                Project model = new Project();
                string[] dirTemplates = Directory.GetDirectories(HostingEnvironment.WebRootPath + "/Templates");
                List<string> TemplateNames = new List<string>();
                //Taking all the template folder and adding to list
                foreach (string template in dirTemplates)
                {
                    TemplateNames.Add(Path.GetFileName(template));
                    // Reading Template setting file to check for private templates                   
                }

                if (System.IO.File.Exists(HostingEnvironment.WebRootPath + "/Templates/TemplateSetting.json"))
                {
                    string templateSetting = model.ReadJsonFile(HostingEnvironment.WebRootPath + "/Templates/TemplateSetting.json");
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
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t BulkProject \t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return TemplateDetails;
        }

        public List<TemplateDetails> GetTemplatesByTags(string Tags)
        {
            var templates = new TemplateSelection.Templates();
            var Selectedtemplates = new List<TemplateDetails>();
            char delimiter = ',';
            if (!string.IsNullOrEmpty(Tags))
            {
                string[] strComponents = Tags.Split(delimiter);
                try
                {
                    Project model = new Project();
                    string[] dirTemplates = Directory.GetDirectories(HostingEnvironment.WebRootPath + "/Templates");
                    List<string> TemplateNames = new List<string>();
                    //Taking all the template folder and adding to list
                    foreach (string template in dirTemplates)
                    {
                        TemplateNames.Add(Path.GetFileName(template));
                        // Reading Template setting file to check for private templates                   
                    }

                    if (System.IO.File.Exists(HostingEnvironment.WebRootPath + "/Templates/TemplateSetting.json"))
                    {
                        string templateSetting = model.ReadJsonFile(HostingEnvironment.WebRootPath + "/Templates/TemplateSetting.json");
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
                    logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t BulkProject \t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                }
            }
            return Selectedtemplates;
        }

        public string GetTemplate(string TemplateName)
        {
            string template = string.Empty;
            try
            {
                string templatesPath = HostingEnvironment.WebRootPath + "/Templates/";

                if (System.IO.File.Exists(templatesPath + Path.GetFileName(TemplateName) + "/ProjectTemplate.json"))
                {
                    Project objP = new Project();
                    template = objP.ReadJsonFile(templatesPath + Path.GetFileName(TemplateName) + "/ProjectTemplate.json");
                }
                else
                {
                    template = "Template Not Found!";
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return template;
        }

        /// <summary>
        /// Get extracted template path from the given templatepath(url) in request body
        /// </summary>
        /// <param name="TemplateUrl"></param>
        /// <param name="ExtractedTemplate"></param>
        public string GetTemplateFromPath(string TemplateUrl, string ExtractedTemplate, string GithubToken, string UserID = "", string Password = "")
        {
            string templatePath = string.Empty;
            try
            {
                Uri uri = new Uri(TemplateUrl);
                string fileName = Path.GetFileName(TemplateUrl);
                string extension = Path.GetExtension(fileName);
                string templateName = ExtractedTemplate.ToLower().Replace(".zip", "").Trim();
                if (!Directory.Exists(HostingEnvironment.ContentRootPath + "/ExtractedZipFile"))
                {
                    Directory.CreateDirectory(HostingEnvironment.ContentRootPath + "/ExtractedZipFile");
                }
                var path = HostingEnvironment.ContentRootPath + "/ExtractedZipFile/" + ExtractedTemplate;
                if (uri.Host == "github.com")
                {
                    string gUri = uri.ToString();
                    gUri = gUri.Replace("github.com", "raw.githubusercontent.com").Replace("/blob/", "/");
                    TemplateUrl = gUri.ToString();
                }
                //Downloading template from source of type github
                if (uri.Host == "raw.githubusercontent.com")
                {
                    var githubToken = GithubToken;
                    //var url = TemplateUrl.Replace("github.com/", "raw.githubusercontent.com/").Replace("/blob/master/", "/master/");

                    using (var client = new System.Net.Http.HttpClient())
                    {
                        var credentials = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}:", githubToken);
                        credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(credentials));
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                        var contents = client.GetByteArrayAsync(TemplateUrl).Result;
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

                templatePath = ExtractZipFile(path, templateName);

            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            finally
            {
                var zippath = HostingEnvironment.WebRootPath + "/ExtractedZipFile/" + ExtractedTemplate;
                if (File.Exists(zippath))
                    File.Delete(zippath);
            }
            return templatePath;
        }

        public string ExtractZipFile(string path, string templateName)
        {
            string templatePath = string.Empty;
            bool isExtracted = false;
            try
            {
                if (File.Exists(path))
                {
                    if (!Directory.Exists(HostingEnvironment.ContentRootPath + "/PrivateTemplates"))
                    {
                        Directory.CreateDirectory(HostingEnvironment.ContentRootPath + "/PrivateTemplates");
                    }
                    var Extractedpath = HostingEnvironment.WebRootPath + "/PrivateTemplates/" + templateName;
                    System.IO.Compression.ZipFile.ExtractToDirectory(path, Extractedpath);

                    isExtracted = checkTemplateDirectory(Extractedpath);
                    if (isExtracted)
                        templatePath = FindPrivateTemplatePath(Extractedpath);
                    else
                        Directory.Delete(Extractedpath, true);
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return templatePath;

        }

        /// <summary>
        /// Check the valid files from extracted files from zip file in PrivateTemplate Folder 
        /// </summary>
        /// <param name="dir"></param>
        public bool checkTemplateDirectory(string dir)
        {
            try
            {
                if (Directory.Exists(dir))
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
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
            try
            {
                DirectoryInfo di = new DirectoryInfo(privateTemplatePath);
                FileInfo[] TXTFiles = di.GetFiles("*.json");
                if (TXTFiles.Length > 0)
                {
                    templatePath = privateTemplatePath;
                }
                else
                {
                    string[] subdirs = Directory.GetDirectories(privateTemplatePath);
                    templatePath = FindPrivateTemplatePath(subdirs[0] + "/");
                }

            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return templatePath;
        }

        public string checkSelectedTemplateIsPrivate(string extractPath)
        {
            string response = string.Empty;
            try
            {
                bool isExtracted = checkTemplateDirectory(extractPath);
                if (!isExtracted)
                {
                    response = "File or the folder contains unwanted entries, so discarding the files, please try again";
                }
                else
                {
                    bool settingFile = (System.IO.File.Exists(extractPath + "//ProjectSettings.json") ? true : false);
                    bool projectFile = (System.IO.File.Exists(extractPath + "//ProjectTemplate.json") ? true : false);

                    if (settingFile && projectFile)
                    {
                        string projectFileData = System.IO.File.ReadAllText(extractPath + "//ProjectTemplate.json");
                        ProjectSetting settings = JsonConvert.DeserializeObject<ProjectSetting>(projectFileData);
                        response = "SUCCESS";
                    }
                    else if (!settingFile && !projectFile)
                    {
                        string[] folderName = System.IO.Directory.GetDirectories(extractPath);
                        string subDir = "";
                        if (folderName.Length > 0)
                        {
                            subDir = folderName[0];
                        }
                        else
                        {
                            response = "Could not find required preoject setting and project template file.";
                        }
                        if (subDir != "")
                        {
                            response = checkSelectedTemplateIsPrivate(subDir);
                        }
                        if (response != "SUCCESS")
                        {
                            Directory.Delete(extractPath, true);
                            response = "Project setting and project template files not found!. Include the files in zip and try again";
                        }
                    }
                    else
                    {
                        if (!settingFile)
                        {
                            Directory.Delete(extractPath, true);
                            response = "Project setting file not found! plase include the files in zip and try again";
                            //return Json("SETTINGNOTFOUND");
                        }
                        if (!projectFile)
                        {
                            Directory.Delete(extractPath, true);
                            response = "Project template file not found! plase include the files in zip and try again";
                            //return Json("PROJECTFILENOTFOUND");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return response;
        }

        public void deletePrivateTemplate(string Template)
        {
            try
            {
                if (!string.IsNullOrEmpty(Template))
                {
                    var templatepath = HostingEnvironment.ContentRootPath + "/PrivateTemplates/" + Template;
                    if (Directory.Exists(templatepath))
                    {
                        Directory.Delete(templatepath, true);
                    }
                    string[] subdirs = Directory.GetDirectories(HostingEnvironment.ContentRootPath + "/PrivateTemplates/")
                            .Select(Path.GetFileName)
                            .ToArray();
                    foreach (string folderName in subdirs)
                    {
                        DirectoryInfo d = new DirectoryInfo(HostingEnvironment.ContentRootPath + "/PrivateTemplates/" + folderName);
                        if (d.CreationTime < DateTime.Now.AddHours(-1))
                            Directory.Delete(HostingEnvironment.ContentRootPath + "/PrivateTemplates/" + folderName, true);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
        }
    }
}