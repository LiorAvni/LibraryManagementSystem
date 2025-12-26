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
    public partial class SearchBooksPage : Page
    {
        private readonly BookDB _bookDB;
        private readonly LoanDB _loanDB;
        private readonly ReservationDB _reservationDB;
        private ObservableCollection<BookDisplayModel> allBooks;
        private ObservableCollection<BookDisplayModel> filteredBooks;

        public SearchBooksPage()
        {
            InitializeComponent();
            
            _bookDB = new BookDB();
            _loanDB = new LoanDB();
            _reservationDB = new ReservationDB();
            allBooks = new ObservableCollection<BookDisplayModel>();
            filteredBooks = new ObservableCollection<BookDisplayModel>();
            booksGrid.ItemsSource = filteredBooks;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAllBooks();
        }

        private void LoadAllBooks()
        {
            try
            {
                // Get ALL books from database (not just new arrivals)
                DataTable dt = _bookDB.GetAllBooksWithDetails();
                
                allBooks.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    allBooks.Add(new BookDisplayModel
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

                // Initially show all books
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading books: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void TxtKeyword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplyFilters();
            }
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            // Filters will be applied when user clicks Search button
        }

        private void ApplyFilters()
        {
            var keyword = txtKeyword.Text?.Trim().ToLower() ?? "";
            var selectedCategory = (cmbCategory.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "";
            var availableOnly = chkAvailableOnly.IsChecked == true;

            filteredBooks.Clear();

            var filtered = allBooks.Where(book =>
            {
                // Keyword filter (search in title, author, publisher, category, ISBN)
                var matchesKeyword = string.IsNullOrEmpty(keyword) ||
                    book.Title.ToLower().Contains(keyword) ||
                    book.Author.ToLower().Contains(keyword) ||
                    book.Publisher.ToLower().Contains(keyword) ||
                    book.Category.ToLower().Contains(keyword) ||
                    book.ISBN.ToLower().Contains(keyword);

                // Category filter
                var matchesCategory = string.IsNullOrEmpty(selectedCategory) ||
                    book.Category == selectedCategory;

                // Availability filter
                var matchesAvailability = !availableOnly || book.IsAvailable;

                return matchesKeyword && matchesCategory && matchesAvailability;
            });

            foreach (var book in filtered)
            {
                filteredBooks.Add(book);
            }

            txtTotalBooks.Text = filteredBooks.Count.ToString();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            txtKeyword.Clear();
            cmbCategory.SelectedIndex = 0;
            chkAvailableOnly.IsChecked = false;
            ApplyFilters();
        }

        private void QuickLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var category = button.Tag?.ToString();
                if (!string.IsNullOrEmpty(category))
                {
                    // Set category filter and apply
                    cmbCategory.SelectedIndex = cmbCategory.Items.Cast<ComboBoxItem>()
                        .ToList()
                        .FindIndex(item => item.Tag?.ToString() == category);
                    ApplyFilters();
                }
            }
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

                    // ✅ CHECK IF MEMBER IS SUSPENDED
                    var memberDB = new MemberDB();
                    bool isSuspended = memberDB.IsMemberSuspended(currentUser.UserIdString);
                    if (isSuspended)
                    {
                        MessageBox.Show(
                            "Your account is currently suspended. You cannot borrow books.\n\n" +
                            "Please contact the library for more information.",
                            "Account Suspended",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
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
                            
                            // Reload books to update available copies
                            LoadAllBooks();
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

                    // ✅ CHECK IF MEMBER IS SUSPENDED
                    var memberDB = new MemberDB();
                    bool isSuspended = memberDB.IsMemberSuspended(currentUser.UserIdString);
                    if (isSuspended)
                    {
                        MessageBox.Show(
                            "Your account is currently suspended. You cannot reserve books.\n\n" +
                            "Please contact the library for more information.",
                            "Account Suspended",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
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







