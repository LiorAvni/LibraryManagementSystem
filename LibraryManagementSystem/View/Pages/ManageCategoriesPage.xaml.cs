using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class ManageCategoriesPage : Page
    {
        private readonly CategoryDB _categoryDB;
        private ObservableCollection<CategoryModel> allCategories;
        private ObservableCollection<CategoryModel> filteredCategories;
        
        private int currentPage = 0;
        private int pageSize = 20;
        private int totalPages = 0;
        private int totalRecords = 0;

        public ManageCategoriesPage()
        {
            InitializeComponent();
            _categoryDB = new CategoryDB();
            allCategories = new ObservableCollection<CategoryModel>();
            filteredCategories = new ObservableCollection<CategoryModel>();
            dgCategories.ItemsSource = filteredCategories;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                DataTable dt = _categoryDB.GetAllCategories();
                
                allCategories.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    allCategories.Add(new CategoryModel
                    {
                        CategoryId = row["category_id"].ToString(),
                        Name = row["name"].ToString(),
                        Description = row["description"] != DBNull.Value ? row["description"].ToString() : ""
                    });
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            var searchText = txtSearchName.Text?.Trim().ToLower() ?? "";

            // Filter categories
            var filtered = allCategories.Where(category =>
            {
                if (string.IsNullOrEmpty(searchText))
                    return true;

                return category.Name.ToLower().Contains(searchText);
            }).ToList();

            // Update total records and pages
            totalRecords = filtered.Count;
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            
            // Reset to first page if current page is out of range
            if (currentPage >= totalPages && totalPages > 0)
            {
                currentPage = 0;
            }

            // Get categories for current page
            filteredCategories.Clear();
            var pagedCategories = filtered.Skip(currentPage * pageSize).Take(pageSize);
            foreach (var category in pagedCategories)
            {
                filteredCategories.Add(category);
            }

            // Update UI
            UpdatePaginationUI();
            UpdateFilterInfo();
        }

        private void UpdatePaginationUI()
        {
            if (totalPages > 0)
            {
                txtPaginationInfo.Text = $"Page {currentPage + 1} of {totalPages} ({totalRecords} records)";
                btnFirstPage.IsEnabled = currentPage > 0;
                btnPrevPage.IsEnabled = currentPage > 0;
                btnNextPage.IsEnabled = currentPage < totalPages - 1;
                btnLastPage.IsEnabled = currentPage < totalPages - 1;
                paginationSection.Visibility = Visibility.Visible;
            }
            else
            {
                txtPaginationInfo.Text = "No records found";
                btnFirstPage.IsEnabled = false;
                btnPrevPage.IsEnabled = false;
                btnNextPage.IsEnabled = false;
                btnLastPage.IsEnabled = false;
                paginationSection.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateFilterInfo()
        {
            string infoText = $"{totalRecords} category(ies) found";
            if (totalPages > 1)
            {
                infoText += $" | Page {currentPage + 1} of {totalPages}";
            }
            txtFilterInfo.Text = infoText;
        }

        // Search handlers
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            currentPage = 0;
            ApplyFilters();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            txtSearchName.Clear();
            currentPage = 0;
            ApplyFilters();
        }

        private void TxtSearchName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search_Click(sender, e);
            }
        }

        // Pagination handlers
        private void FirstPage_Click(object sender, RoutedEventArgs e)
        {
            currentPage = 0;
            ApplyFilters();
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                ApplyFilters();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < totalPages - 1)
            {
                currentPage++;
                ApplyFilters();
            }
        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            currentPage = totalPages - 1;
            ApplyFilters();
        }

        // Action handlers
        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddCategoryPage());
        }

        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CategoryModel category)
            {
                NavigationService?.Navigate(new EditCategoryPage(category.CategoryId));
            }
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CategoryModel category)
            {
                // Check if category has books
                try
                {
                    int bookCount = _categoryDB.GetCategoryBookCount(category.CategoryId);
                    
                    if (bookCount > 0)
                    {
                        MessageBox.Show(
                            $"Cannot delete this category.\n\n" +
                            $"{category.Name} is currently assigned to {bookCount} book(s) in the library.\n\n" +
                            "Please reassign or remove all books from this category before deleting.",
                            "Cannot Delete Category",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    // Confirm deletion
                    var result = MessageBox.Show(
                        $"Are you sure you want to delete this category?\n\n" +
                        $"Name: {category.Name}\n\n" +
                        "This action cannot be undone.",
                        "Confirm Delete",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        bool success = _categoryDB.DeleteCategory(category.CategoryId);
                        
                        if (success)
                        {
                            MessageBox.Show(
                                $"Category '{category.Name}' has been deleted successfully.",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            
                            LoadCategories(); // Reload the list
                        }
                        else
                        {
                            MessageBox.Show(
                                "Failed to delete category. Please try again.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting category: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    /// <summary>
    /// Model for displaying category information
    /// </summary>
    public class CategoryModel
    {
        public string CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string DescriptionPreview
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Description))
                    return "N/A";
                
                return Description.Length > 100 
                    ? Description.Substring(0, 100) + "..." 
                    : Description;
            }
        }
    }
}
