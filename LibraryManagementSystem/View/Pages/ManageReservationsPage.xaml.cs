using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class ManageReservationsPage : Page
    {
        private readonly ReservationDB _reservationDB;
        private ObservableCollection<ManageReservationDisplayModel> reservations;

        public ManageReservationsPage()
        {
            InitializeComponent();
            _reservationDB = new ReservationDB();
            reservations = new ObservableCollection<ManageReservationDisplayModel>();
            dgReservations.ItemsSource = reservations;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadReservations();
        }

        private void LoadReservations()
        {
            try
            {
                reservations.Clear();

                // Get all reservations from database
                DataTable dt = _reservationDB.GetAllReservationsForManagement();

                foreach (DataRow row in dt.Rows)
                {
                    reservations.Add(new ManageReservationDisplayModel
                    {
                        ReservationId = row["reservation_id"]?.ToString() ?? "",
                        BookId = row["book_id"]?.ToString() ?? "",
                        BookTitle = row["BookTitle"]?.ToString() ?? "N/A",
                        MemberName = row["MemberName"]?.ToString() ?? "N/A",
                        ReservationDate = row["reservation_date"] != DBNull.Value ? Convert.ToDateTime(row["reservation_date"]) : DateTime.Now,
                        ExpiryDate = row["expiry_date"] != DBNull.Value ? Convert.ToDateTime(row["expiry_date"]) : (DateTime?)null,
                        Status = row["reservation_status"]?.ToString() ?? "PENDING"
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading reservations: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApproveReservation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ManageReservationDisplayModel reservation)
            {
                var result = MessageBox.Show(
                    $"Approve this reservation?{Environment.NewLine}{Environment.NewLine}" +
                    $"Book: {reservation.BookTitle}{Environment.NewLine}" +
                    $"Member: {reservation.MemberName}{Environment.NewLine}{Environment.NewLine}" +
                    "This will set aside a copy for the member.",
                    "Confirm Approval",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = _reservationDB.ApproveReservation(reservation.ReservationId, reservation.BookId);

                        if (success)
                        {
                            MessageBox.Show(
                                $"Reservation approved successfully!{Environment.NewLine}{Environment.NewLine}" +
                                $"Book: {reservation.BookTitle}{Environment.NewLine}" +
                                $"Member: {reservation.MemberName}{Environment.NewLine}{Environment.NewLine}" +
                                "A copy has been reserved for the member.",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                            // Reload reservations
                            LoadReservations();
                        }
                        else
                        {
                            MessageBox.Show("Failed to approve the reservation. Please try again.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error approving reservation: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CancelReservation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ManageReservationDisplayModel reservation)
            {
                string statusMessage = reservation.Status == "RESERVED" 
                    ? "This will delete the reservation and release the copy back to AVAILABLE."
                    : "This will delete the reservation.";
                
                var result = MessageBox.Show(
                    $"Cancel this reservation?{Environment.NewLine}{Environment.NewLine}" +
                    $"Book: {reservation.BookTitle}{Environment.NewLine}" +
                    $"Member: {reservation.MemberName}{Environment.NewLine}{Environment.NewLine}" +
                    statusMessage,
                    "Confirm Cancellation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Delete the reservation (same as member cancellation)
                        bool success = _reservationDB.CancelReservation(reservation.ReservationId);

                        if (success)
                        {
                            MessageBox.Show(
                                $"Reservation cancelled successfully!{Environment.NewLine}{Environment.NewLine}" +
                                $"Book: {reservation.BookTitle}{Environment.NewLine}" +
                                $"Member: {reservation.MemberName}{Environment.NewLine}{Environment.NewLine}" +
                                "The reservation has been deleted.",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                            // Reload reservations
                            LoadReservations();
                        }
                        else
                        {
                            MessageBox.Show("Failed to cancel the reservation. Please try again.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error cancelling reservation: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DisApproveReservation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ManageReservationDisplayModel reservation)
            {
                var result = MessageBox.Show(
                    $"Dis-approve this reservation?{Environment.NewLine}{Environment.NewLine}" +
                    $"Book: {reservation.BookTitle}{Environment.NewLine}" +
                    $"Member: {reservation.MemberName}{Environment.NewLine}{Environment.NewLine}" +
                    "This will change the status from RESERVED back to PENDING and release the copy.",
                    "Confirm Dis-Approval",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = _reservationDB.DisApproveReservation(reservation.ReservationId);

                        if (success)
                        {
                            MessageBox.Show(
                                $"Reservation dis-approved successfully!{Environment.NewLine}{Environment.NewLine}" +
                                $"Book: {reservation.BookTitle}{Environment.NewLine}" +
                                $"Member: {reservation.MemberName}{Environment.NewLine}{Environment.NewLine}" +
                                "Status changed back to PENDING. The copy has been released.",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                            // Reload reservations
                            LoadReservations();
                        }
                        else
                        {
                            MessageBox.Show("Failed to dis-approve the reservation. Please try again.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error dis-approving reservation: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }

    public class ManageReservationDisplayModel
    {
        public string ReservationId { get; set; }
        public string BookId { get; set; }
        public string BookTitle { get; set; }
        public string MemberName { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; }

        // PENDING: Show Approve (green) and Cancel (red)
        public bool CanApprove => Status == "PENDING";
        public bool CanCancelPending => Status == "PENDING";
        
        // RESERVED: Show Dis-Approve (red) and Cancel (red)
        public bool CanDisApprove => Status == "RESERVED";
        public bool CanCancelReserved => Status == "RESERVED";
    }
}

