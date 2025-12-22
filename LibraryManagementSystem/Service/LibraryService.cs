using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LibraryManagementSystem.Model;
using LibraryManagementSystem.ViewModel;

namespace LibraryManagementSystem.Service
{
    /// <summary>
    /// Implementation of Library Service with business logic
    /// </summary>
    public class LibraryService : ILibraryService
    {
        private readonly UserDB _userDB;
        private readonly MemberDB _memberDB;
        private readonly BookDB _bookDB;
        private readonly BookCopyDB _bookCopyDB;
        private readonly LoanDB _loanDB;
        private readonly ReservationDB _reservationDB;
        private readonly AuthorDB _authorDB;
        private readonly PublisherDB _publisherDB;
        private readonly CategoryDB _categoryDB;

        // Business rule constants
        private const int MAX_BOOKS_PER_MEMBER = 3;
        private const int MAX_RESERVATIONS_PER_MEMBER = 3;
        private const int DEFAULT_LOAN_PERIOD_DAYS = 14;
        private const int MAX_RENEWAL_COUNT = 2;
        private const decimal FINE_PER_DAY = 0.50m;
        private const decimal MAX_FINE_ALLOWED = 10.00m;

        public LibraryService()
        {
            _userDB = new UserDB();
            _memberDB = new MemberDB();
            _bookDB = new BookDB();
            _bookCopyDB = new BookCopyDB();
            _loanDB = new LoanDB();
            _reservationDB = new ReservationDB();
            _authorDB = new AuthorDB();
            _publisherDB = new PublisherDB();
            _categoryDB = new CategoryDB();
        }

        // ========== AUTHENTICATION ==========
        
        public User Login(string email, string password)
        {
            try
            {
                string passwordHash = HashPassword(password);
                return _userDB.Login(email, passwordHash);
            }
            catch (Exception ex)
            {
                throw new Exception($"Login failed: {ex.Message}", ex);
            }
        }

        public bool IsUserActive(int userId)
        {
            try
            {
                User user = _userDB.GetUserByID(userId);
                return user != null && user.IsActive;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to check user status: {ex.Message}", ex);
            }
        }

        // ========== MEMBER OPERATIONS ==========

