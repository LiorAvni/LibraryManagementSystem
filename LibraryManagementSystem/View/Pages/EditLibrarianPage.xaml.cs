using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class EditLibrarianPage : Page
    {
        private readonly MemberDB _memberDB;
        private readonly string _librarianId;

        public EditLibrarianPage(string librarianId)
        {
            InitializeComponent();
            _memberDB = new MemberDB();
            _librarianId = librarianId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLibrarianData();
        }

        private void LoadLibrarianData()
        {
            try
            {
                DataTable dt = _memberDB.GetLibrarianDetailsById(_librarianId);
                
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    
                    // Set info box
                    txtInfoLibrarianId.Text = row["librarian_id"].ToString();
                    txtInfoUserId.Text = row["user_id"].ToString();
                    txtInfoEmployeeId.Text = row["employee_id"]?.ToString() ?? "N/A";
                    
                    if (row["hire_date"] != DBNull.Value)
                    {
                        DateTime hireDate = Convert.ToDateTime(row["hire_date"]);
                        txtInfoHireDate.Text = hireDate.ToString("MMMM dd, yyyy");
                    }
                    
                    // Set form fields
                    txtFirstName.Text = row["first_name"]?.ToString() ?? "";
                    txtLastName.Text = row["last_name"]?.ToString() ?? "";
                    txtEmail.Text = row["email"]?.ToString() ?? "";
                    txtPhone.Text = row["phone"]?.ToString() ?? "";
                    txtAddress.Text = row["address"]?.ToString() ?? "";
                    
                    // Set status
                    string status = row["librarian_status"]?.ToString() ?? "ACTIVE";
                    foreach (ComboBoxItem item in cmbLibrarianStatus.Items)
                    {
                        if (item.Content.ToString() == status)
                        {
                            cmbLibrarianStatus.SelectedItem = item;
                            break;
                        }
                    }
                }
                else
                {
                    ShowError("Librarian not found.");
                    NavigationService?.GoBack();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading librarian data: {ex.Message}");
            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Hide previous messages
                borderError.Visibility = Visibility.Collapsed;
                borderSuccess.Visibility = Visibility.Collapsed;
                
                // Validate required fields
                if (string.IsNullOrWhiteSpace(txtFirstName.Text))
                {
                    ShowError("First Name is required.");
                    txtFirstName.Focus();
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(txtLastName.Text))
                {
                    ShowError("Last Name is required.");
                    txtLastName.Focus();
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    ShowError("Email is required.");
                    txtEmail.Focus();
                    return;
                }
                
                // Validate email format
                if (!IsValidEmail(txtEmail.Text))
                {
                    ShowError("Please enter a valid email address.");
                    txtEmail.Focus();
                    return;
                }
                
                // Get values
                string firstName = txtFirstName.Text.Trim();
                string lastName = txtLastName.Text.Trim();
                string email = txtEmail.Text.Trim();
                string phone = txtPhone.Text.Trim();
                string address = txtAddress.Text.Trim();
                string status = (cmbLibrarianStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "ACTIVE";
                
                // Update librarian
                bool success = _memberDB.UpdateLibrarianInformation(_librarianId, firstName, lastName, 
                                                                     email, phone, address, status);
                
                if (success)
                {
                    ShowSuccess($"Librarian information updated successfully!\n\nName: {firstName} {lastName}\nStatus: {status}");
                    
                    // Navigate back after 2 seconds
                    var timer = new System.Windows.Threading.DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(2);
                    timer.Tick += (s, args) =>
                    {
                        timer.Stop();
                        NavigationService?.Navigate(new ManageMembersPage());
                    };
                    timer.Start();
                }
                else
                {
                    ShowError("Failed to update librarian information. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error saving changes: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ManageMembersPage());
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            borderError.Visibility = Visibility.Visible;
            borderSuccess.Visibility = Visibility.Collapsed;
        }

        private void ShowSuccess(string message)
        {
            txtSuccess.Text = message;
            borderSuccess.Visibility = Visibility.Visible;
            borderError.Visibility = Visibility.Collapsed;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
