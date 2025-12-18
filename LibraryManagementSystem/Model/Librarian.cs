using System;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Represents a Librarian user who manages the library
    /// </summary>
    public class Librarian : User
    {
        /// <summary>
        /// Librarian ID (Primary Key)
        /// </summary>
        public int LibrarianID { get; set; }

        /// <summary>
        /// Employee ID for the librarian
        /// </summary>
        public string EmployeeID { get; set; }

        /// <summary>
        /// Date when the librarian was hired
        /// </summary>
        public DateTime HireDate { get; set; }

        /// <summary>
        /// Indicates whether the librarian has admin privileges
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Department the librarian works in
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// Salary of the librarian
        /// </summary>
        public decimal? Salary { get; set; }

        public Librarian()
        {
            HireDate = DateTime.Now;
            IsAdmin = false;
        }
    }
}
