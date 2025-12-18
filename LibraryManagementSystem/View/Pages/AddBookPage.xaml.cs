using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.View.Pages
{
    public partial class AddBookPage : Page
    {
        private readonly BookDB _bookDB;

        public AddBookPage()
        {
            InitializeComponent();
            _bookDB = new BookDB();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDropdowns();
        }

        private void LoadDropdowns()
        {
            try
            {
                // Load Publishers
                DataTable publishers = _bookDB.GetAllPublishers();
                // Add empty option at the beginning
                DataRow emptyPublisher = publishers.NewRow();
                emptyPublisher["publisher_id"] = DBNull.Value;
                emptyPublisher["name"] = "-- Select Publisher --";
                publishers.Rows.InsertAt(emptyPublisher, 0);
                
                cmbPublisher.ItemsSource = publishers.DefaultView;
                cmbPublisher.SelectedIndex = 0;

                // Load Categories
                DataTable categories = _bookDB.GetAllCategories();
                // Add empty option at the beginning
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

        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            // Hide any previous error messages
            HideError();

            // Validate inputs
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

            if (!int.TryParse(txtCopies.Text, out int totalCopies) || totalCopies < 1)
            {
                ShowError("Total Copies must be a positive number.");
                txtCopies.Focus();
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

                // Add book to database
                string newBookId = _bookDB.AddNewBook(isbn, title, publisherId, publicationYear, 
                                                       categoryId, description, authorIds, totalCopies);

                // Show success message
                MessageBox.Show(
                    $"Book added successfully!{Environment.NewLine}{Environment.NewLine}" +
                    $"Title: {title}{Environment.NewLine}" +
                    $"ISBN: {isbn}{Environment.NewLine}" +
                    $"Copies created: {totalCopies}{Environment.NewLine}" +
                    $"Book ID: {newBookId}",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Navigate back to Manage Books page
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                ShowError($"Error adding book: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to Manage Books page
            NavigationService?.GoBack();
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            borderError.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            borderError.Visibility = Visibility.Collapsed;
            txtError.Text = string.Empty;
        }
    }
}
