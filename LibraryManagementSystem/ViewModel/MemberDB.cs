using System;
using System.Data;
using System.Data.OleDb;
using LibraryManagementSystem.Model;

namespace LibraryManagementSystem.ViewModel
{
    /// <summary>
    /// Data Access Layer for Member operations
    /// </summary>
    public class MemberDB : BaseDB
    {
        /// <summary>
        /// Gets a member by member ID
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <returns>Member object or null</returns>
        public Member GetMemberByID(int memberId)
        {
            try
            {
                string query = @"SELECT M.*, U.* FROM Members M 
                                INNER JOIN Users U ON M.UserID = U.UserID 
                                WHERE M.MemberID = ?";
                
                OleDbParameter param = new OleDbParameter("@MemberID", memberId);
                DataTable dt = ExecuteQuery(query, param);
                
                if (dt.Rows.Count > 0)
                {
                    return MapRowToMember(dt.Rows[0]);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get member: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a member by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Member object or null</returns>
        public Member GetMemberByUserID(int userId)
        {
            try
            {
                string query = @"SELECT M.*, U.* FROM Members M 
                                INNER JOIN Users U ON M.UserID = U.UserID 
                                WHERE M.UserID = ?";
                
                OleDbParameter param = new OleDbParameter("@UserID", userId);
                DataTable dt = ExecuteQuery(query, param);
                
                if (dt.Rows.Count > 0)
                {
                    return MapRowToMember(dt.Rows[0]);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get member by user ID: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Searches members by name (first name or last name)
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>DataTable with member and user information</returns>
        public DataTable SearchMembers(string searchTerm)
        {
            try
            {
                string query = @"
                    SELECT 
                        m.member_id,
                        m.user_id,
                        m.membership_status,
                        u.first_name,
                        u.last_name,
                        u.email
                    FROM members m
                    INNER JOIN users u ON m.user_id = u.user_id
                    WHERE (u.first_name LIKE ? OR u.last_name LIKE ?)
                    AND m.membership_status = 'ACTIVE'
                    AND u.is_active = True
                    ORDER BY u.last_name, u.first_name";
                
                string searchPattern = $"%{searchTerm}%";
                OleDbParameter[] parameters = {
                    new OleDbParameter("@SearchTerm1", OleDbType.VarChar, 100) { Value = searchPattern },
                    new OleDbParameter("@SearchTerm2", OleDbType.VarChar, 100) { Value = searchPattern }
                };
                
                return ExecuteQuery(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to search members: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all active members for lending (with user info)
        /// </summary>
        /// <returns>DataTable with member and user information</returns>
        public DataTable GetAllActiveMembersForLending()
        {
            try
            {
                string query = @"
                    SELECT 
                        m.member_id,
                        m.user_id,
                        m.membership_status,
                        u.first_name,
                        u.last_name,
                        u.email
                    FROM members m
                    INNER JOIN users u ON m.user_id = u.user_id
                    WHERE m.membership_status = 'ACTIVE'
                    AND u.is_active = True
                    ORDER BY u.last_name, u.first_name";
                
                return ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get active members: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all non-deleted members with their details for management page
        /// </summary>
        /// <returns>DataTable with member information</returns>
        public DataTable GetAllMembersWithDetails()
        {
            try
            {
                string query = @"
                    SELECT 
                        m.member_id,
                        u.first_name & ' ' & u.last_name AS full_name,
                        u.email,
                        u.phone,
                        m.membership_date,
                        m.membership_status
                    FROM members m
                    INNER JOIN users u ON m.user_id = u.user_id
                    WHERE m.membership_status <> 'DELETED'
                    ORDER BY u.last_name, u.first_name";
                
                return ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get members with details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets member statistics (total, active, suspended) excluding DELETED members
        /// </summary>
        /// <returns>DataTable with count statistics</returns>
        public DataTable GetMemberStatistics()
        {
            try
            {
                string query = @"
                    SELECT 
                        COUNT(*) AS TotalMembers,
                        SUM(IIF(m.membership_status = 'ACTIVE', 1, 0)) AS ActiveMembers,
                        SUM(IIF(m.membership_status = 'SUSPENDED', 1, 0)) AS SuspendedMembers
                    FROM members m
                    WHERE m.membership_status <> 'DELETED'";
                
                return ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get member statistics: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets detailed member information by member ID
        /// </summary>
        /// <param name="memberId">Member ID (GUID string)</param>
        /// <returns>DataTable with detailed member information</returns>
        public DataTable GetMemberDetailsById(string memberId)
        {
            try
            {
                string query = @"
                    SELECT 
                        m.member_id,
                        m.user_id,
                        m.membership_date,
                        m.membership_status,
                        u.first_name,
                        u.last_name,
                        u.email,
                        u.phone,
                        u.address
                    FROM members m
                    INNER JOIN users u ON m.user_id = u.user_id
                    WHERE m.member_id = ?";
                
                OleDbParameter param = new OleDbParameter("@MemberID", OleDbType.VarChar, 36) { Value = memberId };
                return ExecuteQuery(query, param);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get member details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if a user is an admin (has ADMIN role)
        /// </summary>
        /// <param name="userId">User ID (GUID string)</param>
        /// <returns>True if user has ADMIN role</returns>
        public bool IsUserAdmin(string userId)
        {
            try
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM user_roles 
                    WHERE user_id = ? AND role = 'ADMIN'";
                
                OleDbParameter param = new OleDbParameter("@UserID", OleDbType.VarChar, 36) { Value = userId };
                object result = ExecuteScalar(query, param);
                
                return result != null && Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to check admin status: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates member information (for admin use)
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="email">Email</param>
        /// <param name="phone">Phone</param>
        /// <param name="address">Address</param>
        /// <param name="membershipStatus">Membership status</param>
        /// <returns>True if successful</returns>
        public bool UpdateMemberInformation(string memberId, string firstName, string lastName, 
                                            string email, string phone, string address, string membershipStatus)
        {
            try
            {
                // First, get the user_id for this member
                string getUserQuery = "SELECT user_id FROM members WHERE member_id = ?";
                OleDbParameter getUserParam = new OleDbParameter("@MemberID", OleDbType.VarChar, 36) { Value = memberId };
                object userIdObj = ExecuteScalar(getUserQuery, getUserParam);
                
                if (userIdObj == null)
                {
                    return false;
                }
                
                string userId = userIdObj.ToString();
                
                // Update user information
                string updateUserQuery = @"
                    UPDATE users 
                    SET first_name = ?, last_name = ?, email = ?, phone = ?, address = ?
                    WHERE user_id = ?";
                
                OleDbParameter[] userParams = {
                    new OleDbParameter("@FirstName", OleDbType.VarChar, 50) { Value = firstName },
                    new OleDbParameter("@LastName", OleDbType.VarChar, 50) { Value = lastName },
                    new OleDbParameter("@Email", OleDbType.VarChar, 100) { Value = email },
                    new OleDbParameter("@Phone", OleDbType.VarChar, 20) { Value = phone ?? (object)DBNull.Value },
                    new OleDbParameter("@Address", OleDbType.LongVarChar) { Value = address ?? (object)DBNull.Value },
                    new OleDbParameter("@UserID", OleDbType.VarChar, 36) { Value = userId }
                };
                
                int userResult = ExecuteNonQuery(updateUserQuery, userParams);
                
                // Update member status
                string updateMemberQuery = @"
                    UPDATE members 
                    SET membership_status = ?
                    WHERE member_id = ?";
                
                OleDbParameter[] memberParams = {
                    new OleDbParameter("@Status", OleDbType.VarChar, 20) { Value = membershipStatus },
                    new OleDbParameter("@MemberID", OleDbType.VarChar, 36) { Value = memberId }
                };
                
                int memberResult = ExecuteNonQuery(updateMemberQuery, memberParams);
                
                return userResult > 0 && memberResult > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update member information: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Toggles member status between ACTIVE and SUSPENDED
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <param name="newStatus">New status (ACTIVE or SUSPENDED)</param>
        /// <returns>True if successful</returns>
        public bool UpdateMemberStatus(string memberId, string newStatus)
        {
            try
            {
                string query = @"
                    UPDATE members 
                    SET membership_status = ?
                    WHERE member_id = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@Status", OleDbType.VarChar, 20) { Value = newStatus },
                    new OleDbParameter("@MemberID", OleDbType.VarChar, 36) { Value = memberId }
                };
                
                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update member status: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Soft deletes a member by setting their status to 'DELETED'
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <returns>True if successful</returns>
        public bool DeleteMember(string memberId)
        {
            try
            {
                // Soft delete by changing status to DELETED
                string query = @"
                    UPDATE members 
                    SET membership_status = 'DELETED'
                    WHERE member_id = ?";
                
                OleDbParameter param = new OleDbParameter("@MemberID", OleDbType.VarChar, 36) { Value = memberId };
                
                return ExecuteNonQuery(query, param) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete member: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets member count for validation before deletion
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <returns>Count of active loans and reservations</returns>
        public DataTable GetMemberActivityCount(string memberId)
        {
            try
            {
                string query = @"
                    SELECT 
                        (SELECT COUNT(*) FROM loans WHERE member_id = ? AND return_date IS NULL) AS ActiveLoans,
                        (SELECT COUNT(*) FROM reservations WHERE member_id = ? AND reservation_status = 'ACTIVE') AS ActiveReservations";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@MemberID1", OleDbType.VarChar, 36) { Value = memberId },
                    new OleDbParameter("@MemberID2", OleDbType.VarChar, 36) { Value = memberId }
                };
                
                return ExecuteQuery(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get member activity count: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if a member is suspended by user ID
        /// </summary>
        /// <param name="userId">User ID (GUID string)</param>
        /// <returns>True if member is SUSPENDED, false if ACTIVE or not found</returns>
        public bool IsMemberSuspended(string userId)
        {
            try
            {
                string query = @"
                    SELECT membership_status
                    FROM members
                    WHERE user_id = ?";
                
                OleDbParameter param = new OleDbParameter("@UserID", OleDbType.VarChar, 36) { Value = userId };
                object result = ExecuteScalar(query, param);
                
                if (result != null)
                {
                    string status = result.ToString();
                    return status.Equals("SUSPENDED", StringComparison.OrdinalIgnoreCase);
                }
                
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to check member suspension status: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets member status by user ID
        /// </summary>
        /// <param name="userId">User ID (GUID string)</param>
        /// <returns>Membership status (ACTIVE, SUSPENDED, etc.) or null if not found</returns>
        public string GetMemberStatusByUserId(string userId)
        {
            try
            {
                string query = @"
                    SELECT membership_status
                    FROM members
                    WHERE user_id = ?";
                
                OleDbParameter param = new OleDbParameter("@UserID", OleDbType.VarChar, 36) { Value = userId };
                object result = ExecuteScalar(query, param);
                
                return result?.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get member status: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all members
        /// </summary>
        /// <returns>List of all members</returns>
        public MembersList GetAllMembers()
        {
            try
            {
                string query = @"SELECT M.*, U.* FROM Members M 
                                INNER JOIN Users U ON M.UserID = U.UserID 
                                ORDER BY U.LastName, U.FirstName";
                
                DataTable dt = ExecuteQuery(query);
                return MapDataTableToMembersList(dt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get members: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all active members
        /// </summary>
        /// <returns>List of active members</returns>
        public MembersList GetActiveMembers()
        {
            try
            {
                string query = @"SELECT M.*, U.* FROM Members M 
                                INNER JOIN Users U ON M.UserID = U.UserID 
                                WHERE M.Status = 'Active' AND U.IsActive = True 
                                ORDER BY U.LastName, U.FirstName";
                
                DataTable dt = ExecuteQuery(query);
                return MapDataTableToMembersList(dt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get active members: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Inserts a new member
        /// </summary>
        /// <param name="member">Member object to insert</param>
        /// <param name="userId">Associated user ID</param>
        /// <returns>True if successful</returns>
        public bool InsertMember(Member member, int userId)
        {
            try
            {
                string query = @"INSERT INTO Members (UserID, MembershipDate, Status, 
                                ExpiryDate, FinesOwed, MaxBooksAllowed, CurrentBooksCount, 
                                CardNumber, CreatedAt, IsActive)
                                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                OleDbParameter[] parameters = {
                    new OleDbParameter("@UserID", userId),
                    new OleDbParameter("@MembershipDate", member.MembershipDate),
                    new OleDbParameter("@Status", member.Status ?? "Active"),
                    new OleDbParameter("@ExpiryDate", member.ExpiryDate ?? (object)DBNull.Value),
                    new OleDbParameter("@FinesOwed", member.FinesOwed),
                    new OleDbParameter("@MaxBooksAllowed", member.MaxBooksAllowed),
                    new OleDbParameter("@CurrentBooksCount", member.CurrentBooksCount),
                    new OleDbParameter("@CardNumber", member.CardNumber ?? (object)DBNull.Value),
                    new OleDbParameter("@CreatedAt", member.CreatedAt),
                    new OleDbParameter("@IsActive", member.IsActive)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to insert member: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates member information
        /// </summary>
        /// <param name="member">Member object with updated data</param>
        /// <returns>True if successful</returns>
        public bool UpdateMember(Member member)
        {
            try
            {
                string query = @"UPDATE Members 
                                SET Status = ?, ExpiryDate = ?, FinesOwed = ?, 
                                MaxBooksAllowed = ?, CurrentBooksCount = ?, 
                                CardNumber = ?, UpdatedAt = ? 
                                WHERE MemberID = ?";

                OleDbParameter[] parameters = {
                    new OleDbParameter("@Status", member.Status ?? "Active"),
                    new OleDbParameter("@ExpiryDate", member.ExpiryDate ?? (object)DBNull.Value),
                    new OleDbParameter("@FinesOwed", member.FinesOwed),
                    new OleDbParameter("@MaxBooksAllowed", member.MaxBooksAllowed),
                    new OleDbParameter("@CurrentBooksCount", member.CurrentBooksCount),
                    new OleDbParameter("@CardNumber", member.CardNumber ?? (object)DBNull.Value),
                    new OleDbParameter("@UpdatedAt", DateTime.Now),
                    new OleDbParameter("@MemberID", member.MemberID)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update member: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates member's fine amount
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <param name="fineAmount">New fine amount</param>
        /// <returns>True if successful</returns>
        public bool UpdateMemberFines(int memberId, decimal fineAmount)
        {
            try
            {
                string query = @"UPDATE Members 
                                SET FinesOwed = ?, UpdatedAt = ? 
                                WHERE MemberID = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@FinesOwed", fineAmount),
                    new OleDbParameter("@UpdatedAt", DateTime.Now),
                    new OleDbParameter("@MemberID", memberId)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update member fines: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates member's current books count
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <param name="increment">Amount to increment (positive) or decrement (negative)</param>
        /// <returns>True if successful</returns>
        public bool UpdateCurrentBooksCount(int memberId, int increment)
        {
            try
            {
                string query = @"UPDATE Members 
                                SET CurrentBooksCount = CurrentBooksCount + ?, UpdatedAt = ? 
                                WHERE MemberID = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@Increment", increment),
                    new OleDbParameter("@UpdatedAt", DateTime.Now),
                    new OleDbParameter("@MemberID", memberId)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update current books count: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Maps a DataTable to a MembersList
        /// </summary>
        private MembersList MapDataTableToMembersList(DataTable dt)
        {
            MembersList members = new MembersList();
            foreach (DataRow row in dt.Rows)
            {
                members.Add(MapRowToMember(row));
            }
            return members;
        }

        /// <summary>
        /// Maps a DataRow to a Member object
        /// </summary>
        private Member MapRowToMember(DataRow row)
        {
            return new Member
            {
                MemberID = row["MemberID"] != DBNull.Value ? Convert.ToInt32(row["MemberID"]) : 0,
                UserID = row["UserID"] != DBNull.Value ? Convert.ToInt32(row["UserID"]) : 0,
                Email = row["Email"]?.ToString(),
                PasswordHash = row["PasswordHash"]?.ToString(),
                FirstName = row["FirstName"]?.ToString(),
                LastName = row["LastName"]?.ToString(),
                Phone = row["Phone"]?.ToString(),
                Role = row["Role"]?.ToString(),
                Address = row["Address"]?.ToString(),
                City = row["City"]?.ToString(),
                PostalCode = row["PostalCode"]?.ToString(),
                RegistrationDate = row["RegistrationDate"] != DBNull.Value ? Convert.ToDateTime(row["RegistrationDate"]) : DateTime.Now,
                MembershipDate = row["MembershipDate"] != DBNull.Value ? Convert.ToDateTime(row["MembershipDate"]) : DateTime.Now,
                Status = row["Status"]?.ToString() ?? "Active",
                ExpiryDate = row["ExpiryDate"] != DBNull.Value ? Convert.ToDateTime(row["ExpiryDate"]) : (DateTime?)null,
                FinesOwed = row["FinesOwed"] != DBNull.Value ? Convert.ToDecimal(row["FinesOwed"]) : 0,
                MaxBooksAllowed = row["MaxBooksAllowed"] != DBNull.Value ? Convert.ToInt32(row["MaxBooksAllowed"]) : 3,
                CurrentBooksCount = row["CurrentBooksCount"] != DBNull.Value ? Convert.ToInt32(row["CurrentBooksCount"]) : 0,
                CardNumber = row["CardNumber"]?.ToString(),
                CreatedAt = row["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(row["CreatedAt"]) : DateTime.Now,
                UpdatedAt = row["UpdatedAt"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedAt"]) : (DateTime?)null,
                IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"])
            };
        }

        /// <summary>
        /// Gets librarian status by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Librarian status (ACTIVE, SUSPENDED, etc.) or null if not found</returns>
        public string GetLibrarianStatusByUserId(string userId)
        {
            try
            {
                string query = @"
                    SELECT librarian_status
                    FROM librarians
                    WHERE user_id = ?";
                
                OleDbParameter param = new OleDbParameter("@UserID", OleDbType.VarChar, 36) { Value = userId };
                object result = ExecuteScalar(query, param);
                
                return result?.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get librarian status: {ex.Message}", ex);
            }
        }
    }
}
