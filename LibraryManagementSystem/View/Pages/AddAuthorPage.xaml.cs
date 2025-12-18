using System;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class AddAuthorPage : Page
    {
        private readonly BookDB _bookDB;

        public AddAuthorPage()
        {
            InitializeComponent();
            _bookDB = new BookDB();
        }

        private void AddAuthor_Click(object sender, RoutedEventArgs e)
        {
            // Hide previous error
            borderError.Visibility = Visibility.Collapsed;

            // Validate input
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();

            if (string.IsNullOrEmpty(firstName))
            {
                ShowError("First Name is required.");
                txtFirstName.Focus();
                return;
            }

            if (string.IsNullOrEmpty(lastName))
            {
                ShowError("Last Name is required.");
                txtLastName.Focus();
                return;
            }

            try
            {
                // Generate new author ID
                string authorId = Guid.NewGuid().ToString();

                // Get birth date (nullable)
                DateTime? birthDate = dpBirthDate.SelectedDate;

                // Get biography (optional)
                string biography = txtBiography.Text.Trim();
                if (string.IsNullOrEmpty(biography))
                    biography = null;

                // Insert author
                bool success = _bookDB.InsertAuthor(authorId, firstName, lastName, biography, birthDate);

                if (success)
                {
                    MessageBox.Show(
                        $"Author '{firstName} {lastName}' has been added successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Navigate back to Manage Authors page
                    NavigationService?.Navigate(new ManageAuthorsPage());
                }
                else
                {
                    ShowError("Failed to add author. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error adding author: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ManageAuthorsPage());
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            borderError.Visibility = Visibility.Visible;
        }
    }
}
