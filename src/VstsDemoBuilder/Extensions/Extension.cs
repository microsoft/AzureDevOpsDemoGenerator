using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using VstsDemoBuilder.Models;
using System.Net;

namespace VstsDemoBuilder.Extensions
{
    public static class Extension
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

        public static bool IsPrivate(this IPAddress ipAddress)
        {
            byte[] bytes = ipAddress.GetAddressBytes();
            switch (bytes[0])
            {
                case 10:
                    return true; // 10.0.0.0/8
                case 172:
                    return bytes[1] >= 16 && bytes[1] <= 31; // 172.16.0.0/12
                case 192:
                    return bytes[1] == 168; // 192.168.0.0/16
                default:
                    return false;
            }
        }
    }
}