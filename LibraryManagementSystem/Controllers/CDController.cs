using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Controllers
{
    public class CDController : Controller
    {
        string cs = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;

        public ActionResult Index()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                string q = SqlQueryHelper.GetQuery("SelectAllCDs");
                SqlDataAdapter da = new SqlDataAdapter(q, con);
                da.Fill(dt);
            }
            return View(dt);
        }


        public ActionResult Details(int id)
        {
            return View();
        }


        public ActionResult Create()
        {
            return View(new CD());
        }


        [HttpPost]
        public ActionResult Create(CD cd)
        {
            try
            {
                if (IsDuplicate(cd.CdTitle))
                {
                    TempData["DuplicateError"] = "CD Title already exists.";
                    return View(cd);
                }
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    string q = SqlQueryHelper.GetQuery("InsertCD");
                    SqlCommand cmd = new SqlCommand(q, con);
                    cmd.Parameters.AddWithValue("@CD_TITLE", cd.CdTitle);
                    cmd.Parameters.AddWithValue("@AUTHOR", cd.Author);
                    cmd.Parameters.AddWithValue("@PUBLICATION", cd.Publication);
                    cmd.Parameters.AddWithValue("@PRINTED_YEAR", cd.PrintedYear);
                    cmd.Parameters.AddWithValue("@CATEGORY", cd.Category);
                    cmd.ExecuteNonQuery();
                }
                TempData["SuccessMessage"] = "CD Details Added Successfully!";
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        private bool IsDuplicate(string CdTitle)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                string q = SqlQueryHelper.GetQuery("CheckDuplicateCD");
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@CD_TITLE", CdTitle);
                int count = (int)cmd.ExecuteScalar();
                con.Close();
                return count > 0;
            }
        }


        public ActionResult Edit(int id)
        {
            CD cd = new CD();
            DataTable datatable = new DataTable();

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                string q = SqlQueryHelper.GetQuery("SelectCDById");
                SqlDataAdapter da = new SqlDataAdapter(q, con);
                da.SelectCommand.Parameters.AddWithValue("@id", id);
                da.Fill(datatable);
            }
            if (datatable.Rows.Count == 1)
            {
                cd.CdId = Convert.ToInt32(datatable.Rows[0][0].ToString());
                cd.CdTitle = datatable.Rows[0][1].ToString();
                cd.Author = datatable.Rows[0][2].ToString();
                cd.Publication = datatable.Rows[0][3].ToString();
                cd.PrintedYear = datatable.Rows[0][4] != DBNull.Value ? Convert.ToInt32(datatable.Rows[0][4].ToString()) : 1111;
                cd.Category = datatable.Rows[0][5].ToString();

                return View(cd);
            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult Edit(int id, CD cd)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    string q = SqlQueryHelper.GetQuery("UpdateCD");
                    SqlCommand cmd = new SqlCommand(q, con);
                    cmd.Parameters.AddWithValue("@CD_TITLE", cd.CdTitle);
                    cmd.Parameters.AddWithValue("@AUTHOR", cd.Author);
                    cmd.Parameters.AddWithValue("@PUBLICATION", cd.Publication);
                    cmd.Parameters.AddWithValue("@PRINTED_YEAR", cd.PrintedYear.HasValue ? (object)cd.PrintedYear.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@CATEGORY", (object)cd.Category ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["SuccessMessage"] = "CD Details Updated Successfully!";
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }


        public ActionResult Delete(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    string q = SqlQueryHelper.GetQuery("DeleteCD");
                    SqlCommand cmd = new SqlCommand(q, con);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "CD Details Deleted Successfully!";
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                {
                    TempData["ErrorMessage"] = "Cannot delete this cd because it is currently issued.";
                }
                else
                {
                    TempData["ErrorMessage"] = "An error occurred while deleting the cd.";
                }
            }

            return RedirectToAction("Index");
        }


        public ActionResult Search()
        {
            return View();
        }

        [HttpGet]
        public ActionResult SearchResults(string BookReferenceNumber, string BookTitle, string Author)
        {
            List<BookViewModel> books = new List<BookViewModel>();

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                string query = "SearchBooks";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@BookReferenceNumber", string.IsNullOrEmpty(BookReferenceNumber) ? (object)DBNull.Value : BookReferenceNumber);
                cmd.Parameters.AddWithValue("@BookTitle", string.IsNullOrEmpty(BookTitle) ? (object)DBNull.Value : BookTitle);
                cmd.Parameters.AddWithValue("@Author", string.IsNullOrEmpty(Author) ? (object)DBNull.Value : Author);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    books.Add(new BookViewModel
                    {
                        BookReferenceNumber = reader["BOOK_REFERENCE_NUMBER"].ToString(),
                        Title = reader["TITLE"].ToString(),
                        Publication = reader["PUBLICATION"].ToString(),
                        Author = reader["AUTHOR"].ToString(),
                        StudentName = reader["StudentName"] != DBNull.Value ? reader["StudentName"].ToString() : null,
                        IssueDate = reader["ISSUE_DATE"] != DBNull.Value ? (DateTime?)reader["ISSUE_DATE"] : null,
                        ReturnDate = reader["RETURN_DATE"] != DBNull.Value ? (DateTime?)reader["RETURN_DATE"] : null
                    });
                }
            }

            return View("Search", books);
        }
    }
}