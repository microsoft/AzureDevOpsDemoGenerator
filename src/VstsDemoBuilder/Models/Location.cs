

using System;
using System.Net;
using System.Web;
using System.Xml;

namespace VstsDemoBuilder.Models
{
    public class Location
    {
        public class IPHostGenerator
        {
            internal string GetCurrentPageUrl()
            {
                return System.Web.HttpContext.Current.Request.Url.AbsoluteUri;
            }
            public string GetVisitorDetails()
            {
                string varIpAddress = string.Empty;
                try
                {
                    string varIPAddress = string.Empty;
                    string varVisitorCountry = string.Empty;
                    varIpAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(varIpAddress))
                    {
                        if (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
                        {
                            varIpAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                        }
                    }

                    if (varIPAddress == "" || varIPAddress == null)
                    {
                        if (HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"] != null)
                        {
                            varIpAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                        }
                    }
                }
                catch(Exception)
                {
                    return null;
                }
                return varIpAddress;
            }

            public string GetLocation(string varIPAddress)
            {
                WebRequest varWebRequest = WebRequest.Create("http://freegeoip.net/xml/");
                try
                {

                    XmlDocument doc = new XmlDocument();

                    string getdetails = "http://www.freegeoip.net/xml/" + varIPAddress;

                    doc.Load(getdetails);

                    XmlNodeList nodeLstCity = doc.GetElementsByTagName("CountryName");

                    string location = nodeLstCity[0].InnerText;
                    return location;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}