using System;
using System.Data;
using System.Data.OleDb;

namespace LibraryManagementSystem.ViewModel
{
    /// <summary>
    /// Data Access Layer for Reports and Statistics
    /// </summary>
    public class ReportsDB : BaseDB
    {
        /// <summary>
        /// Gets loan statistics for the last N days
        /// </summary>
        /// <param name="days">Number of days</param>
        /// <returns>DataTable with statistics</returns>
        public DataTable GetLoanStatistics(int days)
        {
            try
            {
                DateTime startDate = DateTime.Now.AddDays(-days);
                string query = @"SELECT 
                                    COUNT(*) AS TotalLoans,
                                    COUNT(DISTINCT MemberID) AS UniqueMembers,
                                    SUM(CASE WHEN ReturnDate IS NULL THEN 1 ELSE 0 END) AS ActiveLoans,
                                    SUM(CASE WHEN ReturnDate IS NOT NULL THEN 1 ELSE 0 END) AS ReturnedLoans
                                FROM Loans 
                                WHERE LoanDate >= ?";
                
                OleDbParameter param = new OleDbParameter("@StartDate", startDate);
                return ExecuteQuery(query, param);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get loan statistics: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets most borrowed books
        /// </summary>
        /// <param name="topN">Number of top books</param>
        /// <returns>DataTable with book borrowing statistics</returns>
        public DataTable GetMostBorrowedBooks(int topN)
        {
            try
            {
                string query = $@"SELECT TOP {topN} 
                                    B.Title, B.ISBN, COUNT(L.LoanID) AS LoanCount
                                FROM Books B
                                INNER JOIN BookCopies BC ON B.BookID = BC.BookID
                                INNER JOIN Loans L ON BC.CopyID = L.CopyID
                                GROUP BY B.BookID, B.Title, B.ISBN
                                ORDER BY COUNT(L.LoanID) DESC";
                
                return ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get most borrowed books: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets members with most loans
        /// </summary>
        /// <param name="topN">Number of top members</param>
        /// <returns>DataTable with member borrowing statistics</returns>
        public DataTable GetMostActiveMembers(int topN)
        {
            try
            {
                string query = $@"SELECT TOP {topN} 
                                    U.FirstName, U.LastName, M.CardNumber, COUNT(L.LoanID) AS LoanCount
                                FROM Members M
                                INNER JOIN Users U ON M.UserID = U.UserID
                                INNER JOIN Loans L ON M.MemberID = L.MemberID
                                GROUP BY M.MemberID, U.FirstName, U.LastName, M.CardNumber
                                ORDER BY COUNT(L.LoanID) DESC";
                
                return ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get most active members: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets overdue books summary
        /// </summary>
        /// <returns>DataTable with overdue information</returns>
        public DataTable GetOverdueBooksSummary()
        {
            try
            {
                string query = @"SELECT 
                                    B.Title, U.FirstName, U.LastName, L.DueDate,
                                    (Date() - L.DueDate) AS DaysOverdue,
                                    L.FineAmount
                                FROM Loans L
                                INNER JOIN BookCopies BC ON L.CopyID = BC.CopyID
                                INNER JOIN Books B ON BC.BookID = B.BookID
                                INNER JOIN Members M ON L.MemberID = M.MemberID
                                INNER JOIN Users U ON M.UserID = U.UserID
                                WHERE L.ReturnDate IS NULL AND L.DueDate < Date()
                                ORDER BY L.DueDate ASC";
                
                return ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get overdue books summary: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets total fines collected
        /// </summary>
        /// <param name="days">Number of days to look back</param>
        /// <returns>Total fines amount</returns>
        public decimal GetTotalFinesCollected(int days)
        {
            try
            {
                DateTime startDate = DateTime.Now.AddDays(-days);
                string query = @"SELECT SUM(FineAmount) 
                                FROM Loans 
                                WHERE FinePaid = True AND UpdatedAt >= ?";
                
                OleDbParameter param = new OleDbParameter("@StartDate", startDate);
                object result = ExecuteScalar(query, param);
                
                return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get total fines collected: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets book availability summary
        /// </summary>
        /// <returns>DataTable with availability statistics</returns>
        public DataTable GetBookAvailabilitySummary()
        {
            try
            {
                string query = @"SELECT 
                                    B.Title, B.ISBN,
                                    COUNT(BC.CopyID) AS TotalCopies,
                                    SUM(CASE WHEN BC.Status = 'Available' THEN 1 ELSE 0 END) AS AvailableCopies,
                                    SUM(CASE WHEN BC.Status = 'Borrowed' THEN 1 ELSE 0 END) AS BorrowedCopies
                                FROM Books B
                                LEFT JOIN BookCopies BC ON B.BookID = BC.BookID
                                WHERE B.IsActive = True AND B.IsRetired = False
                                GROUP BY B.BookID, B.Title, B.ISBN
                                ORDER BY B.Title";
                
                return ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get book availability summary: {ex.Message}", ex);
            }
        }
    }
}
