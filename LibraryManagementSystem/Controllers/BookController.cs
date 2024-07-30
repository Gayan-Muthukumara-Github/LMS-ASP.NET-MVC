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
    public class BookController : Controller
    {
        string cs = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;
  
        public ActionResult Index()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                string q = SqlQueryHelper.GetQuery("SelectAllBooks");
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
            return View(new Book());
        }

        
        [HttpPost]
        public ActionResult Create(Book book)
        {
            try
            {
                if (IsDuplicate(book.BookReferenceNumber, book.ISBN))
                {
                    TempData["DuplicateError"] = "Book Reference Number or ISBN already exists.";
                    return View(book);
                }
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    string q = SqlQueryHelper.GetQuery("InsertBook");
                    SqlCommand cmd = new SqlCommand(q, con);
                    cmd.Parameters.AddWithValue("@BOOK_REFERENCE_NUMBER", book.BookReferenceNumber);
                    cmd.Parameters.AddWithValue("@TITLE", book.Title);
                    cmd.Parameters.AddWithValue("@ISBN", book.ISBN);
                    cmd.Parameters.AddWithValue("@AUTHOR", book.Author);
                    cmd.Parameters.AddWithValue("@PUBLICATION", book.Publication);
                    cmd.Parameters.AddWithValue("@EDITION", book.Edition);
                    cmd.Parameters.AddWithValue("@PUBLISHED_YEAR", book.PublishedYear.HasValue ? (object)book.PublishedYear.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@CATEGORY", (object)book.Category ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NO_OF_COPY", book.NoOfCopy);
                    cmd.Parameters.AddWithValue("@AVAILBLE_COPY", book.NoOfCopy); // Assuming available copies is initially the same as total copies
                    cmd.ExecuteNonQuery();
                }
                TempData["SuccessMessage"] = "Book Details Added Successfully!";
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        private bool IsDuplicate(string bookReferenceNumber, string isbn)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                string q = SqlQueryHelper.GetQuery("CheckDuplicate");
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@BOOK_REFERENCE_NUMBER", bookReferenceNumber);
                cmd.Parameters.AddWithValue("@ISBN", isbn);
                int count = (int)cmd.ExecuteScalar();
                con.Close();
                return count > 0;
            }
        }

      
        public ActionResult Edit(int id)
        {
            Book book = new Book();
            DataTable datatable = new DataTable();

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                string q = SqlQueryHelper.GetQuery("SelectBookById");
                SqlDataAdapter da = new SqlDataAdapter(q, con);
                da.SelectCommand.Parameters.AddWithValue("@id", id);
                da.Fill(datatable);
            }
            if (datatable.Rows.Count == 1)
            {
                book.BookId = Convert.ToInt32(datatable.Rows[0][0].ToString());
                book.BookReferenceNumber = datatable.Rows[0][1].ToString();
                book.Title = datatable.Rows[0][2].ToString();
                book.ISBN = datatable.Rows[0][3].ToString();
                book.Author = datatable.Rows[0][4].ToString();
                book.Publication = datatable.Rows[0][5].ToString();
                book.Edition = datatable.Rows[0][6].ToString();
                book.PublishedYear = datatable.Rows[0][7] != DBNull.Value ? Convert.ToInt32(datatable.Rows[0][7].ToString()) : 1111;
                book.Category = datatable.Rows[0][8].ToString();
                book.NoOfCopy = datatable.Rows[0][9] != DBNull.Value ? Convert.ToInt32(datatable.Rows[0][9].ToString()) : 0;

                return View(book);
            }
            return RedirectToAction("Index");
        }

     
        [HttpPost]
        public ActionResult Edit(int id, Book book)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    string q = SqlQueryHelper.GetQuery("UpdateBook");
                    SqlCommand cmd = new SqlCommand(q, con);
                    cmd.Parameters.AddWithValue("@BOOK_REFERENCE_NUMBER", book.BookReferenceNumber);
                    cmd.Parameters.AddWithValue("@TITLE", book.Title);
                    cmd.Parameters.AddWithValue("@ISBN", book.ISBN);
                    cmd.Parameters.AddWithValue("@AUTHOR", book.Author);
                    cmd.Parameters.AddWithValue("@PUBLICATION", book.Publication);
                    cmd.Parameters.AddWithValue("@EDITION", book.Edition);
                    cmd.Parameters.AddWithValue("@PUBLISHED_YEAR", book.PublishedYear.HasValue ? (object)book.PublishedYear.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@CATEGORY", (object)book.Category ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NO_OF_COPY", book.NoOfCopy.HasValue ? (object)book.NoOfCopy.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["SuccessMessage"] = "Book Details Updated Successfully!";
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
                    string q = SqlQueryHelper.GetQuery("DeleteBook");
                    SqlCommand cmd = new SqlCommand(q, con);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Book Details Deleted Successfully!";
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) 
                {
                    TempData["ErrorMessage"] = "Cannot delete this book because it is currently issued.";
                }
                else
                {
                    TempData["ErrorMessage"] = "An error occurred while deleting the book.";
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
