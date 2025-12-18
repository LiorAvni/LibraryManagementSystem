using System;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Represents a Publisher of books
    /// </summary>
    public class Publisher : BaseEntity
    {
        /// <summary>
        /// Publisher ID (Primary Key)
        /// </summary>
        public int PublisherID { get; set; }

        /// <summary>
        /// Name of the publisher
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Publisher's address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Publisher's phone number
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Publisher's email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Publisher's website
        /// </summary>
        public string Website { get; set; }

        /// <summary>
        /// Country where the publisher is based
        /// </summary>
        public string Country { get; set; }

        public Publisher()
        {
        }
    }
}
