using System;
using System.Data;
using System.Data.OleDb;
using LibraryManagementSystem.Model;

namespace LibraryManagementSystem.ViewModel
{
    /// <summary>
    /// Data Access Layer for Book operations
    /// </summary>
    public class BookDB : BaseDB
    {
        /// <summary>
        /// Gets new arrivals (books added in the last N days) with full details
        /// </summary>
        /// <param name="daysBack">Number of days to look back</param>
        /// <returns>DataTable with book details including author, publisher, and category</returns>
        public DataTable GetNewArrivalsWithDetails(int daysBack = 90)
        {
            try
            {
                // Two-step approach: First get unique books, then get their details
                string query = @"
                    SELECT TOP 50
                        b.book_id,
                        b.title,
                        b.isbn,
                        b.publication_year,
                        b.created_date
                    FROM books b
                    WHERE b.created_date >= DateAdd('d', ?, Date())
                    ORDER BY b.created_date DESC, b.title";
                
                OleDbParameter param = new OleDbParameter("@DaysBack", OleDbType.Integer) { Value = -daysBack };
                DataTable dt = ExecuteQuery(query, param);
                
                // Add the new columns to the DataTable
                if (!dt.Columns.Contains("Author"))
                    dt.Columns.Add("Author", typeof(string));
                if (!dt.Columns.Contains("Publisher"))
                    dt.Columns.Add("Publisher", typeof(string));
                if (!dt.Columns.Contains("Category"))
                    dt.Columns.Add("Category", typeof(string));
                if (!dt.Columns.Contains("AvailableCopies"))
                    dt.Columns.Add("AvailableCopies", typeof(int));
                if (!dt.Columns.Contains("TotalCopies"))
                    dt.Columns.Add("TotalCopies", typeof(int));
                
                // Now populate additional details for each book
                foreach (DataRow row in dt.Rows)
                {
                    string bookId = row["book_id"].ToString();
                    
                    // Get first author
                    string authorQuery = @"
                        SELECT TOP 1 a.first_name & ' ' & a.last_name AS Author
                        FROM book_authors ba
                        INNER JOIN authors a ON ba.author_id = a.author_id
                        WHERE ba.book_id = ?";
                    OleDbParameter authorParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                    DataTable authorDt = ExecuteQuery(authorQuery, authorParam);
                    row["Author"] = authorDt.Rows.Count > 0 ? authorDt.Rows[0]["Author"] : "Unknown Author";
                    
                    // Get publisher name from books table
                    string publisherQuery = @"
                        SELECT p.name
                        FROM books b
                        INNER JOIN publishers p ON b.publisher_id = p.publisher_id
                        WHERE b.book_id = ?";
                    OleDbParameter pubParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                    object publisherName = ExecuteScalar(publisherQuery, pubParam);
                    row["Publisher"] = publisherName != null ? publisherName.ToString() : "Unknown Publisher";
                    
                    // Get category name from books table
                    string categoryQuery = @"
                        SELECT c.name
                        FROM books b
                        INNER JOIN categories c ON b.category_id = c.category_id
                        WHERE b.book_id = ?";
                    OleDbParameter catParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                    object categoryName = ExecuteScalar(categoryQuery, catParam);
                    row["Category"] = categoryName != null ? categoryName.ToString() : "Uncategorized";
                    
                    // Get available copies count
                    string availQuery = "SELECT COUNT(*) FROM book_copies WHERE book_id = ? AND status = 'AVAILABLE'";
                    OleDbParameter availParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                    object availCount = ExecuteScalar(availQuery, availParam);
                    row["AvailableCopies"] = availCount != null ? Convert.ToInt32(availCount) : 0;
                    
                    // Get total copies count
                    string totalQuery = "SELECT COUNT(*) FROM book_copies WHERE book_id = ?";
                    OleDbParameter totalParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                    object totalCount = ExecuteScalar(totalQuery, totalParam);
                    row["TotalCopies"] = totalCount != null ? Convert.ToInt32(totalCount) : 0;
                }
                
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get new arrivals: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets ALL books with full details (for Search Books page)
        /// </summary>
        /// <returns>DataTable with all book details</returns>
        public DataTable GetAllBooksWithDetails()
        {
            try
            {
                // Get all books from database
                string query = @"
                    SELECT 
                        b.book_id,
                        b.title,
                        b.isbn,
                        b.publication_year,
                        b.created_date
                    FROM books b
                    ORDER BY b.title";
                
                DataTable dt = ExecuteQuery(query);
                
                // Add the new columns to the DataTable
                if (!dt.Columns.Contains("Author"))
                    dt.Columns.Add("Author", typeof(string));
                if (!dt.Columns.Contains("Publisher"))
                    dt.Columns.Add("Publisher", typeof(string));
                if (!dt.Columns.Contains("Category"))
                    dt.Columns.Add("Category", typeof(string));
                if (!dt.Columns.Contains("AvailableCopies"))
                    dt.Columns.Add("AvailableCopies", typeof(int));
                if (!dt.Columns.Contains("TotalCopies"))
                    dt.Columns.Add("TotalCopies", typeof(int));
                
                // Populate additional details for each book
                foreach (DataRow row in dt.Rows)
                {
                    string bookId = row["book_id"].ToString();
                    
                    // Get all authors
                    string authorQuery = @"
                        SELECT a.first_name & ' ' & a.last_name AS AuthorName
                        FROM book_authors ba
                        INNER JOIN authors a ON ba.author_id = a.author_id
                        WHERE ba.book_id = ?
                        ORDER BY ba.author_id";
                    
                    OleDbParameter authorParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                    DataTable authorDt = ExecuteQuery(authorQuery, authorParam);
                    
                    if (authorDt.Rows.Count > 0)
                    {
                        var authors = new System.Collections.Generic.List<string>();
                        foreach (DataRow authorRow in authorDt.Rows)
                        {
                            authors.Add(authorRow["AuthorName"].ToString());
                        }
                        row["Author"] = string.Join(", ", authors);
                    }
                    else
                    {
                        row["Author"] = "Unknown Author";
                    }
                    
                    // Get publisher
                    string publisherQuery = @"
                        SELECT p.name
                        FROM books b
                        INNER JOIN publishers p ON b.publisher_id = p.publisher_id
                        WHERE b.book_id = ?";
                    OleDbParameter pubParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                    object publisherName = ExecuteScalar(publisherQuery, pubParam);
                    row["Publisher"] = publisherName != null ? publisherName.ToString() : "Unknown Publisher";
                    
                    // Get category
                    string categoryQuery = @"
                        SELECT c.name
                        FROM books b
                        INNER JOIN categories c ON b.category_id = c.category_id
                        WHERE b.book_id = ?";
                    OleDbParameter catParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                    object categoryName = ExecuteScalar(categoryQuery, catParam);
                    row["Category"] = categoryName != null ? categoryName.ToString() : "Uncategorized";
                    
                    // Get copy counts
                    string availQuery = "SELECT COUNT(*) FROM book_copies WHERE book_id = ? AND status = 'AVAILABLE'";
                    OleDbParameter availParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                    object availCount = ExecuteScalar(availQuery, availParam);
                    row["AvailableCopies"] = availCount != null ? Convert.ToInt32(availCount) : 0;
                    
                    string totalQuery = "SELECT COUNT(*) FROM book_copies WHERE book_id = ?";
                    OleDbParameter totalParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                    object totalCount = ExecuteScalar(totalQuery, totalParam);
                    row["TotalCopies"] = totalCount != null ? Convert.ToInt32(totalCount) : 0;
                }
                
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get all books: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Searches for books based on keyword and search type
        /// </summary>
        /// <param name="keyword">Search keyword</param>
        /// <param name="searchType">Type of search: all, title, author, isbn, category</param>
        /// <returns>DataTable with book details</returns>
        public DataTable SearchBooksWithDetails(string keyword, string searchType = "all")
        {
            try
            {
                string whereClause = "";
                keyword = $"%{keyword}%"; // Add wildcards for LIKE search
                
                switch (searchType.ToLower())
                {
                    case "title":
                        whereClause = "WHERE b.title LIKE ?";
                        break;
                    case "isbn":
                        whereClause = "WHERE b.isbn LIKE ?";
                        break;
                    case "category":
                        whereClause = "WHERE c.name LIKE ?";
                        break;
                    case "author":
                        // For author search, we'll handle it differently
                        return SearchBooksByAuthor(keyword);
                    case "all":
                    default:
                        // Search in title, ISBN, category, or description
                        whereClause = "WHERE b.title LIKE ? OR b.isbn LIKE ? OR c.name LIKE ? OR b.description LIKE ?";
                        break;
                }
                
                // Build the query
                string query = $@"
                    SELECT TOP 100
                        b.book_id,
                        b.title,
                        b.isbn,
                        b.publication_year,
                        b.created_date,
                        c.name AS Category
                    FROM books b
                    LEFT JOIN categories c ON b.category_id = c.category_id
                    {whereClause}
                    ORDER BY b.title";
                
                // Add parameters based on search type
                OleDbParameter[] parameters;
                if (searchType.ToLower() == "all")
                {
                    parameters = new OleDbParameter[] {
                        new OleDbParameter("@Keyword1", OleDbType.VarChar, 255) { Value = keyword },
                        new OleDbParameter("@Keyword2", OleDbType.VarChar, 255) { Value = keyword },
                        new OleDbParameter("@Keyword3", OleDbType.VarChar, 255) { Value = keyword },
                        new OleDbParameter("@Keyword4", OleDbType.VarChar, 255) { Value = keyword }
                    };
                }
                else
                {
                    parameters = new OleDbParameter[] {
                        new OleDbParameter("@Keyword", OleDbType.VarChar, 255) { Value = keyword }
                    };
                }
                
                DataTable dt = ExecuteQuery(query, parameters);
                
                // Add additional columns
                if (!dt.Columns.Contains("Author"))
                    dt.Columns.Add("Author", typeof(string));
                if (!dt.Columns.Contains("Publisher"))
                    dt.Columns.Add("Publisher", typeof(string));
                if (!dt.Columns.Contains("AvailableCopies"))
                    dt.Columns.Add("AvailableCopies", typeof(int));
                if (!dt.Columns.Contains("TotalCopies"))
                    dt.Columns.Add("TotalCopies", typeof(int));
                
                // Populate additional details for each book
                foreach (DataRow row in dt.Rows)
                {
                    string bookId = row["book_id"].ToString();
                    
                    // Get all authors
                    string authorQuery = @"
                        SELECT a.first_name & ' ' & a.last_name AS AuthorName
                        FROM book_authors ba
                        INNER JOIN authors a ON ba.author_id = a.author_id
                        WHERE ba.book_id = ?
                        ORDER BY ba.author_id";
                    
                    OleDbParameter authorParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                    DataTable authorDt = ExecuteQuery(authorQuery, authorParam);
                    
                    if (authorDt.Rows.Count > 0)
                    {
                        var authors = new System.Collections.Generic.List<string>();
                        foreach (DataRow authorRow in authorDt.Rows)
                        {
                            authors.Add(authorRow["AuthorName"].ToString());
                        }
                        row["Author"] = string.Join(", ", authors);
                    }
                    else
                    {
                        row["Author"] = "Unknown Author";
                    }
                    
                    // Get publisher
                    string publisherQuery = @"
                        SELECT p.name
                        FROM books b
                        INNER JOIN publishers p ON b.publisher_id = p.publisher_id
                        WHERE b.book_id = ?";
                    OleDbParameter pubParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                    object publisherName = ExecuteScalar(publisherQuery, pubParam);
                    row["Publisher"] = publisherName != null ? publisherName.ToString() : "Unknown Publisher";
                    
                    // Get copy counts
                    string availQuery = "SELECT COUNT(*) FROM book_copies WHERE book_id = ? AND status = 'AVAILABLE'";
                    OleDbParameter availParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                    object availCount = ExecuteScalar(availQuery, availParam);
                    row["AvailableCopies"] = availCount != null ? Convert.ToInt32(availCount) : 0;
                    
                    string totalQuery = "SELECT COUNT(*) FROM book_copies WHERE book_id = ?";
                    OleDbParameter totalParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                    object totalCount = ExecuteScalar(totalQuery, totalParam);
                    row["TotalCopies"] = totalCount != null ? Convert.ToInt32(totalCount) : 0;
                }
                
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to search books: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Searches for books by author name
        /// </summary>
        private DataTable SearchBooksByAuthor(string authorKeyword)
        {
            try
            {
                // First, find books that match the author
                string query = @"
                    SELECT DISTINCT TOP 100
                        b.book_id,
                        b.title,
                        b.isbn,
                        b.publication_year,
                        b.created_date
                    FROM ((books b
                    INNER JOIN book_authors ba ON b.book_id = ba.book_id)
                    INNER JOIN authors a ON ba.author_id = a.author_id)
                    WHERE (a.first_name LIKE ? OR a.last_name LIKE ? OR (a.first_name & ' ' & a.last_name) LIKE ?)
                    ORDER BY b.title";
                
                OleDbParameter[] parameters = new OleDbParameter[] {
                    new OleDbParameter("@Keyword1", OleDbType.VarChar, 255) { Value = authorKeyword },
                    new OleDbParameter("@Keyword2", OleDbType.VarChar, 255) { Value = authorKeyword },
                    new OleDbParameter("@Keyword3", OleDbType.VarChar, 255) { Value = authorKeyword }
                };
                
                DataTable dt = ExecuteQuery(query, parameters);
                
                // Add additional columns
                if (!dt.Columns.Contains("Author"))
                    dt.Columns.Add("Author", typeof(string));
                if (!dt.Columns.Contains("Publisher"))
                    dt.Columns.Add("Publisher", typeof(string));
                if (!dt.Columns.Contains("Category"))
                    dt.Columns.Add("Category", typeof(string));
                if (!dt.Columns.Contains("AvailableCopies"))
                    dt.Columns.Add("AvailableCopies", typeof(int));
                if (!dt.Columns.Contains("TotalCopies"))
                    dt.Columns.Add("TotalCopies", typeof(int));
                
                // Populate additional details
                foreach (DataRow row in dt.Rows)
                {
                    string bookId = row["book_id"].ToString();
                    
                    // Get all authors
                    string authorQuery = @"
                        SELECT a.first_name & ' ' & a.last_name AS AuthorName
                        FROM book_authors ba
                        INNER JOIN authors a ON ba.author_id = a.author_id
                        WHERE ba.book_id = ?
                        ORDER BY ba.author_id";
                    
                    OleDbParameter authorParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                    DataTable authorDt = ExecuteQuery(authorQuery, authorParam);
                    
                    if (authorDt.Rows.Count > 0)
                    {
                        var authors = new System.Collections.Generic.List<string>();
                        foreach (DataRow authorRow in authorDt.Rows)
                        {
                            authors.Add(authorRow["AuthorName"].ToString());
                        }
                        row["Author"] = string.Join(", ", authors);
                    }
                    else
                    {
                        row["Author"] = "Unknown Author";
                    }
                    
                    // Get publisher
                    string publisherQuery = @"
                        SELECT p.name
                        FROM books b
                        INNER JOIN publishers p ON b.publisher_id = p.publisher_id
                        WHERE b.book_id = ?";
                    OleDbParameter pubParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                    object publisherName = ExecuteScalar(publisherQuery, pubParam);
                    row["Publisher"] = publisherName != null ? publisherName.ToString() : "Unknown Publisher";
                    
                    // Get category
                    string categoryQuery = @"
                        SELECT c.name
                        FROM books b
                        INNER JOIN categories c ON b.category_id = c.category_id
                        WHERE b.book_id = ?";
                    OleDbParameter catParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                    object categoryName = ExecuteScalar(categoryQuery, catParam);
                    row["Category"] = categoryName != null ? categoryName.ToString() : "Uncategorized";
                    
                    // Get copy counts
                    string availQuery = "SELECT COUNT(*) FROM book_copies WHERE book_id = ? AND status = 'AVAILABLE'";
                    OleDbParameter availParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                    object availCount = ExecuteScalar(availQuery, availParam);
                    row["AvailableCopies"] = availCount != null ? Convert.ToInt32(availCount) : 0;
                    
                    string totalQuery = "SELECT COUNT(*) FROM book_copies WHERE book_id = ?";
                    OleDbParameter totalParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                    object totalCount = ExecuteScalar(totalQuery, totalParam);
                    row["TotalCopies"] = totalCount != null ? Convert.ToInt32(totalCount) : 0;
                }
                
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to search books by author: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all publishers
        /// </summary>
        /// <returns>DataTable with publisher_id and name</returns>
        public DataTable GetAllPublishers()
        {
            try
            {
                string query = @"
                    SELECT publisher_id, name
                    FROM publishers
                    ORDER BY name";
                
                return ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get publishers: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <returns>DataTable with category_id and name</returns>
        public DataTable GetAllCategories()
        {
            try
            {
                string query = @"
                    SELECT category_id, name
                    FROM categories
                    ORDER BY name";
                
                return ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get categories: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all authors
        /// </summary>
        /// <returns>DataTable with author_id and full name</returns>
        public DataTable GetAllAuthors()
        {
            try
            {
                string query = @"
                    SELECT author_id, first_name, last_name, biography, birth_date, first_name & ' ' & last_name AS full_name
                    FROM authors
                    ORDER BY last_name, first_name";
                
                return ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get authors: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a specific author by ID
        /// </summary>
        /// <param name="authorId">Author ID (GUID string)</param>
        /// <returns>DataTable with author information</returns>
        public DataTable GetAuthorById(string authorId)
        {
            try
            {
                string query = @"
                    SELECT author_id, first_name, last_name, biography, birth_date
                    FROM authors
                    WHERE author_id = ?";
                
                OleDbParameter param = new OleDbParameter("@AuthorID", OleDbType.VarChar, 36) { Value = authorId };
                return ExecuteQuery(query, param);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get author: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Inserts a new author
        /// </summary>
        /// <param name="authorId">Author ID (GUID string)</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="biography">Biography (optional)</param>
        /// <param name="birthDate">Birth date (optional)</param>
        /// <returns>True if successful</returns>
        public bool InsertAuthor(string authorId, string firstName, string lastName, string biography, DateTime? birthDate)
        {
            try
            {
                string query = @"
                    INSERT INTO authors (author_id, first_name, last_name, biography, birth_date)
                    VALUES (?, ?, ?, ?, ?)";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@AuthorID", OleDbType.VarChar, 36) { Value = authorId },
                    new OleDbParameter("@FirstName", OleDbType.VarChar, 50) { Value = firstName },
                    new OleDbParameter("@LastName", OleDbType.VarChar, 50) { Value = lastName },
                    new OleDbParameter("@Biography", OleDbType.LongVarChar) { Value = biography ?? (object)DBNull.Value },
                    new OleDbParameter("@BirthDate", OleDbType.Date) { Value = birthDate ?? (object)DBNull.Value }
                };
                
                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to insert author: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates an existing author
        /// </summary>
        /// <param name="authorId">Author ID (GUID string)</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="biography">Biography (optional)</param>
        /// <param name="birthDate">Birth date (optional)</param>
        /// <returns>True if successful</returns>
        public bool UpdateAuthor(string authorId, string firstName, string lastName, string biography, DateTime? birthDate)
        {
            try
            {
                string query = @"
                    UPDATE authors
                    SET first_name = ?, last_name = ?, biography = ?, birth_date = ?
                    WHERE author_id = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@FirstName", OleDbType.VarChar, 50) { Value = firstName },
                    new OleDbParameter("@LastName", OleDbType.VarChar, 50) { Value = lastName },
                    new OleDbParameter("@Biography", OleDbType.LongVarChar) { Value = biography ?? (object)DBNull.Value },
                    new OleDbParameter("@BirthDate", OleDbType.Date) { Value = birthDate ?? (object)DBNull.Value },
                    new OleDbParameter("@AuthorID", OleDbType.VarChar, 36) { Value = authorId }
                };
                
                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update author: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deletes an author (only if they have no books)
        /// </summary>
        /// <param name="authorId">Author ID (GUID string)</param>
        /// <returns>True if successful</returns>
        public bool DeleteAuthor(string authorId)
        {
            try
            {
                // Check if author has books
                int bookCount = GetAuthorBookCount(authorId);
                if (bookCount > 0)
                {
                    throw new Exception($"Cannot delete author. Author is associated with {bookCount} book(s).");
                }
                
                string query = "DELETE FROM authors WHERE author_id = ?";
                OleDbParameter param = new OleDbParameter("@AuthorID", OleDbType.VarChar, 36) { Value = authorId };
                
                return ExecuteNonQuery(query, param) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete author: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the count of books associated with an author
        /// </summary>
        /// <param name="authorId">Author ID (GUID string)</param>
        /// <returns>Number of books</returns>
        public int GetAuthorBookCount(string authorId)
        {
            try
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM book_authors 
                    WHERE author_id = ?";
                
                OleDbParameter param = new OleDbParameter("@AuthorID", OleDbType.VarChar, 36) { Value = authorId };
                object result = ExecuteScalar(query, param);
                
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get author book count: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets book details by ID for editing
        /// </summary>
        /// <param name="bookId">Book ID</param>
        /// <returns>DataRow with book details</returns>
        public DataRow GetBookDetailsForEdit(string bookId)
        {
            try
            {
                string query = @"
                    SELECT 
                        b.book_id,
                        b.title,
                        b.isbn,
                        b.publisher_id,
                        b.publication_year,
                        b.category_id,
                        b.description
                    FROM books b
                    WHERE b.book_id = ?";
                
                OleDbParameter param = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                DataTable dt = ExecuteQuery(query, param);
                
                if (dt.Rows.Count > 0)
                {
                    return dt.Rows[0];
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get book details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets author IDs for a book
        /// </summary>
        /// <param name="bookId">Book ID</param>
        /// <returns>List of author IDs</returns>
        public System.Collections.Generic.List<string> GetBookAuthorIds(string bookId)
        {
            try
            {
                string query = @"
                    SELECT author_id
                    FROM book_authors
                    WHERE book_id = ?";
                
                OleDbParameter param = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                DataTable dt = ExecuteQuery(query, param);
                
                var authorIds = new System.Collections.Generic.List<string>();
                foreach (DataRow row in dt.Rows)
                {
                    authorIds.Add(row["author_id"].ToString());
                }
                
                return authorIds;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get book authors: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets book copies for editing
        /// </summary>
        /// <param name="bookId">Book ID</param>
        /// <returns>DataTable with copy details</returns>
        public DataTable GetBookCopies(string bookId)
        {
            try
            {
                string query = @"
                    SELECT 
                        copy_id,
                        copy_number,
                        status,
                        condition,
                        location,
                        acquisition_date
                    FROM book_copies
                    WHERE book_id = ?
                    ORDER BY copy_number";
                
                OleDbParameter param = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                return ExecuteQuery(query, param);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get book copies: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if a copy is in use (borrowed or reserved)
        /// </summary>
        /// <param name="copyId">Copy ID</param>
        /// <returns>True if copy is in use</returns>
        public bool IsCopyInUse(string copyId)
        {
            try
            {
                string query = @"
                    SELECT status
                    FROM book_copies
                    WHERE copy_id = ?";
                
                OleDbParameter param = new OleDbParameter("@CopyID", OleDbType.VarChar, 36) { Value = copyId };
                object status = ExecuteScalar(query, param);
                
                if (status != null)
                {
                    string statusStr = status.ToString();
                    return statusStr == "BORROWED" || statusStr == "RESERVED";
                }
                
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to check copy status: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates book information
        /// </summary>
        /// <param name="bookId">Book ID</param>
        /// <param name="isbn">ISBN</param>
        /// <param name="title">Book title</param>
        /// <param name="publisherId">Publisher ID (nullable)</param>
        /// <param name="publicationYear">Publication year</param>
        /// <param name="categoryId">Category ID (nullable)</param>
        /// <param name="description">Book description (nullable)</param>
        /// <param name="authorIds">List of author IDs</param>
        /// <returns>True if successful</returns>
        public bool UpdateBook(string bookId, string isbn, string title, string publisherId, int publicationYear, 
                               string categoryId, string description, System.Collections.Generic.List<string> authorIds)
        {
            try
            {
                // Update book
                string updateBookQuery = @"
                    UPDATE books 
                    SET title = ?, 
                        isbn = ?, 
                        publisher_id = ?, 
                        publication_year = ?, 
                        category_id = ?, 
                        description = ?
                    WHERE book_id = ?";
                
                OleDbParameter[] bookParams = {
                    new OleDbParameter("@Title", OleDbType.VarChar, 200) { Value = title },
                    new OleDbParameter("@ISBN", OleDbType.VarChar, 20) { Value = string.IsNullOrEmpty(isbn) ? (object)DBNull.Value : isbn },
                    new OleDbParameter("@PublisherID", OleDbType.VarChar, 36) { Value = string.IsNullOrEmpty(publisherId) ? (object)DBNull.Value : publisherId },
                    new OleDbParameter("@PublicationYear", OleDbType.Integer) { Value = publicationYear > 0 ? (object)publicationYear : DBNull.Value },
                    new OleDbParameter("@CategoryID", OleDbType.VarChar, 36) { Value = string.IsNullOrEmpty(categoryId) ? (object)DBNull.Value : categoryId },
                    new OleDbParameter("@Description", OleDbType.LongVarChar) { Value = string.IsNullOrEmpty(description) ? (object)DBNull.Value : description },
                    new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId }
                };
                
                int bookResult = ExecuteNonQuery(updateBookQuery, bookParams);
                
                if (bookResult <= 0)
                {
                    return false;
                }
                
                // Delete existing author relationships
                string deleteAuthorsQuery = "DELETE FROM book_authors WHERE book_id = ?";
                OleDbParameter deleteParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                ExecuteNonQuery(deleteAuthorsQuery, deleteParam);
                
                // Insert new author relationships
                if (authorIds != null && authorIds.Count > 0)
                {
                    foreach (string authorId in authorIds)
                    {
                        string insertAuthorQuery = @"
                            INSERT INTO book_authors (book_id, author_id)
                            VALUES (?, ?)";
                        
                        OleDbParameter[] authorParams = {
                            new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId },
                            new OleDbParameter("@AuthorID", OleDbType.VarChar, 36) { Value = authorId }
                        };
                        
                        ExecuteNonQuery(insertAuthorQuery, authorParams);
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update book: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates a book copy
        /// </summary>
        /// <param name="copyId">Copy ID</param>
        /// <param name="status">New status</param>
        /// <param name="condition">New condition</param>
        /// <param name="location">New location</param>
        /// <returns>True if successful</returns>
        public bool UpdateBookCopy(string copyId, string status, string condition, string location)
        {
            try
            {
                string query = @"
                    UPDATE book_copies 
                    SET status = ?, 
                        condition = ?, 
                        location = ?
                    WHERE copy_id = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@Status", OleDbType.VarChar, 20) { Value = status },
                    new OleDbParameter("@Condition", OleDbType.VarChar, 20) { Value = condition },
                    new OleDbParameter("@Location", OleDbType.VarChar, 100) { Value = location },
                    new OleDbParameter("@CopyID", OleDbType.VarChar, 36) { Value = copyId }
                };
                
                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update book copy: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Adds a new copy for an existing book
        /// </summary>
        /// <param name="bookId">Book ID</param>
        /// <returns>New copy ID if successful</returns>
        public string AddBookCopy(string bookId)
        {
            try
            {
                // Get the next copy number
                string maxCopyQuery = @"
                    SELECT MAX(copy_number) 
                    FROM book_copies 
                    WHERE book_id = ?";
                
                OleDbParameter maxParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                object maxCopyObj = ExecuteScalar(maxCopyQuery, maxParam);
                
                int nextCopyNumber = (maxCopyObj != null && maxCopyObj != DBNull.Value) 
                    ? Convert.ToInt32(maxCopyObj) + 1 
                    : 1;
                
                // Create new copy
                string copyId = Guid.NewGuid().ToString();
                
                string insertQuery = @"
                    INSERT INTO book_copies (copy_id, book_id, copy_number, status, condition, acquisition_date, location)
                    VALUES (?, ?, ?, ?, ?, ?, ?)";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@CopyID", OleDbType.VarChar, 36) { Value = copyId },
                    new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId },
                    new OleDbParameter("@CopyNumber", OleDbType.Integer) { Value = nextCopyNumber },
                    new OleDbParameter("@Status", OleDbType.VarChar, 20) { Value = "AVAILABLE" },
                    new OleDbParameter("@Condition", OleDbType.VarChar, 20) { Value = "GOOD" },
                    new OleDbParameter("@AcquisitionDate", OleDbType.Date) { Value = DateTime.Now },
                    new OleDbParameter("@Location", OleDbType.VarChar, 100) { Value = "Main Library" }
                };
                
                int result = ExecuteNonQuery(insertQuery, parameters);
                
                return result > 0 ? copyId : null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add book copy: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Adds a new book with authors and copies
        /// </summary>
        /// <param name="isbn">ISBN</param>
        /// <param name="title">Book title</param>
        /// <param name="publisherId">Publisher ID (nullable)</param>
        /// <param name="publicationYear">Publication year</param>
        /// <param name="categoryId">Category ID (nullable)</param>
        /// <param name="description">Book description (nullable)</param>
        /// <param name="authorIds">List of author IDs</param>
        /// <param name="totalCopies">Number of copies to create</param>
        /// <returns>New book ID (GUID string)</returns>
        public string AddNewBook(string isbn, string title, string publisherId, int publicationYear, 
                                 string categoryId, string description, System.Collections.Generic.List<string> authorIds, 
                                 int totalCopies)
        {
            try
            {
                // Generate new book ID
                string bookId = Guid.NewGuid().ToString();
                
                // Insert book
                string insertBookQuery = @"
                    INSERT INTO books (book_id, title, isbn, publisher_id, publication_year, category_id, description, created_date)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
                
                OleDbParameter[] bookParams = {
                    new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId },
                    new OleDbParameter("@Title", OleDbType.VarChar, 200) { Value = title },
                    new OleDbParameter("@ISBN", OleDbType.VarChar, 20) { Value = string.IsNullOrEmpty(isbn) ? (object)DBNull.Value : isbn },
                    new OleDbParameter("@PublisherID", OleDbType.VarChar, 36) { Value = string.IsNullOrEmpty(publisherId) ? (object)DBNull.Value : publisherId },
                    new OleDbParameter("@PublicationYear", OleDbType.Integer) { Value = publicationYear > 0 ? (object)publicationYear : DBNull.Value },
                    new OleDbParameter("@CategoryID", OleDbType.VarChar, 36) { Value = string.IsNullOrEmpty(categoryId) ? (object)DBNull.Value : categoryId },
                    new OleDbParameter("@Description", OleDbType.LongVarChar) { Value = string.IsNullOrEmpty(description) ? (object)DBNull.Value : description },
                    new OleDbParameter("@CreatedDate", OleDbType.Date) { Value = DateTime.Now }
                };
                
                int bookResult = ExecuteNonQuery(insertBookQuery, bookParams);
                
                if (bookResult <= 0)
                {
                    throw new Exception("Failed to insert book");
                }
                
                // Insert book authors
                if (authorIds != null && authorIds.Count > 0)
                {
                    foreach (string authorId in authorIds)
                    {
                        string insertAuthorQuery = @"
                            INSERT INTO book_authors (book_id, author_id)
                            VALUES (?, ?)";
                        
                        OleDbParameter[] authorParams = {
                            new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId },
                            new OleDbParameter("@AuthorID", OleDbType.VarChar, 36) { Value = authorId }
                        };
                        
                        ExecuteNonQuery(insertAuthorQuery, authorParams);
                    }
                }
                
                // Insert book copies
                for (int i = 1; i <= totalCopies; i++)
                {
                    string copyId = Guid.NewGuid().ToString();
                    
                    string insertCopyQuery = @"
                        INSERT INTO book_copies (copy_id, book_id, copy_number, status, condition, acquisition_date, location)
                        VALUES (?, ?, ?, ?, ?, ?, ?)";
                    
                    OleDbParameter[] copyParams = {
                        new OleDbParameter("@CopyID", OleDbType.VarChar, 36) { Value = copyId },
                        new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId },
                        new OleDbParameter("@CopyNumber", OleDbType.Integer) { Value = i },
                        new OleDbParameter("@Status", OleDbType.VarChar, 20) { Value = "AVAILABLE" },
                        new OleDbParameter("@Condition", OleDbType.VarChar, 20) { Value = "GOOD" },
                        new OleDbParameter("@AcquisitionDate", OleDbType.Date) { Value = DateTime.Now },
                        new OleDbParameter("@Location", OleDbType.VarChar, 100) { Value = "Main Library" }
                    };
                    
                    ExecuteNonQuery(insertCopyQuery, copyParams);
                }
                
                return bookId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add new book: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all books
        /// </summary>
        /// <returns>List of all books</returns>
        public BooksList SelectAllBooks()
        {
            try
            {
                string query = @"SELECT * FROM Books 
                                WHERE IsActive = True AND IsRetired = False 
                                ORDER BY Title";
                
                DataTable dt = ExecuteQuery(query);
                return MapDataTableToBooksList(dt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get books: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Searches books by filter criteria
        /// </summary>
        /// <param name="title">Book title (partial match)</param>
        /// <param name="authorId">Author ID (0 for all)</param>
        /// <param name="categoryId">Category ID (0 for all)</param>
        /// <returns>Filtered list of books</returns>
        public BooksList SelectBooksByFilter(string title, int authorId, int categoryId)
        {
            try
            {
                string query = @"SELECT * FROM Books WHERE IsActive = True AND IsRetired = False";
                
                if (!string.IsNullOrEmpty(title))
                {
                    query += " AND Title LIKE ?";
                }
                if (authorId > 0)
                {
                    query += " AND AuthorID = ?";
                }
                if (categoryId > 0)
                {
                    query += " AND CategoryID = ?";
                }
                
                query += " ORDER BY Title";

                var parameters = new System.Collections.Generic.List<OleDbParameter>();
                
                if (!string.IsNullOrEmpty(title))
                {
                    parameters.Add(new OleDbParameter("@Title", $"%{title}%"));
                }
                if (authorId > 0)
                {
                    parameters.Add(new OleDbParameter("@AuthorID", authorId));
                }
                if (categoryId > 0)
                {
                    parameters.Add(new OleDbParameter("@CategoryID", categoryId));
                }

                DataTable dt = ExecuteQuery(query, parameters.ToArray());
                return MapDataTableToBooksList(dt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to filter books: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets available books (with copies available for borrowing)
        /// </summary>
        /// <returns>List of available books</returns>
        public BooksList SelectAvailableBooks()
        {
            try
            {
                string query = @"SELECT * FROM Books 
                                WHERE IsActive = True AND IsRetired = False 
                                AND AvailableCopies > 0 
                                ORDER BY Title";
                
                DataTable dt = ExecuteQuery(query);
                return MapDataTableToBooksList(dt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get available books: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a book by ID
        /// </summary>
        /// <param name="bookId">Book ID</param>
        /// <returns>Book object or null</returns>
        public Book GetBookByID(int bookId)
        {
            try
            {
                string query = "SELECT * FROM Books WHERE BookID = ?";
                OleDbParameter param = new OleDbParameter("@BookID", bookId);
                
                DataTable dt = ExecuteQuery(query, param);
                
                if (dt.Rows.Count > 0)
                {
                    return MapRowToBook(dt.Rows[0]);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get book: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a book by ISBN
        /// </summary>
        /// <param name="isbn">ISBN number</param>
        /// <returns>Book object or null</returns>
        public Book GetBookByISBN(string isbn)
        {
            try
            {
                string query = "SELECT * FROM Books WHERE ISBN = ?";
                OleDbParameter param = new OleDbParameter("@ISBN", isbn);
                
                DataTable dt = ExecuteQuery(query, param);
                
                if (dt.Rows.Count > 0)
                {
                    return MapRowToBook(dt.Rows[0]);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get book by ISBN: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Inserts a new book
        /// </summary>
        /// <param name="book">Book object to insert</param>
        /// <returns>True if successful</returns>
        public bool InsertBook(Book book)
        {
            try
            {
                string query = @"INSERT INTO Books (Title, ISBN, PublisherID, CategoryID, 
                                AuthorID, PublicationYear, Edition, Language, Pages, 
                                Description, CoverImagePath, TotalCopies, AvailableCopies, 
                                IsRetired, CreatedAt, IsActive)
                                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                OleDbParameter[] parameters = {
                    new OleDbParameter("@Title", book.Title ?? (object)DBNull.Value),
                    new OleDbParameter("@ISBN", book.ISBN ?? (object)DBNull.Value),
                    new OleDbParameter("@PublisherID", book.PublisherID),
                    new OleDbParameter("@CategoryID", book.CategoryID),
                    new OleDbParameter("@AuthorID", book.AuthorID),
                    new OleDbParameter("@PublicationYear", book.PublicationYear ?? (object)DBNull.Value),
                    new OleDbParameter("@Edition", book.Edition ?? (object)DBNull.Value),
                    new OleDbParameter("@Language", book.Language ?? "English"),
                    new OleDbParameter("@Pages", book.Pages ?? (object)DBNull.Value),
                    new OleDbParameter("@Description", book.Description ?? (object)DBNull.Value),
                    new OleDbParameter("@CoverImagePath", book.CoverImagePath ?? (object)DBNull.Value),
                    new OleDbParameter("@TotalCopies", book.TotalCopies),
                    new OleDbParameter("@AvailableCopies", book.AvailableCopies),
                    new OleDbParameter("@IsRetired", book.IsRetired),
                    new OleDbParameter("@CreatedAt", book.CreatedAt),
                    new OleDbParameter("@IsActive", book.IsActive)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to insert book: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates an existing book
        /// </summary>
        /// <param name="book">Book object with updated data</param>
        /// <returns>True if successful</returns>
        public bool UpdateBook(Book book)
        {
            try
            {
                string query = @"UPDATE Books SET Title = ?, ISBN = ?, PublisherID = ?, 
                                CategoryID = ?, AuthorID = ?, PublicationYear = ?, 
                                Edition = ?, Language = ?, Pages = ?, Description = ?, 
                                CoverImagePath = ?, TotalCopies = ?, AvailableCopies = ?, 
                                UpdatedAt = ?
                                WHERE BookID = ?";

                OleDbParameter[] parameters = {
                    new OleDbParameter("@Title", book.Title ?? (object)DBNull.Value),
                    new OleDbParameter("@ISBN", book.ISBN ?? (object)DBNull.Value),
                    new OleDbParameter("@PublisherID", book.PublisherID),
                    new OleDbParameter("@CategoryID", book.CategoryID),
                    new OleDbParameter("@AuthorID", book.AuthorID),
                    new OleDbParameter("@PublicationYear", book.PublicationYear ?? (object)DBNull.Value),
                    new OleDbParameter("@Edition", book.Edition ?? (object)DBNull.Value),
                    new OleDbParameter("@Language", book.Language ?? "English"),
                    new OleDbParameter("@Pages", book.Pages ?? (object)DBNull.Value),
                    new OleDbParameter("@Description", book.Description ?? (object)DBNull.Value),
                    new OleDbParameter("@CoverImagePath", book.CoverImagePath ?? (object)DBNull.Value),
                    new OleDbParameter("@TotalCopies", book.TotalCopies),
                    new OleDbParameter("@AvailableCopies", book.AvailableCopies),
                    new OleDbParameter("@UpdatedAt", DateTime.Now),
                    new OleDbParameter("@BookID", book.BookID)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update book: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retires a book (sets IsRetired to true)
        /// </summary>
        /// <param name="bookId">Book ID to retire</param>
        /// <returns>True if successful</returns>
        public bool RetireBook(int bookId)
        {
            try
            {
                string query = "UPDATE Books SET IsRetired = True, UpdatedAt = ? WHERE BookID = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@UpdatedAt", DateTime.Now),
                    new OleDbParameter("@BookID", bookId)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retire book: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates available copies count for a book
        /// </summary>
        /// <param name="bookId">Book ID</param>
        /// <param name="increment">Amount to increment (positive) or decrement (negative)</param>
        /// <returns>True if successful</returns>
        public bool UpdateAvailableCopies(int bookId, int increment)
        {
            try
            {
                string query = @"UPDATE Books 
                                SET AvailableCopies = AvailableCopies + ?, UpdatedAt = ? 
                                WHERE BookID = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@Increment", increment),
                    new OleDbParameter("@UpdatedAt", DateTime.Now),
                    new OleDbParameter("@BookID", bookId)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update available copies: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Maps a DataTable to a BooksList
        /// </summary>
        private BooksList MapDataTableToBooksList(DataTable dt)
        {
            BooksList books = new BooksList();
            foreach (DataRow row in dt.Rows)
            {
                books.Add(MapRowToBook(row));
            }
            return books;
        }

        /// <summary>
        /// Maps a DataRow to a Book object
        /// </summary>
        private Book MapRowToBook(DataRow row)
        {
            return new Book
            {
                BookID = Convert.ToInt32(row["BookID"]),
                Title = row["Title"]?.ToString(),
                ISBN = row["ISBN"]?.ToString(),
                PublisherID = row["PublisherID"] != DBNull.Value ? Convert.ToInt32(row["PublisherID"]) : 0,
                CategoryID = row["CategoryID"] != DBNull.Value ? Convert.ToInt32(row["CategoryID"]) : 0,
                AuthorID = row["AuthorID"] != DBNull.Value ? Convert.ToInt32(row["AuthorID"]) : 0,
                PublicationYear = row["PublicationYear"] != DBNull.Value ? Convert.ToInt32(row["PublicationYear"]) : (int?)null,
                Edition = row["Edition"]?.ToString(),
                Language = row["Language"]?.ToString() ?? "English",
                Pages = row["Pages"] != DBNull.Value ? Convert.ToInt32(row["Pages"]) : (int?)null,
                Description = row["Description"]?.ToString(),
                CoverImagePath = row["CoverImagePath"]?.ToString(),
                TotalCopies = row["TotalCopies"] != DBNull.Value ? Convert.ToInt32(row["TotalCopies"]) : 0,
                AvailableCopies = row["AvailableCopies"] != DBNull.Value ? Convert.ToInt32(row["AvailableCopies"]) : 0,
                IsRetired = row["IsRetired"] != DBNull.Value && Convert.ToBoolean(row["IsRetired"]),
                CreatedAt = row["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(row["CreatedAt"]) : DateTime.Now,
                UpdatedAt = row["UpdatedAt"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedAt"]) : (DateTime?)null,
                IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"])
            };
        }
    }
}
