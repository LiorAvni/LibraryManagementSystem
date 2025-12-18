using System;
using System.Data;
using System.Data.OleDb;
using LibraryManagementSystem.Model;

namespace LibraryManagementSystem.ViewModel
{
    /// <summary>
    /// Data Access Layer for User operations
    /// </summary>
    public class UserDB : BaseDB
    {
        /// <summary>
        /// Authenticates a user with email and password
        /// </summary>
        /// <param name="email">User's email</param>
        /// <param name="passwordHash">Hashed password</param>
        /// <returns>User object if successful, null otherwise</returns>
        public User Login(string email, string passwordHash)
        {
            try
            {
                string query = @"SELECT u.*, ur.role FROM users u 
                                LEFT JOIN user_roles ur ON u.user_id = ur.user_id
                                WHERE u.email = ? AND u.password_hash = ? AND u.is_active = True";

                OleDbParameter[] parameters = {
                    new OleDbParameter("@Email", email),
                    new OleDbParameter("@PasswordHash", passwordHash)
                };

                DataTable dt = ExecuteQuery(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    return MapRowToUser(dt.Rows[0]);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Login failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a user by email address
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>User object or null</returns>
        public User GetUserByEmail(string email)
        {
            try
            {
                string query = @"SELECT u.*, ur.role FROM users u 
                                LEFT JOIN user_roles ur ON u.user_id = ur.user_id
                                WHERE u.email = ?";
                OleDbParameter param = new OleDbParameter("@Email", email);
                
                DataTable dt = ExecuteQuery(query, param);
                
                if (dt.Rows.Count > 0)
                {
                    return MapRowToUser(dt.Rows[0]);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get user: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a user by ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User object or null</returns>
        public User GetUserByID(int userId)
        {
            try
            {
                string query = "SELECT * FROM Users WHERE UserID = ?";
                OleDbParameter param = new OleDbParameter("@UserID", userId);
                
                DataTable dt = ExecuteQuery(query, param);
                
                if (dt.Rows.Count > 0)
                {
                    return MapRowToUser(dt.Rows[0]);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get user: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns>List of all users</returns>
        public UsersList GetAllUsers()
        {
            try
            {
                string query = "SELECT * FROM Users ORDER BY LastName, FirstName";
                DataTable dt = ExecuteQuery(query);
                
                UsersList users = new UsersList();
                foreach (DataRow row in dt.Rows)
                {
                    users.Add(MapRowToUser(row));
                }
                return users;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get users: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Inserts a new user
        /// </summary>
        /// <param name="user">User object to insert</param>
        /// <returns>True if successful</returns>
        public bool InsertUser(User user)
        {
            try
            {
                string query = @"INSERT INTO Users (Email, PasswordHash, FirstName, LastName, 
                                Phone, Role, Address, City, PostalCode, RegistrationDate, 
                                CreatedAt, IsActive)
                                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                OleDbParameter[] parameters = {
                    new OleDbParameter("@Email", user.Email ?? (object)DBNull.Value),
                    new OleDbParameter("@PasswordHash", user.PasswordHash ?? (object)DBNull.Value),
                    new OleDbParameter("@FirstName", user.FirstName ?? (object)DBNull.Value),
                    new OleDbParameter("@LastName", user.LastName ?? (object)DBNull.Value),
                    new OleDbParameter("@Phone", user.Phone ?? (object)DBNull.Value),
                    new OleDbParameter("@Role", user.Role ?? (object)DBNull.Value),
                    new OleDbParameter("@Address", user.Address ?? (object)DBNull.Value),
                    new OleDbParameter("@City", user.City ?? (object)DBNull.Value),
                    new OleDbParameter("@PostalCode", user.PostalCode ?? (object)DBNull.Value),
                    new OleDbParameter("@RegistrationDate", user.RegistrationDate),
                    new OleDbParameter("@CreatedAt", user.CreatedAt),
                    new OleDbParameter("@IsActive", user.IsActive)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to insert user: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates an existing user
        /// </summary>
        /// <param name="user">User object with updated data</param>
        /// <returns>True if successful</returns>
        public bool UpdateUser(User user)
        {
            try
            {
                string query = @"UPDATE Users SET Email = ?, FirstName = ?, LastName = ?, 
                                Phone = ?, Role = ?, Address = ?, City = ?, PostalCode = ?, 
                                UpdatedAt = ?, IsActive = ?
                                WHERE UserID = ?";

                OleDbParameter[] parameters = {
                    new OleDbParameter("@Email", user.Email ?? (object)DBNull.Value),
                    new OleDbParameter("@FirstName", user.FirstName ?? (object)DBNull.Value),
                    new OleDbParameter("@LastName", user.LastName ?? (object)DBNull.Value),
                    new OleDbParameter("@Phone", user.Phone ?? (object)DBNull.Value),
                    new OleDbParameter("@Role", user.Role ?? (object)DBNull.Value),
                    new OleDbParameter("@Address", user.Address ?? (object)DBNull.Value),
                    new OleDbParameter("@City", user.City ?? (object)DBNull.Value),
                    new OleDbParameter("@PostalCode", user.PostalCode ?? (object)DBNull.Value),
                    new OleDbParameter("@UpdatedAt", DateTime.Now),
                    new OleDbParameter("@IsActive", user.IsActive),
                    new OleDbParameter("@UserID", user.UserID)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update user: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deletes a user (soft delete - sets IsActive to false)
        /// </summary>
        /// <param name="userId">User ID to delete</param>
        /// <returns>True if successful</returns>
        public bool DeleteUser(int userId)
        {
            try
            {
                string query = "UPDATE Users SET IsActive = False, UpdatedAt = ? WHERE UserID = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@UpdatedAt", DateTime.Now),
                    new OleDbParameter("@UserID", userId)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete user: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Maps a DataRow to a User object
        /// </summary>
        private User MapRowToUser(DataRow row)
        {
            // Check if role column exists and has a value
            string role = "Member"; // Default role
            if (row.Table.Columns.Contains("role") && row["role"] != DBNull.Value)
            {
                role = row["role"]?.ToString()?.Trim();
            }
            
            // If role is still null or empty, use default
            if (string.IsNullOrWhiteSpace(role))
            {
                role = "Member";
            }

            // Get the actual user_id from database (GUID string)
            string userIdString = row["user_id"]?.ToString() ?? "";

            return new User
            {
                UserID = Math.Abs(userIdString.GetHashCode()), // Legacy integer ID for compatibility
                UserIdString = userIdString, // Actual GUID from database
                Email = row["email"]?.ToString(),
                PasswordHash = row["password_hash"]?.ToString(),
                FirstName = row["first_name"]?.ToString(),
                LastName = row["last_name"]?.ToString(),
                Phone = row["phone"]?.ToString(),
                Role = role,
                Address = row["address"]?.ToString(),
                City = "", // Not in your schema
                PostalCode = "", // Not in your schema
                RegistrationDate = row["created_date"] != DBNull.Value 
                    ? Convert.ToDateTime(row["created_date"]) 
                    : DateTime.Now,
                CreatedAt = row["created_date"] != DBNull.Value 
                    ? Convert.ToDateTime(row["created_date"]) 
                    : DateTime.Now,
                UpdatedAt = null,
                IsActive = row["is_active"] != DBNull.Value && Convert.ToBoolean(row["is_active"])
            };
        }
    }
}
