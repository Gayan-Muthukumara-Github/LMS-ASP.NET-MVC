using LibraryManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LibraryManagementSystem.Controllers
{
    public class BookIssueController : Controller
    {
        string cs = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;
     
        
        public ActionResult Index()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                string q = SqlQueryHelper.GetQuery("SelectAllIssueBooks");
                SqlDataAdapter da = new SqlDataAdapter(q, con);
                da.Fill(dt);
            }
            return View(dt);
        }

       
        public ActionResult Create()
        {
            ViewBag.StudentList = GetStudentList();
            ViewBag.BookList = GetBookList();
            return View(new BookIssue());
        }

        
        [HttpPost]
        public ActionResult Create(BookIssue bookIssue)
        {
            try
            {
                if (bookIssue.ReturnDate <= bookIssue.IssueDate)
                {
                    TempData["DuplicateError"] = "Return Date must be greater than Issue Date.!";
                    ViewBag.StudentList = GetStudentList();
                    ViewBag.BookList = GetBookList();
                    return View(bookIssue);
                }

                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();

                    string query = SqlQueryHelper.GetQuery("InsertIssueBook");
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@BOOK_ID", bookIssue.BookId);
                    cmd.Parameters.AddWithValue("@STUDENT_ID", bookIssue.StudentId);
                    cmd.Parameters.AddWithValue("@ISSUE_DATE", bookIssue.IssueDate);
                    cmd.Parameters.AddWithValue("@RETURN_DATE", bookIssue.ReturnDate);
                    cmd.ExecuteNonQuery();

                    string updateQuery = SqlQueryHelper.GetQuery("UpdateBookAvailableCopy");
                    SqlCommand updateCmd = new SqlCommand(updateQuery, con);
                    updateCmd.Parameters.AddWithValue("@BOOK_ID", bookIssue.BookId);
                    int rowsAffected = updateCmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        TempData["DuplicateError"] = "No available copies to issue.";
                        ViewBag.StudentList = GetStudentList();
                        ViewBag.BookList = GetBookList();
                        return View(bookIssue);
                    }
                }

                TempData["SuccessMessage"] = "Book Issued Successfully!";
                return RedirectToAction("Index");
            }
            catch
            {
                ViewBag.StudentList = GetStudentList();
                ViewBag.BookList = GetBookList();
                return View(bookIssue);
            }
        }


     
        public ActionResult Edit(int id)
        {
            BookIssue bookIssue = new BookIssue();
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = SqlQueryHelper.GetQuery("SelectIssueBookById");
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@IssueId", id);
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    bookIssue.IssueId = Convert.ToInt32(rdr["ISSUE_ID"]);
                    bookIssue.BookId = Convert.ToInt32(rdr["BOOK_ID"]);
                    bookIssue.StudentId = Convert.ToInt32(rdr["STUDENT_ID"]);
                    bookIssue.IssueDate = Convert.ToDateTime(rdr["ISSUE_DATE"]);
                    bookIssue.ReturnDate = Convert.ToDateTime(rdr["RETURN_DATE"]);
                }
            }
            ViewBag.StudentList = GetStudentList();
            ViewBag.BookList = GetBookList();
            return View(bookIssue);
        }

  
        [HttpPost]
        public ActionResult Edit(int id, BookIssue bookIssue)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    string q = SqlQueryHelper.GetQuery("UpdateIssueBook");
                    SqlCommand cmd = new SqlCommand(q, con);
                    cmd.Parameters.AddWithValue("@BOOK_ID", bookIssue.BookId);
                    cmd.Parameters.AddWithValue("@STUDENT_ID", bookIssue.StudentId);
                    cmd.Parameters.AddWithValue("@ISSUE_DATE", bookIssue.IssueDate);
                    cmd.Parameters.AddWithValue("@RETURN_DATE", bookIssue.ReturnDate);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["SuccessMessage"] = "Book Issuing details Updated Successfully!";
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }


        private List<SelectListItem> GetStudentList()
        {
            List<SelectListItem> studentList = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = SqlQueryHelper.GetQuery("SelectActiveStudents");
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    int studentId = Convert.ToInt32(rdr["STUDENT_ID"]);
                    string studentName = rdr["STUDENT_NAME"].ToString();
                    studentList.Add(new SelectListItem { Value = studentId.ToString(), Text = studentName });
                }
            }
            return studentList;
        }

   
        private List<SelectListItem> GetBookList()
        {
            List<SelectListItem> bookList = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = SqlQueryHelper.GetQuery("SelectAvailableBooks");
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    int bookId = Convert.ToInt32(rdr["BOOK_ID"]);
                    string title = rdr["TITLE"].ToString();
                    bookList.Add(new SelectListItem { Value = bookId.ToString(), Text = title });
                }
            }
            return bookList;
        }
    }
}