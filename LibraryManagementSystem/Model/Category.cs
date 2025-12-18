using System;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Represents a Category/Genre of books
    /// </summary>
    public class Category : BaseEntity
    {
        /// <summary>
        /// Category ID (Primary Key)
        /// </summary>
        public int CategoryID { get; set; }

        /// <summary>
        /// Name of the category
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the category
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Parent category ID (for hierarchical categories)
        /// </summary>
        public int? ParentCategoryID { get; set; }

        public Category()
        {
        }
    }
}
