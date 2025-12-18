using System;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Represents library system settings and configuration
    /// </summary>
    public class LibrarySetting : BaseEntity
    {
        /// <summary>
        /// Setting ID (Primary Key)
        /// </summary>
        public int SettingID { get; set; }

        /// <summary>
        /// Setting key/name
        /// </summary>
        public string SettingKey { get; set; }

        /// <summary>
        /// Setting value
        /// </summary>
        public string SettingValue { get; set; }

        /// <summary>
        /// Description of the setting
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Data type of the setting (e.g., "String", "Int", "Decimal", "Boolean", "DateTime")
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Category of the setting (e.g., "General", "Loans", "Fines", "Reservations")
        /// </summary>
        public string Category { get; set; }

        public LibrarySetting()
        {
            DataType = "String";
            Category = "General";
        }

        // Common setting keys as constants
        public const string MAX_BOOKS_PER_MEMBER = "MaxBooksPerMember";
        public const string DEFAULT_LOAN_PERIOD_DAYS = "DefaultLoanPeriodDays";
        public const string MAX_RENEWAL_COUNT = "MaxRenewalCount";
        public const string FINE_PER_DAY = "FinePerDay";
        public const string RESERVATION_EXPIRY_DAYS = "ReservationExpiryDays";
        public const string LIBRARY_NAME = "LibraryName";
        public const string LIBRARY_EMAIL = "LibraryEmail";
        public const string LIBRARY_PHONE = "LibraryPhone";
        public const string LIBRARY_ADDRESS = "LibraryAddress";
    }
}
