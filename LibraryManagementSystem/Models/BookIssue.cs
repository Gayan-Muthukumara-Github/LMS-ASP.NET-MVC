using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibraryManagementSystem.Models
{
    public class BookIssue
    {
        public int IssueId { get; set; }
        public int BookId { get; set; }

        public int StudentId { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime ReturnDate { get; set; }

    }
}