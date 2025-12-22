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

        /// <summary>
        /// Gets librarian statistics (total, active, suspended) excluding DELETED
        /// Users with LIBRARIAN role but NOT ADMIN role
        /// </summary>
        public DataTable GetLibrarianStatistics()
        {
            try
            {
                // Get all user_ids that have LIBRARIAN role
                string librarianUsersQuery = "SELECT DISTINCT user_id FROM user_roles WHERE role = 'LIBRARIAN'";
                DataTable librarianUsers = ExecuteQuery(librarianUsersQuery);
                
                // Get all user_ids that have ADMIN role
                string adminUsersQuery = "SELECT DISTINCT user_id FROM user_roles WHERE role = 'ADMIN'";
                DataTable adminUsers = ExecuteQuery(adminUsersQuery);
                
                // Build list of admin user IDs to exclude
                var adminUserIds = new System.Collections.Generic.HashSet<string>();
                foreach (DataRow row in adminUsers.Rows)
                {
                    adminUserIds.Add(row["user_id"].ToString());
                }
                
                // Count librarians who are NOT admins
                int totalLibrarians = 0;
                int activeLibrarians = 0;
                int suspendedLibrarians = 0;
                
                foreach (DataRow row in librarianUsers.Rows)
                {
                    string userId = row["user_id"].ToString();
                    
                    // Skip if this user is also an admin
                    if (adminUserIds.Contains(userId))
                        continue;
                    
                    // Check if user has entry in librarians table and get their status
                    string statusQuery = "SELECT librarian_status FROM librarians WHERE user_id = ?";
                    DataTable statusDt = ExecuteQuery(statusQuery, new OleDbParameter("@UserId", userId));
                    
                    if (statusDt.Rows.Count > 0)
                    {
                        string status = statusDt.Rows[0]["librarian_status"].ToString();
                        
                        if (status != "DELETED")
                        {
                            totalLibrarians++;
                            if (status == "ACTIVE") activeLibrarians++;
                            else if (status == "SUSPENDED") suspendedLibrarians++;
                        }
                    }
                }
                
                // Create result DataTable
                DataTable result = new DataTable();
                result.Columns.Add("TotalLibrarians", typeof(int));
                result.Columns.Add("ActiveLibrarians", typeof(int));
                result.Columns.Add("SuspendedLibrarians", typeof(int));
                result.Rows.Add(totalLibrarians, activeLibrarians, suspendedLibrarians);
                
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get librarian statistics: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all non-deleted librarians (non-admins) with their details
        /// Users with LIBRARIAN role but NOT ADMIN role
        /// </summary>
        public DataTable GetAllLibrariansWithDetails()
        {
            try
            {
                // Get all user_ids that have LIBRARIAN role
                string librarianUsersQuery = "SELECT DISTINCT user_id FROM user_roles WHERE role = 'LIBRARIAN'";
                DataTable librarianUsers = ExecuteQuery(librarianUsersQuery);
                
                // Get all user_ids that have ADMIN role
                string adminUsersQuery = "SELECT DISTINCT user_id FROM user_roles WHERE role = 'ADMIN'";
                DataTable adminUsers = ExecuteQuery(adminUsersQuery);
                
                // Build list of admin user IDs to exclude
                var adminUserIds = new System.Collections.Generic.HashSet<string>();
                foreach (DataRow row in adminUsers.Rows)
                {
                    adminUserIds.Add(row["user_id"].ToString());
                }
                
                // Create result DataTable
                DataTable result = new DataTable();
                result.Columns.Add("librarian_id", typeof(string));
                result.Columns.Add("employee_id", typeof(string));
                result.Columns.Add("full_name", typeof(string));
                result.Columns.Add("email", typeof(string));
                result.Columns.Add("hire_date", typeof(DateTime));
                result.Columns.Add("librarian_status", typeof(string));
                
                // Process each librarian
                foreach (DataRow row in librarianUsers.Rows)
                {
                    string userId = row["user_id"].ToString();
                    
                    // Skip if this user is also an admin
                    if (adminUserIds.Contains(userId))
                        continue;
                    
                    // Try to get librarian details from librarians table
                    string librarianDetailsQuery = @"
                        SELECT l.librarian_id, l.employee_id, l.hire_date, l.librarian_status
                        FROM librarians l
                        WHERE l.user_id = ?";
                    
                    DataTable librarianDt = ExecuteQuery(librarianDetailsQuery, new OleDbParameter("@UserId", userId));
                    
                    // Get user details
                    string userDetailsQuery = @"
                        SELECT u.first_name & ' ' & u.last_name AS full_name, u.email
                        FROM users u
                        WHERE u.user_id = ?";
                    
                    DataTable userDt = ExecuteQuery(userDetailsQuery, new OleDbParameter("@UserId", userId));
                    
                    if (userDt.Rows.Count > 0)
                    {
                        DataRow userRow = userDt.Rows[0];
                        
                        if (librarianDt.Rows.Count > 0)
                        {
                            // User has entry in librarians table
                            DataRow libRow = librarianDt.Rows[0];
                            string status = libRow["librarian_status"].ToString();
                            
                            // Only include if not DELETED
                            if (status != "DELETED")
                            {
                                result.Rows.Add(
                                    libRow["librarian_id"],
                                    libRow["employee_id"] != DBNull.Value ? libRow["employee_id"].ToString() : "",
                                    userRow["full_name"],
                                    userRow["email"],
                                    libRow["hire_date"],
                                    status
                                );
                            }
                        }
                        else
                        {
                            // User doesn't have entry in librarians table - use user data
                            result.Rows.Add(
                                userId, // Use user_id as librarian_id
                                "", // No employee_id
                                userRow["full_name"],
                                userRow["email"],
                                DateTime.Now, // Use current date as hire_date
                                "ACTIVE" // Default status
                            );
                        }
                    }
                }
                
                // Sort by full_name
                DataView dv = result.DefaultView;
                dv.Sort = "full_name ASC";
                return dv.ToTable();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get librarians: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets admin statistics (total, active, suspended) excluding DELETED
        /// Users with ADMIN role (regardless of other roles)
        /// </summary>
        public DataTable GetAdminStatistics()
        {
            try
            {
                // Get all user_ids that have ADMIN role
                string adminUsersQuery = "SELECT DISTINCT user_id FROM user_roles WHERE role = 'ADMIN'";
                DataTable adminUsers = ExecuteQuery(adminUsersQuery);
                
                // Count admins and their statuses
                int totalAdmins = 0;
                int activeAdmins = 0;
                int suspendedAdmins = 0;
                
                foreach (DataRow row in adminUsers.Rows)
                {
                    string userId = row["user_id"].ToString();
                    
                    // Check if user has entry in librarians table and get their status
                    string statusQuery = "SELECT librarian_status FROM librarians WHERE user_id = ?";
                    DataTable statusDt = ExecuteQuery(statusQuery, new OleDbParameter("@UserId", userId));
                    
                    if (statusDt.Rows.Count > 0)
                    {
                        string status = statusDt.Rows[0]["librarian_status"].ToString();
                        
                        if (status != "DELETED")
                        {
                            totalAdmins++;
                            if (status == "ACTIVE") activeAdmins++;
                            else if (status == "SUSPENDED") suspendedAdmins++;
                        }
                    }
                }
                
                // Create result DataTable
                DataTable result = new DataTable();
                result.Columns.Add("TotalAdmins", typeof(int));
                result.Columns.Add("ActiveAdmins", typeof(int));
                result.Columns.Add("SuspendedAdmins", typeof(int));
                result.Rows.Add(totalAdmins, activeAdmins, suspendedAdmins);
                
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get admin statistics: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all non-deleted admins with their details
        /// Users with ADMIN role (regardless of other roles)
        /// </summary>
        public DataTable GetAllAdminsWithDetails()
        {
            try
            {
                // Get all user_ids that have ADMIN role
                string adminUsersQuery = "SELECT DISTINCT user_id FROM user_roles WHERE role = 'ADMIN'";
                DataTable adminUsers = ExecuteQuery(adminUsersQuery);
                
                // Create result DataTable
                DataTable result = new DataTable();
                result.Columns.Add("librarian_id", typeof(string));
                result.Columns.Add("employee_id", typeof(string));
                result.Columns.Add("full_name", typeof(string));
                result.Columns.Add("email", typeof(string));
                result.Columns.Add("hire_date", typeof(DateTime));
                result.Columns.Add("librarian_status", typeof(string));
                
                // Process each admin
                foreach (DataRow row in adminUsers.Rows)
                {
                    string userId = row["user_id"].ToString();
                    
                    // Try to get librarian details from librarians table
                    string librarianDetailsQuery = @"
                        SELECT l.librarian_id, l.employee_id, l.hire_date, l.librarian_status
                        FROM librarians l
                        WHERE l.user_id = ?";
                    
                    DataTable librarianDt = ExecuteQuery(librarianDetailsQuery, new OleDbParameter("@UserId", userId));
                    
                    // Get user details
                    string userDetailsQuery = @"
                        SELECT u.first_name & ' ' & u.last_name AS full_name, u.email
                        FROM users u
                        WHERE u.user_id = ?";
                    
                    DataTable userDt = ExecuteQuery(userDetailsQuery, new OleDbParameter("@UserId", userId));
                    
                    if (userDt.Rows.Count > 0)
                    {
                        DataRow userRow = userDt.Rows[0];
                        
                        if (librarianDt.Rows.Count > 0)
                        {
                            // User has entry in librarians table
                            DataRow libRow = librarianDt.Rows[0];
                            string status = libRow["librarian_status"].ToString();
                            
                            // Only include if not DELETED
                            if (status != "DELETED")
                            {
                                result.Rows.Add(
                                    libRow["librarian_id"],
                                    libRow["employee_id"] != DBNull.Value ? libRow["employee_id"].ToString() : "",
                                    userRow["full_name"],
                                    userRow["email"],
                                    libRow["hire_date"],
                                    status
                                );
                            }
                        }
                        else
                        {
                            // User doesn't have entry in librarians table - use user data
                            result.Rows.Add(
                                userId, // Use user_id as librarian_id
                                "", // No employee_id
                                userRow["full_name"],
                                userRow["email"],
                                DateTime.Now, // Use current date as hire_date
                                "ACTIVE" // Default status
                            );
                        }
                    }
                }
                
                // Sort by full_name
                DataView dv = result.DefaultView;
                dv.Sort = "full_name ASC";
                return dv.ToTable();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get admins: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates librarian status (ACTIVE/SUSPENDED)
        /// </summary>
        /// <param name="userId">User ID (GUID string or librarian_id)</param>
        /// <param name="isActive">New active status</param>
        public bool UpdateUserStatus(string userId, bool isActive)
        {
            try
            {
                // First try to find user by librarian_id
                string findUserQuery = @"
                    SELECT u.user_id 
                    FROM users u 
                    LEFT JOIN librarians l ON u.user_id = l.user_id
                    WHERE u.user_id = ? OR l.librarian_id = ?";
                
                DataTable dt = ExecuteQuery(findUserQuery,
                    new OleDbParameter("@UserId1", userId),
                    new OleDbParameter("@UserId2", userId));
                
                if (dt.Rows.Count == 0)
                    return false;
                
                string actualUserId = dt.Rows[0]["user_id"].ToString();
                
                // Update librarian_status instead of is_active
                string query = "UPDATE librarians SET librarian_status = ? WHERE user_id = ?";
                string newStatus = isActive ? "ACTIVE" : "SUSPENDED";
                
                return ExecuteNonQuery(query,
                    new OleDbParameter("@Status", newStatus),
                    new OleDbParameter("@UserId", actualUserId)) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update user status: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Soft deletes a librarian by setting their status to 'DELETED'
        /// </summary>
        /// <param name="userId">User ID (GUID string or librarian_id)</param>
        /// <returns>True if successful</returns>
        public bool DeleteLibrarian(string userId)
        {
            try
            {
                // First try to find user by librarian_id
                string findUserQuery = @"
                    SELECT u.user_id 
                    FROM users u 
                    LEFT JOIN librarians l ON u.user_id = l.user_id
                    WHERE u.user_id = ? OR l.librarian_id = ?";
                
                DataTable dt = ExecuteQuery(findUserQuery,
                    new OleDbParameter("@UserId1", userId),
                    new OleDbParameter("@UserId2", userId));
                
                if (dt.Rows.Count == 0)
                    return false;
                
                string actualUserId = dt.Rows[0]["user_id"].ToString();
                
                // Soft delete by changing status to DELETED
                string query = "UPDATE librarians SET librarian_status = 'DELETED' WHERE user_id = ?";
                
                return ExecuteNonQuery(query,
                    new OleDbParameter("@UserId", actualUserId)) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete librarian: {ex.Message}", ex);
            }
        }
    }
}
