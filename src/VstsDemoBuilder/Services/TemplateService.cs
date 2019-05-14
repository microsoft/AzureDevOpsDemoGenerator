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

namespace VstsDemoBuilder.Services
{
    public class TemplateService :ITemplateService
    {


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
        public bool GetTemplateFromPath(string TemplateUrl, string ExtractedTemplate, string GithubToken)
        {
            bool isvalidFile = false;
            try
            {
                string fileName = Path.GetFileName(TemplateUrl);
                string extension = Path.GetExtension(fileName);

                string templateName = ExtractedTemplate.ToLower().Replace(".zip", "").Trim();
                var path = HostingEnvironment.MapPath("~") + @"\ExtractedZipFile\" + ExtractedTemplate;

                //Uri FilePathUri = new Uri(TemplateUrl);
                //string FilePathWithoutQuery = FilePathUri.GetLeftPart(UriPartial.Path);
                //WebClient webClient = new WebClient();
                //webClient.DownloadFile(TemplateUrl, path);
                //webClient.Dispose();

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
                if (File.Exists(path))
                {
                    var Extractedpath = HostingEnvironment.MapPath("~") + @"\PrivateTemplates\" + templateName;
                    System.IO.Compression.ZipFile.ExtractToDirectory(path, Extractedpath);

                    isvalidFile = checkTemplateDirectory(Extractedpath);
                    if (isvalidFile)
                        ProjectService.PrivateTemplatePath = FindPrivateTemplatePath(Extractedpath);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                var zippath = HostingEnvironment.MapPath("~") + @"\ExtractedZipFile\" + ExtractedTemplate;
                if (File.Exists(zippath))
                    File.Delete(zippath);
            }
            return isvalidFile;
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