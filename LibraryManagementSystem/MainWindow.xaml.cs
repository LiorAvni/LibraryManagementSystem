using System.Windows;
using System.Windows.Navigation;
using LibraryManagementSystem.Model;
using LibraryManagementSystem.View.Pages;

namespace LibraryManagementSystem;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public static User CurrentUser { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        NavigateToLogin();
    }

    private void NavigateToLogin()
    {
        MainFrame.Navigate(new LoginPage());
    }

    public void UpdateUserInfo(User user)
    {
        CurrentUser = user;
        if (user != null)
        {
            txtUserInfo.Text = $"Welcome, {user.FullName} ({user.Role.ToUpper()})";
            btnLogout.Visibility = Visibility.Visible;
            
            // Show navigation menu based on role
            if (user.Role?.ToLower() == "member")
            {
                memberNavBar.Visibility = Visibility.Visible;
                librarianNavBar.Visibility = Visibility.Collapsed;
            }
            else if (user.Role?.ToLower() == "librarian")
            {
                memberNavBar.Visibility = Visibility.Collapsed;
                librarianNavBar.Visibility = Visibility.Visible;
            }
            else
            {
                memberNavBar.Visibility = Visibility.Collapsed;
                librarianNavBar.Visibility = Visibility.Collapsed;
            }
        }
        else
        {
            txtUserInfo.Text = "Welcome, Guest";
            btnLogout.Visibility = Visibility.Collapsed;
            memberNavBar.Visibility = Visibility.Collapsed;
            librarianNavBar.Visibility = Visibility.Collapsed;
        }
    }

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            "Are you sure you want to logout?",
            "Confirm Logout",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            CurrentUser = null;
            UpdateUserInfo(null);
            NavigateToLogin();
        }
    }

    // Member Navigation Handlers
    private void NavDashboard_Click(object sender, RoutedEventArgs e)
    {
        ResetMemberNavigationButtons();
        btnNavDashboard.Tag = "Active";
        MainFrame.Navigate(new MemberDashboard());
    }

    private void NavNewArrivals_Click(object sender, RoutedEventArgs e)
    {
        ResetMemberNavigationButtons();
        btnNavNewArrivals.Tag = "Active";
        MainFrame.Navigate(new NewArrivalsPage());
    }

    private void NavSearchBooks_Click(object sender, RoutedEventArgs e)
    {
        ResetMemberNavigationButtons();
        btnNavSearchBooks.Tag = "Active";
        MainFrame.Navigate(new SearchBooksPage());
    }

    // Librarian Navigation Handler
    private void NavLibrarianDashboard_Click(object sender, RoutedEventArgs e)
    {
        btnNavLibrarianDashboard.Tag = "Active";
        MainFrame.Navigate(new LibrarianDashboard());
    }

    private void MainFrame_Navigated(object sender, NavigationEventArgs e)
    {
        // Update active navigation button based on current page and role
        if (CurrentUser?.Role?.ToLower() == "member")
        {
            ResetMemberNavigationButtons();
            
            if (e.Content is MemberDashboard)
                btnNavDashboard.Tag = "Active";
            else if (e.Content is NewArrivalsPage)
                btnNavNewArrivals.Tag = "Active";
            else if (e.Content is SearchBooksPage)
                btnNavSearchBooks.Tag = "Active";
        }
        else if (CurrentUser?.Role?.ToLower() == "librarian")
        {
            // Librarian dashboard is always active (only one button)
            if (e.Content is LibrarianDashboard)
                btnNavLibrarianDashboard.Tag = "Active";
        }
    }

    private void ResetMemberNavigationButtons()
    {
        btnNavDashboard.Tag = null;
        btnNavNewArrivals.Tag = null;
        btnNavSearchBooks.Tag = null;
    }
}