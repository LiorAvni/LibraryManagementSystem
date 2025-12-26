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
    public partial class ManageBooksPage : Page
    {
        private readonly BookDB _bookDB;
        private readonly CategoryDB _categoryDB;
        private ObservableCollection<BookManageModel> allBooks;
        private ObservableCollection<BookManageModel> filteredBooks;
        
        private int currentPage = 0;
        private int pageSize = 20;
        private int totalPages = 0;
        private int totalRecords = 0;

        public ManageBooksPage()
        {
            InitializeComponent();
            
            _bookDB = new BookDB();
            _categoryDB = new CategoryDB();
            allBooks = new ObservableCollection<BookManageModel>();
            filteredBooks = new ObservableCollection<BookManageModel>();
            booksDataGrid.ItemsSource = filteredBooks;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategories();
            LoadBooks();
        }

        private void LoadCategories()
        {
            try
            {
                // Get all categories from database
                DataTable categories = _categoryDB.GetAllCategories();
                
                // Clear existing items (keep "All Categories")
                cmbCategory.Items.Clear();
                cmbCategory.Items.Add(new ComboBoxItem { Content = "All Categories", Tag = "", IsSelected = true });
                
                // Add categories
                foreach (DataRow row in categories.Rows)
                {
                    cmbCategory.Items.Add(new ComboBoxItem 
                    { 
                        Content = row["name"].ToString(), 
                        Tag = row["name"].ToString() 
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBooks()
        {
            try
            {
                // Get ALL books from database with full details
                DataTable dt = _bookDB.GetAllBooksWithDetails();
                
                allBooks.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    allBooks.Add(new BookManageModel
                    {
                        BookId = row["book_id"]?.ToString() ?? "",
                        Title = row["title"]?.ToString() ?? "Unknown Title",
                        Author = row["Author"]?.ToString() ?? "Unknown Author",
                        Publisher = row["Publisher"]?.ToString() ?? "Unknown Publisher",
                        Year = row["publication_year"] != DBNull.Value ? row["publication_year"].ToString() : "N/A",
                        Category = row["Category"]?.ToString() ?? "Uncategorized",
                        AvailableCopies = row["AvailableCopies"] != DBNull.Value ? Convert.ToInt32(row["AvailableCopies"]) : 0,
                        TotalCopies = row["TotalCopies"] != DBNull.Value ? Convert.ToInt32(row["TotalCopies"]) : 0,
                        ISBN = row["isbn"]?.ToString() ?? "",
                        Description = "" // Description not returned by query, set to empty
                    });
                }

                // Apply filters (initially shows all books)
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading books: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            var searchText = txtSearch.Text?.Trim().ToLower() ?? "";
            var selectedCategory = (cmbCategory.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "";

            // Filter books
            var filtered = allBooks.Where(book =>
            {
                // Search filter (Title, Author, Publisher, ISBN)
                var matchesSearch = string.IsNullOrEmpty(searchText) ||
                    book.Title.ToLower().Contains(searchText) ||
                    book.Author.ToLower().Contains(searchText) ||
                    book.Publisher.ToLower().Contains(searchText) ||
                    book.ISBN.ToLower().Contains(searchText);

                // Category filter
                var matchesCategory = string.IsNullOrEmpty(selectedCategory) ||
                    book.Category.Equals(selectedCategory, StringComparison.OrdinalIgnoreCase);

                return matchesSearch && matchesCategory;
            }).ToList();

            // Update total records and pages
            totalRecords = filtered.Count;
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            
            // Reset to first page if current page is out of range
            if (currentPage >= totalPages && totalPages > 0)
            {
                currentPage = 0;
            }

            // Get books for current page
            filteredBooks.Clear();
            var pagedBooks = filtered.Skip(currentPage * pageSize).Take(pageSize);
            foreach (var book in pagedBooks)
            {
                filteredBooks.Add(book);
            }

            // Update pagination UI
            UpdatePaginationUI();
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
            }
            else
            {
                txtPaginationInfo.Text = "No records found";
                btnFirstPage.IsEnabled = false;
                btnPrevPage.IsEnabled = false;
                btnNextPage.IsEnabled = false;
                btnLastPage.IsEnabled = false;
            }
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            currentPage = 0; // Reset to first page when filtering
            ApplyFilters();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cmbCategory.SelectedIndex = 0;
            currentPage = 0;
            ApplyFilters();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Filter_Click(sender, e);
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

        // Action button handlers
        private void AddNewBook_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to Add Book page
            NavigationService?.Navigate(new AddBookPage());
        }

        private void EditBook_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BookManageModel book)
            {
                // Navigate to Edit Book page with book ID
                NavigationService?.Navigate(new EditBookPage(book.BookId));
            }
        }

        private void RetireBook_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BookManageModel book)
            {
                try
                {
                    // Get copy statistics
                    var stats = _bookDB.GetBookCopyStatistics(book.BookId);
                    int totalCopies = stats.Total;
                    int availableCopies = stats.Available;
                    int unavailableCopies = stats.Unavailable;

                    // Determine the appropriate message based on available copies
                    string message;
                    MessageBoxImage icon;
                    
                    if (availableCopies == 0)
                    {
                        // No copies available to retire
                        message = $"There are no available copies of '{book.Title}' to retire.\n\n" +
                                 $"Total copies: {totalCopies}\n" +
                                 $"Available: {availableCopies}\n" +
                                 $"Unavailable (Borrowed/Reserved/etc.): {unavailableCopies}\n\n" +
                                 "Only available copies can be retired. Please wait for borrowed or reserved copies to be returned.";
                        icon = MessageBoxImage.Information;
                        
                        MessageBox.Show(message, "No Available Copies", MessageBoxButton.OK, icon);
                        return;
                    }
                    else if (availableCopies == totalCopies)
                    {
                        // All copies are available
                        message = $"All {totalCopies} copies of '{book.Title}' are currently available.\n\n" +
                                 "Would you like to retire them all?";
                        icon = MessageBoxImage.Question;
                    }
                    else
                    {
                        // Some copies are available
                        message = $"{availableCopies} out of {totalCopies} copies of '{book.Title}' are currently available.\n\n" +
                                 $"Available copies: {availableCopies}\n" +
                                 $"Unavailable copies: {unavailableCopies}\n\n" +
                                 "Would you like to retire all available copies?";
                        icon = MessageBoxImage.Question;
                    }

                    var result = MessageBox.Show(message, "Confirm Retire Book Copies", MessageBoxButton.YesNo, icon);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Retire available copies
                        int retiredCount = _bookDB.RetireAvailableCopies(book.BookId);
                        
                        if (retiredCount > 0)
                        {
                            MessageBox.Show(
                                $"Successfully retired {retiredCount} copy/copies of '{book.Title}'.\n\n" +
                                $"Retired copies: {retiredCount}\n" +
                                $"Remaining unavailable copies: {unavailableCopies}",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            
                            // Reload books to update the display
                            LoadBooks();
                        }
                        else
                        {
                            MessageBox.Show(
                                "No copies were retired. Please try again.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error retiring book: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LendBook_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BookManageModel book)
            {
                if (!book.IsAvailable)
                {
                    MessageBox.Show("This book has no available copies to lend.", "Not Available", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Get current user from MainWindow
                var currentUser = MainWindow.CurrentUser;
                if (currentUser == null || string.IsNullOrEmpty(currentUser.UserIdString))
                {
                    MessageBox.Show("Please login to lend books.", "Login Required", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get librarian ID
                LoanDB loanDB = new LoanDB();
                string librarianId = loanDB.GetLibrarianIdByUserId(currentUser.UserIdString);
                
                if (string.IsNullOrEmpty(librarianId))
                {
                    MessageBox.Show("Librarian record not found.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Navigate to Lend Book page
                NavigationService?.Navigate(new LendBookPage(book.BookId, librarianId));
            }
        }
    }

    /// <summary>
    /// Model for displaying books in the manage books grid
    /// </summary>
    public class BookManageModel
    {
        public string BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public string Year { get; set; }
        public string Category { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public string ISBN { get; set; }
        public string Description { get; set; }

        public bool IsAvailable => AvailableCopies > 0;

        public string StatusText
        {
            get
            {
                if (TotalCopies == 0)
                    return "Retired";
                else if (AvailableCopies > 0)
                    return "Available";
                else
                    return "Unavailable";
            }
        }

        public Style StatusBadgeStyle
        {
            get
            {
                var page = Application.Current.Windows.OfType<Window>().FirstOrDefault()?.Content as Frame;
                if (page == null) return null;

                if (TotalCopies == 0)
                    return page.FindResource("BadgeRetired") as Style;
                else if (AvailableCopies > 0)
                    return page.FindResource("BadgeAvailable") as Style;
                else
                    return page.FindResource("BadgeUnavailable") as Style;
            }
        }

        public Style StatusTextStyle
        {
            get
            {
                var page = Application.Current.Windows.OfType<Window>().FirstOrDefault()?.Content as Frame;
                if (page == null) return null;

                if (TotalCopies == 0)
                    return page.FindResource("BadgeRetiredText") as Style;
                else if (AvailableCopies > 0)
                    return page.FindResource("BadgeAvailableText") as Style;
                else
                    return page.FindResource("BadgeUnavailableText") as Style;
            }
        }
    }
}
