using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class EditBookPage : Page
    {
        private readonly BookDB _bookDB;
        private string _bookId;
        private ObservableCollection<BookCopyModel> copies;

        public EditBookPage(string bookId)
        {
            InitializeComponent();
            _bookDB = new BookDB();
            _bookId = bookId;
            copies = new ObservableCollection<BookCopyModel>();
            dgCopies.ItemsSource = copies;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDropdowns();
            LoadBookData();
            LoadBookCopies();
        }

        private void LoadDropdowns()
        {
            try
            {
                // Load Publishers
                DataTable publishers = _bookDB.GetAllPublishers();
                DataRow emptyPublisher = publishers.NewRow();
                emptyPublisher["publisher_id"] = DBNull.Value;
                emptyPublisher["name"] = "-- Select Publisher --";
                publishers.Rows.InsertAt(emptyPublisher, 0);
                
                cmbPublisher.ItemsSource = publishers.DefaultView;
                cmbPublisher.SelectedIndex = 0;

                // Load Categories
                DataTable categories = _bookDB.GetAllCategories();
                DataRow emptyCategory = categories.NewRow();
                emptyCategory["category_id"] = DBNull.Value;
                emptyCategory["name"] = "-- Select Category --";
                categories.Rows.InsertAt(emptyCategory, 0);
                
                cmbCategory.ItemsSource = categories.DefaultView;
                cmbCategory.SelectedIndex = 0;

                // Load Authors
                DataTable authors = _bookDB.GetAllAuthors();
                lstAuthors.ItemsSource = authors.DefaultView;
            }
            catch (Exception ex)
            {
                ShowError($"Error loading data: {ex.Message}");
            }
        }

        private void LoadBookData()
        {
            try
            {
                // Get book details
                DataRow book = _bookDB.GetBookDetailsForEdit(_bookId);
                
                if (book == null)
                {
                    ShowError("Book not found.");
                    return;
                }

                // Populate form fields
                txtBookId.Text = _bookId;
                txtISBN.Text = book["isbn"] != DBNull.Value ? book["isbn"].ToString() : "";
                txtTitle.Text = book["title"] != DBNull.Value ? book["title"].ToString() : "";
                txtYear.Text = book["publication_year"] != DBNull.Value ? book["publication_year"].ToString() : "0";
                txtDescription.Text = book["description"] != DBNull.Value ? book["description"].ToString() : "";

                // Set publisher selection
                if (book["publisher_id"] != DBNull.Value)
                {
                    string publisherId = book["publisher_id"].ToString();
                    foreach (DataRowView item in cmbPublisher.Items)
                    {
                        if (item["publisher_id"] != DBNull.Value && item["publisher_id"].ToString() == publisherId)
                        {
                            cmbPublisher.SelectedItem = item;
                            break;
                        }
                    }
                }

                // Set category selection
                if (book["category_id"] != DBNull.Value)
                {
                    string categoryId = book["category_id"].ToString();
                    foreach (DataRowView item in cmbCategory.Items)
                    {
                        if (item["category_id"] != DBNull.Value && item["category_id"].ToString() == categoryId)
                        {
                            cmbCategory.SelectedItem = item;
                            break;
                        }
                    }
                }

                // Load and select authors
                List<string> bookAuthorIds = _bookDB.GetBookAuthorIds(_bookId);
                foreach (DataRowView item in lstAuthors.Items)
                {
                    string authorId = item["author_id"].ToString();
                    if (bookAuthorIds.Contains(authorId))
                    {
                        lstAuthors.SelectedItems.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading book data: {ex.Message}");
            }
        }

        private void LoadBookCopies()
        {
            try
            {
                copies.Clear();
                DataTable dt = _bookDB.GetBookCopies(_bookId);

                foreach (DataRow row in dt.Rows)
                {
                    string copyId = row["copy_id"].ToString();
                    bool inUse = _bookDB.IsCopyInUse(copyId);

                    copies.Add(new BookCopyModel
                    {
                        CopyId = copyId,
                        CopyNumber = Convert.ToInt32(row["copy_number"]),
                        Status = row["status"].ToString(),
                        Condition = row["condition"].ToString(),
                        Location = row["location"] != DBNull.Value ? row["location"].ToString() : "",
                        IsInUse = inUse
                    });
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading book copies: {ex.Message}");
            }
        }

        private void UpdateBook_Click(object sender, RoutedEventArgs e)
        {
            HideMessages();

            // Validate
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                ShowError("Title is required.");
                txtTitle.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtISBN.Text))
            {
                ShowError("ISBN is required.");
                txtISBN.Focus();
                return;
            }

            // Show confirmation dialog
            var result = MessageBox.Show(
                $"Are you sure you want to update this book?{Environment.NewLine}{Environment.NewLine}" +
                $"Title: {txtTitle.Text.Trim()}{Environment.NewLine}" +
                $"ISBN: {txtISBN.Text.Trim()}{Environment.NewLine}{Environment.NewLine}" +
                "This will save all changes to the book information.",
                "Confirm Update Book",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                // Get form values
                string isbn = txtISBN.Text.Trim();
                string title = txtTitle.Text.Trim();
                string publisherId = cmbPublisher.SelectedValue?.ToString();
                int publicationYear = int.TryParse(txtYear.Text, out int year) ? year : 0;
                string categoryId = cmbCategory.SelectedValue?.ToString();
                string description = txtDescription.Text.Trim();

                // Get selected authors
                List<string> authorIds = new List<string>();
                foreach (DataRowView item in lstAuthors.SelectedItems)
                {
                    string authorId = item["author_id"].ToString();
                    if (!string.IsNullOrEmpty(authorId))
                    {
                        authorIds.Add(authorId);
                    }
                }

                // Update book
                bool success = _bookDB.UpdateBook(_bookId, isbn, title, publisherId, publicationYear, 
                                                   categoryId, description, authorIds);

                if (success)
                {
                    ShowSuccess("Book updated successfully!");
                }
                else
                {
                    ShowError("Failed to update book.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error updating book: {ex.Message}");
            }
        }

        private void SaveCopy_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BookCopyModel copy)
            {
                try
                {
                    bool success = _bookDB.UpdateBookCopy(copy.CopyId, copy.Status, copy.Condition, copy.Location);

                    if (success)
                    {
                        ShowSuccess($"Copy #{copy.CopyNumber} updated successfully!");
                    }
                    else
                    {
                        ShowError("Failed to update copy.");
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Error updating copy: {ex.Message}");
                }
            }
        }

        private void AddCopy_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to add a new copy of this book?",
                "Add New Copy",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string newCopyId = _bookDB.AddBookCopy(_bookId);

                    if (!string.IsNullOrEmpty(newCopyId))
                    {
                        ShowSuccess("New copy added successfully!");
                        LoadBookCopies(); // Reload to show new copy
                    }
                    else
                    {
                        ShowError("Failed to add new copy.");
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Error adding copy: {ex.Message}");
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
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

        private void HideMessages()
        {
            borderError.Visibility = Visibility.Collapsed;
            borderSuccess.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Model for displaying book copy information in DataGrid
    /// </summary>
    public class BookCopyModel : INotifyPropertyChanged
    {
        private string _status;
        private string _condition;
        private string _location;

        public string CopyId { get; set; }
        public int CopyNumber { get; set; }
        
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
        
        public string Condition
        {
            get => _condition;
            set
            {
                _condition = value;
                OnPropertyChanged(nameof(Condition));
            }
        }
        
        public string Location
        {
            get => _location;
            set
            {
                _location = value;
                OnPropertyChanged(nameof(Location));
            }
        }

        public bool IsInUse { get; set; }

        public bool CanEditStatus => !IsInUse;

        public Visibility StatusLockedVisibility => IsInUse ? Visibility.Visible : Visibility.Collapsed;

        public List<StatusOption> StatusOptions => new List<StatusOption>
        {
            new StatusOption { Value = "AVAILABLE", IsEnabled = true },
            new StatusOption { Value = "BORROWED", IsEnabled = false }, // Disabled - use Lend Book feature
            new StatusOption { Value = "RESERVED", IsEnabled = true },
            new StatusOption { Value = "IN_REPAIR", IsEnabled = true },
            new StatusOption { Value = "DAMAGED", IsEnabled = true },
            new StatusOption { Value = "LOST", IsEnabled = true },
            new StatusOption { Value = "RETIRED", IsEnabled = true }
        };

        public List<string> ConditionOptions => new List<string>
        {
            "NEW",
            "GOOD",
            "FAIR",
            "POOR",
            "DAMAGED"
        };

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Helper class for status dropdown options with enabled/disabled state
    /// </summary>
    public class StatusOption
    {
        public string Value { get; set; }
        public bool IsEnabled { get; set; }
    }
}
