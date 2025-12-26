using System;
using System.Data.OleDb;

namespace LibraryManagementSystem.ViewModel
{
    public class SettingsDB : BaseDB
    {
        /// <summary>
        /// Get a setting value from the database
        /// </summary>
        /// <param name="settingKey">The setting key (e.g., "FINE_PER_DAY")</param>
        /// <param name="defaultValue">Default value if setting doesn't exist</param>
        /// <returns>Setting value or default value</returns>
        public string GetSetting(string settingKey, string defaultValue = "")
        {
            string query = "SELECT setting_value FROM library_settings WHERE setting_key = @settingKey";
            
            try
            {
                object result = ExecuteScalar(query, new OleDbParameter("@settingKey", settingKey));
                
                if (result != null && result != DBNull.Value)
                {
                    return result.ToString();
                }
                
                // If setting doesn't exist, insert it with default value
                InsertSetting(settingKey, defaultValue);
                return defaultValue;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetSetting: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Update a setting value in the database
        /// </summary>
        /// <param name="settingKey">The setting key</param>
        /// <param name="settingValue">The new value</param>
        /// <returns>True if successful</returns>
        public bool UpdateSetting(string settingKey, string settingValue)
        {
            // First check if the setting exists
            string checkQuery = "SELECT COUNT(*) FROM library_settings WHERE setting_key = @settingKey";
            string updateQuery = "UPDATE library_settings SET setting_value = @settingValue WHERE setting_key = @settingKey";
            
            try
            {
                // Check if setting exists
                object countResult = ExecuteScalar(checkQuery, new OleDbParameter("@settingKey", settingKey));
                int count = Convert.ToInt32(countResult);
                
                if (count == 0)
                {
                    // Setting doesn't exist, insert it
                    return InsertSetting(settingKey, settingValue);
                }
                
                // Update existing setting
                int rowsAffected = ExecuteNonQuery(updateQuery, 
                    new OleDbParameter("@settingValue", settingValue),
                    new OleDbParameter("@settingKey", settingKey));
                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateSetting: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Insert a new setting into the database
        /// </summary>
        /// <param name="settingKey">The setting key</param>
        /// <param name="settingValue">The setting value</param>
        /// <param name="description">Optional description</param>
        /// <returns>True if successful</returns>
        private bool InsertSetting(string settingKey, string settingValue, string description = null)
        {
            string insertQuery = "INSERT INTO library_settings (setting_key, setting_value, description) VALUES (@settingKey, @settingValue, @description)";
            
            try
            {
                int rowsAffected = ExecuteNonQuery(insertQuery,
                    new OleDbParameter("@settingKey", settingKey),
                    new OleDbParameter("@settingValue", settingValue),
                    new OleDbParameter("@description", description ?? GetDefaultDescription(settingKey)));
                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in InsertSetting: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get default description for a setting key
        /// </summary>
        private string GetDefaultDescription(string settingKey)
        {
            return settingKey switch
            {
                "FINE_PER_DAY" => "Fine amount per day for overdue books (in dollars)",
                "LOAN_PERIOD_DAYS" => "Default loan period in days",
                "MAX_LOANS_PER_MEMBER" => "Maximum number of active loans per member",
                "MAX_RESERVATIONS_PER_MEMBER" => "Maximum number of active reservations per member",
                _ => ""
            };
        }

        /// <summary>
        /// Get fine per day as a decimal value
        /// </summary>
        public decimal GetFinePerDay()
        {
            string value = GetSetting("FINE_PER_DAY", "1.00");
            if (decimal.TryParse(value, out decimal finePerDay))
            {
                return finePerDay;
            }
            return 1.00m; // Default to $1.00
        }

        /// <summary>
        /// Get loan period in days as an integer
        /// </summary>
        public int GetLoanPeriodDays()
        {
            string value = GetSetting("LOAN_PERIOD_DAYS", "7");
            if (int.TryParse(value, out int loanPeriod))
            {
                return loanPeriod;
            }
            return 7; // Default to 7 days
        }

        /// <summary>
        /// Get maximum loans per member as an integer
        /// </summary>
        public int GetMaxLoansPerMember()
        {
            string value = GetSetting("MAX_LOANS_PER_MEMBER", "3");
            if (int.TryParse(value, out int maxLoans))
            {
                return maxLoans;
            }
            return 3; // Default to 3 loans
        }

        /// <summary>
        /// Get maximum reservations per member as an integer
        /// </summary>
        public int GetMaxReservationsPerMember()
        {
            string value = GetSetting("MAX_RESERVATIONS_PER_MEMBER", "3");
            if (int.TryParse(value, out int maxReservations))
            {
                return maxReservations;
            }
            return 3; // Default to 3 reservations
        }
    }
}

