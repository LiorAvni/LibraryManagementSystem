using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Collection of Loan objects
    /// </summary>
    public class LoansList : List<Loan>
    {
        public LoansList() : base()
        {
        }

        public LoansList(IEnumerable<Loan> collection) : base(collection)
        {
        }

        /// <summary>
        /// Find loans by member ID
        /// </summary>
        public LoansList FindByMemberID(int memberID)
        {
            return new LoansList(this.FindAll(l => l.MemberID == memberID));
        }

        /// <summary>
        /// Find loans by copy ID
        /// </summary>
        public LoansList FindByCopyID(int copyID)
        {
            return new LoansList(this.FindAll(l => l.CopyID == copyID));
        }

        /// <summary>
        /// Get all active loans (not yet returned)
        /// </summary>
        public LoansList GetActiveLoans()
        {
            return new LoansList(this.FindAll(l => 
                l.Status?.ToLower() == "active" && l.ReturnDate == null));
        }

        /// <summary>
        /// Get active loans for a specific member
        /// </summary>
        public LoansList GetActiveLoansByMember(int memberID)
        {
            return new LoansList(this.FindAll(l => 
                l.MemberID == memberID && l.Status?.ToLower() == "active" && l.ReturnDate == null));
        }

        /// <summary>
        /// Get all overdue loans
        /// </summary>
        public LoansList GetOverdueLoans()
        {
            return new LoansList(this.FindAll(l => l.IsOverdue));
        }

        /// <summary>
        /// Get overdue loans for a specific member
        /// </summary>
        public LoansList GetOverdueLoansByMember(int memberID)
        {
            return new LoansList(this.FindAll(l => 
                l.MemberID == memberID && l.IsOverdue));
        }

        /// <summary>
        /// Get returned loans
        /// </summary>
        public LoansList GetReturnedLoans()
        {
            return new LoansList(this.FindAll(l => 
                l.Status?.ToLower() == "returned" && l.ReturnDate != null));
        }

        /// <summary>
        /// Get loans within a date range
        /// </summary>
        public LoansList GetLoansByDateRange(DateTime startDate, DateTime endDate)
        {
            return new LoansList(this.FindAll(l => 
                l.LoanDate >= startDate && l.LoanDate <= endDate));
        }

        /// <summary>
        /// Get loans with unpaid fines
        /// </summary>
        public LoansList GetLoansWithUnpaidFines()
        {
            return new LoansList(this.FindAll(l => 
                l.FineAmount > 0 && !l.FinePaid));
        }

        /// <summary>
        /// Calculate total fines owed
        /// </summary>
        public decimal GetTotalFinesOwed()
        {
            return this.Where(l => l.FineAmount > 0 && !l.FinePaid)
                      .Sum(l => l.FineAmount);
        }
    }
}
