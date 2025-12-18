using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Collection of Reservation objects
    /// </summary>
    public class ReservationsList : List<Reservation>
    {
        public ReservationsList() : base()
        {
        }

        public ReservationsList(IEnumerable<Reservation> collection) : base(collection)
        {
        }

        /// <summary>
        /// Find reservations by member ID
        /// </summary>
        public ReservationsList FindByMemberID(int memberID)
        {
            return new ReservationsList(this.FindAll(r => r.MemberID == memberID));
        }

        /// <summary>
        /// Find reservations by book ID
        /// </summary>
        public ReservationsList FindByBookID(int bookID)
        {
            return new ReservationsList(this.FindAll(r => r.BookID == bookID));
        }

        /// <summary>
        /// Get all pending reservations
        /// </summary>
        public ReservationsList GetPendingReservations()
        {
            return new ReservationsList(this.FindAll(r => 
                r.Status?.ToLower() == "pending"));
        }

        /// <summary>
        /// Get pending reservations for a specific member
        /// </summary>
        public ReservationsList GetPendingReservationsByMember(int memberID)
        {
            return new ReservationsList(this.FindAll(r => 
                r.MemberID == memberID && r.Status?.ToLower() == "pending"));
        }

        /// <summary>
        /// Get ready reservations (book available for pickup)
        /// </summary>
        public ReservationsList GetReadyReservations()
        {
            return new ReservationsList(this.FindAll(r => 
                r.Status?.ToLower() == "ready"));
        }

        /// <summary>
        /// Get fulfilled reservations
        /// </summary>
        public ReservationsList GetFulfilledReservations()
        {
            return new ReservationsList(this.FindAll(r => 
                r.Status?.ToLower() == "fulfilled"));
        }

        /// <summary>
        /// Get expired reservations
        /// </summary>
        public ReservationsList GetExpiredReservations()
        {
            return new ReservationsList(this.FindAll(r => 
                r.Status?.ToLower() == "expired" || 
                (r.ExpiryDate.HasValue && r.ExpiryDate.Value < DateTime.Now)));
        }

        /// <summary>
        /// Get reservations within a date range
        /// </summary>
        public ReservationsList GetReservationsByDateRange(DateTime startDate, DateTime endDate)
        {
            return new ReservationsList(this.FindAll(r => 
                r.ReservationDate >= startDate && r.ReservationDate <= endDate));
        }

        /// <summary>
        /// Get reservations sorted by queue position
        /// </summary>
        public ReservationsList GetSortedByQueuePosition()
        {
            return new ReservationsList(this.OrderBy(r => r.QueuePosition));
        }
    }
}
