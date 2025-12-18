using System.Collections.Generic;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Collection of User objects
    /// </summary>
    public class UsersList : List<User>
    {
        public UsersList() : base()
        {
        }

        public UsersList(IEnumerable<User> collection) : base(collection)
        {
        }

        /// <summary>
        /// Find a user by email
        /// </summary>
        public User FindByEmail(string email)
        {
            return this.Find(u => u.Email?.ToLower() == email?.ToLower());
        }

        /// <summary>
        /// Find users by role
        /// </summary>
        public UsersList FindByRole(string role)
        {
            return new UsersList(this.FindAll(u => u.Role?.ToLower() == role?.ToLower()));
        }

        /// <summary>
        /// Get all active users
        /// </summary>
        public UsersList GetActiveUsers()
        {
            return new UsersList(this.FindAll(u => u.IsActive));
        }
    }
}
