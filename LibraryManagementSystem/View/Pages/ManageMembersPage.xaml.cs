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
        private readonly UserDB _userDB;
        private ObservableCollection<MemberModel> members;
        private ObservableCollection<LibrarianModel> librarians;
        private ObservableCollection<AdminModel> admins;
        private bool isAdmin = false;

        public ManageMembersPage()
        {
            InitializeComponent();
            _memberDB = new MemberDB();
            _userDB = new UserDB();
            members = new ObservableCollection<MemberModel>();
            librarians = new ObservableCollection<LibrarianModel>();
            admins = new ObservableCollection<AdminModel>();
            
            dgMembers.ItemsSource = members;
            dgLibrarians.ItemsSource = librarians;
            dgAdmins.ItemsSource = admins;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CheckAdminStatus();
            
            if (isAdmin)
            {
                // Admin view - show all three sections
                txtPageTitle.Text = "Manage Users";
                membersSectionContainer.Visibility = Visibility.Visible;
                librariansSectionContainer.Visibility = Visibility.Visible;
                adminsSectionContainer.Visibility = Visibility.Visible;
                
                LoadMemberStatistics();
                LoadMembers();
                LoadLibrarianStatistics();
                LoadLibrarians();
                LoadAdminStatistics();
                LoadAdmins();
            }
            else
            {
                // Regular librarian view - show only members
                txtPageTitle.Text = "Manage Members";
                pageHeaderBorder.Visibility = Visibility.Collapsed; // Hide duplicate header
                membersSectionContainer.Visibility = Visibility.Visible;
                librariansSectionContainer.Visibility = Visibility.Collapsed;
                adminsSectionContainer.Visibility = Visibility.Collapsed;
                
                LoadMemberStatistics();
                LoadMembers();
            }
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

        #region Members Section

        private void LoadMemberStatistics()
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
                MessageBox.Show($"Error loading member statistics: {ex.Message}", "Error", 
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
                            
                            LoadMemberStatistics();
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
                            
                            LoadMemberStatistics();
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

        #endregion

        #region Librarians Section

        private void LoadLibrarianStatistics()
        {
            try
            {
                DataTable dt = _userDB.GetLibrarianStatistics();
                
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    txtTotalLibrarians.Text = row["TotalLibrarians"]?.ToString() ?? "0";
                    txtActiveLibrarians.Text = row["ActiveLibrarians"]?.ToString() ?? "0";
                    txtSuspendedLibrarians.Text = row["SuspendedLibrarians"]?.ToString() ?? "0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading librarian statistics: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadLibrarians()
        {
            try
            {
                DataTable dt = _userDB.GetAllLibrariansWithDetails();
                
                System.Diagnostics.Debug.WriteLine($"LoadLibrarians: Retrieved {dt.Rows.Count} rows");
                
                librarians.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    var librarian = new LibrarianModel
                    {
                        LibrarianId = row["librarian_id"].ToString(),
                        EmployeeId = row["employee_id"]?.ToString() ?? "",
                        FullName = row["full_name"].ToString(),
                        Email = row["email"]?.ToString() ?? "",
                        HireDate = row["hire_date"] != DBNull.Value 
                            ? Convert.ToDateTime(row["hire_date"]) 
                            : DateTime.Now,
                        Status = row["librarian_status"]?.ToString() ?? "ACTIVE"
                    };
                    
                    System.Diagnostics.Debug.WriteLine($"Adding librarian: {librarian.FullName} - {librarian.Status}");
                    librarians.Add(librarian);
                }
                
                System.Diagnostics.Debug.WriteLine($"Total librarians in collection: {librarians.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadLibrarians ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error loading librarians: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewLibrarian_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is LibrarianModel librarian)
            {
                MessageBox.Show($"View Librarian Details\n\nID: {librarian.LibrarianId}\nName: {librarian.FullName}", 
                    "Librarian Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ToggleSuspendLibrarian_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is LibrarianModel librarian)
            {
                try
                {
                    bool newStatus = librarian.Status != "ACTIVE";
                    string action = !newStatus ? "suspend" : "activate";
                    
                    var result = MessageBox.Show(
                        $"Are you sure you want to {action} this librarian?\n\n" +
                        $"Name: {librarian.FullName}\n" +
                        $"Current Status: {librarian.Status}",
                        $"Confirm {action.ToUpper()}",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        bool success = _userDB.UpdateUserStatus(librarian.LibrarianId, newStatus);
                        
                        if (success)
                        {
                            MessageBox.Show(
                                $"Librarian status updated to {(newStatus ? "ACTIVE" : "SUSPENDED")}.",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            
                            LoadLibrarianStatistics();
                            LoadLibrarians();
                        }
                        else
                        {
                            MessageBox.Show(
                                "Failed to update librarian status.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating librarian status: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteLibrarian_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is LibrarianModel librarian)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete this librarian?\n\n" +
                    $"Name: {librarian.FullName}\n" +
                    $"Employee ID: {librarian.EmployeeId}\n\n" +
                    "This will mark the librarian as deleted.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = _userDB.DeleteLibrarian(librarian.LibrarianId);
                        
                        if (success)
                        {
                            MessageBox.Show(
                                $"Librarian '{librarian.FullName}' has been deleted successfully.",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            
                            LoadLibrarianStatistics();
                            LoadLibrarians();
                        }
                        else
                        {
                            MessageBox.Show(
                                "Failed to delete librarian.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting librarian: {ex.Message}", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        #endregion

        #region Admins Section

        private void LoadAdminStatistics()
        {
            try
            {
                DataTable dt = _userDB.GetAdminStatistics();
                
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    txtTotalAdmins.Text = row["TotalAdmins"]?.ToString() ?? "0";
                    txtActiveAdmins.Text = row["ActiveAdmins"]?.ToString() ?? "0";
                    txtSuspendedAdmins.Text = row["SuspendedAdmins"]?.ToString() ?? "0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading admin statistics: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAdmins()
        {
            try
            {
                DataTable dt = _userDB.GetAllAdminsWithDetails();
                
                System.Diagnostics.Debug.WriteLine($"LoadAdmins: Retrieved {dt.Rows.Count} rows");
                
                admins.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    var admin = new AdminModel
                    {
                        AdminId = row["librarian_id"].ToString(),
                        EmployeeId = row["employee_id"]?.ToString() ?? "",
                        FullName = row["full_name"].ToString(),
                        Email = row["email"]?.ToString() ?? "",
                        HireDate = row["hire_date"] != DBNull.Value 
                            ? Convert.ToDateTime(row["hire_date"]) 
                            : DateTime.Now,
                        Status = row["librarian_status"]?.ToString() ?? "ACTIVE"
                    };
                    
                    System.Diagnostics.Debug.WriteLine($"Adding admin: {admin.FullName} - {admin.Status}");
                    admins.Add(admin);
                }
                
                System.Diagnostics.Debug.WriteLine($"Total admins in collection: {admins.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadAdmins ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error loading admins: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is AdminModel admin)
            {
                MessageBox.Show($"View Admin Details\n\nID: {admin.AdminId}\nName: {admin.FullName}", 
                    "Admin Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ToggleSuspendAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is AdminModel admin)
            {
                try
                {
                    bool newStatus = admin.Status != "ACTIVE";
                    string action = !newStatus ? "suspend" : "activate";
                    
                    var result = MessageBox.Show(
                        $"Are you sure you want to {action} this admin?\n\n" +
                        $"Name: {admin.FullName}\n" +
                        $"Current Status: {admin.Status}",
                        $"Confirm {action.ToUpper()}",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        bool success = _userDB.UpdateUserStatus(admin.AdminId, newStatus);
                        
                        if (success)
                        {
                            MessageBox.Show(
                                $"Admin status updated to {(newStatus ? "ACTIVE" : "SUSPENDED")}.",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            
                            LoadAdminStatistics();
                            LoadAdmins();
                        }
                        else
                        {
                            MessageBox.Show(
                                "Failed to update admin status.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating admin status: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is AdminModel admin)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete this admin?\n\n" +
                    $"Name: {admin.FullName}\n" +
                    $"Employee ID: {admin.EmployeeId}\n\n" +
                    "This will mark the admin as deleted.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = _userDB.DeleteLibrarian(admin.AdminId);
                        
                        if (success)
                        {
                            MessageBox.Show(
                                $"Admin '{admin.FullName}' has been deleted successfully.",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            
                            LoadAdminStatistics();
                            LoadAdmins();
                        }
                        else
                        {
                            MessageBox.Show(
                                "Failed to delete admin.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting admin: {ex.Message}", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        #endregion
    }

    #region Model Classes

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

    // Model for displaying librarian data in DataGrid
    public class LibrarianModel
    {
        public string LibrarianId { get; set; }
        public string EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime HireDate { get; set; }
        public string Status { get; set; }

        public string HireDateFormatted => HireDate.ToString("MMM dd, yyyy");

        public SolidColorBrush StatusBadgeBackground
        {
            get
            {
                return Status?.ToUpper() switch
                {
                    "ACTIVE" => new SolidColorBrush(Color.FromRgb(212, 237, 218)), // #d4edda
                    "SUSPENDED" => new SolidColorBrush(Color.FromRgb(248, 215, 218)), // #f8d7da
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
                    _ => new SolidColorBrush(Color.FromRgb(44, 62, 80)) // #2c3e50
                };
            }
        }
        
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

    // Model for displaying admin data in DataGrid
    public class AdminModel
    {
        public string AdminId { get; set; }
        public string EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime HireDate { get; set; }
        public string Status { get; set; }

        public string HireDateFormatted => HireDate.ToString("MMM dd, yyyy");

        public SolidColorBrush StatusBadgeBackground
        {
            get
            {
                return Status?.ToUpper() switch
                {
                    "ACTIVE" => new SolidColorBrush(Color.FromRgb(212, 237, 218)), // #d4edda
                    "SUSPENDED" => new SolidColorBrush(Color.FromRgb(248, 215, 218)), // #f8d7da
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
                    _ => new SolidColorBrush(Color.FromRgb(44, 62, 80)) // #2c3e50
                };
            }
        }
        
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

    #endregion
}
