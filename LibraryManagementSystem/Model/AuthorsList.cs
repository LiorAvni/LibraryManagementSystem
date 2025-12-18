using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Collection of Author objects
    /// </summary>
    public class AuthorsList : List<Author>
    {
        public AuthorsList() : base()
        {
        }

        public AuthorsList(IEnumerable<Author> collection) : base(collection)
        {
        }

        /// <summary>
        /// Find authors by name (partial match)
        /// </summary>
        public AuthorsList FindByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new AuthorsList();

            return new AuthorsList(this.FindAll(a => 
                a.FirstName?.ToLower().Contains(name.ToLower()) == true ||
                a.LastName?.ToLower().Contains(name.ToLower()) == true ||
                a.FullName?.ToLower().Contains(name.ToLower()) == true));
        }

        /// <summary>
        /// Find authors by nationality
        /// </summary>
        public AuthorsList FindByNationality(string nationality)
        {
            return new AuthorsList(this.FindAll(a => 
                a.Nationality?.ToLower() == nationality?.ToLower()));
        }

        /// <summary>
        /// Get all active authors
        /// </summary>
        public AuthorsList GetActiveAuthors()
        {
            return new AuthorsList(this.FindAll(a => a.IsActive));
        }
    }
}
