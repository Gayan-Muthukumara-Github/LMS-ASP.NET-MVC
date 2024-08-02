using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibraryManagementSystem.Models
{
    public class BookViewModel
    {
        public string BookReferenceNumber { get; set; }
        public string Title { get; set; }
        public string Publication { get; set; }
        public string Author { get; set; }
        public string GuestName { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
    }
}