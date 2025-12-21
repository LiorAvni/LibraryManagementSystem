using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class ManageLoansPage : Page
    {
        private readonly LoanDB _loanDB;
        private ObservableCollection<ManageLoanDisplayModel> loans;
        
        private int currentPage = 0;
        private int pageSize = 20;
        private int totalRecords = 0;
        private int totalPages = 0;
        
        private string filterMemberName = "";
        private string filterBookTitle = "";
        private string filterStatus = "ALL";

        public ManageLoansPage()
        {
            InitializeComponent();
            _loanDB = new LoanDB();
            loans = new ObservableCollection<ManageLoanDisplayModel>();
            dgLoans.ItemsSource = loans;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLoans();
        }

        private void LoadLoans()
        {
            try
            {
                loans.Clear();

                // Get loans from database with filters
                DataTable dt = _loanDB.GetLoansForManagement(
                    filterMemberName, 
                    filterBookTitle, 
                    filterStatus, 
                    currentPage, 
                    pageSize,
                    out totalRecords);

                // Calculate total pages
                totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                if (totalPages == 0) totalPages = 1;

                foreach (DataRow row in dt.Rows)
                {
                    loans.Add(new ManageLoanDisplayModel
                    {
                        LoanId = row["loan_id"]?.ToString() ?? "",
                        CopyId = row["copy_id"]?.ToString() ?? "",
                        MemberName = row["MemberName"]?.ToString() ?? "N/A",
                        BookTitle = row["BookTitle"]?.ToString() ?? "N/A",
                        LoanDate = row["loan_date"] != DBNull.Value ? Convert.ToDateTime(row["loan_date"]) : DateTime.Now,
                        DueDate = row["due_date"] != DBNull.Value ? Convert.ToDateTime(row["due_date"]) : DateTime.Now,
                        ReturnDate = row["return_date"] != DBNull.Value ? Convert.ToDateTime(row["return_date"]) : (DateTime?)null,
                        FineAmount = row["fine_amount"] != DBNull.Value ? Convert.ToDecimal(row["fine_amount"]) : 0,
                        Status = DetermineStatus(row)
                    });
                }

                UpdatePaginationUI();
                UpdateStatsUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading loans: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string DetermineStatus(DataRow row)
        {
            DateTime? returnDate = row["return_date"] != DBNull.Value ? Convert.ToDateTime(row["return_date"]) : (DateTime?)null;
            
            if (returnDate.HasValue)
            {
                return "RETURNED";
            }
            
            DateTime dueDate = row["due_date"] != DBNull.Value ? Convert.ToDateTime(row["due_date"]) : DateTime.Now;
            
            if (DateTime.Now > dueDate)
            {
                return "OVERDUE";
            }
            
            return "ACTIVE";
        }

        private void UpdatePaginationUI()
        {
            txtPaginationInfo.Text = $"Page {currentPage + 1} of {totalPages} ({totalRecords} records)";
            
            btnFirst.IsEnabled = currentPage > 0;
            btnPrevious.IsEnabled = currentPage > 0;
            btnNext.IsEnabled = currentPage < totalPages - 1;
            btnLast.IsEnabled = currentPage < totalPages - 1;

            if (totalPages <= 1)
            {
                paginationSection.Visibility = Visibility.Collapsed;
            }
            else
            {
                paginationSection.Visibility = Visibility.Visible;
            }
        }

        private void UpdateStatsUI()
        {
            txtTotalRecords.Text = totalRecords.ToString();
            txtCurrentPage.Text = (currentPage + 1).ToString();
            txtTotalPages.Text = totalPages.ToString();
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            filterMemberName = txtMemberName.Text.Trim();
            filterBookTitle = txtBookTitle.Text.Trim();
            filterStatus = ((ComboBoxItem)cmbStatus.SelectedItem)?.Tag?.ToString() ?? "ALL";
            currentPage = 0; // Reset to first page
            LoadLoans();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            txtMemberName.Clear();
            txtBookTitle.Clear();
            cmbStatus.SelectedIndex = 0;
            filterMemberName = "";
            filterBookTitle = "";
            filterStatus = "ALL";
            currentPage = 0;
            LoadLoans();
        }

        private void FirstPage_Click(object sender, RoutedEventArgs e)
        {
            currentPage = 0;
            LoadLoans();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                LoadLoans();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < totalPages - 1)
            {
                currentPage++;
                LoadLoans();
            }
        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            currentPage = totalPages - 1;
            LoadLoans();
        }

        private void ReturnBook_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ManageLoanDisplayModel loan)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to return this book?{Environment.NewLine}{Environment.NewLine}" +
                    $"Book: {loan.BookTitle}{Environment.NewLine}" +
                    $"Member: {loan.MemberName}{Environment.NewLine}" +
                    $"Fine Amount: ${loan.FineAmount:F2}",
                    "Confirm Return",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = _loanDB.ReturnBook(loan.LoanId, DateTime.Now, loan.FineAmount);

                        if (success)
                        {
                            MessageBox.Show(
                                $"Book returned successfully!{Environment.NewLine}{Environment.NewLine}" +
                                $"Book: {loan.BookTitle}{Environment.NewLine}" +
                                $"Return Date: {DateTime.Now:yyyy-MM-dd}{Environment.NewLine}" +
                                $"Fine Amount: ${loan.FineAmount:F2}",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                            // Reload loans
                            LoadLoans();
                        }
                        else
                        {
                            MessageBox.Show("Failed to return the book. Please try again.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error returning book: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }

    public class ManageLoanDisplayModel
    {
        public string LoanId { get; set; }
        public string CopyId { get; set; }
        public string MemberName { get; set; }
        public string BookTitle { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public decimal FineAmount { get; set; }
        public string Status { get; set; }

        public string ReturnDateDisplay => ReturnDate.HasValue ? ReturnDate.Value.ToString("yyyy-MM-dd") : "-";
        public bool CanReturn => Status == "ACTIVE" || Status == "OVERDUE";
    }
}

