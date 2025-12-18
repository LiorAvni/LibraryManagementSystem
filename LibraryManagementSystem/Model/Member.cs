using System;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Represents a Member user who can borrow books
    /// </summary>
    public class Member : User
    {
        /// <summary>
        /// Member ID (Primary Key, can be same as UserID or separate)
        /// </summary>
        public int MemberID { get; set; }

        /// <summary>
        /// Date when the member joined the library
        /// </summary>
        public DateTime MembershipDate { get; set; }

        /// <summary>
        /// Membership status (e.g., "Active", "Suspended", "Expired")
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Membership expiry date
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Total fines owed by the member
        /// </summary>
        public decimal FinesOwed { get; set; }

        /// <summary>
        /// Maximum number of books that can be borrowed at once
        /// </summary>
        public int MaxBooksAllowed { get; set; }

        /// <summary>
        /// Current number of books borrowed
        /// </summary>
        public int CurrentBooksCount { get; set; }

        /// <summary>
        /// Member card number
        /// </summary>
        public string CardNumber { get; set; }

        public Member()
        {
            MembershipDate = DateTime.Now;
            Status = "Active";
            MaxBooksAllowed = 3;
            CurrentBooksCount = 0;
            FinesOwed = 0;
        }
    }
}
