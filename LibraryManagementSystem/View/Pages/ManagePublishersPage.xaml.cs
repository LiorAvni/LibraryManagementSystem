using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class ManagePublishersPage : Page
    {
        private readonly PublisherDB _publisherDB;
        private ObservableCollection<PublisherModel> allPublishers;
        private ObservableCollection<PublisherModel> filteredPublishers;
        
        private int currentPage = 0;
        private int pageSize = 20;
        private int totalPages = 0;
        private int totalRecords = 0;

        public ManagePublishersPage()
        {
            InitializeComponent();
            _publisherDB = new PublisherDB();
            allPublishers = new ObservableCollection<PublisherModel>();
            filteredPublishers = new ObservableCollection<PublisherModel>();
            dgPublishers.ItemsSource = filteredPublishers;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPublishers();
        }

        private void LoadPublishers()
        {
            try
            {
                DataTable dt = _publisherDB.GetAllPublishers();
                
                allPublishers.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    allPublishers.Add(new PublisherModel
                    {
                        PublisherId = row["publisher_id"].ToString(),
                        Name = row["name"].ToString(),
                        Country = row["country"] != DBNull.Value ? row["country"].ToString() : "",
                        Website = row["website"] != DBNull.Value ? row["website"].ToString() : "",
                        FoundedYear = row["founded_year"] != DBNull.Value ? Convert.ToInt32(row["founded_year"]) : (int?)null
                    });
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading publishers: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            var searchText = txtSearchName.Text?.Trim().ToLower() ?? "";

            // Filter publishers
            var filtered = allPublishers.Where(publisher =>
            {
                if (string.IsNullOrEmpty(searchText))
                    return true;

                return publisher.Name.ToLower().Contains(searchText);
            }).ToList();

            // Update total records and pages
            totalRecords = filtered.Count;
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            
            // Reset to first page if current page is out of range
            if (currentPage >= totalPages && totalPages > 0)
            {
                currentPage = 0;
            }

            // Get publishers for current page
            filteredPublishers.Clear();
            var pagedPublishers = filtered.Skip(currentPage * pageSize).Take(pageSize);
            foreach (var publisher in pagedPublishers)
            {
                filteredPublishers.Add(publisher);
            }

            // Update UI
            UpdatePaginationUI();
            UpdateFilterInfo();
        }

        private void UpdatePaginationUI()
        {
            if (totalPages > 0)
            {
                txtPaginationInfo.Text = $"Page {currentPage + 1} of {totalPages} ({totalRecords} records)";
                btnFirstPage.IsEnabled = currentPage > 0;
                btnPrevPage.IsEnabled = currentPage > 0;
                btnNextPage.IsEnabled = currentPage < totalPages - 1;
                btnLastPage.IsEnabled = currentPage < totalPages - 1;
                paginationSection.Visibility = Visibility.Visible;
            }
            else
            {
                txtPaginationInfo.Text = "No records found";
                btnFirstPage.IsEnabled = false;
                btnPrevPage.IsEnabled = false;
                btnNextPage.IsEnabled = false;
                btnLastPage.IsEnabled = false;
                paginationSection.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateFilterInfo()
        {
            string infoText = $"{totalRecords} publisher(s) found";
            if (totalPages > 1)
            {
                infoText += $" | Page {currentPage + 1} of {totalPages}";
            }
            txtFilterInfo.Text = infoText;
        }

        // Search handlers
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            currentPage = 0;
            ApplyFilters();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            txtSearchName.Clear();
            currentPage = 0;
            ApplyFilters();
        }

        private void TxtSearchName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search_Click(sender, e);
            }
        }

        // Pagination handlers
        private void FirstPage_Click(object sender, RoutedEventArgs e)
        {
            currentPage = 0;
            ApplyFilters();
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                ApplyFilters();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < totalPages - 1)
            {
                currentPage++;
                ApplyFilters();
            }
        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            currentPage = totalPages - 1;
            ApplyFilters();
        }

        // Action handlers
        private void AddPublisher_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddPublisherPage());
        }

        private void EditPublisher_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PublisherModel publisher)
            {
                NavigationService?.Navigate(new EditPublisherPage(publisher.PublisherId));
            }
        }

        private void DeletePublisher_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PublisherModel publisher)
            {
                // Check if publisher has books
                try
                {
                    int bookCount = _publisherDB.GetPublisherBookCount(publisher.PublisherId);
                    
                    if (bookCount > 0)
                    {
                        MessageBox.Show(
                            $"Cannot delete this publisher.\n\n" +
                            $"{publisher.Name} is currently assigned to {bookCount} book(s) in the library.\n\n" +
                            "Please reassign or remove all books from this publisher before deleting.",
                            "Cannot Delete Publisher",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    // Confirm deletion
                    var result = MessageBox.Show(
                        $"Are you sure you want to delete this publisher?\n\n" +
                        $"Name: {publisher.Name}\n\n" +
                        "This action cannot be undone.",
                        "Confirm Delete",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        bool success = _publisherDB.DeletePublisher(publisher.PublisherId);
                        
                        if (success)
                        {
                            MessageBox.Show(
                                $"Publisher '{publisher.Name}' has been deleted successfully.",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            
                            LoadPublishers(); // Reload the list
                        }
                        else
                        {
                            MessageBox.Show(
                                "Failed to delete publisher. Please try again.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting publisher: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    /// <summary>
    /// Model for displaying publisher information
    /// </summary>
    public class PublisherModel
    {
        public string PublisherId { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string Website { get; set; }
        public int? FoundedYear { get; set; }

        public string FoundedYearDisplay
        {
            get
            {
                return FoundedYear.HasValue ? FoundedYear.Value.ToString() : "N/A";
            }
        }
    }
}
