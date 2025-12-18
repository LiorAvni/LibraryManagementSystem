using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using LibraryManagementSystem.Service;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    // Value Converters for styling
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.ToString() == "OVERDUE")
                return new SolidColorBrush(Color.FromRgb(220, 53, 69)); // Red
            return new SolidColorBrush(Color.FromRgb(40, 167, 69)); // Green
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FineToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal fine && fine > 0)
                return new SolidColorBrush(Color.FromRgb(220, 53, 69)); // Red
            return new SolidColorBrush(Color.FromRgb(52, 58, 64)); // Dark gray
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusToBadgeStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value?.ToString();
            if (status == "OVERDUE")
                return "BadgeOverdue";
            else if (status == "ACTIVE")
                return "BadgeActive";
            else if (status == "RETURNED")
                return "BadgeReturned";
            return "BadgeWarning";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusToBadgeTextStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value?.ToString();
            if (status == "OVERDUE")
                return "BadgeOverdueText";
            else if (status == "ACTIVE")
                return "BadgeActiveText";
            else if (status == "RETURNED")
                return "BadgeReturnedText";
            return "BadgeWarningText";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MemberDashboard : Page
    {
        private readonly LibraryService _libraryService;
        private readonly LoanDB _loanDB;
        private readonly ReservationDB _reservationDB;
        private ObservableCollection<LoanDisplayModel> currentLoans;
        private ObservableCollection<ReservationDisplayModel> reservations;
        private ObservableCollection<LoanDisplayModel> loanHistory;
        private ObservableCollection<LoanDisplayModel> unpaidFines;
        private string currentFilter = "LAST_30_DAYS";
        private int totalBorrowedAllTime = 0; // Total loans across all time

        public MemberDashboard()
        {
            InitializeComponent();
            _libraryService = new LibraryService();
            _loanDB = new LoanDB();
            _reservationDB = new ReservationDB();
            
            currentLoans = new ObservableCollection<LoanDisplayModel>();
            reservations = new ObservableCollection<ReservationDisplayModel>();
            loanHistory = new ObservableCollection<LoanDisplayModel>();
            unpaidFines = new ObservableCollection<LoanDisplayModel>();
            
            dgCurrentLoans.ItemsSource = currentLoans;
            dgReservations.ItemsSource = reservations;
            dgLoanHistory.ItemsSource = loanHistory;
            // dgUnpaidFines will be bound in XAML when we add it

            // Set default filter button to active
            btnLast30Days.Tag = "Active";
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            var currentUser = MainWindow.CurrentUser;
            if (currentUser != null)
            {
                // Set user info
                txtMemberName.Text = $"{currentUser.FirstName} {currentUser.LastName}";
                txtMemberId.Text = currentUser.UserIdString ?? currentUser.UserID.ToString();
                txtStatus.Text = currentUser.IsActive ? "ACTIVE" : "INACTIVE";
                
                // Load real data from database
                LoadCurrentLoans();
                LoadReservations();
                LoadUnpaidFines(); // Load unpaid fines
                LoadLoanHistory(); // This loads based on current filter (default: 30 days)
                LoadTotalBorrowedCount(); // Load all-time total for statistics
                
                UpdateStatistics();
                UpdateHistoryStatistics(); // Update loan history statistics
            }
        }

        private void LoadTotalBorrowedCount()
        {
            try
            {
                var currentUser = MainWindow.CurrentUser;
                if (currentUser == null) return;

                // Get ALL loan history (no date filter) for total count
                DataTable dt = _loanDB.GetMemberLoanHistoryWithDetails(currentUser.UserIdString, 0);
                
                // Store the total count (we don't need to populate the grid, just count)
                totalBorrowedAllTime = dt.Rows.Count;
            }
            catch (Exception ex)
            {
                totalBorrowedAllTime = 0;
            }
        }

        private void LoadCurrentLoans()
        {
            try
            {
                currentLoans.Clear();
                
                var currentUser = MainWindow.CurrentUser;
                if (currentUser == null) return;

                // Get active loans with book details from database
                DataTable dt = _loanDB.GetMemberActiveLoansWithDetails(currentUser.UserIdString);
                
                foreach (DataRow row in dt.Rows)
                {
                    // Handle loan_id as TEXT/GUID, convert to hashcode for display
                    string loanIdStr = row["loan_id"]?.ToString() ?? "";
                    int loanId = string.IsNullOrEmpty(loanIdStr) ? 0 : Math.Abs(loanIdStr.GetHashCode());
                    
                    currentLoans.Add(new LoanDisplayModel
                    {
                        LoanId = loanId,
                        LoanIdString = loanIdStr, // Store actual GUID
                        BookTitle = row["BookTitle"]?.ToString() ?? "Unknown",
                        Author = row["Author"]?.ToString() ?? "Unknown Author",
                        LoanDate = row["LoanDate"] != DBNull.Value ? Convert.ToDateTime(row["LoanDate"]) : DateTime.Now,
                        DueDate = row["DueDate"] != DBNull.Value ? Convert.ToDateTime(row["DueDate"]) : DateTime.Now,
                        ReturnDate = row["ReturnDate"] != DBNull.Value ? Convert.ToDateTime(row["ReturnDate"]) : (DateTime?)null,
                        Status = row["Status"]?.ToString() ?? "ACTIVE",
                        Fine = row["Fine"] != DBNull.Value ? Convert.ToDecimal(row["Fine"]) : 0
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading current loans: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadReservations()
        {
            try
            {
                reservations.Clear();
                
                var currentUser = MainWindow.CurrentUser;
                if (currentUser == null) return;

                // Get reservations with book details from database
                DataTable dt = _reservationDB.GetMemberReservationsWithDetails(currentUser.UserIdString);
                
                foreach (DataRow row in dt.Rows)
                {
                    // Handle reservation_id as TEXT/GUID, convert to hashcode for display
                    string reservationIdStr = row["reservation_id"]?.ToString() ?? "";
                    int reservationId = string.IsNullOrEmpty(reservationIdStr) ? 0 : Math.Abs(reservationIdStr.GetHashCode());
                    
                    reservations.Add(new ReservationDisplayModel
                    {
                        ReservationId = reservationId,
                        ReservationIdString = reservationIdStr, // Store actual GUID
                        BookTitle = row["BookTitle"]?.ToString() ?? "Unknown",
                        Author = row["Author"]?.ToString() ?? "Unknown Author",
                        ReservationDate = row["ReservationDate"] != DBNull.Value ? Convert.ToDateTime(row["ReservationDate"]) : DateTime.Now,
                        ExpiryDate = row["ExpiryDate"] != DBNull.Value ? Convert.ToDateTime(row["ExpiryDate"]) : DateTime.Now.AddDays(7),
                        Status = row["Status"]?.ToString() ?? "PENDING"
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading reservations: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadLoanHistory()
        {
            try
            {
                if (loanHistory == null)
                    loanHistory = new ObservableCollection<LoanDisplayModel>();
                    
                loanHistory.Clear();
                
                var currentUser = MainWindow.CurrentUser;
                if (currentUser == null) return;

                // Determine days back based on filter
                int daysBack = currentFilter switch
                {
                    "LAST_7_DAYS" => 7,
                    "LAST_30_DAYS" => 30,
                    "LAST_90_DAYS" => 90,
                    _ => 0 // ALL_TIME
                };

                // Get loan history with book details from database
                DataTable dt = _loanDB.GetMemberLoanHistoryWithDetails(currentUser.UserIdString, daysBack);
                
                foreach (DataRow row in dt.Rows)
                {
                    // Handle loan_id as TEXT/GUID, convert to hashcode for display
                    string loanIdStr = row["loan_id"]?.ToString() ?? "";
                    int loanId = string.IsNullOrEmpty(loanIdStr) ? 0 : Math.Abs(loanIdStr.GetHashCode());
                    
                    loanHistory.Add(new LoanDisplayModel
                    {
                        LoanId = loanId,
                        LoanIdString = loanIdStr, // Store actual GUID
                        BookTitle = row["BookTitle"]?.ToString() ?? "Unknown",
                        Author = row["Author"]?.ToString() ?? "Unknown Author",
                        LoanDate = row["LoanDate"] != DBNull.Value ? Convert.ToDateTime(row["LoanDate"]) : DateTime.Now,
                        DueDate = row["DueDate"] != DBNull.Value ? Convert.ToDateTime(row["DueDate"]) : DateTime.Now,
                        ReturnDate = row["ReturnDate"] != DBNull.Value ? Convert.ToDateTime(row["ReturnDate"]) : (DateTime?)null,
                        Status = row["Status"]?.ToString() ?? "ACTIVE",
                        Fine = row["Fine"] != DBNull.Value ? Convert.ToDecimal(row["Fine"]) : 0
                    });
                }
                
                // Update history statistics after loading
                UpdateHistoryStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading loan history: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUnpaidFines()
        {
            try
            {
                unpaidFines.Clear();
                
                var currentUser = MainWindow.CurrentUser;
                if (currentUser == null) return;

                // Get unpaid fines from database
                DataTable dt = _loanDB.GetMemberUnpaidFines(currentUser.UserIdString);
                
                foreach (DataRow row in dt.Rows)
                {
                    string loanIdStr = row["loan_id"]?.ToString() ?? "";
                    int loanId = string.IsNullOrEmpty(loanIdStr) ? 0 : Math.Abs(loanIdStr.GetHashCode());
                    
                    unpaidFines.Add(new LoanDisplayModel
                    {
                        LoanId = loanId,
                        LoanIdString = loanIdStr,
                        BookTitle = row["BookTitle"]?.ToString() ?? "Unknown",
                        Author = row["Author"]?.ToString() ?? "Unknown Author",
                        LoanDate = row["LoanDate"] != DBNull.Value ? Convert.ToDateTime(row["LoanDate"]) : DateTime.Now,
                        DueDate = row["DueDate"] != DBNull.Value ? Convert.ToDateTime(row["DueDate"]) : DateTime.Now,
                        ReturnDate = row["ReturnDate"] != DBNull.Value ? Convert.ToDateTime(row["ReturnDate"]) : (DateTime?)null,
                        Status = row["Status"]?.ToString() ?? "RETURNED",
                        Fine = row["Fine"] != DBNull.Value ? Convert.ToDecimal(row["Fine"]) : 0,
                        OverdueDays = row["OverdueDays"] != DBNull.Value ? Convert.ToInt32(row["OverdueDays"]) : 0
                    });
                }
                
                // Show/hide unpaid fines section based on whether there are any
                var border = FindName("borderUnpaidFines") as Border;
                if (border != null)
                {
                    border.Visibility = unpaidFines.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                }
                
                // Bind data to DataGrid
                var dataGrid = FindName("dgUnpaidFines") as DataGrid;
                if (dataGrid != null)
                {
                    dataGrid.ItemsSource = unpaidFines;
                }
            }
            catch (Exception ex)
            {
                // Hide section on error
                var border = FindName("borderUnpaidFines") as Border;
                if (border != null)
                {
                    border.Visibility = Visibility.Collapsed;
                }
                // Silent fail - unpaid fines is not critical
            }
        }

        private void UpdateHistoryStatistics()
        {
            try
            {
                if (loanHistory == null) return;

                // Total loans in the current time frame
                int totalLoans = loanHistory.Count;
                
                // Returned loans in the current time frame
                int returnedLoans = loanHistory.Count(l => l.Status == "RETURNED");
                
                // Total fines in the current time frame
                decimal totalFines = loanHistory.Sum(l => l.Fine);
                
                // Update UI (check if elements exist first - they are defined in XAML)
                try
                {
                    var totalLoansControl = FindName("txtHistoryTotalLoans") as TextBlock;
                    var returnedControl = FindName("txtHistoryReturned") as TextBlock;
                    var totalFinesControl = FindName("txtHistoryTotalFines") as TextBlock;
                    
                    if (totalLoansControl != null)
                        totalLoansControl.Text = totalLoans.ToString();
                    if (returnedControl != null)
                        returnedControl.Text = returnedLoans.ToString();
                    if (totalFinesControl != null)
                        totalFinesControl.Text = $"${totalFines:F2}";
                }
                catch
                {
                    // Controls not found - silently fail
                }
            }
            catch (Exception ex)
            {
                // Silent fail for statistics
            }
        }

        private void UpdateStatistics()
        {
            try
            {
                // Count active loans (ACTIVE + OVERDUE - both count as active)
                var activeCount = currentLoans.Count(l => l.Status == "ACTIVE" || l.Status == "OVERDUE");
                
                // Count overdue books specifically
                var overdueCount = currentLoans.Count(l => l.Status == "OVERDUE");
                
                // Total borrowed = all-time total from database
                var totalBorrowed = totalBorrowedAllTime;
                
                // Update UI
                txtActiveLoansCount.Text = activeCount.ToString();
                txtOverdueCount.Text = overdueCount > 0 ? overdueCount.ToString() : "No";
                txtTotalBorrowed.Text = totalBorrowed.ToString();
            }
            catch (Exception ex)
            {
                // Silent fail for statistics - don't interrupt user experience
                txtActiveLoansCount.Text = "0";
                txtOverdueCount.Text = "No";
                txtTotalBorrowed.Text = "0";
            }
        }

        private void ReturnBook_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is LoanDisplayModel loan)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to return '{loan.BookTitle}'?{Environment.NewLine}{Environment.NewLine}" +
                    $"Fine amount: ${loan.Fine:F2}",
                    "Confirm Return",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Update the loan in the database
                        bool success = _loanDB.ReturnBook(loan.LoanIdString, DateTime.Now, loan.Fine);
                        
                        if (success)
                        {
                            // Reload all data from database
                            LoadCurrentLoans(); // This will remove the returned book
                            LoadLoanHistory(); // This will show the book as returned
                            LoadTotalBorrowedCount(); // Update total count
                            UpdateStatistics(); // Update statistics
                            
                            MessageBox.Show(
                                $"Book '{loan.BookTitle}' returned successfully!{Environment.NewLine}" +
                                $"Fine amount: ${loan.Fine:F2}",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(
                                "Failed to return the book. Please try again.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Error returning book: {ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CancelReservation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ReservationDisplayModel reservation)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to cancel reservation for '{reservation.BookTitle}'?",
                    "Confirm Cancellation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(reservation.ReservationIdString))
                        {
                            MessageBox.Show("Invalid reservation ID.", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        
                        // Cancel the reservation in the database (this also updates book copy status to AVAILABLE)
                        bool success = _reservationDB.CancelReservation(reservation.ReservationIdString);
                        
                        if (success)
                        {
                            // Reload reservations from database
                            LoadReservations();
                            
                            MessageBox.Show(
                                $"Reservation for '{reservation.BookTitle}' cancelled successfully!",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(
                                "Failed to cancel the reservation. Please try again.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Error cancelling reservation: {ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }

        private void PayFine_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is LoanDisplayModel loan)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to pay the fine for '{loan.BookTitle}'?{Environment.NewLine}{Environment.NewLine}" +
                    $"Fine amount: ${loan.Fine:F2}{Environment.NewLine}" +
                    $"This will mark the fine as paid.",
                    "Confirm Fine Payment",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(loan.LoanIdString))
                        {
                            MessageBox.Show("Invalid loan ID.", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        
                        // Pay the fine in the database
                        bool success = _loanDB.PayFine(loan.LoanIdString);
                        
                        if (success)
                        {
                            // Reload all data from database
                            LoadUnpaidFines(); // This will remove the paid fine from the list
                            LoadLoanHistory(); // Update loan history to show payment
                            UpdateHistoryStatistics(); // Update statistics
                            
                            MessageBox.Show(
                                $"Fine payment successful!{Environment.NewLine}{Environment.NewLine}" +
                                $"Book: '{loan.BookTitle}'{Environment.NewLine}" +
                                $"Amount paid: ${loan.Fine:F2}{Environment.NewLine}" +
                                $"Payment date: {DateTime.Now:yyyy-MM-dd}",
                                "Payment Successful",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(
                                "Failed to process the fine payment. Please try again.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Error processing fine payment: {ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }

        private void FilterHistory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // Reset all button tags
                btnLast7Days.Tag = null;
                btnLast30Days.Tag = null;
                btnLast90Days.Tag = null;
                btnAllTime.Tag = null;
                
                // Set active button and filter
                button.Tag = "Active";
                currentFilter = button.Name switch
                {
                    "btnLast7Days" => "LAST_7_DAYS",
                    "btnLast30Days" => "LAST_30_DAYS",
                    "btnLast90Days" => "LAST_90_DAYS",
                    "btnAllTime" => "ALL_TIME",
                    _ => "LAST_30_DAYS"
                };
                
                // Reload data with new filter
                LoadLoanHistory(); // This will call UpdateHistoryStatistics() automatically
            }
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (cmbStatusFilter.SelectedItem is ComboBoxItem item && loanHistory != null)
            {
                string status = item.Tag?.ToString() ?? "";
                
                // Reload all data first
                LoadLoanHistory(); // This will call UpdateHistoryStatistics() with full data
                
                // Then filter in memory if specific status selected
                if (!string.IsNullOrEmpty(status))
                {
                    var filtered = loanHistory.Where(l => l.Status == status).ToList();
                    loanHistory.Clear();
                    foreach (var loan in filtered)
                    {
                        loanHistory.Add(loan);
                    }
                    // Update statistics based on filtered data
                    UpdateHistoryStatistics();
                }
            }
        }
    }

    // Display Models for DataGrid binding
    public class LoanDisplayModel
    {
        public int LoanId { get; set; } // Display ID (hashcode)
        public string LoanIdString { get; set; } // Actual GUID from database
        public string BookTitle { get; set; }
        public string Author { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; }
        public decimal Fine { get; set; }
        public int OverdueDays { get; set; } // Number of days overdue
    }

    public class ReservationDisplayModel
    {
        public int ReservationId { get; set; } // Display ID (hashcode)
        public string ReservationIdString { get; set; } // Actual GUID from database
        public string BookTitle { get; set; }
        public string Author { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; }
    }
}
