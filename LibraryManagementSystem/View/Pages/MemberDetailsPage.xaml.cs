using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class MemberDetailsPage : Page
    {
        private readonly MemberDB _memberDB;
        private readonly string _memberId;

        public MemberDetailsPage(string memberId)
        {
            InitializeComponent();
            _memberDB = new MemberDB();
            _memberId = memberId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMemberDetails();
        }

        private void LoadMemberDetails()
        {
            try
            {
                DataTable dt = _memberDB.GetMemberDetailsById(_memberId);
                
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    
                    // Set member information
                    txtMemberId.Text = row["member_id"].ToString();
                    txtFullName.Text = $"{row["first_name"]} {row["last_name"]}";
                    txtEmail.Text = row["email"]?.ToString() ?? "N/A";
                    txtPhone.Text = row["phone"]?.ToString() ?? "N/A";
                    txtAddress.Text = row["address"]?.ToString() ?? "N/A";
                    txtUserId.Text = row["user_id"].ToString();
                    
                    // Format membership date
                    if (row["membership_date"] != DBNull.Value)
                    {
                        DateTime membershipDate = Convert.ToDateTime(row["membership_date"]);
                        txtMembershipDate.Text = membershipDate.ToString("MMMM dd, yyyy");
                    }
                    
                    // Set status with badge styling
                    string status = row["membership_status"]?.ToString() ?? "ACTIVE";
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
                    MessageBox.Show("Member not found.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService?.GoBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading member details: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToMembers_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ManageMembersPage());
        }

        private void NavDashboard_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LibrarianDashboard());
        }

        private void NavMembers_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ManageMembersPage());
        }
    }
}
