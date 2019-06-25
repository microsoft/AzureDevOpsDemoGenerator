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
using VstsRestAPI.Viewmodel.Extractor;
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
            string template = string.Empty;
            try
            {
                string templatesPath = HostingEnvironment.MapPath("~") + @"\Templates\";

                if (System.IO.File.Exists(templatesPath + Path.GetFileName(TemplateName) + @"\ProjectTemplate.json"))
                {
                    Project objP = new Project();
                    template = objP.ReadJsonFile(templatesPath + Path.GetFileName(TemplateName) + @"\ProjectTemplate.json");
                }
                else
                {
                    template = "Template Not Found!";
                }
            }
            catch (Exception ex)
            {
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
                var path = HostingEnvironment.MapPath("~") + @"\ExtractedZipFile\" + ExtractedTemplate;

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
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            finally
            {
                var zippath = HostingEnvironment.MapPath("~") + @"\ExtractedZipFile\" + ExtractedTemplate;
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
                    var Extractedpath = HostingEnvironment.MapPath("~") + @"\PrivateTemplates\" + templateName;
                    System.IO.Compression.ZipFile.ExtractToDirectory(path, Extractedpath);

                    isExtracted = checkTemplateDirectory(Extractedpath);
                    if (isExtracted)
                        templatePath = FindPrivateTemplatePath(Extractedpath);
                }
            }
            catch (Exception ex)
            {
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
            catch (Exception ex)
            {
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
                    templatePath = FindPrivateTemplatePath(subdirs[0] + @"\");
                }

            }
            catch (Exception ex)
            {
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return templatePath;
        }

        public string checkSelectedTemplateIsPrivate(string extractPath)
        {
            string response = string.Empty;
            try
            {

                bool settingFile = (System.IO.File.Exists(extractPath + "\\ProjectSettings.json") ? true : false);
                bool projectFile = (System.IO.File.Exists(extractPath + "\\ProjectTemplate.json") ? true : false);

                if (settingFile && projectFile)
                {
                    string projectFileData = System.IO.File.ReadAllText(extractPath + "\\ProjectTemplate.json");
                    ProjectSetting settings = JsonConvert.DeserializeObject<ProjectSetting>(projectFileData);

                    if (!string.IsNullOrEmpty(settings.IsPrivate))
                    {
                        response = "SUCCESS";
                    }
                    else
                    {
                        Directory.Delete(extractPath, true);
                        response = "\"IsPrivate\" flag is not set to true in project template file, update the flag and try again.";
                    }
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
                        bool settingFile1 = (System.IO.File.Exists(subDir + "\\ProjectSettings.json") ? true : false);
                        bool projectFile1 = (System.IO.File.Exists(subDir + "\\ProjectTemplate.json") ? true : false);
                        if (settingFile1 && projectFile1)
                        {
                            string projectFileData1 = System.IO.File.ReadAllText(subDir + "\\ProjectTemplate.json");
                            ProjectSetting settings1 = JsonConvert.DeserializeObject<ProjectSetting>(projectFileData1);

                            if (!string.IsNullOrEmpty(settings1.IsPrivate))
                            {
                                string sourceDirectory = subDir;
                                string targetDirectory = extractPath;
                                string backupDirectory = System.Web.HttpContext.Current.Server.MapPath("~/TemplateBackUp/");
                                if (!Directory.Exists(backupDirectory))
                                {
                                    Directory.CreateDirectory(backupDirectory);
                                }
                                //Create a tempprary directory
                                string backupDirectoryRandom = backupDirectory + DateTime.Now.ToString("MMMdd_yyyy_HHmmss");

                                if (Directory.Exists(sourceDirectory))
                                {

                                    if (Directory.Exists(targetDirectory))
                                    {

                                        //copy the content of source directory to temp directory
                                        Directory.Move(sourceDirectory, backupDirectoryRandom);

                                        //Delete the target directory
                                        Directory.Delete(targetDirectory);

                                        //Target Directory should not be exist, it will create a new directory
                                        Directory.Move(backupDirectoryRandom, targetDirectory);

                                        DirectoryInfo di = new DirectoryInfo(backupDirectory);

                                        foreach (FileInfo file in di.GetFiles())
                                        {
                                            file.Delete();
                                        }
                                        foreach (DirectoryInfo dir in di.GetDirectories())
                                        {
                                            dir.Delete(true);
                                        }
                                    }
                                }

                                //return Json("SUCCESS");
                                response = "SUCCESS";
                            }
                            else
                            {
                                Directory.Delete(extractPath, true);
                                response = "\"IsPrivate\" flag is not set to true in project template file, update the flag and try again.";
                                //return Json("ISPRIVATEERROR");
                            }
                        }
                    }
                    Directory.Delete(extractPath, true);
                    response = "Project setting and project template files not found! plase include the files in zip and try again";
                    //return Json("PROJECTANDSETTINGNOTFOUND");
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
            catch (Exception ex)
            {
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return response;
        }

        public void deletePrivateTemplate(string Template)
        {
            try
            {
                if (!string.IsNullOrEmpty(Template))
                {
                    var templatepath = HostingEnvironment.MapPath("~") + @"\PrivateTemplates\" + Template;
                    if (Directory.Exists(templatepath))
                    {
                        Directory.Delete(templatepath, true);
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
        }
    }
}