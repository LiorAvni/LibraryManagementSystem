using System;
using System.Data;
using System.Data.OleDb;
using LibraryManagementSystem.Model;

namespace LibraryManagementSystem.ViewModel
{
    /// <summary>
    /// Data Access Layer for Author, Publisher, and Category operations
    /// </summary>
    
    // Author DB Operations
    public class AuthorDB : BaseDB
    {
        public AuthorsList GetAllAuthors()
        {
            string query = "SELECT * FROM Authors WHERE IsActive = True ORDER BY LastName, FirstName";
            DataTable dt = ExecuteQuery(query);
            AuthorsList authors = new AuthorsList();
            foreach (DataRow row in dt.Rows)
            {
                authors.Add(new Author
                {
                    AuthorID = Convert.ToInt32(row["AuthorID"]),
                    FirstName = row["FirstName"]?.ToString(),
                    LastName = row["LastName"]?.ToString(),
                    Biography = row["Biography"]?.ToString(),
                    Nationality = row["Nationality"]?.ToString(),
                    DateOfBirth = row["DateOfBirth"] != DBNull.Value ? Convert.ToDateTime(row["DateOfBirth"]) : (DateTime?)null,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                    IsActive = Convert.ToBoolean(row["IsActive"])
                });
            }
            return authors;
        }

        public bool InsertAuthor(Author author)
        {
            string query = "INSERT INTO Authors (FirstName, LastName, Biography, Nationality, DateOfBirth, CreatedAt, IsActive) VALUES (?, ?, ?, ?, ?, ?, ?)";
            return ExecuteNonQuery(query,
                new OleDbParameter("@FirstName", author.FirstName ?? (object)DBNull.Value),
                new OleDbParameter("@LastName", author.LastName ?? (object)DBNull.Value),
                new OleDbParameter("@Biography", author.Biography ?? (object)DBNull.Value),
                new OleDbParameter("@Nationality", author.Nationality ?? (object)DBNull.Value),
                new OleDbParameter("@DateOfBirth", author.DateOfBirth ?? (object)DBNull.Value),
                new OleDbParameter("@CreatedAt", author.CreatedAt),
                new OleDbParameter("@IsActive", author.IsActive)
            ) > 0;
        }

        public bool UpdateAuthor(Author author)
        {
            string query = "UPDATE Authors SET FirstName=?, LastName=?, Biography=?, Nationality=?, DateOfBirth=?, UpdatedAt=? WHERE AuthorID=?";
            return ExecuteNonQuery(query,
                new OleDbParameter("@FirstName", author.FirstName ?? (object)DBNull.Value),
                new OleDbParameter("@LastName", author.LastName ?? (object)DBNull.Value),
                new OleDbParameter("@Biography", author.Biography ?? (object)DBNull.Value),
                new OleDbParameter("@Nationality", author.Nationality ?? (object)DBNull.Value),
                new OleDbParameter("@DateOfBirth", author.DateOfBirth ?? (object)DBNull.Value),
                new OleDbParameter("@UpdatedAt", DateTime.Now),
                new OleDbParameter("@AuthorID", author.AuthorID)
            ) > 0;
        }
    }

