using System;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Represents a reservation for a book
    /// </summary>
    public class Reservation : BaseEntity
    {
        /// <summary>
        /// Reservation ID (Primary Key)
        /// </summary>
        public int ReservationID { get; set; }

        /// <summary>
        /// Book ID (Foreign Key) - the book being reserved
        /// </summary>
        public int BookID { get; set; }

        /// <summary>
        /// Member ID (Foreign Key) - the member making the reservation
        /// </summary>
        public int MemberID { get; set; }

        /// <summary>
        /// Date when the reservation was made
        /// </summary>
        public DateTime ReservationDate { get; set; }

        /// <summary>
        /// Status of the reservation (e.g., "Pending", "Ready", "Fulfilled", "Cancelled", "Expired")
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Date when the book became available for the member
        /// </summary>
        public DateTime? AvailableDate { get; set; }

        /// <summary>
        /// Expiry date for the reservation (if not picked up by this date, it expires)
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Date when the reservation was fulfilled (book picked up)
        /// </summary>
        public DateTime? FulfilledDate { get; set; }

        /// <summary>
        /// Copy ID that was assigned to this reservation
        /// </summary>
        public int? AssignedCopyID { get; set; }

        /// <summary>
        /// Priority in the reservation queue
        /// </summary>
        public int QueuePosition { get; set; }

        /// <summary>
        /// Notes about the reservation
        /// </summary>
        public string Notes { get; set; }

        public Reservation()
        {
            ReservationDate = DateTime.Now;
            Status = "Pending";
            QueuePosition = 1;
        }
    }
}
