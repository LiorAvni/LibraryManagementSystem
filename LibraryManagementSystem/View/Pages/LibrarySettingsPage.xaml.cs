using System;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class LibrarySettingsPage : Page
    {
        private readonly SettingsDB _settingsDB;

        public LibrarySettingsPage()
        {
            InitializeComponent();
            _settingsDB = new SettingsDB();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                // Load current settings from database
                txtFinePerDay.Text = _settingsDB.GetSetting("FINE_PER_DAY", "1.00");
                txtLoanPeriodDays.Text = _settingsDB.GetSetting("LOAN_PERIOD_DAYS", "7");
                txtMaxLoansPerMember.Text = _settingsDB.GetSetting("MAX_LOANS_PER_MEMBER", "3");
                txtMaxReservationsPerMember.Text = _settingsDB.GetSetting("MAX_RESERVATIONS_PER_MEMBER", "3");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                if (!ValidateInputs())
                {
                    return;
                }

                // Save settings to database
                bool success = true;
                success &= _settingsDB.UpdateSetting("FINE_PER_DAY", txtFinePerDay.Text.Trim());
                success &= _settingsDB.UpdateSetting("LOAN_PERIOD_DAYS", txtLoanPeriodDays.Text.Trim());
                success &= _settingsDB.UpdateSetting("MAX_LOANS_PER_MEMBER", txtMaxLoansPerMember.Text.Trim());
                success &= _settingsDB.UpdateSetting("MAX_RESERVATIONS_PER_MEMBER", txtMaxReservationsPerMember.Text.Trim());

                if (success)
                {
                    MessageBox.Show(
                        "Settings saved successfully!\n\nNew settings will take effect immediately.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Failed to save some settings. Please try again.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInputs()
        {
            // Validate Fine Per Day (must be a positive decimal)
            if (!decimal.TryParse(txtFinePerDay.Text.Trim(), out decimal finePerDay) || finePerDay < 0)
            {
                MessageBox.Show(
                    "Fine Per Day must be a valid positive number (e.g., 1.00, 0.50)",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtFinePerDay.Focus();
                return false;
            }

            // Validate Loan Period Days (must be a positive integer)
            if (!int.TryParse(txtLoanPeriodDays.Text.Trim(), out int loanPeriod) || loanPeriod <= 0)
            {
                MessageBox.Show(
                    "Loan Period Days must be a positive whole number (e.g., 7, 14, 21)",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtLoanPeriodDays.Focus();
                return false;
            }

            // Validate Max Loans Per Member (must be a positive integer)
            if (!int.TryParse(txtMaxLoansPerMember.Text.Trim(), out int maxLoans) || maxLoans <= 0)
            {
                MessageBox.Show(
                    "Maximum Loans must be a positive whole number (e.g., 3, 5, 10)",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtMaxLoansPerMember.Focus();
                return false;
            }

            // Validate Max Reservations Per Member (must be a positive integer)
            if (!int.TryParse(txtMaxReservationsPerMember.Text.Trim(), out int maxReservations) || maxReservations <= 0)
            {
                MessageBox.Show(
                    "Maximum Reservations must be a positive whole number (e.g., 3, 5, 10)",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtMaxReservationsPerMember.Focus();
                return false;
            }

            return true;
        }
    }
}
