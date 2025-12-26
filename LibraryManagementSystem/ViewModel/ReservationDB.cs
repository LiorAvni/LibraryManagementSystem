using System;
using System.Data;
using System.Data.OleDb;
using LibraryManagementSystem.Model;

namespace LibraryManagementSystem.ViewModel
{
    /// <summary>
    /// Data Access Layer for Reservation operations
    /// </summary>
    public class ReservationDB : BaseDB
    {
        /// <summary>
        /// Gets all reservations for management (librarian view)
        /// </summary>
        /// <returns>DataTable with all reservations</returns>
        public DataTable GetAllReservationsForManagement()
        {
            try
            {
                string query = @"
                    SELECT 
                        r.reservation_id,
                        r.book_id,
                        b.title AS BookTitle,
                        u.first_name & ' ' & u.last_name AS MemberName,
                        r.reservation_date,
                        r.expiry_date,
                        r.reservation_status
                    FROM ((reservations r
                    INNER JOIN books b ON r.book_id = b.book_id)
                    INNER JOIN members m ON r.member_id = m.member_id)
                    INNER JOIN users u ON m.user_id = u.user_id
                    WHERE r.reservation_status IN ('PENDING', 'RESERVED', 'CANCELLED', 'FULFILLED', 'EXPIRED')
                    ORDER BY r.reservation_date DESC";
                
                return ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get reservations for management: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets reservations for management page with filtering
        /// </summary>
        /// <param name="memberName">Filter by member name (optional)</param>
        /// <param name="bookTitle">Filter by book title (optional)</param>
        /// <param name="status">Filter by status: ALL, PENDING, RESERVED, CANCELLED, FULFILLED, EXPIRED</param>
        /// <returns>DataTable with reservation information</returns>
        public DataTable GetReservationsForManagement(string memberName, string bookTitle, string status)
        {
            try
            {
                // Build WHERE clause based on filters
                var whereConditions = new System.Collections.Generic.List<string>();
                var parameters = new System.Collections.Generic.List<OleDbParameter>();

                // Always include status filter to show relevant reservations
                whereConditions.Add("r.reservation_status IN ('PENDING', 'RESERVED', 'CANCELLED', 'FULFILLED', 'EXPIRED')");

                // Member name filter - search in first name, last name, or full name
                if (!string.IsNullOrEmpty(memberName))
                {
                    whereConditions.Add("(u.first_name LIKE ? OR u.last_name LIKE ?)");
                    string searchPattern = $"%{memberName}%";
                    parameters.Add(new OleDbParameter("@FirstName", OleDbType.VarChar, 100) { Value = searchPattern });
                    parameters.Add(new OleDbParameter("@LastName", OleDbType.VarChar, 100) { Value = searchPattern });
                }

                // Book title filter
                if (!string.IsNullOrEmpty(bookTitle))
                {
                    whereConditions.Add("b.title LIKE ?");
                    parameters.Add(new OleDbParameter("@BookTitle", OleDbType.VarChar, 200) { Value = $"%{bookTitle}%" });
                }

                // Status filter
                if (!string.IsNullOrEmpty(status) && status != "ALL")
                {
                    whereConditions.Add("r.reservation_status = ?");
                    parameters.Add(new OleDbParameter("@Status", OleDbType.VarChar, 20) { Value = status });
                }

                string whereClause = "WHERE " + string.Join(" AND ", whereConditions);

                // Build query
                string query = $@"
                    SELECT 
                        r.reservation_id,
                        r.book_id,
                        b.title AS BookTitle,
                        u.first_name & ' ' & u.last_name AS MemberName,
                        r.reservation_date,
                        r.expiry_date,
                        r.reservation_status
                    FROM ((reservations r
                    INNER JOIN books b ON r.book_id = b.book_id)
                    INNER JOIN members m ON r.member_id = m.member_id)
                    INNER JOIN users u ON m.user_id = u.user_id
                    {whereClause}
                    ORDER BY r.reservation_date DESC";

                return ExecuteQuery(query, parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get reservations for management: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Approves a reservation by assigning a copy and changing status
        /// </summary>
        /// <param name="reservationId">Reservation ID (GUID string)</param>
        /// <param name="bookId">Book ID to find available copy</param>
        /// <returns>True if successful</returns>
        public bool ApproveReservation(string reservationId, string bookId)
        {
            try
            {
                // STEP 1: Find an available copy for this book
                string findCopyQuery = "SELECT TOP 1 copy_id FROM book_copies WHERE book_id = ? AND status = 'AVAILABLE' ORDER BY copy_number";
                
                OleDbParameter findCopyParam = new OleDbParameter("?", OleDbType.VarChar, 36) { Value = bookId };
                object copyIdObj = ExecuteScalar(findCopyQuery, findCopyParam);
                
                if (copyIdObj == null)
                {
                    throw new Exception("No available copies found for this book.");
                }
                
                string copyId = copyIdObj.ToString();
                
                // STEP 2: Update ONLY the reservation status (simplified - no book_copy_id)
                string updateReservationQuery = "UPDATE reservations SET reservation_status = ? WHERE reservation_id = ?";
                
                OleDbParameter[] reservationParams = {
                    new OleDbParameter("?", OleDbType.VarChar, 20) { Value = "RESERVED" },
                    new OleDbParameter("?", OleDbType.VarChar, 36) { Value = reservationId }
                };
                
                int reservationResult = ExecuteNonQuery(updateReservationQuery, reservationParams);
                
                if (reservationResult > 0)
                {
                    // STEP 3: Update copy status to RESERVED
                    string updateCopyQuery = "UPDATE book_copies SET status = ? WHERE copy_id = ?";
                    OleDbParameter[] updateCopyParams = {
                        new OleDbParameter("?", OleDbType.VarChar, 20) { Value = "RESERVED" },
                        new OleDbParameter("?", OleDbType.VarChar, 36) { Value = copyId }
                    };
                    ExecuteNonQuery(updateCopyQuery, updateCopyParams);
                    
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to approve reservation: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cancels a reservation by librarian (releases copy back to AVAILABLE)
        /// </summary>
        /// <param name="reservationId">Reservation ID (GUID string)</param>
        /// <returns>True if successful</returns>
        public bool CancelReservationByLibrarian(string reservationId)
        {
            try
            {
                // Get the copy_id from reservation (if any)
                string getCopyQuery = "SELECT book_copy_id FROM reservations WHERE reservation_id = ?";
                OleDbParameter getCopyParam = new OleDbParameter("@ReservationID", OleDbType.VarChar, 36) { Value = reservationId };
                object copyIdObj = ExecuteScalar(getCopyQuery, getCopyParam);
                
                // Update reservation status to CANCELLED
                string updateReservationQuery = @"
                    UPDATE reservations 
                    SET reservation_status = ?
                    WHERE reservation_id = ?";
                
                OleDbParameter[] updateParams = {
                    new OleDbParameter("@Status", OleDbType.VarChar, 20) { Value = "CANCELLED" },
                    new OleDbParameter("@ReservationID", OleDbType.VarChar, 36) { Value = reservationId }
                };
                int result = ExecuteNonQuery(updateReservationQuery, updateParams);
                
                // If there was a copy assigned, release it back to AVAILABLE
                if (copyIdObj != null && copyIdObj != DBNull.Value)
                {
                    string copyId = copyIdObj.ToString();
                    string updateCopyQuery = "UPDATE book_copies SET status = ? WHERE copy_id = ?";
                    OleDbParameter[] updateCopyParams = {
                        new OleDbParameter("@Status", OleDbType.VarChar, 20) { Value = "AVAILABLE" },
                        new OleDbParameter("@CopyID", OleDbType.VarChar, 36) { Value = copyId }
                    };
                    ExecuteNonQuery(updateCopyQuery, updateCopyParams);
                }
                
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to cancel reservation: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Dis-approves a reservation (changes status from RESERVED back to PENDING and releases copy)
        /// </summary>
        /// <param name="reservationId">Reservation ID (GUID string)</param>
        /// <returns>True if successful</returns>
        public bool DisApproveReservation(string reservationId)
        {
            try
            {
                // STEP 1: Get the copy_id from reservation BEFORE updating
                string getCopyQuery = "SELECT book_copy_id FROM reservations WHERE reservation_id = ?";
                OleDbParameter getCopyParam = new OleDbParameter("?", OleDbType.VarChar, 36) { Value = reservationId };
                object copyIdObj = ExecuteScalar(getCopyQuery, getCopyParam);
                
                // STEP 2: Update reservation status to PENDING (simplified - just change status)
                string updateReservationQuery = "UPDATE reservations SET reservation_status = ? WHERE reservation_id = ?";
                
                OleDbParameter[] updateParams = {
                    new OleDbParameter("?", OleDbType.VarChar, 20) { Value = "PENDING" },
                    new OleDbParameter("?", OleDbType.VarChar, 36) { Value = reservationId }
                };
                int result = ExecuteNonQuery(updateReservationQuery, updateParams);
                
                // STEP 3: If there was a copy assigned, release it back to AVAILABLE
                if (copyIdObj != null && copyIdObj != DBNull.Value)
                {
                    string copyId = copyIdObj.ToString();
                    string updateCopyQuery = "UPDATE book_copies SET status = ? WHERE copy_id = ?";
                    OleDbParameter[] updateCopyParams = {
                        new OleDbParameter("?", OleDbType.VarChar, 20) { Value = "AVAILABLE" },
                        new OleDbParameter("?", OleDbType.VarChar, 36) { Value = copyId }
                    };
                    ExecuteNonQuery(updateCopyQuery, updateCopyParams);
                }
                
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to dis-approve reservation: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all reservations for a specific member with book details
        /// </summary>
        /// <param name="userId">User ID (from users table)</param>
        /// <returns>DataTable with reservation and book information</returns>
        public DataTable GetMemberReservationsWithDetails(string userId)
        {
            try
            {
                // First get reservations without authors to avoid duplicates
                string query = @"
                    SELECT 
                        r.reservation_id,
                        b.title AS BookTitle,
                        b.book_id,
                        r.reservation_date AS ReservationDate,
                        r.expiry_date AS ExpiryDate,
                        r.reservation_status AS Status
                    FROM (reservations r
                    INNER JOIN members m ON r.member_id = m.member_id)
                    INNER JOIN books b ON r.book_id = b.book_id
                    WHERE m.user_id = ? 
                    AND r.reservation_status IN ('PENDING', 'RESERVED')
                    ORDER BY r.reservation_date DESC";
                
                OleDbParameter param = new OleDbParameter("@UserID", OleDbType.VarChar, 36) { Value = userId };
                DataTable dt = ExecuteQuery(query, param);
                
                // Add Author column and populate it with all authors for each book
                if (!dt.Columns.Contains("Author"))
                    dt.Columns.Add("Author", typeof(string));
                
                foreach (DataRow row in dt.Rows)
                {
                    string bookId = row["book_id"].ToString();
                    
                    // Get all authors for this book
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
                        // Concatenate all author names with ", "
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
                }
                
                // Remove book_id column as it's not needed in the result
                dt.Columns.Remove("book_id");
                
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get reservations with details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all reservations for a specific member
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <returns>List of reservations</returns>
        public ReservationsList SelectReservationsByMember(int memberId)
        {
            try
            {
                string query = @"SELECT * FROM Reservations 
                                WHERE MemberID = ? 
                                ORDER BY ReservationDate DESC";
                
                OleDbParameter param = new OleDbParameter("@MemberID", memberId);
                DataTable dt = ExecuteQuery(query, param);
                
                return MapDataTableToReservationsList(dt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get reservations: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets pending reservations for a specific book
        /// </summary>
        /// <param name="bookId">Book ID</param>
        /// <returns>List of pending reservations</returns>
        public ReservationsList SelectPendingReservationsByBook(int bookId)
        {
            try
            {
                string query = @"SELECT * FROM Reservations 
                                WHERE BookID = ? AND Status = 'Pending' 
                                ORDER BY QueuePosition ASC, ReservationDate ASC";
                
                OleDbParameter param = new OleDbParameter("@BookID", bookId);
                DataTable dt = ExecuteQuery(query, param);
                
                return MapDataTableToReservationsList(dt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get pending reservations: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all ready reservations (books available for pickup)
        /// </summary>
        /// <returns>List of ready reservations</returns>
        public ReservationsList SelectReadyReservations()
        {
            try
            {
                string query = @"SELECT * FROM Reservations 
                                WHERE Status = 'Ready' 
                                ORDER BY AvailableDate ASC";
                
                DataTable dt = ExecuteQuery(query);
                return MapDataTableToReservationsList(dt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get ready reservations: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Inserts a new reservation
        /// </summary>
        /// <param name="reservation">Reservation object to insert</param>
        /// <returns>True if successful</returns>
        public bool InsertReservation(Reservation reservation)
        {
            try
            {
                string query = @"INSERT INTO Reservations (BookID, MemberID, ReservationDate, 
                                Status, QueuePosition, Notes, CreatedAt, IsActive)
                                VALUES (?, ?, ?, ?, ?, ?, ?, ?)";

                OleDbParameter[] parameters = {
                    new OleDbParameter("@BookID", reservation.BookID),
                    new OleDbParameter("@MemberID", reservation.MemberID),
                    new OleDbParameter("@ReservationDate", reservation.ReservationDate),
                    new OleDbParameter("@Status", reservation.Status ?? "Pending"),
                    new OleDbParameter("@QueuePosition", reservation.QueuePosition),
                    new OleDbParameter("@Notes", reservation.Notes ?? (object)DBNull.Value),
                    new OleDbParameter("@CreatedAt", reservation.CreatedAt),
                    new OleDbParameter("@IsActive", reservation.IsActive)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to insert reservation: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates reservation status
        /// </summary>
        /// <param name="reservationId">Reservation ID</param>
        /// <param name="status">New status</param>
        /// <returns>True if successful</returns>
        public bool UpdateReservationStatus(int reservationId, string status)
        {
            try
            {
                string query = @"UPDATE Reservations 
                                SET Status = ?, UpdatedAt = ? 
                                WHERE ReservationID = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@Status", status),
                    new OleDbParameter("@UpdatedAt", DateTime.Now),
                    new OleDbParameter("@ReservationID", reservationId)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update reservation status: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Marks a reservation as ready and assigns a copy
        /// </summary>
        /// <param name="reservationId">Reservation ID</param>
        /// <param name="copyId">Copy ID to assign</param>
        /// <returns>True if successful</returns>
        public bool MarkReservationReady(int reservationId, int copyId)
        {
            try
            {
                string query = @"UPDATE Reservations 
                                SET Status = 'Ready', AssignedCopyID = ?, AvailableDate = ?, 
                                ExpiryDate = ?, UpdatedAt = ? 
                                WHERE ReservationID = ?";
                
                DateTime availableDate = DateTime.Now;
                DateTime expiryDate = availableDate.AddDays(3); // 3 days to pick up
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@AssignedCopyID", copyId),
                    new OleDbParameter("@AvailableDate", availableDate),
                    new OleDbParameter("@ExpiryDate", expiryDate),
                    new OleDbParameter("@UpdatedAt", DateTime.Now),
                    new OleDbParameter("@ReservationID", reservationId)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to mark reservation ready: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Fulfills a reservation (member picked up the book)
        /// </summary>
        /// <param name="reservationId">Reservation ID</param>
        /// <returns>True if successful</returns>
        public bool FulfillReservation(int reservationId)
        {
            try
            {
                string query = @"UPDATE Reservations 
                                SET Status = 'Fulfilled', FulfilledDate = ?, UpdatedAt = ? 
                                WHERE ReservationID = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@FulfilledDate", DateTime.Now),
                    new OleDbParameter("@UpdatedAt", DateTime.Now),
                    new OleDbParameter("@ReservationID", reservationId)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to fulfill reservation: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cancels a reservation by DELETING it from the database
        /// (No copy status changes since reservations don't lock copies)
        /// </summary>
        /// <param name="reservationId">Reservation ID (GUID string)</param>
        /// <returns>True if successful</returns>
        public bool CancelReservation(string reservationId)
        {
            try
            {
                // DELETE the reservation from the database
                string deleteReservationQuery = @"
                    DELETE FROM reservations 
                    WHERE reservation_id = ?";
                
                OleDbParameter deleteParam = new OleDbParameter("@ReservationID", OleDbType.VarChar, 36) { Value = reservationId };
                int result = ExecuteNonQuery(deleteReservationQuery, deleteParam);
                
                // NOTE: We do NOT update any copy status because reservations don't lock copies
                // Copies are only assigned/locked when librarian processes the reservation
                
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to cancel reservation: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Cancels a reservation (legacy method for integer IDs)
        /// </summary>
        /// <param name="reservationId">Reservation ID</param>
        /// <returns>True if successful</returns>
        public bool CancelReservation(int reservationId)
        {
            try
            {
                return UpdateReservationStatus(reservationId, "Cancelled");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to cancel reservation: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the next queue position for a book
        /// </summary>
        /// <param name="bookId">Book ID</param>
        /// <returns>Next queue position number</returns>
        public int GetNextQueuePosition(int bookId)
        {
            try
            {
                string query = @"SELECT MAX(QueuePosition) FROM Reservations 
                                WHERE BookID = ? AND Status = 'Pending'";
                
                OleDbParameter param = new OleDbParameter("@BookID", bookId);
                object result = ExecuteScalar(query, param);
                
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result) + 1;
                }
                return 1;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get next queue position: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets active reservation count for a member
        /// </summary>
        /// <param name="memberId">Member ID (GUID string)</param>
        /// <returns>Number of active reservations (PENDING or RESERVED)</returns>
        public int GetActiveReservationCount(string memberId)
        {
            try
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM reservations 
                    WHERE member_id = ? 
                    AND reservation_status IN ('PENDING', 'RESERVED')";
                
                OleDbParameter param = new OleDbParameter("@MemberID", OleDbType.VarChar, 36) { Value = memberId };
                object result = ExecuteScalar(query, param);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get active reservation count: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Generates a new reservation ID as a UUID/GUID
        /// </summary>
        /// <returns>New UUID string</returns>
        private string GenerateNextReservationId()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Creates a new reservation for a book (NO copy assignment, NO status changes)
        /// </summary>
        /// <param name="bookId">Book ID (GUID string)</param>
        /// <param name="userId">User ID (GUID string)</param>
        /// <param name="expiryDays">Days until reservation expires (default 7)</param>
        /// <returns>True if successful</returns>
        public bool ReserveBook(string bookId, string userId, int expiryDays = 7)
        {
            try
            {
                // Get member_id and membership_status from user_id
                string memberQuery = "SELECT member_id, membership_status FROM members WHERE user_id = ?";
                OleDbParameter memberParam = new OleDbParameter("@UserID", OleDbType.VarChar, 36) { Value = userId };
                DataTable memberDt = ExecuteQuery(memberQuery, memberParam);
                
                if (memberDt.Rows.Count == 0)
                {
                    throw new Exception("Member not found for this user.");
                }
                
                DataRow memberRow = memberDt.Rows[0];
                string memberId = memberRow["member_id"].ToString();
                string membershipStatus = memberRow["membership_status"]?.ToString() ?? "ACTIVE";
                
                // ? CHECK IF MEMBER IS SUSPENDED
                if (membershipStatus.Equals("SUSPENDED", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Your account is currently SUSPENDED. You cannot make new reservations at this time.\n\nPlease contact the library administration for more information.");
                }
                
                // Check MAX_RESERVATIONS_PER_MEMBER from library_settings
                string maxReservationsQuery = "SELECT setting_value FROM library_settings WHERE setting_key = 'MAX_RESERVATIONS_PER_MEMBER'";
                object maxReservationsObj = ExecuteScalar(maxReservationsQuery);
                int maxReservations = 3; // Default
                if (maxReservationsObj != null && int.TryParse(maxReservationsObj.ToString(), out int max))
                {
                    maxReservations = max;
                }
                
                // Check current active reservation count
                int currentReservations = GetActiveReservationCount(memberId);
                if (currentReservations >= maxReservations)
                {
                    throw new Exception($"You have reached the maximum number of reservations ({maxReservations}). Please cancel an existing reservation or wait for one to expire.");
                }
                
                // Check if user already has an active reservation for this book
                string checkQuery = @"
                    SELECT COUNT(*) 
                    FROM reservations 
                    WHERE book_id = ? AND member_id = ? 
                    AND reservation_status IN ('PENDING', 'RESERVED')";
                
                OleDbParameter[] checkParams = {
                    new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId },
                    new OleDbParameter("@MemberID", OleDbType.VarChar, 36) { Value = memberId }
                };
                
                object existingCount = ExecuteScalar(checkQuery, checkParams);
                
                if (existingCount != null && Convert.ToInt32(existingCount) > 0)
                {
                    throw new Exception("You already have an active reservation for this book.");
                }
                
                // Generate new reservation ID
                string reservationId = GenerateNextReservationId();
                DateTime reservationDate = DateTime.Now;
                DateTime expiryDate = reservationDate.AddDays(expiryDays);
                
                // Insert reservation WITHOUT book_copy_id (no copy assignment, no status changes)
                string insertQuery = @"
                    INSERT INTO reservations (reservation_id, book_id, member_id, reservation_date, expiry_date, reservation_status)
                    VALUES (?, ?, ?, ?, ?, 'PENDING')";
                
                OleDbParameter[] insertParams = {
                    new OleDbParameter("@ReservationID", OleDbType.VarChar, 36) { Value = reservationId },
                    new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId },
                    new OleDbParameter("@MemberID", OleDbType.VarChar, 36) { Value = memberId },
                    new OleDbParameter("@ReservationDate", OleDbType.Date) { Value = reservationDate },
                    new OleDbParameter("@ExpiryDate", OleDbType.Date) { Value = expiryDate }
                };
                
                int result = ExecuteNonQuery(insertQuery, insertParams);
                
                // NOTE: We do NOT assign a copy or change any copy status
                // Reservations are just requests - copy assignment happens later by librarian
                
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to reserve book: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Maps a DataTable to a ReservationsList
        /// </summary>
        private ReservationsList MapDataTableToReservationsList(DataTable dt)
        {
            ReservationsList reservations = new ReservationsList();
            foreach (DataRow row in dt.Rows)
            {
                reservations.Add(MapRowToReservation(row));
            }
            return reservations;
        }

        /// <summary>
        /// Maps a DataRow to a Reservation object
        /// </summary>
        private Reservation MapRowToReservation(DataRow row)
        {
            return new Reservation
            {
                ReservationID = Convert.ToInt32(row["ReservationID"]),
                BookID = Convert.ToInt32(row["BookID"]),
                MemberID = Convert.ToInt32(row["MemberID"]),
                ReservationDate = Convert.ToDateTime(row["ReservationDate"]),
                Status = row["Status"]?.ToString() ?? "Pending",
                AvailableDate = row["AvailableDate"] != DBNull.Value ? Convert.ToDateTime(row["AvailableDate"]) : (DateTime?)null,
                ExpiryDate = row["ExpiryDate"] != DBNull.Value ? Convert.ToDateTime(row["ExpiryDate"]) : (DateTime?)null,
                FulfilledDate = row["FulfilledDate"] != DBNull.Value ? Convert.ToDateTime(row["FulfilledDate"]) : (DateTime?)null,
                AssignedCopyID = row["AssignedCopyID"] != DBNull.Value ? Convert.ToInt32(row["AssignedCopyID"]) : (int?)null,
                QueuePosition = row["QueuePosition"] != DBNull.Value ? Convert.ToInt32(row["QueuePosition"]) : 1,
                Notes = row["Notes"]?.ToString(),
                CreatedAt = row["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(row["CreatedAt"]) : DateTime.Now,
                UpdatedAt = row["UpdatedAt"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedAt"]) : (DateTime?)null,
                IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"])
            };
        }
    }
}
