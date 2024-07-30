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
    public class StudentController : Controller
    {
        string cs = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;
       
        public ActionResult Index()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                string q = SqlQueryHelper.GetQuery("GetAllStudents");
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
            return View(new Student());
        }

        [HttpPost]
        public ActionResult Create(Student student)
        {
            try
            {
                if (!ValidateCreateStudent(student))
                {
                    return View(student);
                }
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    string q = SqlQueryHelper.GetQuery("InsertStudent");
                    SqlCommand cmd = new SqlCommand(q, con);
                    cmd.Parameters.AddWithValue("@ID_NUMBER", student.IdNumber);
                    cmd.Parameters.AddWithValue("@FIRST_NAME", student.FirstName);
                    cmd.Parameters.AddWithValue("@LAST_NAME", student.LastName);
                    cmd.Parameters.AddWithValue("@EMAIL", student.Email);
                    cmd.Parameters.AddWithValue("@ADDRESS", student.Address);
                    cmd.Parameters.AddWithValue("@TELEPHONE", student.Telephone);
                    cmd.Parameters.AddWithValue("@REGISTERED_DATE", student.RegisteredDate);

                    if (student.TerminatedDate.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@TERMINATED_DATE", student.TerminatedDate.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@TERMINATED_DATE", DBNull.Value);
                    }

                    cmd.ExecuteNonQuery();
                    con.Close();
                    TempData["SuccessMessage"] = "Student Details Added Successfully!";
                }

                return RedirectToAction("Index");
            }
            catch
            {
                return View(student);
            }
        }

        private bool ValidateCreateStudent(Student student)
        {
            if (IsDuplicate(student.IdNumber, student.Email))
            {
                TempData["DuplicateError"] = "ID Number or Email already exists.";
                return false;
            }

            if (!IsValidEmail(student.Email))
            {
                TempData["DuplicateError"] = "Invalid email format.";
                return false;
            }

            if (!IsValidTelephone(student.Telephone))
            {
                TempData["DuplicateError"] = "Invalid telephone number format.";
                return false;
            }

            return true;
        }

        private bool ValidateEditStudent(Student student)
        {
            if (!IsValidEmail(student.Email))
            {
                TempData["DuplicateError"] = "Invalid email format.";
                return false;
            }

            if (!IsValidTelephone(student.Telephone))
            {
                TempData["DuplicateError"] = "Invalid telephone number format.";
                return false;
            }

            if (!IsValidTerminatedDate(student.RegisteredDate,student.TerminatedDate))
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

        // GET: Student/Edit/5
        public ActionResult Edit(int id)
        {
            Student student = new Student();
            DataTable datatable = new DataTable();

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                string q = SqlQueryHelper.GetQuery("GetStudentById");
                SqlDataAdapter da = new SqlDataAdapter(q, con);
                da.SelectCommand.Parameters.AddWithValue("@id", id);
                da.Fill(datatable);
                con.Close();
            }
            if (datatable.Rows.Count == 1)
            {
                student.StudentId = Convert.ToInt32(datatable.Rows[0][0].ToString());
                student.IdNumber = datatable.Rows[0][1].ToString();
                student.FirstName = datatable.Rows[0][2].ToString();
                student.LastName = datatable.Rows[0][3].ToString();
                student.Email = datatable.Rows[0][4].ToString();
                student.Address = datatable.Rows[0][5].ToString();
                student.Telephone = datatable.Rows[0][6].ToString();
                student.RegisteredDate = Convert.ToDateTime(datatable.Rows[0][7].ToString());
                student.TerminatedDate = datatable.Rows[0][8] != DBNull.Value ? (DateTime?)Convert.ToDateTime(datatable.Rows[0][8].ToString()) : null;

            return View(student);
            }
            
            return RedirectToAction("Index");
        }

        // POST: Student/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, Student student)
        {
            try
            {
                if (!ValidateEditStudent(student))
                {
                    return View(student);
                }

                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    string q = SqlQueryHelper.GetQuery("UpdateStudent");
                    SqlCommand cmd = new SqlCommand(q, con);
                    cmd.Parameters.AddWithValue("@ID_NUMBER", student.IdNumber);
                    cmd.Parameters.AddWithValue("@FIRST_NAME", student.FirstName);
                    cmd.Parameters.AddWithValue("@LAST_NAME", student.LastName);
                    cmd.Parameters.AddWithValue("@EMAIL", student.Email);
                    cmd.Parameters.AddWithValue("@ADDRESS", student.Address);
                    cmd.Parameters.AddWithValue("@TELEPHONE", student.Telephone);
                    cmd.Parameters.AddWithValue("@REGISTERED_DATE", student.RegisteredDate);
                    if (student.TerminatedDate.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@TERMINATED_DATE", student.TerminatedDate.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@TERMINATED_DATE", DBNull.Value);
                    }

                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                TempData["SuccessMessage"] = "Student Details Updated Successfully!";
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Student/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    string q = SqlQueryHelper.GetQuery("DeleteStudent");
                    SqlCommand cmd = new SqlCommand(q, con);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                TempData["SuccessMessage"] = "Student Details Deleted Successfully!";
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // Foreign key violation
                {
                    TempData["ErrorMessage"] = "Cannot delete this student because there are associated issued books.";
                }
                else
                {
                    TempData["ErrorMessage"] = "An error occurred while deleting the student.";
                }
            }

            return RedirectToAction("Index");
        }


    }
}
