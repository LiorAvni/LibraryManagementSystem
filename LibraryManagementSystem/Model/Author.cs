using System;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Represents an Author of books
    /// </summary>
    public class Author : BaseEntity
    {
        /// <summary>
        /// Author ID (Primary Key)
        /// </summary>
        public int AuthorID { get; set; }

        /// <summary>
        /// Author's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Author's last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Biography of the author
        /// </summary>
        public string Biography { get; set; }

        /// <summary>
        /// Author's nationality
        /// </summary>
        public string Nationality { get; set; }

        /// <summary>
        /// Date of birth
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Full name of the author
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        public Author()
        {
        }
    }
}
