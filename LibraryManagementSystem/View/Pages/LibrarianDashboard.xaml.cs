using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LibraryManagementSystem.View.Pages
{
    public partial class LibrarianDashboard : Page
    {
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
            // TODO: Navigate to Manage Loans page
            MessageBox.Show("Navigate to Manage Loans page", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ManageReservations_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigate to Manage Reservations page
            MessageBox.Show("Navigate to Manage Reservations page", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
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
            // TODO: Navigate to Settings page
            MessageBox.Show("Navigate to Settings page", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ManageCategories_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigate to Manage Categories page
            MessageBox.Show("Navigate to Manage Categories page", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void NavigateToSearch(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new SearchBooksPage());
        }
    }
}

