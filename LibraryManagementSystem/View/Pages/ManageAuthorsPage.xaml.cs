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
    public partial class ManageAuthorsPage : Page
    {
        private readonly BookDB _bookDB;
        private ObservableCollection<AuthorModel> allAuthors;
        private ObservableCollection<AuthorModel> filteredAuthors;
        
        private int currentPage = 0;
        private int pageSize = 20;
        private int totalPages = 0;
        private int totalRecords = 0;

        public ManageAuthorsPage()
        {
            InitializeComponent();
            _bookDB = new BookDB();
            allAuthors = new ObservableCollection<AuthorModel>();
            filteredAuthors = new ObservableCollection<AuthorModel>();
            dgAuthors.ItemsSource = filteredAuthors;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAuthors();
        }

        private void LoadAuthors()
        {
            try
            {
                DataTable dt = _bookDB.GetAllAuthors();
                
                allAuthors.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    allAuthors.Add(new AuthorModel
                    {
                        AuthorId = row["author_id"].ToString(),
                        FirstName = row["first_name"].ToString(),
                        LastName = row["last_name"].ToString(),
                        Biography = row["biography"] != DBNull.Value ? row["biography"].ToString() : "",
                        BirthDate = row["birth_date"] != DBNull.Value ? Convert.ToDateTime(row["birth_date"]) : (DateTime?)null
                    });
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading authors: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            var searchText = txtSearchName.Text?.Trim().ToLower() ?? "";

            // Filter authors
            var filtered = allAuthors.Where(author =>
            {
                if (string.IsNullOrEmpty(searchText))
                    return true;

                return author.FirstName.ToLower().Contains(searchText) ||
                       author.LastName.ToLower().Contains(searchText) ||
                       author.FullName.ToLower().Contains(searchText);
            }).ToList();

            // Update total records and pages
            totalRecords = filtered.Count;
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            
            // Reset to first page if current page is out of range
            if (currentPage >= totalPages && totalPages > 0)
            {
                currentPage = 0;
            }

            // Get authors for current page
            filteredAuthors.Clear();
            var pagedAuthors = filtered.Skip(currentPage * pageSize).Take(pageSize);
            foreach (var author in pagedAuthors)
            {
                filteredAuthors.Add(author);
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
            string infoText = $"{totalRecords} author(s) found";
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
        private void AddAuthor_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddAuthorPage());
        }

        private void EditAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is AuthorModel author)
            {
                NavigationService?.Navigate(new EditAuthorPage(author.AuthorId));
            }
        }

        private void DeleteAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is AuthorModel author)
            {
                // Check if author has books
                try
                {
                    int bookCount = _bookDB.GetAuthorBookCount(author.AuthorId);
                    
                    if (bookCount > 0)
                    {
                        MessageBox.Show(
                            $"Cannot delete this author.\n\n" +
                            $"{author.FullName} is currently listed as an author of {bookCount} book(s) in the library.\n\n" +
                            "Please remove the author from all books before deleting.",
                            "Cannot Delete Author",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    // Confirm deletion
                    var result = MessageBox.Show(
                        $"Are you sure you want to delete this author?\n\n" +
                        $"Name: {author.FullName}\n\n" +
                        "This action cannot be undone.",
                        "Confirm Delete",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        bool success = _bookDB.DeleteAuthor(author.AuthorId);
                        
                        if (success)
                        {
                            MessageBox.Show(
                                $"Author '{author.FullName}' has been deleted successfully.",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            
                            LoadAuthors(); // Reload the list
                        }
                        else
                        {
                            MessageBox.Show(
                                "Failed to delete author. Please try again.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting author: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Navigation handler
        private void NavDashboard_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LibrarianDashboard());
        }
    }

    /// <summary>
    /// Model for displaying author information
    /// </summary>
    public class AuthorModel
    {
        public string AuthorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Biography { get; set; }
        public DateTime? BirthDate { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public string BiographyPreview
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Biography))
                    return "N/A";
                
                return Biography.Length > 100 
                    ? Biography.Substring(0, 100) + "..." 
                    : Biography;
            }
        }

        public string BirthDateFormatted
        {
            get
            {
                if (BirthDate.HasValue)
                    return BirthDate.Value.ToString("MMM dd, yyyy");
                return "N/A";
            }
        }
    }
}
