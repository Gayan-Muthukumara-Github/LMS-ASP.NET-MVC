using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibraryManagementSystem.Models
{
    public class Book
    {

        public int BookId { get; set; }

        public string BookReferenceNumber { get; set; }

        public string Title { get; set; }

        public string ISBN { get; set; }

        public string Author { get; set; }

        public string Publication { get; set; }

        public string Edition { get; set; }

        public int? PublishedYear { get; set; }

        public string Category { get; set; }

        public int? NoOfCopy { get; set; }

        public int? AvailbleCopy { get; set; }
    }
}