using System;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class AddPublisherPage : Page
    {
        private readonly PublisherDB _publisherDB;

        public AddPublisherPage()
        {
            InitializeComponent();
            _publisherDB = new PublisherDB();
        }

        private void AddPublisher_Click(object sender, RoutedEventArgs e)
        {
            // Hide previous error
            borderError.Visibility = Visibility.Collapsed;

            // Validate inputs
            if (!ValidateInputs())
                return;

            try
            {
                string name = txtName.Text.Trim();
                string country = txtCountry.Text.Trim();
                string website = txtWebsite.Text.Trim();
                int? foundedYear = null;

                // Parse founded year if provided
                if (!string.IsNullOrWhiteSpace(txtFoundedYear.Text))
                {
                    if (int.TryParse(txtFoundedYear.Text.Trim(), out int year))
                    {
                        foundedYear = year;
                    }
                    else
                    {
                        ShowError("Founded Year must be a valid number.");
                        return;
                    }
                }

                // Check if publisher name already exists
                if (_publisherDB.PublisherNameExists(name))
                {
                    ShowError($"A publisher with the name '{name}' already exists. Please use a different name.");
                    return;
                }

                // Insert publisher
                bool success = _publisherDB.InsertPublisher(name, country, website, foundedYear);

                if (success)
                {
                    MessageBox.Show(
                        $"Publisher '{name}' has been added successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Navigate back to Manage Publishers page
                    NavigationService?.Navigate(new ManagePublishersPage());
                }
                else
                {
                    ShowError("Failed to add publisher. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error adding publisher: {ex.Message}");
            }
        }

        private bool ValidateInputs()
        {
            // Validate name
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                ShowError("Please enter a publisher name.");
                txtName.Focus();
                return false;
            }

            // Check name length
            if (txtName.Text.Trim().Length > 100)
            {
                ShowError("Publisher name cannot exceed 100 characters.");
                txtName.Focus();
                return false;
            }

            // Validate founded year if provided
            if (!string.IsNullOrWhiteSpace(txtFoundedYear.Text))
            {
                if (!int.TryParse(txtFoundedYear.Text.Trim(), out int year))
                {
                    ShowError("Founded Year must be a valid number.");
                    txtFoundedYear.Focus();
                    return false;
                }

                int currentYear = DateTime.Now.Year;
                if (year < 1000 || year > currentYear)
                {
                    ShowError($"Founded Year must be between 1000 and {currentYear}.");
                    txtFoundedYear.Focus();
                    return false;
                }
            }

            return true;
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            borderError.Visibility = Visibility.Visible;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to Manage Publishers page
            NavigationService?.Navigate(new ManagePublishersPage());
        }
    }
}
