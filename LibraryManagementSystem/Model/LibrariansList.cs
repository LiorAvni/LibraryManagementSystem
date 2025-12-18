using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Collection of Librarian objects
    /// </summary>
    public class LibrariansList : List<Librarian>
    {
        public LibrariansList() : base()
        {
        }

        public LibrariansList(IEnumerable<Librarian> collection) : base(collection)
        {
        }

        /// <summary>
        /// Find a librarian by employee ID
        /// </summary>
        public Librarian FindByEmployeeID(string employeeID)
        {
            return this.Find(l => l.EmployeeID == employeeID);
        }

        /// <summary>
        /// Get all admin librarians
        /// </summary>
        public LibrariansList GetAdmins()
        {
            return new LibrariansList(this.FindAll(l => l.IsAdmin));
        }

        /// <summary>
        /// Get all active librarians
        /// </summary>
        public LibrariansList GetActiveLibrarians()
        {
            return new LibrariansList(this.FindAll(l => l.IsActive));
        }
    }
}
