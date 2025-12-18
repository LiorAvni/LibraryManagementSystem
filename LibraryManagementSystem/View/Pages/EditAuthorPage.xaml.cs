using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class EditAuthorPage : Page
    {
        private readonly BookDB _bookDB;
        private readonly string _authorId;

        public EditAuthorPage(string authorId)
        {
            InitializeComponent();
            _bookDB = new BookDB();
            _authorId = authorId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAuthorData();
        }

        private void LoadAuthorData()
        {
            try
            {
                DataTable dt = _bookDB.GetAuthorById(_authorId);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    txtAuthorId.Text = _authorId;
                    txtFirstName.Text = row["first_name"].ToString();
                    txtLastName.Text = row["last_name"].ToString();
                    
                    if (row["birth_date"] != DBNull.Value)
                    {
                        dpBirthDate.SelectedDate = Convert.ToDateTime(row["birth_date"]);
                    }

                    if (row["biography"] != DBNull.Value)
                    {
                        txtBiography.Text = row["biography"].ToString();
                    }
                }
                else
                {
                    MessageBox.Show("Author not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService?.Navigate(new ManageAuthorsPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading author: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService?.Navigate(new ManageAuthorsPage());
            }
        }

        private void UpdateAuthor_Click(object sender, RoutedEventArgs e)
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
                // Get birth date (nullable)
                DateTime? birthDate = dpBirthDate.SelectedDate;

                // Get biography (optional)
                string biography = txtBiography.Text.Trim();
                if (string.IsNullOrEmpty(biography))
                    biography = null;

                // Update author
                bool success = _bookDB.UpdateAuthor(_authorId, firstName, lastName, biography, birthDate);

                if (success)
                {
                    MessageBox.Show(
                        $"Author '{firstName} {lastName}' has been updated successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Navigate back to Manage Authors page
                    NavigationService?.Navigate(new ManageAuthorsPage());
                }
                else
                {
                    ShowError("Failed to update author. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error updating author: {ex.Message}");
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
