using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LibraryManagementSystem.Service;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class SearchResultsPage : Page
    {
        private readonly LibraryService _libraryService;
        private readonly BookDB _bookDB;
        private readonly LoanDB _loanDB;
        private readonly ReservationDB _reservationDB;
        private ObservableCollection<BookDisplayModel> searchResults;
        private string _searchKeyword;
        private string _searchType;

        public SearchResultsPage(string keyword, string searchType = "all")
        {
            InitializeComponent();
            _libraryService = new LibraryService();
            _bookDB = new BookDB();
            _loanDB = new LoanDB();
            _reservationDB = new ReservationDB();
            searchResults = new ObservableCollection<BookDisplayModel>();
            booksGrid.ItemsSource = searchResults;
            
            _searchKeyword = keyword;
            _searchType = searchType;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSearchResults();
        }

        private void LoadSearchResults()
        {
            try
            {
                // Update search summary
                string searchTypeText = _searchType switch
                {
                    "title" => "titles",
                    "author" => "authors",
                    "isbn" => "ISBN",
                    "category" => "categories",
                    _ => "all fields"
                };

                txtSearchQuery.Text = $"Searching in {searchTypeText} for: \"{_searchKeyword}\"";
                
                // Get search results from database
                DataTable dt = _bookDB.SearchBooksWithDetails(_searchKeyword, _searchType);
                
                searchResults.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    searchResults.Add(new BookDisplayModel
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
                        Description = ""
                    });
                }

                // Update results count
                txtTotalResults.Text = $"Found {searchResults.Count} book(s)";

                // Show/hide no results panel
                if (searchResults.Count == 0)
                {
                    booksGrid.Visibility = Visibility.Collapsed;
                    noResultsPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    booksGrid.Visibility = Visibility.Visible;
                    noResultsPanel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading search results: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Show no results on error
                booksGrid.Visibility = Visibility.Collapsed;
                noResultsPanel.Visibility = Visibility.Visible;
                txtTotalResults.Text = "Error loading results";
            }
        }

        private void BackToSearch_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SearchBooksPage());
        }

        private void BrowseAll_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SearchBooksPage());
        }

        private void BorrowBook_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BookDisplayModel book)
            {
                try
                {
                    var currentUser = MainWindow.CurrentUser;
                    
                    if (currentUser == null || string.IsNullOrEmpty(currentUser.UserIdString))
                    {
                        MessageBox.Show("Please login to borrow books.", "Login Required", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (!book.IsAvailable)
                    {
                        MessageBox.Show("This book is currently not available for borrowing.", "Not Available", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    var result = MessageBox.Show(
                        $"Are you sure you want to borrow '{book.Title}'?{Environment.NewLine}{Environment.NewLine}" +
                        $"Author: {book.Author}{Environment.NewLine}" +
                        $"Available: {book.AvailableCopies} / {book.TotalCopies}{Environment.NewLine}{Environment.NewLine}" +
                        $"Loan Period: 14 days{Environment.NewLine}" +
                        $"Due Date: {DateTime.Now.AddDays(14):yyyy-MM-dd}",
                        "Confirm Borrow",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Borrow the book
                        bool success = _loanDB.BorrowBook(book.BookId, currentUser.UserIdString, 14);
                        
                        if (success)
                        {
                            MessageBox.Show(
                                $"Book '{book.Title}' borrowed successfully!{Environment.NewLine}{Environment.NewLine}" +
                                $"Due date: {DateTime.Now.AddDays(14):yyyy-MM-dd}{Environment.NewLine}" +
                                "Please return the book on or before the due date to avoid fines.",
                                "Success", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Information);
                            
                            // Reload search results to update available copies
                            LoadSearchResults();
                        }
                        else
                        {
                            MessageBox.Show("Failed to borrow the book. Please try again.", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error borrowing book: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ReserveBook_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BookDisplayModel book)
            {
                try
                {
                    var currentUser = MainWindow.CurrentUser;
                    
                    if (currentUser == null || string.IsNullOrEmpty(currentUser.UserIdString))
                    {
                        MessageBox.Show("Please login to reserve books.", "Login Required", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var message = book.IsAvailable
                        ? $"Reserve '{book.Title}' for later pickup?{Environment.NewLine}{Environment.NewLine}" +
                          $"Current status: {book.AvailableCopies} / {book.TotalCopies} available{Environment.NewLine}{Environment.NewLine}" +
                          "The book will be held for you for 7 days."
                        : $"'{book.Title}' is currently unavailable.{Environment.NewLine}{Environment.NewLine}" +
                          "Reserve to be notified when it becomes available?{Environment.NewLine}" +
                          "Reservation will expire in 7 days if not fulfilled.";

                    var result = MessageBox.Show(
                        message,
                        "Confirm Reservation",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Reserve the book
                        bool success = _reservationDB.ReserveBook(book.BookId, currentUser.UserIdString, 7);
                        
                        if (success)
                        {
                            MessageBox.Show(
                                $"Book '{book.Title}' reserved successfully!{Environment.NewLine}{Environment.NewLine}" +
                                "You will be notified when the book is available for pickup.{Environment.NewLine}" +
                                $"Reservation expires: {DateTime.Now.AddDays(7):yyyy-MM-dd}",
                                "Success", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to reserve the book. Please try again.", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reserving book: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

