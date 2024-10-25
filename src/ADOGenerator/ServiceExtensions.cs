using ADOGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOGenerator
{
    public static class ServiceExtensions
    {
        public static string ReadJsonFile(this Project file, string filePath)
        {
            string fileContents = string.Empty;

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    fileContents = sr.ReadToEnd();
                }
            }

            return fileContents;
        }

        public static string ErrorId(this string str)
        {
            str = str + "_Errors";
            return str;
        }
    }
}
