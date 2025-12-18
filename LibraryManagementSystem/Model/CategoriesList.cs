using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Collection of Category objects
    /// </summary>
    public class CategoriesList : List<Category>
    {
        public CategoriesList() : base()
        {
        }

        public CategoriesList(IEnumerable<Category> collection) : base(collection)
        {
        }

        /// <summary>
        /// Find categories by name (partial match)
        /// </summary>
        public CategoriesList FindByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new CategoriesList();

            return new CategoriesList(this.FindAll(c => 
                c.Name?.ToLower().Contains(name.ToLower()) == true));
        }

        /// <summary>
        /// Get all active categories
        /// </summary>
        public CategoriesList GetActiveCategories()
        {
            return new CategoriesList(this.FindAll(c => c.IsActive));
        }

        /// <summary>
        /// Get root categories (no parent)
        /// </summary>
        public CategoriesList GetRootCategories()
        {
            return new CategoriesList(this.FindAll(c => !c.ParentCategoryID.HasValue));
        }

        /// <summary>
        /// Get subcategories of a specific category
        /// </summary>
        public CategoriesList GetSubCategories(int parentCategoryID)
        {
            return new CategoriesList(this.FindAll(c => 
                c.ParentCategoryID.HasValue && c.ParentCategoryID.Value == parentCategoryID));
        }
    }
}
