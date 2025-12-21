using System;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class AddCategoryPage : Page
    {
        private readonly CategoryDB _categoryDB;

        public AddCategoryPage()
        {
            InitializeComponent();
            _categoryDB = new CategoryDB();
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            // Hide previous error
            borderError.Visibility = Visibility.Collapsed;

            // Validate inputs
            if (!ValidateInputs())
                return;

            try
            {
                string name = txtName.Text.Trim();
                string description = txtDescription.Text.Trim();

                // Check if category name already exists
                if (_categoryDB.CategoryNameExists(name))
                {
                    ShowError($"A category with the name '{name}' already exists. Please use a different name.");
                    return;
                }

                // Insert category
                bool success = _categoryDB.InsertCategory(name, description);

                if (success)
                {
                    MessageBox.Show(
                        $"Category '{name}' has been added successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Navigate back to Manage Categories page
                    NavigationService?.Navigate(new ManageCategoriesPage());
                }
                else
                {
                    ShowError("Failed to add category. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error adding category: {ex.Message}");
            }
        }

        private bool ValidateInputs()
        {
            // Validate name
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                ShowError("Please enter a category name.");
                txtName.Focus();
                return false;
            }

            // Check name length
            if (txtName.Text.Trim().Length > 50)
            {
                ShowError("Category name cannot exceed 50 characters.");
                txtName.Focus();
                return false;
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
            // Navigate back to Manage Categories page
            NavigationService?.Navigate(new ManageCategoriesPage());
        }
    }
}