        public bool LoanBook(int memberId, int copyId)
        {
            try
            {
                // Get member details
                Member member = _memberDB.GetMemberByID(memberId);
                if (member == null || member.Status != "Active")
                    throw new Exception("Member is not active");

                // Check loan limit
                int activeLoanCount = _loanDB.GetActiveLoansCount(memberId);
                if (activeLoanCount >= MAX_BOOKS_PER_MEMBER)
                    throw new Exception($"Member has reached maximum book limit ({MAX_BOOKS_PER_MEMBER})");

                // Check fines
                if (member.FinesOwed > MAX_FINE_ALLOWED)
                    throw new Exception($"Member has outstanding fines exceeding ${MAX_FINE_ALLOWED}. Please pay fines before borrowing.");

                // Check if copy is available
                BookCopy copy = _bookCopyDB.GetCopyByID(copyId);
                if (copy == null || copy.Status != "Available")
                    throw new Exception("Book copy is not available");

                // Create loan
                Loan loan = new Loan
                {
                    CopyID = copyId,
                    MemberID = memberId,
                    LoanDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(DEFAULT_LOAN_PERIOD_DAYS),
                    Status = "Active"
                };

                // Update database
                bool loanCreated = _loanDB.InsertLoan(loan);
                if (loanCreated)
                {
                    _bookCopyDB.UpdateCopyStatus(copyId, "Borrowed");
                    _bookDB.UpdateAvailableCopies(copy.BookID, -1);
                    _memberDB.UpdateCurrentBooksCount(memberId, 1);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to loan book: {ex.Message}", ex);
            }
        }

        public bool ReserveBook(int memberId, int bookId)
        {
            try
            {
                // Get member details
                Member member = _memberDB.GetMemberByID(memberId);
                if (member == null || member.Status != "Active")
                    throw new Exception("Member is not active");

                // Check reservation limit
                var activeReservations = _reservationDB.SelectReservationsByMember(memberId)
                    .GetPendingReservations();
                if (activeReservations.Count >= MAX_RESERVATIONS_PER_MEMBER)
                    throw new Exception($"Member has reached maximum reservation limit ({MAX_RESERVATIONS_PER_MEMBER})");

                // Check if book exists and is not retired
                Book book = _bookDB.GetBookByID(bookId);
                if (book == null || book.IsRetired)
                    throw new Exception("Book is not available for reservation");

                // Get next queue position
                int queuePosition = _reservationDB.GetNextQueuePosition(bookId);

                // Create reservation
                Reservation reservation = new Reservation
                {
                    BookID = bookId,
                    MemberID = memberId,
                    ReservationDate = DateTime.Now,
                    Status = "Pending",
                    QueuePosition = queuePosition
                };

                return _reservationDB.InsertReservation(reservation);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to reserve book: {ex.Message}", ex);
            }
        }

        public LoansList GetMemberLoanHistory(int memberId)
        {
            return _loanDB.SelectLoansByMember(memberId);
        }

        public LoansList GetMemberActiveLoans(int memberId)
        {
            return _loanDB.SelectActiveLoansByMember(memberId);
        }

        public ReservationsList GetMemberReservations(int memberId)
        {
            return _reservationDB.SelectReservationsByMember(memberId);
        }

        public bool RenewLoan(int loanId)
        {
            try
            {
                Loan loan = _loanDB.GetLoanByID(loanId);
                if (loan == null || loan.ReturnDate != null)
                    throw new Exception("Loan cannot be renewed");

                if (loan.RenewalCount >= MAX_RENEWAL_COUNT)
                    throw new Exception($"Maximum renewal limit ({MAX_RENEWAL_COUNT}) reached");

                // Check if there are pending reservations for this book
                BookCopy copy = _bookCopyDB.GetCopyByID(loan.CopyID);
                var pendingReservations = _reservationDB.SelectPendingReservationsByBook(copy.BookID);
                if (pendingReservations.Count > 0)
                    throw new Exception("Cannot renew - book has pending reservations");

                DateTime newDueDate = loan.DueDate.AddDays(DEFAULT_LOAN_PERIOD_DAYS);
                return _loanDB.RenewLoan(loanId, newDueDate);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to renew loan: {ex.Message}", ex);
            }
        }

        // ========== LIBRARIAN OPERATIONS ==========

        public bool AddNewBook(Book book, int copyCount)
        {
            try
            {
                book.TotalCopies = copyCount;
                book.AvailableCopies = copyCount;
                
                bool bookAdded = _bookDB.InsertBook(book);
                
                if (bookAdded && copyCount > 0)
                {
                    // Add copies
                    for (int i = 0; i < copyCount; i++)
                    {
                        BookCopy copy = new BookCopy
                        {
                            BookID = book.BookID,
                            Barcode = $"{book.ISBN}-{i + 1:D3}",
                            Status = "Available",
                            Condition = "New",
                            AcquisitionDate = DateTime.Now
                        };
                        _bookCopyDB.InsertBookCopy(copy);
                    }
                }
                
                return bookAdded;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add new book: {ex.Message}", ex);
            }
        }

        public bool UpdateBookDetails(Book book)
        {
            return _bookDB.UpdateBook(book);
        }

        public bool RetireBook(int bookId)
        {
            return _bookDB.RetireBook(bookId);
        }

        public bool ReturnBook(int loanId)
        {
            try
            {
                Loan loan = _loanDB.GetLoanByID(loanId);
                if (loan == null || loan.ReturnDate != null)
                    throw new Exception("Invalid loan or already returned");

                DateTime returnDate = DateTime.Now;
                
                // Calculate fine if overdue
                decimal fine = 0;
                if (returnDate > loan.DueDate)
                {
                    int daysOverdue = (returnDate - loan.DueDate).Days;
                    fine = daysOverdue * FINE_PER_DAY;
                }

                // Update loan
                _loanDB.UpdateReturnDate(loanId, returnDate);
                
                if (fine > 0)
                {
                    _loanDB.UpdateFine(loanId, fine, false);
                    
                    // Update member's total fines
                    Member member = _memberDB.GetMemberByID(loan.MemberID);
                    if (member != null)
                    {
                        _memberDB.UpdateMemberFines(member.MemberID, member.FinesOwed + fine);
                    }
                }

                // Update copy status
                BookCopy copy = _bookCopyDB.GetCopyByID(loan.CopyID);
                if (copy != null)
                {
                    _bookCopyDB.UpdateCopyStatus(copy.CopyID, "Available");
                    _bookDB.UpdateAvailableCopies(copy.BookID, 1);
                }

                // Update member's current books count
                _memberDB.UpdateCurrentBooksCount(loan.MemberID, -1);

                // Check if there are pending reservations for this book
                if (copy != null)
                {
                    var pendingReservations = _reservationDB.SelectPendingReservationsByBook(copy.BookID);
                    if (pendingReservations.Count > 0)
                    {
                        // Assign to first in queue
                        var firstReservation = pendingReservations.OrderBy(r => r.QueuePosition).First();
                        ApproveReservation(firstReservation.ReservationID, copy.CopyID);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to return book: {ex.Message}", ex);
            }
        }

        public bool ApproveReservation(int reservationId, int copyId)
        {
            try
            {
                // Mark reservation as ready
                bool approved = _reservationDB.MarkReservationReady(reservationId, copyId);
                
                if (approved)
                {
                    // Update copy status to reserved
                    _bookCopyDB.UpdateCopyStatus(copyId, "Reserved");
                }
                
                return approved;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to approve reservation: {ex.Message}", ex);
            }
        }

        public bool CancelReservation(int reservationId)
        {
            return _reservationDB.CancelReservation(reservationId);
        }

        public bool PayFine(int memberId, decimal amount)
        {
            try
            {
                Member member = _memberDB.GetMemberByID(memberId);
                if (member == null)
                    throw new Exception("Member not found");

                decimal newFineAmount = Math.Max(0, member.FinesOwed - amount);
                return _memberDB.UpdateMemberFines(memberId, newFineAmount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to pay fine: {ex.Message}", ex);
            }
        }

        // ========== ADMIN OPERATIONS ==========

        public bool RegisterNewLibrarian(Librarian librarian, string password)
        {
            try
            {
                // Create user account first
                User user = new User
                {
                    Email = librarian.Email,
                    PasswordHash = HashPassword(password),
                    FirstName = librarian.FirstName,
                    LastName = librarian.LastName,
                    Phone = librarian.Phone,
                    Role = "Librarian",
                    Address = librarian.Address,
                    City = librarian.City,
                    PostalCode = librarian.PostalCode
                };

                bool userCreated = _userDB.InsertUser(user);
                
                if (userCreated)
                {
                    // Create librarian record (would need LibrarianDB - simplified here)
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to register librarian: {ex.Message}", ex);
            }
        }

        public bool RegisterNewMember(Member member, string password)
        {
            try
            {
                // Create user account first
                User user = new User
                {
                    Email = member.Email,
                    PasswordHash = HashPassword(password),
                    FirstName = member.FirstName,
                    LastName = member.LastName,
                    Phone = member.Phone,
                    Role = "Member",
                    Address = member.Address,
                    City = member.City,
                    PostalCode = member.PostalCode
                };

                bool userCreated = _userDB.InsertUser(user);
                
                if (userCreated)
                {
                    User createdUser = _userDB.GetUserByEmail(user.Email);
                    if (createdUser != null)
                    {
                        member.CardNumber = $"M{createdUser.UserID:D6}";
                        return _memberDB.InsertMember(member, createdUser.UserID);
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to register member: {ex.Message}", ex);
            }
        }

        public bool EditMemberDetails(Member member)
        {
            try
            {
                // Update user info
                User user = new User
                {
                    UserID = member.UserID,
                    Email = member.Email,
                    FirstName = member.FirstName,
                    LastName = member.LastName,
                    Phone = member.Phone,
                    Address = member.Address,
                    City = member.City,
                    PostalCode = member.PostalCode
                };
                
                _userDB.UpdateUser(user);
                
                // Update member info
                return _memberDB.UpdateMember(member);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to edit member details: {ex.Message}", ex);
            }
        }

        public bool SuspendMember(int memberId)
        {
            try
            {
                Member member = _memberDB.GetMemberByID(memberId);
                if (member != null)
                {
                    member.Status = "Suspended";
                    return _memberDB.UpdateMember(member);
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to suspend member: {ex.Message}", ex);
            }
        }

        public bool ActivateMember(int memberId)
        {
            try
            {
                Member member = _memberDB.GetMemberByID(memberId);
                if (member != null)
                {
                    member.Status = "Active";
                    return _memberDB.UpdateMember(member);
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to activate member: {ex.Message}", ex);
            }
        }

        // ========== SEARCH OPERATIONS ==========

        public BooksList SearchBooks(string title, int authorId, int categoryId)
        {
            return _bookDB.SelectBooksByFilter(title, authorId, categoryId);
        }

        public BooksList GetAvailableBooks()
        {
            return _bookDB.SelectAvailableBooks();
        }

        public BooksList GetNewArrivals(int days)
        {
            try
            {
                DateTime cutoffDate = DateTime.Now.AddDays(-days);
                var allBooks = _bookDB.SelectAllBooks();
                return new BooksList(allBooks.FindAll(b => b.CreatedAt >= cutoffDate));
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get new arrivals: {ex.Message}", ex);
            }
        }

        // ========== CATALOG MANAGEMENT ==========

        public AuthorsList GetAllAuthors()
        {
            return _authorDB.GetAllAuthors();
        }

        public bool AddAuthor(Author author)
        {
            return _authorDB.InsertAuthor(author);
        }

        public bool UpdateAuthor(Author author)
        {
            return _authorDB.UpdateAuthor(author);
        }

        public PublishersList GetAllPublishers()
        {
            try
            {
                DataTable dt = _publisherDB.GetAllPublishers();
                PublishersList publishers = new PublishersList();
                
                foreach (DataRow row in dt.Rows)
                {
                    publishers.Add(new Publisher
                    {
                        PublisherID = 0, // Not used in actual DB
                        Name = row["name"]?.ToString() ?? "",
                        Country = row["country"] != DBNull.Value ? row["country"]?.ToString() : "",
                        Website = row["website"] != DBNull.Value ? row["website"]?.ToString() : "",
                        Address = "",
                        Phone = "",
                        Email = "",
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    });
                }
                
                return publishers;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get publishers: {ex.Message}", ex);
            }
        }

        public bool AddPublisher(Publisher publisher)
        {
            int? foundedYear = null;
            // If you want to add founded year to Publisher model, extract it here
            return _publisherDB.InsertPublisher(publisher.Name, publisher.Country ?? "", publisher.Website ?? "", foundedYear);
        }

        public CategoriesList GetAllCategories()
        {
            try
            {
                DataTable dt = _categoryDB.GetAllCategories();
                CategoriesList categories = new CategoriesList();
                
                foreach (DataRow row in dt.Rows)
                {
                    categories.Add(new Category
                    {
                        CategoryID = 0, // Not used in actual DB
                        Name = row["name"]?.ToString() ?? "",
                        Description = row["description"] != DBNull.Value ? row["description"]?.ToString() : "",
                        ParentCategoryID = null,
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    });
                }
                
                return categories;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get categories: {ex.Message}", ex);
            }
        }

        public bool AddCategory(Category category)
        {
            return _categoryDB.InsertCategory(category.Name, category.Description ?? "");
        }

        // ========== HELPER METHODS ==========

        /// <summary>
        /// Hashes a password using SHA256 and returns Base64 encoded string
        /// </summary>
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
