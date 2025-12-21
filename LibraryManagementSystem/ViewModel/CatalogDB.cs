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
        public PublishersList GetAllPublishers()
        {
            string query = "SELECT * FROM Publishers WHERE IsActive = True ORDER BY Name";
            DataTable dt = ExecuteQuery(query);
            PublishersList publishers = new PublishersList();
            foreach (DataRow row in dt.Rows)
            {
                publishers.Add(new Publisher
                {
                    PublisherID = Convert.ToInt32(row["PublisherID"]),
                    Name = row["Name"]?.ToString(),
                    Address = row["Address"]?.ToString(),
                    Phone = row["Phone"]?.ToString(),
                    Email = row["Email"]?.ToString(),
                    Website = row["Website"]?.ToString(),
                    Country = row["Country"]?.ToString(),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                    IsActive = Convert.ToBoolean(row["IsActive"])
                });
            }
            return publishers;
        }

        public bool InsertPublisher(Publisher publisher)
        {
            string query = "INSERT INTO Publishers (Name, Address, Phone, Email, Website, Country, CreatedAt, IsActive) VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
            return ExecuteNonQuery(query,
                new OleDbParameter("@Name", publisher.Name ?? (object)DBNull.Value),
                new OleDbParameter("@Address", publisher.Address ?? (object)DBNull.Value),
                new OleDbParameter("@Phone", publisher.Phone ?? (object)DBNull.Value),
                new OleDbParameter("@Email", publisher.Email ?? (object)DBNull.Value),
                new OleDbParameter("@Website", publisher.Website ?? (object)DBNull.Value),
                new OleDbParameter("@Country", publisher.Country ?? (object)DBNull.Value),
                new OleDbParameter("@CreatedAt", publisher.CreatedAt),
                new OleDbParameter("@IsActive", publisher.IsActive)
            ) > 0;
        }
    }

    // Category DB Operations
    public class CategoryDB : BaseDB
    {
        /// <summary>
        /// Gets all categories from the database
        /// </summary>
        public DataTable GetAllCategories()
        {
            try
            {
                string query = "SELECT category_id, name, description FROM categories ORDER BY name";
                return ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get categories: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a single category by ID
        /// </summary>
        public DataRow GetCategoryById(string categoryId)
        {
            try
            {
                string query = "SELECT category_id, name, description FROM categories WHERE category_id = ?";
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
                
                string query = "INSERT INTO categories (category_id, name, description) VALUES (?, ?, ?)";
                return ExecuteNonQuery(query,
                    new OleDbParameter("@CategoryID", OleDbType.VarChar, 36) { Value = categoryId },
                    new OleDbParameter("@Name", OleDbType.VarChar, 50) { Value = name ?? (object)DBNull.Value },
                    new OleDbParameter("@Description", OleDbType.LongVarChar) { Value = description ?? (object)DBNull.Value }
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
        /// Deletes a category
        /// </summary>
        public bool DeleteCategory(string categoryId)
        {
            try
            {
                string query = "DELETE FROM categories WHERE category_id = ?";
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
        /// Checks if a category name already exists (for validation)
        /// </summary>
        public bool CategoryNameExists(string name, string excludeCategoryId = null)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM categories WHERE LOWER(name) = ?";
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
