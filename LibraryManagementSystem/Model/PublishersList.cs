using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Collection of Publisher objects
    /// </summary>
    public class PublishersList : List<Publisher>
    {
        public PublishersList() : base()
        {
        }

        public PublishersList(IEnumerable<Publisher> collection) : base(collection)
        {
        }

        /// <summary>
        /// Find publishers by name (partial match)
        /// </summary>
        public PublishersList FindByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new PublishersList();

            return new PublishersList(this.FindAll(p => 
                p.Name?.ToLower().Contains(name.ToLower()) == true));
        }

        /// <summary>
        /// Find publishers by country
        /// </summary>
        public PublishersList FindByCountry(string country)
        {
            return new PublishersList(this.FindAll(p => 
                p.Country?.ToLower() == country?.ToLower()));
        }

        /// <summary>
        /// Get all active publishers
        /// </summary>
        public PublishersList GetActivePublishers()
        {
            return new PublishersList(this.FindAll(p => p.IsActive));
        }
    }
}
