using System;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Represents a Book in the library
    /// </summary>
    public class Book : BaseEntity
    {
        /// <summary>
        /// Book ID (Primary Key)
        /// </summary>
        public int BookID { get; set; }

        /// <summary>
        /// Title of the book
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// ISBN (International Standard Book Number)
        /// </summary>
        public string ISBN { get; set; }

        /// <summary>
        /// Publisher ID (Foreign Key)
        /// </summary>
        public int PublisherID { get; set; }

        /// <summary>
        /// Category ID (Foreign Key)
        /// </summary>
        public int CategoryID { get; set; }

        /// <summary>
        /// Author ID (Foreign Key) - Main author
        /// </summary>
        public int AuthorID { get; set; }

        /// <summary>
        /// Publication year
        /// </summary>
        public int? PublicationYear { get; set; }

        /// <summary>
        /// Edition of the book
        /// </summary>
        public string Edition { get; set; }

        /// <summary>
        /// Language of the book
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Number of pages
        /// </summary>
        public int? Pages { get; set; }

        /// <summary>
        /// Description/Summary of the book
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Path to book cover image
        /// </summary>
        public string CoverImagePath { get; set; }

        /// <summary>
        /// Total number of copies available in the library
        /// </summary>
        public int TotalCopies { get; set; }

        /// <summary>
        /// Number of copies currently available for borrowing
        /// </summary>
        public int AvailableCopies { get; set; }

        /// <summary>
        /// Indicates if the book is retired/out of circulation
        /// </summary>
        public bool IsRetired { get; set; }

        public Book()
        {
            TotalCopies = 0;
            AvailableCopies = 0;
            IsRetired = false;
            Language = "English";
        }
    }
}
