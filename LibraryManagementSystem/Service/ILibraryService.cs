using System;
using System.Collections.Generic;
using LibraryManagementSystem.Model;

namespace LibraryManagementSystem.Service
{
    /// <summary>
    /// Interface for Library Service operations
    /// Defines all business logic operations for the library system
    /// </summary>
    public interface ILibraryService
    {
        // ========== AUTHENTICATION ==========
        /// <summary>
        /// Authenticates a user login
        /// </summary>
        User Login(string email, string password);

        /// <summary>
        /// Checks if a user is active
        /// </summary>
        bool IsUserActive(int userId);

        // ========== MEMBER OPERATIONS ==========
        /// <summary>
        /// Allows a member to borrow a book
        /// Business rules: Max 3 books, no outstanding fines over limit, book must be available
        /// </summary>
        bool LoanBook(int memberId, int copyId);

        /// <summary>
        /// Allows a member to reserve a book
        /// Business rules: Max 3 active reservations
        /// </summary>
        bool ReserveBook(int memberId, int bookId);

        /// <summary>
        /// Gets loan history for a member
        /// </summary>
        LoansList GetMemberLoanHistory(int memberId);

        /// <summary>
        /// Gets active loans for a member
        /// </summary>
        LoansList GetMemberActiveLoans(int memberId);

        /// <summary>
        /// Gets reservations for a member
        /// </summary>
        ReservationsList GetMemberReservations(int memberId);

        /// <summary>
        /// Renews a loan for additional time
        /// </summary>
        bool RenewLoan(int loanId);

        // ========== LIBRARIAN OPERATIONS ==========
        /// <summary>
        /// Adds a new book with specified number of copies
        /// </summary>
        bool AddNewBook(Book book, int copyCount);

        /// <summary>
        /// Updates book details
        /// </summary>
        bool UpdateBookDetails(Book book);

        /// <summary>
        /// Retires a book from circulation
        /// </summary>
        bool RetireBook(int bookId);

        /// <summary>
        /// Processes a book return
        /// Calculates fines if overdue
        /// </summary>
        bool ReturnBook(int loanId);

        /// <summary>
        /// Approves a reservation and assigns a copy
        /// </summary>
        bool ApproveReservation(int reservationId, int copyId);

        /// <summary>
        /// Cancels a reservation
        /// </summary>
        bool CancelReservation(int reservationId);

        /// <summary>
        /// Processes fine payment
        /// </summary>
        bool PayFine(int memberId, decimal amount);

        // ========== ADMIN OPERATIONS ==========
        /// <summary>
        /// Registers a new librarian
        /// </summary>
        bool RegisterNewLibrarian(Librarian librarian, string password);

        /// <summary>
        /// Registers a new member
        /// </summary>
        bool RegisterNewMember(Member member, string password);

        /// <summary>
        /// Edits member details
        /// </summary>
        bool EditMemberDetails(Member member);

        /// <summary>
        /// Suspends a member
        /// </summary>
        bool SuspendMember(int memberId);

        /// <summary>
        /// Activates a suspended member
        /// </summary>
        bool ActivateMember(int memberId);

        // ========== SEARCH OPERATIONS ==========
        /// <summary>
        /// Searches for books by filter criteria
        /// </summary>
        BooksList SearchBooks(string title, int authorId, int categoryId);

        /// <summary>
        /// Gets all available books
        /// </summary>
        BooksList GetAvailableBooks();

        /// <summary>
        /// Gets new arrivals (books added in last N days)
        /// </summary>
        BooksList GetNewArrivals(int days);

        // ========== CATALOG MANAGEMENT ==========
        /// <summary>
        /// Manages authors
        /// </summary>
        AuthorsList GetAllAuthors();
        bool AddAuthor(Author author);
        bool UpdateAuthor(Author author);

        /// <summary>
        /// Manages publishers
        /// </summary>
        PublishersList GetAllPublishers();
        bool AddPublisher(Publisher publisher);

        /// <summary>
        /// Manages categories
        /// </summary>
        CategoriesList GetAllCategories();
        bool AddCategory(Category category);
    }
}
