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
    public partial class NewArrivalsPage : Page
    {
        private readonly LibraryService _libraryService;
        private readonly BookDB _bookDB;
        private readonly LoanDB _loanDB;
        private readonly ReservationDB _reservationDB;
        private ObservableCollection<BookDisplayModel> allBooks;
        private ObservableCollection<BookDisplayModel> filteredBooks;

        public NewArrivalsPage()
        {
            InitializeComponent();
            _libraryService = new LibraryService();
            _bookDB = new BookDB();
            _loanDB = new LoanDB();
            _reservationDB = new ReservationDB();
            allBooks = new ObservableCollection<BookDisplayModel>();
            filteredBooks = new ObservableCollection<BookDisplayModel>();
            booksGrid.ItemsSource = filteredBooks;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadNewArrivals();
        }

        private void LoadNewArrivals()
        {
            try
            {
                // Get books from last 90 days from database
                DataTable dt = _bookDB.GetNewArrivalsWithDetails(90);
                
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

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading new arrivals: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            cmbCategory.SelectedIndex = 0;
            chkAvailableOnly.IsChecked = false;
            ApplyFilters();
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            // Auto-apply filters on change
            ApplyFilters();
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            // Auto-apply filters on combo box change
            ApplyFilters();
        }

        private void TxtSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                ApplyFilters();
            }
        }

        private void ApplyFilters()
        {
            if (allBooks == null) return;

            var searchText = txtSearch.Text?.ToLower() ?? "";
            var selectedCategory = (cmbCategory.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "";
            var availableOnly = chkAvailableOnly.IsChecked ?? false;

            var filtered = allBooks.Where(book =>
            {
                // Search filter (title, author, or publisher)
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                    book.Title.ToLower().Contains(searchText) ||
                    book.Author.ToLower().Contains(searchText) ||
                    book.Publisher.ToLower().Contains(searchText);

                // Category filter
                bool matchesCategory = string.IsNullOrEmpty(selectedCategory) ||
                    book.Category.Equals(selectedCategory, StringComparison.OrdinalIgnoreCase);

                // Availability filter
                bool matchesAvailability = !availableOnly || book.IsAvailable;

                return matchesSearch && matchesCategory && matchesAvailability;
            }).ToList();

            filteredBooks.Clear();
            foreach (var book in filtered)
            {
                filteredBooks.Add(book);
            }

            txtTotalBooks.Text = filteredBooks.Count.ToString();
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
                            
                            // Reload to update available copies
                            LoadNewArrivals();
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

    public class BookDisplayModel
    {
        public string BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public string Year { get; set; }
        public string Category { get; set; }
        public int AvailableCopies { get; set; }
        public int TotalCopies { get; set; }
        public string ISBN { get; set; }
        public string Description { get; set; }
        
        public bool IsAvailable => AvailableCopies > 0;
        public string StatusText => IsAvailable ? "In Stock" : "Not Available";
        
        public Style StatusBadgeStyle
        {
            get
            {
                var style = new Style(typeof(Border));
                var colorBrush = IsAvailable ? 
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d4edda")) : 
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f8d7da"));
                style.Setters.Add(new Setter(Border.BackgroundProperty, colorBrush));
                style.Setters.Add(new Setter(Border.PaddingProperty, new Thickness(5)));
                style.Setters.Add(new Setter(Border.CornerRadiusProperty, new CornerRadius(5)));
                return style;
            }
        }

        public Style StatusTextStyle
        {
            get
            {
                var style = new Style(typeof(TextBlock));
                var colorBrush = IsAvailable ? 
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString("#155724")) : 
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString("#721c24"));
                style.Setters.Add(new Setter(TextBlock.ForegroundProperty, colorBrush));
                style.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.SemiBold));
                style.Setters.Add(new Setter(TextBlock.FontSizeProperty, 12.0));
                return style;
            }
        }
    }
}

