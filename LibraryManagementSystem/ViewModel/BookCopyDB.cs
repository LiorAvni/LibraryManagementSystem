using System;
using System.Data;
using System.Data.OleDb;
using LibraryManagementSystem.Model;

namespace LibraryManagementSystem.ViewModel
{
    /// <summary>
    /// Data Access Layer for BookCopy operations
    /// </summary>
    public class BookCopyDB : BaseDB
    {
        /// <summary>
        /// Gets all copies of a specific book
        /// </summary>
        /// <param name="bookId">Book ID</param>
        /// <returns>List of book copies</returns>
        public BookCopiesList GetCopiesByBookID(int bookId)
        {
            try
            {
                string query = "SELECT * FROM BookCopies WHERE BookID = ? ORDER BY CopyID";
                OleDbParameter param = new OleDbParameter("@BookID", bookId);
                DataTable dt = ExecuteQuery(query, param);
                
                return MapDataTableToBookCopiesList(dt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get book copies: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets available copies for a specific book
        /// </summary>
        /// <param name="bookId">Book ID</param>
        /// <returns>List of available copies</returns>
        public BookCopiesList GetAvailableCopiesByBookID(int bookId)
        {
            try
            {
                string query = @"SELECT * FROM BookCopies 
                                WHERE BookID = ? AND Status = 'Available' 
                                ORDER BY CopyID";
                
                OleDbParameter param = new OleDbParameter("@BookID", bookId);
                DataTable dt = ExecuteQuery(query, param);
                
                return MapDataTableToBookCopiesList(dt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get available copies: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a copy by ID
        /// </summary>
        /// <param name="copyId">Copy ID</param>
        /// <returns>BookCopy object or null</returns>
        public BookCopy GetCopyByID(int copyId)
        {
            try
            {
                string query = "SELECT * FROM BookCopies WHERE CopyID = ?";
                OleDbParameter param = new OleDbParameter("@CopyID", copyId);
                DataTable dt = ExecuteQuery(query, param);
                
                if (dt.Rows.Count > 0)
                {
                    return MapRowToBookCopy(dt.Rows[0]);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get book copy: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Inserts a new book copy
        /// </summary>
        /// <param name="copy">BookCopy object to insert</param>
        /// <returns>True if successful</returns>
        public bool InsertBookCopy(BookCopy copy)
        {
            try
            {
                string query = @"INSERT INTO BookCopies (BookID, Barcode, Status, Condition, 
                                Location, AcquisitionDate, PurchasePrice, Notes, CreatedAt, IsActive)
                                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                OleDbParameter[] parameters = {
                    new OleDbParameter("@BookID", copy.BookID),
                    new OleDbParameter("@Barcode", copy.Barcode ?? (object)DBNull.Value),
                    new OleDbParameter("@Status", copy.Status ?? "Available"),
                    new OleDbParameter("@Condition", copy.Condition ?? "Good"),
                    new OleDbParameter("@Location", copy.Location ?? (object)DBNull.Value),
                    new OleDbParameter("@AcquisitionDate", copy.AcquisitionDate),
                    new OleDbParameter("@PurchasePrice", copy.PurchasePrice ?? (object)DBNull.Value),
                    new OleDbParameter("@Notes", copy.Notes ?? (object)DBNull.Value),
                    new OleDbParameter("@CreatedAt", copy.CreatedAt),
                    new OleDbParameter("@IsActive", copy.IsActive)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to insert book copy: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates a book copy's status
        /// </summary>
        /// <param name="copyId">Copy ID</param>
        /// <param name="status">New status</param>
        /// <returns>True if successful</returns>
        public bool UpdateCopyStatus(int copyId, string status)
        {
            try
            {
                string query = @"UPDATE BookCopies 
                                SET Status = ?, UpdatedAt = ? 
                                WHERE CopyID = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("@Status", status),
                    new OleDbParameter("@UpdatedAt", DateTime.Now),
                    new OleDbParameter("@CopyID", copyId)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update copy status: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates a book copy
        /// </summary>
        /// <param name="copy">BookCopy object with updated data</param>
        /// <returns>True if successful</returns>
        public bool UpdateBookCopy(BookCopy copy)
        {
            try
            {
                string query = @"UPDATE BookCopies 
                                SET Barcode = ?, Status = ?, Condition = ?, Location = ?, 
                                PurchasePrice = ?, Notes = ?, UpdatedAt = ? 
                                WHERE CopyID = ?";

                OleDbParameter[] parameters = {
                    new OleDbParameter("@Barcode", copy.Barcode ?? (object)DBNull.Value),
                    new OleDbParameter("@Status", copy.Status ?? "Available"),
                    new OleDbParameter("@Condition", copy.Condition ?? "Good"),
                    new OleDbParameter("@Location", copy.Location ?? (object)DBNull.Value),
                    new OleDbParameter("@PurchasePrice", copy.PurchasePrice ?? (object)DBNull.Value),
                    new OleDbParameter("@Notes", copy.Notes ?? (object)DBNull.Value),
                    new OleDbParameter("@UpdatedAt", DateTime.Now),
                    new OleDbParameter("@CopyID", copy.CopyID)
                };

                return ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update book copy: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Maps a DataTable to a BookCopiesList
        /// </summary>
        private BookCopiesList MapDataTableToBookCopiesList(DataTable dt)
        {
            BookCopiesList copies = new BookCopiesList();
            foreach (DataRow row in dt.Rows)
            {
                copies.Add(MapRowToBookCopy(row));
            }
            return copies;
        }

        /// <summary>
        /// Maps a DataRow to a BookCopy object
        /// </summary>
        private BookCopy MapRowToBookCopy(DataRow row)
        {
            return new BookCopy
            {
                CopyID = Convert.ToInt32(row["CopyID"]),
                BookID = Convert.ToInt32(row["BookID"]),
                Barcode = row["Barcode"]?.ToString(),
                Status = row["Status"]?.ToString() ?? "Available",
                Condition = row["Condition"]?.ToString() ?? "Good",
                Location = row["Location"]?.ToString(),
                AcquisitionDate = row["AcquisitionDate"] != DBNull.Value ? Convert.ToDateTime(row["AcquisitionDate"]) : DateTime.Now,
                PurchasePrice = row["PurchasePrice"] != DBNull.Value ? Convert.ToDecimal(row["PurchasePrice"]) : (decimal?)null,
                Notes = row["Notes"]?.ToString(),
                LastBorrowedDate = row["LastBorrowedDate"] != DBNull.Value ? Convert.ToDateTime(row["LastBorrowedDate"]) : (DateTime?)null,
                CreatedAt = row["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(row["CreatedAt"]) : DateTime.Now,
                UpdatedAt = row["UpdatedAt"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedAt"]) : (DateTime?)null,
                IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"])
            };
        }
    }
}
