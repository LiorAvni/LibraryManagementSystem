using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class ManageMembersPage : Page
    {
        private readonly MemberDB _memberDB;
        private ObservableCollection<MemberModel> members;
        private bool isAdmin = false;

        public ManageMembersPage()
        {
            InitializeComponent();
            _memberDB = new MemberDB();
            members = new ObservableCollection<MemberModel>();
            dgMembers.ItemsSource = members;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CheckAdminStatus();
            LoadStatistics();
            LoadMembers();
        }

        private void CheckAdminStatus()
        {
            try
            {
                var currentUser = MainWindow.CurrentUser;
                if (currentUser != null && !string.IsNullOrEmpty(currentUser.UserIdString))
                {
                    isAdmin = _memberDB.IsUserAdmin(currentUser.UserIdString);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking admin status: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStatistics()
        {
            try
            {
                DataTable dt = _memberDB.GetMemberStatistics();
                
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    txtTotalMembers.Text = row["TotalMembers"]?.ToString() ?? "0";
                    txtActiveMembers.Text = row["ActiveMembers"]?.ToString() ?? "0";
                    txtSuspendedMembers.Text = row["SuspendedMembers"]?.ToString() ?? "0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading statistics: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMembers()
        {
            try
            {
                DataTable dt = _memberDB.GetAllMembersWithDetails();
                
                members.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    members.Add(new MemberModel
                    {
                        MemberId = row["member_id"].ToString(),
                        FullName = row["full_name"].ToString(),
                        Email = row["email"]?.ToString() ?? "",
                        Phone = row["phone"]?.ToString() ?? "",
                        MembershipDate = row["membership_date"] != DBNull.Value 
                            ? Convert.ToDateTime(row["membership_date"]) 
                            : DateTime.Now,
                        Status = row["membership_status"]?.ToString() ?? "ACTIVE",
                        IsAdmin = isAdmin
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading members: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewMember_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is MemberModel member)
            {
                NavigationService?.Navigate(new MemberDetailsPage(member.MemberId));
            }
        }

        private void EditMember_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is MemberModel member)
            {
                NavigationService?.Navigate(new EditMemberPage(member.MemberId));
            }
        }

        private void ToggleSuspend_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is MemberModel member)
            {
                try
                {
                    string newStatus = member.Status == "ACTIVE" ? "SUSPENDED" : "ACTIVE";
                    string action = newStatus == "SUSPENDED" ? "suspend" : "activate";
                    
                    var result = MessageBox.Show(
                        $"Are you sure you want to {action} this member?\n\n" +
                        $"Name: {member.FullName}\n" +
                        $"Current Status: {member.Status}",
                        $"Confirm {action.ToUpper()}",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        bool success = _memberDB.UpdateMemberStatus(member.MemberId, newStatus);
                        
                        if (success)
                        {
                            MessageBox.Show(
                                $"Member status updated to {newStatus}.",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            
                            LoadStatistics();
                            LoadMembers();
                        }
                        else
                        {
                            MessageBox.Show(
                                "Failed to update member status.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating member status: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteMember_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is MemberModel member)
            {
                try
                {
                    // Check for active loans/reservations
                    DataTable activityDt = _memberDB.GetMemberActivityCount(member.MemberId);
                    
                    if (activityDt.Rows.Count > 0)
                    {
                        DataRow row = activityDt.Rows[0];
                        int activeLoans = row["ActiveLoans"] != DBNull.Value ? Convert.ToInt32(row["ActiveLoans"]) : 0;
                        int activeReservations = row["ActiveReservations"] != DBNull.Value ? Convert.ToInt32(row["ActiveReservations"]) : 0;
                        
                        if (activeLoans > 0 || activeReservations > 0)
                        {
                            MessageBox.Show(
                                $"Cannot delete this member.\n\n" +
                                $"{member.FullName} has:\n" +
                                $"- {activeLoans} active loan(s)\n" +
                                $"- {activeReservations} active reservation(s)\n\n" +
                                "Please ensure all loans are returned and reservations are cancelled before deleting.",
                                "Cannot Delete Member",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }
                    }
                    
                    var result = MessageBox.Show(
                        $"Are you sure you want to delete this member?\n\n" +
                        $"Name: {member.FullName}\n" +
                        $"Email: {member.Email}\n\n" +
                        "This action cannot be undone!",
                        "Confirm Delete",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        bool success = _memberDB.DeleteMember(member.MemberId);
                        
                        if (success)
                        {
                            MessageBox.Show(
                                $"Member '{member.FullName}' has been deleted successfully.",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            
                            LoadStatistics();
                            LoadMembers();
                        }
                        else
                        {
                            MessageBox.Show(
                                "Failed to delete member.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting member: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    // Model for displaying member data in DataGrid
    public class MemberModel
    {
        public string MemberId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime MembershipDate { get; set; }
        public string Status { get; set; }
        public bool IsAdmin { get; set; }

        public string MembershipDateFormatted => MembershipDate.ToString("MMM dd, yyyy");

        public SolidColorBrush StatusBadgeBackground
        {
            get
            {
                return Status?.ToUpper() switch
                {
                    "ACTIVE" => new SolidColorBrush(Color.FromRgb(212, 237, 218)), // #d4edda
                    "SUSPENDED" => new SolidColorBrush(Color.FromRgb(248, 215, 218)), // #f8d7da
                    "EXPIRED" => new SolidColorBrush(Color.FromRgb(255, 243, 205)), // #fff3cd
                    _ => new SolidColorBrush(Color.FromRgb(236, 240, 241)) // #ecf0f1
                };
            }
        }

        public SolidColorBrush StatusBadgeForeground
        {
            get
            {
                return Status?.ToUpper() switch
                {
                    "ACTIVE" => new SolidColorBrush(Color.FromRgb(21, 87, 36)), // #155724
                    "SUSPENDED" => new SolidColorBrush(Color.FromRgb(114, 28, 36)), // #721c24
                    "EXPIRED" => new SolidColorBrush(Color.FromRgb(133, 100, 4)), // #856404
                    _ => new SolidColorBrush(Color.FromRgb(44, 62, 80)) // #2c3e50
                };
            }
        }

        public Visibility AdminActionsVisibility => IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        
        public string SuspendButtonText => Status == "ACTIVE" ? "Suspend" : "Activate";
        
        public SolidColorBrush SuspendButtonBackground
        {
            get
            {
                return Status == "ACTIVE" 
                    ? new SolidColorBrush(Color.FromRgb(243, 156, 18)) // #f39c12 (warning/orange)
                    : new SolidColorBrush(Color.FromRgb(39, 174, 96)); // #27ae60 (success/green)
            }
        }
    }
}

