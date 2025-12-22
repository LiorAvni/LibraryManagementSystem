using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class EditPublisherPage : Page
    {
        private readonly PublisherDB _publisherDB;
        private readonly string _publisherId;

        public EditPublisherPage(string publisherId)
        {
            InitializeComponent();
            _publisherDB = new PublisherDB();
            _publisherId = publisherId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPublisherData();
        }

        private void LoadPublisherData()
        {
            try
            {
                DataRow publisher = _publisherDB.GetPublisherById(_publisherId);

                if (publisher != null)
                {
                    txtName.Text = publisher["name"].ToString();
                    txtCountry.Text = publisher["country"] != DBNull.Value ? publisher["country"].ToString() : "";
                    txtWebsite.Text = publisher["website"] != DBNull.Value ? publisher["website"].ToString() : "";
                    
                    if (publisher["founded_year"] != DBNull.Value)
                    {
                        txtFoundedYear.Text = publisher["founded_year"].ToString();
                    }
                }
                else
                {
                    MessageBox.Show(
                        "Publisher not found.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    
                    NavigationService?.Navigate(new ManagePublishersPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading publisher: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                NavigationService?.Navigate(new ManagePublishersPage());
            }
        }

        private void UpdatePublisher_Click(object sender, RoutedEventArgs e)
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

                // Check if publisher name already exists (excluding current publisher)
                if (_publisherDB.PublisherNameExists(name, _publisherId))
                {
                    ShowError($"A publisher with the name '{name}' already exists. Please use a different name.");
                    return;
                }

                // Update publisher
                bool success = _publisherDB.UpdatePublisher(_publisherId, name, country, website, foundedYear);

                if (success)
                {
                    MessageBox.Show(
                        $"Publisher '{name}' has been updated successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Navigate back to Manage Publishers page
                    NavigationService?.Navigate(new ManagePublishersPage());
                }
                else
                {
                    ShowError("Failed to update publisher. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error updating publisher: {ex.Message}");
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
