using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;

namespace VstsDemoBuilder.Models
{
	public class Email
	{
		public string EmailAddress { get; set; }
		public string AccountName { get; set; }
		public string ErrorLog { get; set; }

		public bool SendEmail(string toEmail, string body, string subject)
		{
            MailMessage newmsg = new MailMessage(ConfigurationManager.AppSettings["from"], toEmail)
            {
                //newmsg.From = new MailAddress(ConfigurationManager.AppSettings["from"]);
                IsBodyHtml = true,
                Subject = subject,

                //newmsg.To.Add(toEmail);
                Body = body
            };
            SmtpClient smtp = new SmtpClient
            {

                //smtp.Host = Convert.ToString(ConfigurationManager.AppSettings["mailhost"]);
                Host = "smtp.gmail.com",
                Port = 587,
                //smtp.Port = Convert.ToInt16(ConfigurationManager.AppSettings["port"]);
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential
          (Convert.ToString(ConfigurationManager.AppSettings["username"]), Convert.ToString(ConfigurationManager.AppSettings["password"])),

                EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["enableSSL"])
            };
            try
			{
				ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
				{ return true; };
				smtp.Send(newmsg);
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}
	}
}