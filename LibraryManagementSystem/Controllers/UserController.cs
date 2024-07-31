using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Controllers
{
    public class UserController : Controller
    {
        string cs = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(User usr)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                if (string.IsNullOrEmpty(usr.Password))
                {
                    TempData["ErrorMessage"] = "Password cannot be empty!";
                    return View();
                }
                else if (string.IsNullOrEmpty(usr.UserID))
                {
                    TempData["ErrorMessage"] = "User Name cannot be empty!";
                    return View();
                }
                else
                {
                    string queryId = SqlQueryHelper.GetQuery("CheckLoginDetails");
                    SqlCommand cmdId = new SqlCommand(queryId, con);
                    cmdId.Parameters.AddWithValue("@USER_ID", usr.UserID);
                    cmdId.Parameters.AddWithValue("@PASSWORD", usr.Password);
                    object result = cmdId.ExecuteScalar();

                    if (result != null)
                    {
                        Session["UserID"] = usr.UserID;
                        TempData["SuccessMessage"] = "Login Successfully!";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Login Failed!";
                        return View();
                    }
                }

            }
        }

        public ActionResult Logout()
        {
            Session["UserID"] = null;
            return RedirectToAction("Login", "User");
        }
    }
}