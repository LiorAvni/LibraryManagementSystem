using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Collection of Book objects
    /// </summary>
    public class BooksList : List<Book>
    {
        public BooksList() : base()
        {
        }

        public BooksList(IEnumerable<Book> collection) : base(collection)
        {
        }

        /// <summary>
        /// Find a book by ISBN
        /// </summary>
        public Book FindByISBN(string isbn)
        {
            return this.Find(b => b.ISBN == isbn);
        }

        /// <summary>
        /// Find books by title (partial match)
        /// </summary>
        public BooksList FindByTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
                return new BooksList();

            return new BooksList(this.FindAll(b => 
                b.Title?.ToLower().Contains(title.ToLower()) == true));
        }

        /// <summary>
        /// Find books by category
        /// </summary>
        public BooksList FindByCategory(int categoryID)
        {
            return new BooksList(this.FindAll(b => b.CategoryID == categoryID));
        }

        /// <summary>
        /// Find books by author
        /// </summary>
        public BooksList FindByAuthor(int authorID)
        {
            return new BooksList(this.FindAll(b => b.AuthorID == authorID));
        }

        /// <summary>
        /// Find books by publisher
        /// </summary>
        public BooksList FindByPublisher(int publisherID)
        {
            return new BooksList(this.FindAll(b => b.PublisherID == publisherID));
        }

        /// <summary>
        /// Get all available books (not retired and with available copies)
        /// </summary>
        public BooksList GetAvailableBooks()
        {
            return new BooksList(this.FindAll(b => !b.IsRetired && b.AvailableCopies > 0));
        }

        /// <summary>
        /// Get all active books (not retired)
        /// </summary>
        public BooksList GetActiveBooks()
        {
            return new BooksList(this.FindAll(b => !b.IsRetired && b.IsActive));
        }

        /// <summary>
        /// Get books by language
        /// </summary>
        public BooksList FindByLanguage(string language)
        {
            return new BooksList(this.FindAll(b => 
                b.Language?.ToLower() == language?.ToLower()));
        }
    }
}
