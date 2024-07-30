using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibraryManagementSystem.Models
{
    public class Student
    {
        public int StudentId { get; set; }

        public string IdNumber { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Address { get; set; }

        public string Telephone { get; set; }

        public DateTime RegisteredDate { get; set; }

        public DateTime? TerminatedDate { get; set; }
    }

}