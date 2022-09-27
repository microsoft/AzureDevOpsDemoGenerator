using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VstsDemoBuilder.Models;
using VstsDemoBuilder.Services;

namespace VstsDemoBuilder.Controllers
{
    [Route("feedback")]

    public class FeedbackController : Controller
    {
        // GET: Feedback
        
        [Route("storefeedback")]
        [HttpPost]
        [AllowAnonymous]

        public bool storeFeedback(Feedback data)
        {
            try
            {
                //int result;
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand("[dbo].[FeedbackDetails_Insert]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Name", data.Name);
                        cmd.Parameters.AddWithValue("@Email", data.Email);
                        cmd.Parameters.AddWithValue("@noofyears", data.Noofyears == null ? DBNull.Value : (object)Convert.ToString(data.Noofyears));
                        cmd.Parameters.AddWithValue("@know", data.Know == null ? DBNull.Value : (object)Convert.ToString(data.Know));
                        cmd.Parameters.AddWithValue("@purpose", data.Purpose == null ? DBNull.Value : (object)Convert.ToString(data.Purpose));
                        cmd.Parameters.AddWithValue("@used", data.Used == null ? DBNull.Value : (object)Convert.ToString(data.Used));
                        cmd.Parameters.AddWithValue("@usedtemplatenames", data.Usedtemplatenames == null ? DBNull.Value : (object)Convert.ToString(data.Usedtemplatenames));
                        cmd.Parameters.AddWithValue("@kindoftemplates", data.Kindoftemplates == null ? DBNull.Value : (object)Convert.ToString(data.Kindoftemplates));
                        cmd.Parameters.AddWithValue("@otherfeedback", data.Otherfeedback == null ? DBNull.Value : (object)Convert.ToString(data.Otherfeedback));
                        
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();                          
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectService.logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                ViewBag.ErrorMessage = ex.Message;                
            }
            return true;
        }
    }
}