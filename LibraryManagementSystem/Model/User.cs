using System;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Represents a User in the library system
    /// </summary>
    public class User : BaseEntity
    {
        /// <summary>
        /// User ID (Primary Key) - Legacy integer ID
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// User ID as string (GUID from database) - actual primary key
        /// </summary>
        public string UserIdString { get; set; }

        /// <summary>
        /// User's email address (unique)
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Hashed password for security
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// User's phone number
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// User's role (e.g., "Member", "Librarian", "Admin")
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Address line 1
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// City
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Postal/Zip code
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Date of registration
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Full name of the user
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        public User()
        {
            RegistrationDate = DateTime.Now;
        }
    }
}
