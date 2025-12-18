using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Collection of LibrarySetting objects
    /// </summary>
    public class LibrarySettingsList : List<LibrarySetting>
    {
        public LibrarySettingsList() : base()
        {
        }

        public LibrarySettingsList(IEnumerable<LibrarySetting> collection) : base(collection)
        {
        }

        /// <summary>
        /// Find a setting by key
        /// </summary>
        public LibrarySetting FindByKey(string key)
        {
            return this.Find(s => s.SettingKey?.ToLower() == key?.ToLower());
        }

        /// <summary>
        /// Get setting value by key
        /// </summary>
        public string GetValue(string key)
        {
            var setting = FindByKey(key);
            return setting?.SettingValue;
        }

        /// <summary>
        /// Get settings by category
        /// </summary>
        public LibrarySettingsList FindByCategory(string category)
        {
            return new LibrarySettingsList(this.FindAll(s => 
                s.Category?.ToLower() == category?.ToLower()));
        }

        /// <summary>
        /// Get all active settings
        /// </summary>
        public LibrarySettingsList GetActiveSettings()
        {
            return new LibrarySettingsList(this.FindAll(s => s.IsActive));
        }
    }
}
