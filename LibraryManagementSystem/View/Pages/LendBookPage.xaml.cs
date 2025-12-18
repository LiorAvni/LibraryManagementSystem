using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class LendBookPage : Page
    {
        private readonly BookDB _bookDB;
        private readonly MemberDB _memberDB;
        private readonly LoanDB _loanDB;
        private string _bookId;
        private string _librarianId;
        private int _defaultLoanDays = 14; // Default

        public LendBookPage(string bookId, string librarianId)
        {
            InitializeComponent();
            _bookDB = new BookDB();
            _memberDB = new MemberDB();
            _loanDB = new LoanDB();
            _bookId = bookId;
            _librarianId = librarianId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBookInformation();
            LoadDefaultLoanPeriod();
            LoadAllMembers();
        }

        private void LoadBookInformation()
        {
            try
            {
                DataRow book = _bookDB.GetBookDetailsForEdit(_bookId);
                
                if (book != null)
                {
                    txtBookTitle.Text = book["title"]?.ToString() ?? "N/A";
                    txtISBN.Text = book["isbn"]?.ToString() ?? "N/A";
                    
                    // Get available copies count
                    string availQuery = "SELECT COUNT(*) FROM book_copies WHERE book_id = ? AND status = 'AVAILABLE'";
                    var bookDBInstance = new BookDB();
                    // We'll use the GetFirstAvailableCopy method indirectly by checking if null
                    string firstAvailableCopy = _loanDB.GetFirstAvailableCopy(_bookId);
                    
                    if (firstAvailableCopy != null)
                    {
                        // Count all available
                        DataTable copies = _bookDB.GetBookCopies(_bookId);
                        int availableCount = 0;
                        foreach (DataRow row in copies.Rows)
                        {
                            if (row["status"].ToString() == "AVAILABLE")
                                availableCount++;
                        }
                        txtAvailableCopies.Text = availableCount.ToString();
                    }
                    else
                    {
                        txtAvailableCopies.Text = "0";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading book information: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDefaultLoanPeriod()
        {
            try
            {
                string loanDaysStr = _loanDB.GetLibrarySetting("MAX_LOAN_DAYS");
                if (!string.IsNullOrEmpty(loanDaysStr) && int.TryParse(loanDaysStr, out int days))
                {
                    _defaultLoanDays = days;
                }
            }
            catch
            {
                _defaultLoanDays = 14; // Fallback to default
            }
        }

        private void LoadAllMembers()
        {
            try
            {
                DataTable members = _memberDB.GetAllActiveMembersForLending();
                DisplayMembers(members);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading members: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSearch();
            }
        }

        private void PerformSearch()
        {
            try
            {
                string searchTerm = txtSearch.Text.Trim();
                
                DataTable members;
                if (string.IsNullOrEmpty(searchTerm))
                {
                    members = _memberDB.GetAllActiveMembersForLending();
                    txtMembersTitle.Text = "Select Member";
                }
                else
                {
                    members = _memberDB.SearchMembers(searchTerm);
                    txtMembersTitle.Text = $"Search Results for '{searchTerm}'";
                }
                
                DisplayMembers(members);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching members: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayMembers(DataTable members)
        {
            if (members.Rows.Count > 0)
            {
                List<MemberDisplayModel> membersList = new List<MemberDisplayModel>();
                
                foreach (DataRow row in members.Rows)
                {
                    membersList.Add(new MemberDisplayModel
                    {
                        MemberId = row["member_id"].ToString(),
                        UserId = row["user_id"].ToString(),
                        FirstName = row["first_name"].ToString(),
                        LastName = row["last_name"].ToString(),
                        Email = row["email"].ToString()
                    });
                }
                
                icMembers.ItemsSource = membersList;
                icMembers.Visibility = Visibility.Visible;
                txtNoResults.Visibility = Visibility.Collapsed;
            }
            else
            {
                icMembers.ItemsSource = null;
                icMembers.Visibility = Visibility.Collapsed;
                txtNoResults.Visibility = Visibility.Visible;
            }
        }

        private void LendBook_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is MemberDisplayModel member)
            {
                // Confirm lending
                var result = MessageBox.Show(
                    $"Lend this book to {member.FirstName} {member.LastName}?{Environment.NewLine}{Environment.NewLine}" +
                    $"Book: {txtBookTitle.Text}{Environment.NewLine}" +
                    $"Member: {member.FirstName} {member.LastName}{Environment.NewLine}" +
                    $"Loan Period: {_defaultLoanDays} days",
                    "Confirm Lend Book",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Check if member has reached max loans
                        string maxLoansStr = _loanDB.GetLibrarySetting("MAX_BOOKS_PER_MEMBER");
                        int maxLoans = 3; // Default
                        if (!string.IsNullOrEmpty(maxLoansStr) && int.TryParse(maxLoansStr, out int max))
                        {
                            maxLoans = max;
                        }

                        int currentLoans = _loanDB.GetActiveLoanCount(member.MemberId);
                        if (currentLoans >= maxLoans)
                        {
                            MessageBox.Show(
                                $"This member has reached the maximum number of loans ({maxLoans}).{Environment.NewLine}" +
                                "They must return a book before borrowing another.",
                                "Loan Limit Reached",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        // Get first available copy
                        string copyId = _loanDB.GetFirstAvailableCopy(_bookId);
                        if (string.IsNullOrEmpty(copyId))
                        {
                            MessageBox.Show(
                                "No available copies of this book.",
                                "Not Available",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        // Create loan
                        string loanId = _loanDB.CreateLoan(copyId, member.MemberId, _librarianId, _defaultLoanDays);
                        
                        if (!string.IsNullOrEmpty(loanId))
                        {
                            MessageBox.Show(
                                $"Book successfully lent to {member.FirstName} {member.LastName}!{Environment.NewLine}{Environment.NewLine}" +
                                $"Loan ID: {loanId}{Environment.NewLine}" +
                                $"Due Date: {DateTime.Now.AddDays(_defaultLoanDays):yyyy-MM-dd}",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                            // Navigate back to Manage Books
                            NavigationService?.GoBack();
                        }
                        else
                        {
                            MessageBox.Show("Failed to create loan.", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error lending book: {ex.Message}", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }

    /// <summary>
    /// Model for displaying member information in the list
    /// </summary>
    public class MemberDisplayModel
    {
        public string MemberId { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}
