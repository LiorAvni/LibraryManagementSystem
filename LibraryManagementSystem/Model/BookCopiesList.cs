using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Collection of BookCopy objects
    /// </summary>
    public class BookCopiesList : List<BookCopy>
    {
        public BookCopiesList() : base()
        {
        }

        public BookCopiesList(IEnumerable<BookCopy> collection) : base(collection)
        {
        }

        /// <summary>
        /// Find a copy by barcode
        /// </summary>
        public BookCopy FindByBarcode(string barcode)
        {
            return this.Find(c => c.Barcode == barcode);
        }

        /// <summary>
        /// Find all copies of a specific book
        /// </summary>
        public BookCopiesList FindByBookID(int bookID)
        {
            return new BookCopiesList(this.FindAll(c => c.BookID == bookID));
        }

        /// <summary>
        /// Get all available copies
        /// </summary>
        public BookCopiesList GetAvailableCopies()
        {
            return new BookCopiesList(this.FindAll(c => 
                c.Status?.ToLower() == "available"));
        }

        /// <summary>
        /// Get available copies for a specific book
        /// </summary>
        public BookCopiesList GetAvailableCopiesForBook(int bookID)
        {
            return new BookCopiesList(this.FindAll(c => 
                c.BookID == bookID && c.Status?.ToLower() == "available"));
        }

        /// <summary>
        /// Get borrowed copies
        /// </summary>
        public BookCopiesList GetBorrowedCopies()
        {
            return new BookCopiesList(this.FindAll(c => 
                c.Status?.ToLower() == "borrowed"));
        }

        /// <summary>
        /// Get copies by location
        /// </summary>
        public BookCopiesList FindByLocation(string location)
        {
            return new BookCopiesList(this.FindAll(c => 
                c.Location?.ToLower() == location?.ToLower()));
        }

        /// <summary>
        /// Get copies by condition
        /// </summary>
        public BookCopiesList FindByCondition(string condition)
        {
            return new BookCopiesList(this.FindAll(c => 
                c.Condition?.ToLower() == condition?.ToLower()));
        }
    }
}
