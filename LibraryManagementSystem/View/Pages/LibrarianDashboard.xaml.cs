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
                
                // Check if user is admin
                CheckAdminStatus(currentUser.UserIdString);
            }
        }

        private void CheckAdminStatus(string userId)
        {
            try
            {
                var memberDB = new MemberDB();
                isAdmin = memberDB.IsUserAdmin(userId);
                
                // Update UI based on admin status
                if (isAdmin)
                {
                    // Admin sees "Manage Users" instead of "Manage Members"
                    txtManageUsersTitle.Text = "Manage Users";
                    txtManageUsersDesc.Text = "View and manage members, librarians & admins";
                    btnManageUsers.Content = "Go to Users";
                }
                else
                {
                    // Regular librarian sees "Manage Members"
                    txtManageUsersTitle.Text = "Manage Members";
                    txtManageUsersDesc.Text = "View and manage members";
                    btnManageUsers.Content = "Go to Members";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking admin status: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

