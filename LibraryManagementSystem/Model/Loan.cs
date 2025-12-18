using System;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Represents a loan transaction (borrowing of a book copy)
    /// </summary>
    public class Loan : BaseEntity
    {
        /// <summary>
        /// Loan ID (Primary Key)
        /// </summary>
        public int LoanID { get; set; }

        /// <summary>
        /// Copy ID (Foreign Key) - the specific book copy being borrowed
        /// </summary>
        public int CopyID { get; set; }

        /// <summary>
        /// Member ID (Foreign Key) - the member borrowing the book
        /// </summary>
        public int MemberID { get; set; }

        /// <summary>
        /// Librarian ID (Foreign Key) - who processed the loan
        /// </summary>
        public int? LibrarianID { get; set; }

        /// <summary>
        /// Date when the book was loaned
        /// </summary>
        public DateTime LoanDate { get; set; }

        /// <summary>
        /// Due date for returning the book
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Actual return date (null if not yet returned)
        /// </summary>
        public DateTime? ReturnDate { get; set; }

        /// <summary>
        /// Status of the loan (e.g., "Active", "Returned", "Overdue", "Lost")
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Fine amount for late return
        /// </summary>
        public decimal FineAmount { get; set; }

        /// <summary>
        /// Indicates if the fine has been paid
        /// </summary>
        public bool FinePaid { get; set; }

        /// <summary>
        /// Number of times the loan was renewed
        /// </summary>
        public int RenewalCount { get; set; }

        /// <summary>
        /// Notes about the loan
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Indicates if the loan is currently overdue
        /// </summary>
        public bool IsOverdue => ReturnDate == null && DateTime.Now > DueDate;

        /// <summary>
        /// Number of days overdue
        /// </summary>
        public int DaysOverdue
        {
            get
            {
                if (ReturnDate != null)
                {
                    return ReturnDate > DueDate ? (ReturnDate.Value - DueDate).Days : 0;
                }
                return DateTime.Now > DueDate ? (DateTime.Now - DueDate).Days : 0;
            }
        }

        public Loan()
        {
            LoanDate = DateTime.Now;
            DueDate = DateTime.Now.AddDays(14); // Default 2-week loan period
            Status = "Active";
            FineAmount = 0;
            FinePaid = false;
            RenewalCount = 0;
        }
    }
}
