using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibraryManagementSystem.Models
{
    public class CD
    {
        public int CdId { get; set; }

        public string CdTitle { get; set; }

        public string Author { get; set; }

        public string Publication { get; set; }

        public int? PrintedYear { get; set; }

        public string Category { get; set; }
    }
}