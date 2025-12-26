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
            
            System.Diagnostics.Debug.WriteLine($"=== UpdateUserInfo START ===");
            System.Diagnostics.Debug.WriteLine($"User: {user.FullName}");
            System.Diagnostics.Debug.WriteLine($"Role: {user.Role}");
            System.Diagnostics.Debug.WriteLine($"UserIdString: {user.UserIdString}");
            
            // Show navigation menu based on role
            if (user.Role?.ToLower() == "member")
            {
                System.Diagnostics.Debug.WriteLine("Role is MEMBER");
                memberNavBar.Visibility = Visibility.Visible;
                librarianNavBar.Visibility = Visibility.Collapsed;
                adminNavBar.Visibility = Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine($"memberNavBar: Visible, librarianNavBar: Collapsed, adminNavBar: Collapsed");
            }
            else if (user.Role?.ToLower() == "librarian" || user.Role?.ToLower() == "admin")
            {
                System.Diagnostics.Debug.WriteLine($"Role is {user.Role.ToUpper()} - checking admin status...");
                
                // Check if user is admin (either role=ADMIN or has ADMIN in user_roles)
                bool isAdmin = user.Role?.ToLower() == "admin" || CheckIfUserIsAdmin(user.UserIdString);
                
                System.Diagnostics.Debug.WriteLine($"IsAdmin result: {isAdmin}");
                
                memberNavBar.Visibility = Visibility.Collapsed;
                
                if (isAdmin)
                {
                    System.Diagnostics.Debug.WriteLine("User IS admin - showing ADMIN nav bar");
                    librarianNavBar.Visibility = Visibility.Collapsed;
                    adminNavBar.Visibility = Visibility.Visible;
                    System.Diagnostics.Debug.WriteLine($"After setting: memberNavBar={memberNavBar.Visibility}, librarianNavBar={librarianNavBar.Visibility}, adminNavBar={adminNavBar.Visibility}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("User is NOT admin - showing LIBRARIAN nav bar");
                    librarianNavBar.Visibility = Visibility.Visible;
                    adminNavBar.Visibility = Visibility.Collapsed;
                    System.Diagnostics.Debug.WriteLine($"After setting: memberNavBar={memberNavBar.Visibility}, librarianNavBar={librarianNavBar.Visibility}, adminNavBar={adminNavBar.Visibility}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Role is OTHER: {user.Role}");
                memberNavBar.Visibility = Visibility.Collapsed;
                librarianNavBar.Visibility = Visibility.Collapsed;
                adminNavBar.Visibility = Visibility.Collapsed;
            }
            
            System.Diagnostics.Debug.WriteLine($"=== UpdateUserInfo END ===");
        }
        else
        {
            txtUserInfo.Text = "Welcome, Guest";
            btnLogout.Visibility = Visibility.Collapsed;
            memberNavBar.Visibility = Visibility.Collapsed;
            librarianNavBar.Visibility = Visibility.Collapsed;
            adminNavBar.Visibility = Visibility.Collapsed;
        }
    }

    private bool CheckIfUserIsAdmin(string userId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"CheckIfUserIsAdmin called with userId: {userId}");
            var memberDB = new LibraryManagementSystem.ViewModel.MemberDB();
            bool result = memberDB.IsUserAdmin(userId);
            System.Diagnostics.Debug.WriteLine($"IsUserAdmin returned: {result}");
            return result;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in CheckIfUserIsAdmin: {ex.Message}");
            return false;
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

    // Admin Navigation Handler
    private void NavAdminDashboard_Click(object sender, RoutedEventArgs e)
    {
        btnNavAdminDashboard.Tag = "Active";
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
        else if (CurrentUser?.Role?.ToLower() == "librarian" || CurrentUser?.Role?.ToLower() == "admin")
        {
            // Check if user is admin (either role=ADMIN or has ADMIN in user_roles)
            bool isAdmin = CurrentUser.Role?.ToLower() == "admin" || CheckIfUserIsAdmin(CurrentUser.UserIdString);
            
            if (e.Content is LibrarianDashboard)
            {
                if (isAdmin)
                    btnNavAdminDashboard.Tag = "Active";
                else
                    btnNavLibrarianDashboard.Tag = "Active";
            }
        }
    }

    private void ResetMemberNavigationButtons()
    {
        btnNavDashboard.Tag = null;
        btnNavNewArrivals.Tag = null;
        btnNavSearchBooks.Tag = null;
    }
}