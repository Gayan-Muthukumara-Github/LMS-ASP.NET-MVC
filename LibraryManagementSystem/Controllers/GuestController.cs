using LibraryManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace LibraryManagementSystem.Controllers
{
    public class GuestController : Controller
    {
        string cs = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;
       
        public ActionResult Index()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                string q = SqlQueryHelper.GetQuery("GetAllGuests");
                SqlDataAdapter da = new SqlDataAdapter(q, con);
                da.Fill(dt);
                con.Close();
            }

            return View(dt);
        }

        public ActionResult Details(int id)
        {
            return View();
        }

        public ActionResult Create()
        {
            return View(new Guest());
        }

        [HttpPost]
        public ActionResult Create(Guest guest)
        {
            try
            {
                if (!ValidateCreateGuest(guest))
                {
                    return View(guest);
                }
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    string q = SqlQueryHelper.GetQuery("InsertGuest");
                    SqlCommand cmd = new SqlCommand(q, con);
                    cmd.Parameters.AddWithValue("@ID_NUMBER", guest.IdNumber);
                    cmd.Parameters.AddWithValue("@FIRST_NAME", guest.FirstName);
                    cmd.Parameters.AddWithValue("@LAST_NAME", guest.LastName);
                    cmd.Parameters.AddWithValue("@EMAIL", guest.Email);
                    cmd.Parameters.AddWithValue("@ADDRESS", guest.Address);
                    cmd.Parameters.AddWithValue("@TELEPHONE", guest.Telephone);
                    cmd.Parameters.AddWithValue("@REGISTERED_DATE", guest.RegisteredDate);

                    if (guest.TerminatedDate.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@TERMINATED_DATE", guest.TerminatedDate.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@TERMINATED_DATE", DBNull.Value);
                    }

                    cmd.ExecuteNonQuery();
                    con.Close();
                    TempData["SuccessMessage"] = "Guest Details Added Successfully!";
                }

                return RedirectToAction("Index");
            }
            catch
            {
                return View(guest);
            }
        }

        private bool ValidateCreateGuest(Guest guest)
        {
            if (IsDuplicate(guest.IdNumber, guest.Email))
            {
                TempData["DuplicateError"] = "ID Number or Email already exists.";
                return false;
            }

            if (!IsValidEmail(guest.Email))
            {
                TempData["DuplicateError"] = "Invalid email format.";
                return false;
            }

            if (!IsValidTelephone(guest.Telephone))
            {
                TempData["DuplicateError"] = "Invalid telephone number format.";
                return false;
            }

            return true;
        }

        private bool ValidateEditGuest(Guest guest)
        {
            if (!IsValidEmail(guest.Email))
            {
                TempData["DuplicateError"] = "Invalid email format.";
                return false;
            }

            if (!IsValidTelephone(guest.Telephone))
            {
                TempData["DuplicateError"] = "Invalid telephone number format.";
                return false;
            }

            if (!IsValidTerminatedDate(guest.RegisteredDate,guest.TerminatedDate))
            {
                TempData["DuplicateError"] = "Terminated Date must be greater than Registered Date.!";
                return false;
            }

            return true;
        }

        private bool IsDuplicate(string idNumber, string email)
        {
            bool isDuplicate = false;

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();

                string queryId = SqlQueryHelper.GetQuery("CheckDuplicateId");
                SqlCommand cmdId = new SqlCommand(queryId, con);
                cmdId.Parameters.AddWithValue("@ID_NUMBER", idNumber);
                int countId = (int)cmdId.ExecuteScalar();

                string queryEmail = SqlQueryHelper.GetQuery("CheckDuplicateEmail");
                SqlCommand cmdEmail = new SqlCommand(queryEmail, con);
                cmdEmail.Parameters.AddWithValue("@EMAIL", email);
                int countEmail = (int)cmdEmail.ExecuteScalar();

                if (countId > 0 || countEmail > 0)
                {
                    isDuplicate = true;
                }

                con.Close();
            }

            return isDuplicate;
        }

        private bool IsValidEmail(string email)
        {
           
            string emailRegex = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";

            return Regex.IsMatch(email, emailRegex);
        }

        private bool IsValidTerminatedDate(DateTime registeredDate, DateTime? terminatedDate)
        {
            if (terminatedDate.HasValue && registeredDate < terminatedDate)
            {
                return true;
            }
            return false; 
        }


        private bool IsValidTelephone(string telephone)
        {
            string telephoneRegex = @"^\d{3}\d{3}\d{4}$";

            return Regex.IsMatch(telephone, telephoneRegex);
        }

        // GET: Guest/Edit/5
        public ActionResult Edit(int id)
        {
            Guest guest = new Guest();
            DataTable datatable = new DataTable();

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                string q = SqlQueryHelper.GetQuery("GetGuestById");
                SqlDataAdapter da = new SqlDataAdapter(q, con);
                da.SelectCommand.Parameters.AddWithValue("@id", id);
                da.Fill(datatable);
                con.Close();
            }
            if (datatable.Rows.Count == 1)
            {
                guest.GuestId = Convert.ToInt32(datatable.Rows[0][0].ToString());
                guest.IdNumber = datatable.Rows[0][1].ToString();
                guest.FirstName = datatable.Rows[0][2].ToString();
                guest.LastName = datatable.Rows[0][3].ToString();
                guest.Email = datatable.Rows[0][4].ToString();
                guest.Address = datatable.Rows[0][5].ToString();
                guest.Telephone = datatable.Rows[0][6].ToString();
                guest.RegisteredDate = Convert.ToDateTime(datatable.Rows[0][7].ToString());
                guest.TerminatedDate = datatable.Rows[0][8] != DBNull.Value ? (DateTime?)Convert.ToDateTime(datatable.Rows[0][8].ToString()) : null;

            return View(guest);
            }
            
            return RedirectToAction("Index");
        }

        // POST: Guest/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, Guest guest)
        {
            try
            {
                if (!ValidateEditGuest(guest))
                {
                    return View(guest);
                }

                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    string q = SqlQueryHelper.GetQuery("UpdateGuest");
                    SqlCommand cmd = new SqlCommand(q, con);
                    cmd.Parameters.AddWithValue("@ID_NUMBER", guest.IdNumber);
                    cmd.Parameters.AddWithValue("@FIRST_NAME", guest.FirstName);
                    cmd.Parameters.AddWithValue("@LAST_NAME", guest.LastName);
                    cmd.Parameters.AddWithValue("@EMAIL", guest.Email);
                    cmd.Parameters.AddWithValue("@ADDRESS", guest.Address);
                    cmd.Parameters.AddWithValue("@TELEPHONE", guest.Telephone);
                    cmd.Parameters.AddWithValue("@REGISTERED_DATE", guest.RegisteredDate);
                    if (guest.TerminatedDate.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@TERMINATED_DATE", guest.TerminatedDate.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@TERMINATED_DATE", DBNull.Value);
                    }

                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                TempData["SuccessMessage"] = "Guest Details Updated Successfully!";
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Guest/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    string q = SqlQueryHelper.GetQuery("DeleteGuest");
                    SqlCommand cmd = new SqlCommand(q, con);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                TempData["SuccessMessage"] = "Guest Details Deleted Successfully!";
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // Foreign key violation
                {
                    TempData["ErrorMessage"] = "Cannot delete this guest because there are associated issued books.";
                }
                else
                {
                    TempData["ErrorMessage"] = "An error occurred while deleting the guest.";
                }
            }

            return RedirectToAction("Index");
        }


    }
}
