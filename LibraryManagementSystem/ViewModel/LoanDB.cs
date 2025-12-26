using System;
using System.Data;
using System.Data.OleDb;
using LibraryManagementSystem.Model;

namespace LibraryManagementSystem.ViewModel
{
    /// <summary>
    /// Data Access Layer for Loan operations
    /// </summary>
    public class LoanDB : BaseDB
    {
        /// <summary>
        /// Gets loans for management page with filtering and pagination
        /// </summary>
        /// <param name="memberName">Filter by member name (optional)</param>
        /// <param name="bookTitle">Filter by book title (optional)</param>
        /// <param name="status">Filter by status: ALL, ACTIVE, OVERDUE, RETURNED</param>
        /// <param name="page">Page number (0-based)</param>
        /// <param name="pageSize">Number of records per page</param>
        /// <param name="totalRecords">Output: Total number of records matching filters</param>
        /// <returns>DataTable with loan information</returns>
        public DataTable GetLoansForManagement(string memberName, string bookTitle, string status, int page, int pageSize, out int totalRecords)
        {
            try
            {
                // Build WHERE clause based on filters
                var whereConditions = new System.Collections.Generic.List<string>();
                var parameters = new System.Collections.Generic.List<OleDbParameter>();

                // Member name filter
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
                    if (status == "ACTIVE")
                    {
                        whereConditions.Add("(l.return_date IS NULL AND l.due_date >= Date())");
                    }
                    else if (status == "OVERDUE")
                    {
                        whereConditions.Add("(l.return_date IS NULL AND l.due_date < Date())");
                    }
                    else if (status == "RETURNED")
                    {
                        whereConditions.Add("l.return_date IS NOT NULL");
                    }
                }

                string whereClause = whereConditions.Count > 0 ? "WHERE " + string.Join(" AND ", whereConditions) : "";

                // Create parameter collections for count query
                var countParameters = new System.Collections.Generic.List<OleDbParameter>();
                
                // Rebuild parameters for count query
                if (!string.IsNullOrEmpty(memberName))
                {
                    string searchPattern = $"%{memberName}%";
                    countParameters.Add(new OleDbParameter("@FirstName", OleDbType.VarChar, 100) { Value = searchPattern });
                    countParameters.Add(new OleDbParameter("@LastName", OleDbType.VarChar, 100) { Value = searchPattern });
                }
                
                if (!string.IsNullOrEmpty(bookTitle))
                {
                    countParameters.Add(new OleDbParameter("@BookTitle", OleDbType.VarChar, 200) { Value = $"%{bookTitle}%" });
                }

                // Get total count
                string countQuery = $@"
                    SELECT COUNT(*) 
                    FROM (((loans l
                    INNER JOIN members m ON l.member_id = m.member_id)
                    INNER JOIN users u ON m.user_id = u.user_id)
                    INNER JOIN book_copies bc ON l.copy_id = bc.copy_id)
                    INNER JOIN books b ON bc.book_id = b.book_id
                    {whereClause}";

                object countResult = ExecuteScalar(countQuery, countParameters.ToArray());
                totalRecords = countResult != null ? Convert.ToInt32(countResult) : 0;

                // Get paginated results
                int offset = page * pageSize;
                
                string dataQuery = $@"
                    SELECT 
                        l.loan_id,
                        l.copy_id,
                        u.first_name & ' ' & u.last_name AS MemberName,
                        b.title AS BookTitle,
                        l.loan_date,
                        l.due_date,
                        l.return_date,
                        IIF(l.fine_amount IS NULL, 0, l.fine_amount) AS fine_amount
                    FROM (((loans l
                    INNER JOIN members m ON l.member_id = m.member_id)
                    INNER JOIN users u ON m.user_id = u.user_id)
                    INNER JOIN book_copies bc ON l.copy_id = bc.copy_id)
                    INNER JOIN books b ON bc.book_id = b.book_id
                    {whereClause}
                    ORDER BY l.loan_date DESC";

                DataTable allData = ExecuteQuery(dataQuery, parameters.ToArray());
                
                // Manual pagination (Access doesn't support OFFSET/FETCH)
                DataTable paginatedData = allData.Clone();
                int startRow = offset;
                int endRow = Math.Min(offset + pageSize, allData.Rows.Count);
                
                for (int i = startRow; i < endRow; i++)
                {
                    paginatedData.ImportRow(allData.Rows[i]);
                }
                
                return paginatedData;
            }
            catch (Exception ex)
            {
                totalRecords = 0;
                throw new Exception($"Failed to get loans for management: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all active loans for a specific member with book details
        /// </summary>
        /// <param name="userId">User ID (from users table)</param>
        /// <returns>DataTable with loan and book information</returns>
        public DataTable GetMemberActiveLoansWithDetails(string userId)
        {
            try
            {
                // First get loans without authors to avoid duplicates
                string query = @"
                    SELECT 
                        l.loan_id,
                        b.title AS BookTitle,
                        b.book_id,
                        l.loan_date AS LoanDate,
                        l.due_date AS DueDate,
                        l.return_date AS ReturnDate,
                        IIF(l.return_date IS NULL AND l.due_date < Date(), 'OVERDUE', 
                            IIF(l.return_date IS NULL, 'ACTIVE', 'RETURNED')) AS Status,
                        IIF(l.return_date IS NULL AND l.due_date < Date(), 
                            DateDiff('d', l.due_date, Date()) * 
                            (SELECT CDbl(setting_value) FROM library_settings WHERE setting_key = 'FINE_PER_DAY'),
                            IIF(l.fine_amount IS NULL, 0, l.fine_amount)) AS Fine
                    FROM ((loans l
                    INNER JOIN members m ON l.member_id = m.member_id)
                    INNER JOIN book_copies bc ON l.copy_id = bc.copy_id)
                    INNER JOIN books b ON bc.book_id = b.book_id
                    WHERE m.user_id = ? 
                    AND l.return_date IS NULL
                    ORDER BY l.loan_date DESC";
                
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
                throw new Exception($"Failed to get active loans with details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all unpaid fines for a specific member with book details
        /// </summary>
        /// <param name="userId">User ID (from users table)</param>
        /// <returns>DataTable with unpaid fines information</returns>
        public DataTable GetMemberUnpaidFines(string userId)
        {
            try
            {
                // Get loans with unpaid fines (fine_amount > 0 AND fine_payment_date IS NULL)
                string query = @"
                    SELECT 
                        l.loan_id,
                        b.title AS BookTitle,
                        b.book_id,
                        l.loan_date AS LoanDate,
                        l.due_date AS DueDate,
                        l.return_date AS ReturnDate,
                        l.fine_amount AS Fine,
                        IIF(l.return_date > l.due_date, 
                            DateDiff('d', l.due_date, l.return_date), 
                            0) AS OverdueDays,
                        'RETURNED' AS Status
                    FROM ((loans l
                    INNER JOIN members m ON l.member_id = m.member_id)
                    INNER JOIN book_copies bc ON l.copy_id = bc.copy_id)
                    INNER JOIN books b ON bc.book_id = b.book_id
                    WHERE m.user_id = ? 
                      AND l.fine_amount > 0 
                      AND l.fine_payment_date IS NULL
                    ORDER BY l.loan_date DESC";
                
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
                throw new Exception($"Failed to get unpaid fines: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets loan history for a specific member with book details
        /// </summary>
        /// <param name="userId">User ID (from users table)</param>
        /// <param name="daysBack">Number of days to look back (0 for all history)</param>
        /// <returns>DataTable with loan history</returns>
        public DataTable GetMemberLoanHistoryWithDetails(string userId, int daysBack = 0)
        {
            try
            {
                string dateFilter = daysBack > 0 
                    ? "AND l.loan_date >= DateAdd('d', -" + daysBack + ", Date())" 
                    : "";
                
                // First get loans without authors to avoid duplicates
                string query = $@"
                    SELECT 
                        l.loan_id,
                        b.title AS BookTitle,
                        b.book_id,
                        l.loan_date AS LoanDate,
                        l.due_date AS DueDate,
                        l.return_date AS ReturnDate,
                        IIF(l.return_date IS NULL AND l.due_date < Date(), 'OVERDUE', 
                            IIF(l.return_date IS NULL, 'ACTIVE', 'RETURNED')) AS Status,
                        IIF(l.return_date IS NULL AND l.due_date < Date(), 
                            DateDiff('d', l.due_date, Date()) * 
                            (SELECT CDbl(setting_value) FROM library_settings WHERE setting_key = 'FINE_PER_DAY'),
                            IIF(l.fine_amount IS NULL, 0, l.fine_amount)) AS Fine
                    FROM ((loans l
                    INNER JOIN members m ON l.member_id = m.member_id)
                    INNER JOIN book_copies bc ON l.copy_id = bc.copy_id)
                    INNER JOIN books b ON bc.book_id = b.book_id
                    WHERE m.user_id = ? 
                    {dateFilter}
                    ORDER BY l.loan_date DESC";
                
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
                throw new Exception($"Failed to get loan history with details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all active loans for a specific member
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <returns>List of active loans</returns>
        public LoansList SelectActiveLoansByMember(int memberId)
        {
            try
            {
                string query = @"SELECT * FROM Loans 
                                WHERE MemberID = ? AND Status = 'Active' AND ReturnDate IS NULL 
                                ORDER BY LoanDate DESC";
                
                OleDbParameter param = new OleDbParameter("@MemberID", memberId);
                DataTable dt = ExecuteQuery(query, param);
                
                return MapDataTableToLoansList(dt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get active loans: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all loans for a specific member
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <returns>List of all loans</returns>
        public LoansList SelectLoansByMember(int memberId)
        {
            try
            {
                string query = "SELECT * FROM Loans WHERE MemberID = ? ORDER BY LoanDate DESC";
                OleDbParameter param = new OleDbParameter("@MemberID", memberId);
                DataTable dt = ExecuteQuery(query, param);
                
                return MapDataTableToLoansList(dt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get loans: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets loans by date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of loans</returns>
        public LoansList SelectLoansByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                string query = @"SELECT * FROM Loans 
                                WHERE LoanDate >= ? AND LoanDate <= ? 
                                ORDER BY LoanDate DESC";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@StartDate", startDate),
                    new OleDbParameter("@EndDate", endDate)
                };
                
                DataTable dt = ExecuteQuery(query, parameters);
                return MapDataTableToLoansList(dt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get loans by date range: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all overdue loans
        /// </summary>
        /// <returns>List of overdue loans</returns>
        public LoansList SelectOverdueLoans()
        {
            try
            {
                string query = @"SELECT * FROM Loans 
                                WHERE ReturnDate IS NULL AND DueDate < ? 
                                ORDER BY DueDate ASC";
                
                OleDbParameter param = new OleDbParameter("@Today", DateTime.Now.Date);
                DataTable dt = ExecuteQuery(query, param);
                
                return MapDataTableToLoansList(dt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get overdue loans: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a loan by ID
        /// </summary>
        /// <param name="loanId">Loan ID</param>
        /// <returns>Loan object or null</returns>
        public Loan GetLoanByID(int loanId)
        {
            try
            {
                string query = "SELECT * FROM Loans WHERE LoanID = ?";
                OleDbParameter param = new OleDbParameter("@LoanID", loanId);
                
                DataTable dt = ExecuteQuery(query, param);
                
                if (dt.Rows.Count > 0)
                {
                    return MapRowToLoan(dt.Rows[0]);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get loan: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Inserts a new loan
        /// </summary>
        /// <param name="loan">Loan object to insert</param>
        /// <returns>True if successful</returns>
        public bool InsertLoan(Loan loan)
        {
            try
            {
                string query = @"INSERT INTO Loans (CopyID, MemberID, LibrarianID, LoanDate, 
                                DueDate, Status, FineAmount, FinePaid, RenewalCount, 
                                Notes, CreatedAt, IsActive)
                                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                OleDbParameter[] parameters = {
                    new OleDbParameter("@CopyID", loan.CopyID),
                    new OleDbParameter("@MemberID", loan.MemberID),
                    new OleDbParameter("@LibrarianID", loan.LibrarianID ?? (object)DBNull.Value),
                    new OleDbParameter("@LoanDate", loan.LoanDate),
                    new OleDbParameter("@DueDate", loan.DueDate),
                    new OleDbParameter("@Status", loan.Status ?? "Active"),
                    new OleDbParameter("@FineAmount", loan.FineAmount),
                    new OleDbParameter("@FinePaid", loan.FinePaid),
                    new OleDbParameter("@RenewalCount", loan.RenewalCount),
                    new OleDbParameter("@Notes", loan.Notes ?? (object)DBNull.Value),
                    new OleDbParameter("@CreatedAt", loan.CreatedAt),
                    new OleDbParameter("@IsActive", loan.IsActive)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to insert loan: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Returns a book by updating the loan record and setting copy status to AVAILABLE
        /// </summary>
        /// <param name="loanId">Loan ID (GUID string)</param>
        /// <param name="returnDate">Return date</param>
        /// <param name="fineAmount">Fine amount if any</param>
        /// <returns>True if successful</returns>
        public bool ReturnBook(string loanId, DateTime returnDate, decimal fineAmount = 0)
        {
            try
            {
                // First, get the copy_id from the loan
                string getCopyQuery = "SELECT copy_id FROM loans WHERE loan_id = ?";
                OleDbParameter getCopyParam = new OleDbParameter("@LoanID", OleDbType.VarChar, 36) { Value = loanId };
                object copyIdObj = ExecuteScalar(getCopyQuery, getCopyParam);
                
                if (copyIdObj == null)
                {
                    throw new Exception("Loan not found.");
                }
                
                string copyId = copyIdObj.ToString();
                
                // Update the loan record with return date and fine
                string updateLoanQuery = @"
                    UPDATE loans 
                    SET return_date = ?, 
                        fine_amount = ?
                    WHERE loan_id = ?";
                
                OleDbParameter[] loanParams = {
                    new OleDbParameter("@ReturnDate", OleDbType.Date) { Value = returnDate },
                    new OleDbParameter("@FineAmount", OleDbType.Currency) { Value = fineAmount },
                    new OleDbParameter("@LoanID", OleDbType.VarChar, 36) { Value = loanId }
                };

                int loanResult = ExecuteNonQuery(updateLoanQuery, loanParams);
                
                if (loanResult > 0)
                {
                    // Update the copy status to AVAILABLE
                    string updateCopyQuery = "UPDATE book_copies SET status = 'AVAILABLE' WHERE copy_id = ?";
                    OleDbParameter updateCopyParam = new OleDbParameter("@CopyID", OleDbType.VarChar, 36) { Value = copyId };
                    ExecuteNonQuery(updateCopyQuery, updateCopyParam);
                    
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to return book: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates the return date for a loan
        /// </summary>
        /// <param name="loanId">Loan ID</param>
        /// <param name="returnDate">Return date</param>
        /// <returns>True if successful</returns>
        public bool UpdateReturnDate(int loanId, DateTime returnDate)
        {
            try
            {
                string query = @"UPDATE Loans 
                                SET ReturnDate = ?, Status = 'Returned', UpdatedAt = ? 
                                WHERE LoanID = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@ReturnDate", returnDate),
                    new OleDbParameter("@UpdatedAt", DateTime.Now),
                    new OleDbParameter("@LoanID", loanId)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update return date: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates fine information for a loan
        /// </summary>
        /// <param name="loanId">Loan ID</param>
        /// <param name="fineAmount">Fine amount</param>
        /// <param name="finePaid">Whether fine is paid</param>
        /// <returns>True if successful</returns>
        public bool UpdateFine(int loanId, decimal fineAmount, bool finePaid)
        {
            try
            {
                string query = @"UPDATE Loans 
                                SET FineAmount = ?, FinePaid = ?, UpdatedAt = ? 
                                WHERE LoanID = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@FineAmount", fineAmount),
                    new OleDbParameter("@FinePaid", finePaid),
                    new OleDbParameter("@UpdatedAt", DateTime.Now),
                    new OleDbParameter("@LoanID", loanId)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update fine: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Renews a loan (extends due date)
        /// </summary>
        /// <param name="loanId">Loan ID</param>
        /// <param name="newDueDate">New due date</param>
        /// <returns>True if successful</returns>
        public bool RenewLoan(int loanId, DateTime newDueDate)
        {
            try
            {
                string query = @"UPDATE Loans 
                                SET DueDate = ?, RenewalCount = RenewalCount + 1, UpdatedAt = ? 
                                WHERE LoanID = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@DueDate", newDueDate),
                    new OleDbParameter("@UpdatedAt", DateTime.Now),
                    new OleDbParameter("@LoanID", loanId)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to renew loan: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets count of active loans for a member
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <returns>Count of active loans</returns>
        public int GetActiveLoansCount(int memberId)
        {
            try
            {
                string query = @"SELECT COUNT(*) FROM Loans 
                                WHERE MemberID = ? AND Status = 'Active' AND ReturnDate IS NULL";
                
                OleDbParameter param = new OleDbParameter("@MemberID", memberId);
                object result = ExecuteScalar(query, param);
                
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get active loans count: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Pays a fine for a loan by updating the fine_payment_date
        /// </summary>
        /// <param name="loanId">Loan ID (GUID string)</param>
        /// <returns>True if successful</returns>
        public bool PayFine(string loanId)
        {
            try
            {
                string query = @"
                    UPDATE loans 
                    SET fine_payment_date = ?
                    WHERE loan_id = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@PaymentDate", OleDbType.Date) { Value = DateTime.Now },
                    new OleDbParameter("@LoanID", OleDbType.VarChar, 36) { Value = loanId }
                };
                
                int result = ExecuteNonQuery(query, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to pay fine: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets librarian ID from user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Librarian ID</returns>
        public string GetLibrarianIdByUserId(string userId)
        {
            try
            {
                string query = "SELECT librarian_id FROM librarians WHERE user_id = ?";
                OleDbParameter param = new OleDbParameter("@UserID", OleDbType.VarChar, 36) { Value = userId };
                object result = ExecuteScalar(query, param);
                return result?.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get librarian ID: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets library setting value
        /// </summary>
        /// <param name="settingKey">Setting key</param>
        /// <returns>Setting value as string</returns>
        public string GetLibrarySetting(string settingKey)
        {
            try
            {
                string query = "SELECT setting_value FROM library_settings WHERE setting_key = ?";
                OleDbParameter param = new OleDbParameter("@SettingKey", OleDbType.VarChar, 50) { Value = settingKey };
                object result = ExecuteScalar(query, param);
                return result?.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get library setting: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets active loan count for a member
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <returns>Number of active loans</returns>
        public int GetActiveLoanCount(string memberId)
        {
            try
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM loans 
                    WHERE member_id = ? AND return_date IS NULL";
                
                OleDbParameter param = new OleDbParameter("@MemberID", OleDbType.VarChar, 36) { Value = memberId };
                object result = ExecuteScalar(query, param);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get active loan count: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets first available copy for a book
        /// </summary>
        /// <param name="bookId">Book ID</param>
        /// <returns>Copy ID of first available copy, or null if none available</returns>
        public string GetFirstAvailableCopy(string bookId)
        {
            try
            {
                string query = @"
                    SELECT TOP 1 copy_id
                    FROM book_copies
                    WHERE book_id = ? AND status = 'AVAILABLE'
                    ORDER BY copy_number";
                
                OleDbParameter param = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                object result = ExecuteScalar(query, param);
                return result?.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get available copy: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates a new loan
        /// </summary>
        /// <param name="copyId">Copy ID</param>
        /// <param name="memberId">Member ID</param>
        /// <param name="librarianId">Librarian ID</param>
        /// <param name="loanPeriodDays">Loan period in days</param>
        /// <returns>New loan ID</returns>
        public string CreateLoan(string copyId, string memberId, string librarianId, int loanPeriodDays)
        {
            try
            {
                string loanId = Guid.NewGuid().ToString();
                DateTime loanDate = DateTime.Now;
                DateTime dueDate = loanDate.AddDays(loanPeriodDays);
                
                string insertLoanQuery = @"
                    INSERT INTO loans (loan_id, copy_id, member_id, librarian_id, loan_date, due_date)
                    VALUES (?, ?, ?, ?, ?, ?)";
                
                OleDbParameter[] loanParams = {
                    new OleDbParameter("@LoanID", OleDbType.VarChar, 36) { Value = loanId },
                    new OleDbParameter("@CopyID", OleDbType.VarChar, 36) { Value = copyId },
                    new OleDbParameter("@MemberID", OleDbType.VarChar, 36) { Value = memberId },
                    new OleDbParameter("@LibrarianID", OleDbType.VarChar, 36) { Value = string.IsNullOrEmpty(librarianId) ? (object)DBNull.Value : librarianId },
                    new OleDbParameter("@LoanDate", OleDbType.Date) { Value = loanDate },
                    new OleDbParameter("@DueDate", OleDbType.Date) { Value = dueDate }
                };
                
                int result = ExecuteNonQuery(insertLoanQuery, loanParams);
                
                if (result > 0)
                {
                    // Update copy status to BORROWED
                    string updateCopyQuery = "UPDATE book_copies SET status = 'BORROWED' WHERE copy_id = ?";
                    OleDbParameter copyParam = new OleDbParameter("@CopyID", OleDbType.VarChar, 36) { Value = copyId };
                    ExecuteNonQuery(updateCopyQuery, copyParam);
                    
                    return loanId;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create loan: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Generates a new loan ID as a UUID/GUID
        /// </summary>
        /// <returns>New UUID string</returns>
        private string GenerateNextLoanId()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Creates a new loan for a book (borrows a book)
        /// </summary>
        /// <param name="bookId">Book ID (GUID string)</param>
        /// <param name="userId">User ID (GUID string)</param>
        /// <param name="loanPeriodDays">Loan period in days (default 14)</param>
        /// <returns>True if successful</returns>
        public bool BorrowBook(string bookId, string userId, int loanPeriodDays = 14)
        {
            try
            {
                // Get member_id from user_id
                string memberQuery = "SELECT member_id FROM members WHERE user_id = ?";
                OleDbParameter memberParam = new OleDbParameter("@UserID", OleDbType.VarChar, 36) { Value = userId };
                object memberIdObj = ExecuteScalar(memberQuery, memberParam);
                
                if (memberIdObj == null)
                {
                    throw new Exception("Member not found for this user.");
                }
                
                string memberId = memberIdObj.ToString();
                
                // Check MAX_LOANS_PER_MEMBER from library_settings
                string maxLoansQuery = "SELECT setting_value FROM library_settings WHERE setting_key = 'MAX_LOANS_PER_MEMBER'";
                object maxLoansObj = ExecuteScalar(maxLoansQuery);
                int maxLoans = 3; // Default
                if (maxLoansObj != null && int.TryParse(maxLoansObj.ToString(), out int max))
                {
                    maxLoans = max;
                }
                
                // Check current active loan count
                int currentLoans = GetActiveLoanCount(memberId);
                if (currentLoans >= maxLoans)
                {
                    throw new Exception($"You have reached the maximum number of loans ({maxLoans}). Please return a book before borrowing another.");
                }
                
                // Get an available copy for this book
                string copyQuery = @"
                    SELECT TOP 1 copy_id 
                    FROM book_copies 
                    WHERE book_id = ? AND status = 'AVAILABLE'
                    ORDER BY copy_number";
                OleDbParameter copyParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
                object copyIdObj = ExecuteScalar(copyQuery, copyParam);
                
                if (copyIdObj == null)
                {
                    throw new Exception("No available copies for this book.");
                }
                
                string copyId = copyIdObj.ToString();
                
                // Create new loan with readable ID format: loan-###
                string loanId = GenerateNextLoanId();
                DateTime loanDate = DateTime.Now;
                DateTime dueDate = loanDate.AddDays(loanPeriodDays);
                
                string insertLoanQuery = @"
                    INSERT INTO loans (loan_id, copy_id, member_id, loan_date, due_date)
                    VALUES (?, ?, ?, ?, ?)";
                
                OleDbParameter[] loanParams = {
                    new OleDbParameter("@LoanID", OleDbType.VarChar, 36) { Value = loanId },
                    new OleDbParameter("@CopyID", OleDbType.VarChar, 36) { Value = copyId },
                    new OleDbParameter("@MemberID", OleDbType.VarChar, 36) { Value = memberId },
                    new OleDbParameter("@LoanDate", OleDbType.Date) { Value = loanDate },
                    new OleDbParameter("@DueDate", OleDbType.Date) { Value = dueDate }
                };
                
                int result = ExecuteNonQuery(insertLoanQuery, loanParams);
                
                if (result > 0)
                {
                    // Update copy status to BORROWED
                    string updateCopyQuery = "UPDATE book_copies SET status = 'BORROWED' WHERE copy_id = ?";
                    OleDbParameter updateParam = new OleDbParameter("@CopyID", OleDbType.VarChar, 36) { Value = copyId };
                    ExecuteNonQuery(updateCopyQuery, updateParam);
                    
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to borrow book: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Maps a DataTable to a LoansList
        /// </summary>
        private LoansList MapDataTableToLoansList(DataTable dt)
        {
            LoansList loans = new LoansList();
            foreach (DataRow row in dt.Rows)
            {
                loans.Add(MapRowToLoan(row));
            }
            return loans;
        }

        /// <summary>
        /// Maps a DataRow to a Loan object
        /// </summary>
        private Loan MapRowToLoan(DataRow row)
        {
            return new Loan
            {
                LoanID = Convert.ToInt32(row["LoanID"]),
                CopyID = Convert.ToInt32(row["CopyID"]),
                MemberID = Convert.ToInt32(row["MemberID"]),
                LibrarianID = row["LibrarianID"] != DBNull.Value ? Convert.ToInt32(row["LibrarianID"]) : (int?)null,
                LoanDate = Convert.ToDateTime(row["LoanDate"]),
                DueDate = Convert.ToDateTime(row["DueDate"]),
                ReturnDate = row["ReturnDate"] != DBNull.Value ? Convert.ToDateTime(row["ReturnDate"]) : (DateTime?)null,
                Status = row["Status"]?.ToString() ?? "Active",
                FineAmount = row["FineAmount"] != DBNull.Value ? Convert.ToDecimal(row["FineAmount"]) : 0,
                FinePaid = row["FinePaid"] != DBNull.Value && Convert.ToBoolean(row["FinePaid"]),
                RenewalCount = row["RenewalCount"] != DBNull.Value ? Convert.ToInt32(row["RenewalCount"]) : 0,
                Notes = row["Notes"]?.ToString(),
                CreatedAt = row["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(row["CreatedAt"]) : DateTime.Now,
                UpdatedAt = row["UpdatedAt"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedAt"]) : (DateTime?)null,
                IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"])
            };
        }
    }
}
