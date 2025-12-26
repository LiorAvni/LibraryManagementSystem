using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class LibrarianDashboard : Page
    {
        private bool isAdmin = false;
        private bool isSuspended = false;

        public LibrarianDashboard()
        {
            InitializeComponent();
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
                txtLibrarianName.Text = $"{currentUser.FirstName} {currentUser.LastName}";
                // TODO: Get actual employee ID from librarian record
                txtEmployeeId.Text = "EMP" + currentUser.UserID.ToString("D3");
                
                // Check if user is admin and get librarian status
                CheckLibrarianStatus(currentUser.UserIdString);
            }
        }

        private void CheckLibrarianStatus(string userId)
        {
            try
            {
                var memberDB = new MemberDB();
                isAdmin = memberDB.IsUserAdmin(userId);
                
                // Get librarian status
                string librarianStatus = GetLibrarianStatus(userId);
                isSuspended = librarianStatus.Equals("SUSPENDED", StringComparison.OrdinalIgnoreCase);
                
                // Update status badge
                txtStatus.Text = librarianStatus;
                if (isSuspended)
                {
                    borderStatus.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(248, 215, 218)); // #f8d7da
                    txtStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(114, 28, 36)); // #721c24
                    txtSuspendedWarning.Visibility = Visibility.Visible;
                }
                else
                {
                    borderStatus.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(212, 237, 218)); // #d4edda
                    txtStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(21, 87, 36)); // #155724
                    txtSuspendedWarning.Visibility = Visibility.Collapsed;
                }
                
                // Update UI based on admin status
                if (isAdmin)
                {
                    // Admin sees "Manage Users" instead of "Manage Members"
                    txtManageUsersTitle.Text = "Manage Users";
                    txtManageUsersDesc.Text = "View and manage members, librarians & admins";
                    btnManageUsers.Content = "Go to Users";
                    
                    // Show Settings card for admin
                    settingsCard.Visibility = Visibility.Visible;
                }
                else
                {
                    // Regular librarian sees "Manage Members"
                    txtManageUsersTitle.Text = "Manage Members";
                    txtManageUsersDesc.Text = "View and manage members";
                    btnManageUsers.Content = "Go to Members";
                    
                    // Hide Settings card for normal librarian
                    settingsCard.Visibility = Visibility.Collapsed;
                }
                
                // Disable all action buttons if suspended
                if (isSuspended)
                {
                    DisableAllActionButtons();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking librarian status: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetLibrarianStatus(string userId)
        {
            try
            {
                var memberDB = new MemberDB();
                string status = memberDB.GetLibrarianStatusByUserId(userId);
                return status ?? "ACTIVE";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting librarian status: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return "ACTIVE";
            }
        }

        private void DisableAllActionButtons()
        {
            // Disable all navigation buttons
            btnSearchBooks.IsEnabled = false;
            btnManageBooks.IsEnabled = false;
            btnManageUsers.IsEnabled = false;
            btnManageLoans.IsEnabled = false;
            btnManageReservations.IsEnabled = false;
            btnManageAuthors.IsEnabled = false;
            btnManageCategories.IsEnabled = false;
            btnManagePublishers.IsEnabled = false;
            btnViewReports.IsEnabled = false;
            btnSettings.IsEnabled = false;
        }

        // Navigation methods for menu cards
        private void ManageBooks_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ManageBooksPage());
        }

        private void ManageAuthors_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ManageAuthorsPage());
        }

        private void ManageMembers_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ManageMembersPage());
        }

        private void ManageLoans_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ManageLoansPage());
        }

        private void ManageReservations_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ManageReservationsPage());
        }

        private void SearchBooks_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SearchBooksPage());
        }

        private void ViewReports_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigate to Reports page
            MessageBox.Show("Navigate to Reports page", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LibrarySettingsPage());
        }

        private void ManageCategories_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ManageCategoriesPage());
        }

        private void ManagePublishers_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ManagePublishersPage());
        }

        private void NavigateToSearch(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new SearchBooksPage());
        }
    }
}