    // Publisher DB Operations
    public class PublisherDB : BaseDB
    {
        /// <summary>
        /// Gets all non-deleted publishers from the database
        /// </summary>
        public DataTable GetAllPublishers()
        {
            try
            {
                string query = "SELECT publisher_id, name, country, website, founded_year FROM publishers WHERE is_deleted = False OR is_deleted IS NULL ORDER BY name";
                return ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get publishers: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a single non-deleted publisher by ID
        /// </summary>
        public DataRow GetPublisherById(string publisherId)
        {
            try
            {
                string query = "SELECT publisher_id, name, country, website, founded_year FROM publishers WHERE publisher_id = ? AND (is_deleted = False OR is_deleted IS NULL)";
                DataTable dt = ExecuteQuery(query, 
                    new OleDbParameter("@PublisherID", OleDbType.VarChar, 36) { Value = publisherId });
                
                if (dt.Rows.Count > 0)
                    return dt.Rows[0];
                
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get publisher: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Inserts a new publisher
        /// </summary>
        public bool InsertPublisher(string name, string country, string website, int? foundedYear)
        {
            try
            {
                // Generate UUID for new publisher
                string publisherId = Guid.NewGuid().ToString();
                
                string query = "INSERT INTO publishers (publisher_id, name, country, website, founded_year, is_deleted) VALUES (?, ?, ?, ?, ?, ?)";
                return ExecuteNonQuery(query,
                    new OleDbParameter("@PublisherID", OleDbType.VarChar, 36) { Value = publisherId },
                    new OleDbParameter("@Name", OleDbType.VarChar, 100) { Value = name ?? (object)DBNull.Value },
                    new OleDbParameter("@Country", OleDbType.VarChar, 50) { Value = country ?? (object)DBNull.Value },
                    new OleDbParameter("@Website", OleDbType.VarChar, 200) { Value = website ?? (object)DBNull.Value },
                    new OleDbParameter("@FoundedYear", OleDbType.Integer) { Value = foundedYear.HasValue ? (object)foundedYear.Value : DBNull.Value },
                    new OleDbParameter("@IsDeleted", OleDbType.Boolean) { Value = false }
                ) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to insert publisher: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates an existing publisher
        /// </summary>
        public bool UpdatePublisher(string publisherId, string name, string country, string website, int? foundedYear)
        {
            try
            {
                string query = "UPDATE publishers SET name = ?, country = ?, website = ?, founded_year = ? WHERE publisher_id = ?";
                return ExecuteNonQuery(query,
                    new OleDbParameter("@Name", OleDbType.VarChar, 100) { Value = name ?? (object)DBNull.Value },
                    new OleDbParameter("@Country", OleDbType.VarChar, 50) { Value = country ?? (object)DBNull.Value },
                    new OleDbParameter("@Website", OleDbType.VarChar, 200) { Value = website ?? (object)DBNull.Value },
                    new OleDbParameter("@FoundedYear", OleDbType.Integer) { Value = foundedYear.HasValue ? (object)foundedYear.Value : DBNull.Value },
                    new OleDbParameter("@PublisherID", OleDbType.VarChar, 36) { Value = publisherId }
                ) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update publisher: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Soft deletes a publisher (sets is_deleted to True)
        /// </summary>
        public bool DeletePublisher(string publisherId)
        {
            try
            {
                string query = "UPDATE publishers SET is_deleted = True WHERE publisher_id = ?";
                return ExecuteNonQuery(query,
                    new OleDbParameter("@PublisherID", OleDbType.VarChar, 36) { Value = publisherId }
                ) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete publisher: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the count of books from a specific publisher
        /// </summary>
        public int GetPublisherBookCount(string publisherId)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM books WHERE publisher_id = ?";
                DataTable dt = ExecuteQuery(query, 
                    new OleDbParameter("@PublisherID", OleDbType.VarChar, 36) { Value = publisherId });
                
                if (dt.Rows.Count > 0)
                    return Convert.ToInt32(dt.Rows[0][0]);
                
                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get publisher book count: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if a non-deleted publisher name already exists (for validation)
        /// </summary>
        public bool PublisherNameExists(string name, string excludePublisherId = null)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM publishers WHERE LCase(name) = ? AND (is_deleted = False OR is_deleted IS NULL)";
                var parameters = new OleDbParameter[] { new OleDbParameter("@Name", name.ToLower()) };
                
                if (!string.IsNullOrEmpty(excludePublisherId))
                {
                    query += " AND publisher_id <> ?";
                    parameters = new OleDbParameter[] 
                    { 
                        new OleDbParameter("@Name", name.ToLower()),
                        new OleDbParameter("@PublisherID", excludePublisherId)
                    };
                }
                
                DataTable dt = ExecuteQuery(query, parameters);
                
                if (dt.Rows.Count > 0)
                    return Convert.ToInt32(dt.Rows[0][0]) > 0;
                
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to check publisher name: {ex.Message}", ex);
            }
        }
    }

    // Category DB Operations
    public class CategoryDB : BaseDB
    {
        /// <summary>
        /// Gets all non-deleted categories from the database
        /// </summary>
        public DataTable GetAllCategories()
        {
            try
            {
                string query = "SELECT category_id, name, description FROM categories WHERE is_deleted = False OR is_deleted IS NULL ORDER BY name";
                return ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get categories: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a single non-deleted category by ID
        /// </summary>
        public DataRow GetCategoryById(string categoryId)
        {
            try
            {
                string query = "SELECT category_id, name, description FROM categories WHERE category_id = ? AND (is_deleted = False OR is_deleted IS NULL)";
                DataTable dt = ExecuteQuery(query, 
                    new OleDbParameter("@CategoryID", OleDbType.VarChar, 36) { Value = categoryId });
                
                if (dt.Rows.Count > 0)
                    return dt.Rows[0];
                
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get category: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Inserts a new category
        /// </summary>
        public bool InsertCategory(string name, string description)
        {
            try
            {
                // Generate UUID for new category
                string categoryId = Guid.NewGuid().ToString();
                
                string query = "INSERT INTO categories (category_id, name, description, is_deleted) VALUES (?, ?, ?, ?)";
                return ExecuteNonQuery(query,
                    new OleDbParameter("@CategoryID", OleDbType.VarChar, 36) { Value = categoryId },
                    new OleDbParameter("@Name", OleDbType.VarChar, 50) { Value = name ?? (object)DBNull.Value },
                    new OleDbParameter("@Description", OleDbType.LongVarChar) { Value = description ?? (object)DBNull.Value },
                    new OleDbParameter("@IsDeleted", OleDbType.Boolean) { Value = false }
                ) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to insert category: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates an existing category
        /// </summary>
        public bool UpdateCategory(string categoryId, string name, string description)
        {
            try
            {
                string query = "UPDATE categories SET name = ?, description = ? WHERE category_id = ?";
                return ExecuteNonQuery(query,
                    new OleDbParameter("@Name", OleDbType.VarChar, 50) { Value = name ?? (object)DBNull.Value },
                    new OleDbParameter("@Description", OleDbType.LongVarChar) { Value = description ?? (object)DBNull.Value },
                    new OleDbParameter("@CategoryID", OleDbType.VarChar, 36) { Value = categoryId }
                ) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update category: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Soft deletes a category (sets is_deleted to True)
        /// </summary>
        public bool DeleteCategory(string categoryId)
        {
            try
            {
                string query = "UPDATE categories SET is_deleted = True WHERE category_id = ?";
                return ExecuteNonQuery(query,
                    new OleDbParameter("@CategoryID", OleDbType.VarChar, 36) { Value = categoryId }
                ) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete category: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the count of books in a specific category
        /// </summary>
        public int GetCategoryBookCount(string categoryId)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM books WHERE category_id = ?";
                DataTable dt = ExecuteQuery(query, 
                    new OleDbParameter("@CategoryID", OleDbType.VarChar, 36) { Value = categoryId });
                
                if (dt.Rows.Count > 0)
                    return Convert.ToInt32(dt.Rows[0][0]);
                
                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get category book count: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if a non-deleted category name already exists (for validation)
        /// </summary>
        public bool CategoryNameExists(string name, string excludeCategoryId = null)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM categories WHERE LCase(name) = ? AND (is_deleted = False OR is_deleted IS NULL)";
                var parameters = new OleDbParameter[] { new OleDbParameter("@Name", name.ToLower()) };
                
                if (!string.IsNullOrEmpty(excludeCategoryId))
                {
                    query += " AND category_id <> ?";
                    parameters = new OleDbParameter[] 
                    { 
                        new OleDbParameter("@Name", name.ToLower()),
                        new OleDbParameter("@CategoryID", excludeCategoryId)
                    };
                }
                
                DataTable dt = ExecuteQuery(query, parameters);
                
                if (dt.Rows.Count > 0)
                    return Convert.ToInt32(dt.Rows[0][0]) > 0;
                
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to check category name: {ex.Message}", ex);
            }
        }
    }
}
