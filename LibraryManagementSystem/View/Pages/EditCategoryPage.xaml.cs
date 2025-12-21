using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class EditCategoryPage : Page
    {
        private readonly CategoryDB _categoryDB;
        private readonly string _categoryId;

        public EditCategoryPage(string categoryId)
        {
            InitializeComponent();
            _categoryDB = new CategoryDB();
            _categoryId = categoryId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategoryData();
        }

        private void LoadCategoryData()
        {
            try
            {
                DataRow category = _categoryDB.GetCategoryById(_categoryId);

                if (category != null)
                {
                    txtName.Text = category["name"].ToString();
                    txtDescription.Text = category["description"] != DBNull.Value ? category["description"].ToString() : "";
                }
                else
                {
                    MessageBox.Show(
                        "Category not found.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    
                    NavigationService?.Navigate(new ManageCategoriesPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading category: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                NavigationService?.Navigate(new ManageCategoriesPage());
            }
        }

        private void UpdateCategory_Click(object sender, RoutedEventArgs e)
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

                // Check if category name already exists (excluding current category)
                if (_categoryDB.CategoryNameExists(name, _categoryId))
                {
                    ShowError($"A category with the name '{name}' already exists. Please use a different name.");
                    return;
                }

                // Update category
                bool success = _categoryDB.UpdateCategory(_categoryId, name, description);

                if (success)
                {
                    MessageBox.Show(
                        $"Category '{name}' has been updated successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Navigate back to Manage Categories page
                    NavigationService?.Navigate(new ManageCategoriesPage());
                }
                else
                {
                    ShowError("Failed to update category. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error updating category: {ex.Message}");
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
