using System;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementSystem.Model;
using LibraryManagementSystem.Service;

namespace LibraryManagementSystem.View.Pages
{
    public partial class LoginPage : Page
    {
        private readonly LibraryService _libraryService;
        private string _password = "";

        public LoginPage()
        {
            InitializeComponent();
            _libraryService = new LibraryService();
        }

        private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _password = txtPassword.Password;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = txtEmail.Text.Trim();
                
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(_password))
                {
                    ShowMessage("Please enter both email and password", true);
                    return;
                }

                // Attempt login
                User user = _libraryService.Login(email, _password);

                if (user != null)
                {
                    // Update main window with user info
                    MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow?.UpdateUserInfo(user);

                    // Navigate based on role (case-insensitive comparison)
                    string role = user.Role?.Trim() ?? "";
                    
                    if (string.IsNullOrEmpty(role))
                    {
                        ShowMessage($"User role is not set. Please contact administrator.", true);
                        return;
                    }

                    if (role.Equals("Member", StringComparison.OrdinalIgnoreCase))
                    {
                        NavigationService?.Navigate(new MemberDashboard());
                    }
                    else if (role.Equals("Librarian", StringComparison.OrdinalIgnoreCase) || 
                             role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        NavigationService?.Navigate(new LibrarianDashboard());
                    }
                    else
                    {
                        ShowMessage($"Invalid user role: '{role}'. Expected: Member, Librarian, or Admin.", true);
                    }
                }
                else
                {
                    ShowMessage("Invalid email or password", true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Login error: {ex.Message}", true);
            }
        }

        private void LnkRegister_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Registration feature coming soon!\n\nPlease contact the library administrator to create an account.",
                          "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowMessage(string message, bool isError)
        {
            txtMessage.Text = message;
            errorBorder.Visibility = Visibility.Visible;
        }
    }
}

