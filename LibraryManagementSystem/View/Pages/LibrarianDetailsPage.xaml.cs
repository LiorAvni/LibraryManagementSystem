using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class LibrarianDetailsPage : Page
    {
        private readonly MemberDB _memberDB;
        private readonly string _librarianId;
        private readonly bool _isAdmin;

        public LibrarianDetailsPage(string librarianId, bool isAdmin = false)
        {
            InitializeComponent();
            _memberDB = new MemberDB();
            _librarianId = librarianId;
            _isAdmin = isAdmin;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLibrarianDetails();
        }

        private void LoadLibrarianDetails()
        {
            try
            {
                DataTable dt = _memberDB.GetLibrarianDetailsById(_librarianId);
                
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    
                    // Set page title based on role
                    txtPageTitle.Text = _isAdmin ? "Admin Details" : "Librarian Details";
                    
                    // Set librarian information
                    txtEmployeeId.Text = row["employee_id"]?.ToString() ?? "N/A";
                    txtFullName.Text = $"{row["first_name"]} {row["last_name"]}";
                    txtEmail.Text = row["email"]?.ToString() ?? "N/A";
                    txtPhone.Text = row["phone"]?.ToString() ?? "N/A";
                    txtAddress.Text = row["address"]?.ToString() ?? "N/A";
                    
                    // Format hire date
                    if (row["hire_date"] != DBNull.Value)
                    {
                        DateTime hireDate = Convert.ToDateTime(row["hire_date"]);
                        txtHireDate.Text = hireDate.ToString("MMMM dd, yyyy");
                    }
                    
                    // Set status with badge styling
                    string status = row["librarian_status"]?.ToString() ?? "ACTIVE";
                    txtStatus.Text = status;
                    
                    switch (status.ToUpper())
                    {
                        case "ACTIVE":
                            borderStatus.Background = new SolidColorBrush(Color.FromRgb(212, 237, 218)); // #d4edda
                            txtStatus.Foreground = new SolidColorBrush(Color.FromRgb(21, 87, 36)); // #155724
                            break;
                        case "SUSPENDED":
                            borderStatus.Background = new SolidColorBrush(Color.FromRgb(248, 215, 218)); // #f8d7da
                            txtStatus.Foreground = new SolidColorBrush(Color.FromRgb(114, 28, 36)); // #721c24
                            break;
                        case "EXPIRED":
                            borderStatus.Background = new SolidColorBrush(Color.FromRgb(255, 243, 205)); // #fff3cd
                            txtStatus.Foreground = new SolidColorBrush(Color.FromRgb(133, 100, 4)); // #856404
                            break;
                        default:
                            borderStatus.Background = new SolidColorBrush(Color.FromRgb(236, 240, 241)); // #ecf0f1
                            txtStatus.Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)); // #2c3e50
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("Librarian not found.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService?.GoBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading librarian details: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToMembers_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ManageMembersPage());
        }
    }
}
