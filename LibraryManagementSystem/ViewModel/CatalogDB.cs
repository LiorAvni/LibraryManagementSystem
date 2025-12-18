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
        public CategoriesList GetAllCategories()
        {
            try
            {
                // Query actual database structure (no IsActive field)
                string query = "SELECT category_id, name, description FROM categories ORDER BY name";
                DataTable dt = ExecuteQuery(query);
                CategoriesList categories = new CategoriesList();
                
                foreach (DataRow row in dt.Rows)
                {
                    categories.Add(new Category
                    {
                        CategoryID = 0, // Not used, will use Name instead
                        Name = row["name"]?.ToString(),
                        Description = row["description"]?.ToString(),
                        ParentCategoryID = null,
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    });
                }
                
                return categories;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get categories: {ex.Message}", ex);
            }
        }

        public bool InsertCategory(Category category)
        {
            try
            {
                // Generate UUID for new category
                string categoryId = Guid.NewGuid().ToString();
                
                string query = "INSERT INTO categories (category_id, name, description) VALUES (?, ?, ?)";
                return ExecuteNonQuery(query,
                    new OleDbParameter("@CategoryID", OleDbType.VarChar, 36) { Value = categoryId },
                    new OleDbParameter("@Name", category.Name ?? (object)DBNull.Value),
                    new OleDbParameter("@Description", category.Description ?? (object)DBNull.Value)
                ) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to insert category: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Generates a new category ID as a UUID/GUID
        /// </summary>
        /// <returns>New UUID string</returns>
        private string GenerateNextCategoryId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
