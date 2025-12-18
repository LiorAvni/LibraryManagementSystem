using System;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Represents a physical copy of a book
    /// </summary>
    public class BookCopy : BaseEntity
    {
        /// <summary>
        /// Copy ID (Primary Key)
        /// </summary>
        public int CopyID { get; set; }

        /// <summary>
        /// Book ID (Foreign Key) - references the Book
        /// </summary>
        public int BookID { get; set; }

        /// <summary>
        /// Barcode or unique identifier for this copy
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// Status of the copy (e.g., "Available", "Borrowed", "Reserved", "Maintenance", "Lost")
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Condition of the copy (e.g., "New", "Good", "Fair", "Poor", "Damaged")
        /// </summary>
        public string Condition { get; set; }

        /// <summary>
        /// Physical location in the library (e.g., "A-12-3" for Aisle-Shelf-Position)
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Date when the copy was acquired
        /// </summary>
        public DateTime AcquisitionDate { get; set; }

        /// <summary>
        /// Price at which the copy was purchased
        /// </summary>
        public decimal? PurchasePrice { get; set; }

        /// <summary>
        /// Notes about this specific copy
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Date when the copy was last borrowed
        /// </summary>
        public DateTime? LastBorrowedDate { get; set; }

        public BookCopy()
        {
            Status = "Available";
            Condition = "Good";
            AcquisitionDate = DateTime.Now;
        }
    }
}
