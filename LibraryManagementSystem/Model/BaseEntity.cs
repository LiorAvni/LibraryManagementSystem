using System;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Abstract base class for common entity properties
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Date and time when the entity was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date and time when the entity was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Indicates whether the entity is active (soft delete support)
        /// </summary>
        public bool IsActive { get; set; }

        protected BaseEntity()
        {
            CreatedAt = DateTime.Now;
            IsActive = true;
        }
    }
}
