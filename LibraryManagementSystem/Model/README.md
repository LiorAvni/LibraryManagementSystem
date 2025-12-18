# Library Management System - Model Layer

## Phase 1 - COMPLETED ?

This document describes the Model Layer (Data Transfer Objects) for the Library Management System.

## Project Structure

```
LibraryManagementSystem/
??? Model/
    ??? BaseEntity.cs                 (Abstract base class)
    ??? User.cs                       (User entity)
    ??? Member.cs                     (Inherits User)
    ??? Librarian.cs                  (Inherits User)
    ??? Author.cs                     (Author entity)
    ??? Publisher.cs                  (Publisher entity)
    ??? Category.cs                   (Category/Genre entity)
    ??? Book.cs                       (Book entity)
    ??? BookCopy.cs                   (Physical book copy entity)
    ??? Loan.cs                       (Loan transaction entity)
    ??? Reservation.cs                (Reservation entity)
    ??? LibrarySetting.cs            (System settings entity)
    ??? UsersList.cs                  (Collection class)
    ??? MembersList.cs                (Collection class)
    ??? LibrariansList.cs             (Collection class)
    ??? AuthorsList.cs                (Collection class)
    ??? PublishersList.cs             (Collection class)
    ??? CategoriesList.cs             (Collection class)
    ??? BooksList.cs                  (Collection class)
    ??? BookCopiesList.cs             (Collection class)
    ??? LoansList.cs                  (Collection class)
    ??? ReservationsList.cs           (Collection class)
    ??? LibrarySettingsList.cs        (Collection class)
```

## Entity Descriptions

### BaseEntity (Abstract)
Common properties for all entities:
- `ID` - Unique identifier
- `CreatedAt` - Creation timestamp
- `UpdatedAt` - Last update timestamp
- `IsActive` - Soft delete support

### User
Base user class with properties:
- `UserID` - Primary key
- `Email` - Unique email address
- `PasswordHash` - Encrypted password
- `FirstName`, `LastName` - Name fields
- `Phone` - Contact number
- `Role` - User role (Member/Librarian/Admin)
- `Address`, `City`, `PostalCode` - Location info
- `RegistrationDate` - When user registered
- `FullName` - Computed property

### Member (inherits User)
Represents library members who can borrow books:
- `MemberID` - Primary key
- `MembershipDate` - Join date
- `Status` - Active/Suspended/Expired
- `ExpiryDate` - Membership expiry
- `FinesOwed` - Outstanding fines
- `MaxBooksAllowed` - Borrowing limit (default: 3)
- `CurrentBooksCount` - Current borrowed books
- `CardNumber` - Member card number

### Librarian (inherits User)
Represents library staff:
- `LibrarianID` - Primary key
- `EmployeeID` - Employee identifier
- `HireDate` - Employment start date
- `IsAdmin` - Admin privileges flag
- `Department` - Work department
- `Salary` - Compensation

### Author
Book authors:
- `AuthorID` - Primary key
- `FirstName`, `LastName` - Name fields
- `Biography` - Author bio
- `Nationality` - Country
- `DateOfBirth` - Birth date
- `FullName` - Computed property

### Publisher
Publishing companies:
- `PublisherID` - Primary key
- `Name` - Publisher name
- `Address`, `Phone`, `Email` - Contact info
- `Website` - Publisher website
- `Country` - Location

### Category
Book categories/genres:
- `CategoryID` - Primary key
- `Name` - Category name
- `Description` - Category description
- `ParentCategoryID` - Hierarchical support

### Book
Book information:
- `BookID` - Primary key
- `Title` - Book title
- `ISBN` - Unique book identifier
- `PublisherID` - Foreign key to Publisher
- `CategoryID` - Foreign key to Category
- `AuthorID` - Foreign key to Author
- `PublicationYear` - Year published
- `Edition` - Book edition
- `Language` - Book language (default: English)
- `Pages` - Page count
- `Description` - Book summary
- `CoverImagePath` - Image location
- `TotalCopies` - Total physical copies
- `AvailableCopies` - Available for borrowing
- `IsRetired` - Retired from circulation

### BookCopy
Physical book instances:
- `CopyID` - Primary key
- `BookID` - Foreign key to Book
- `Barcode` - Unique copy identifier
- `Status` - Available/Borrowed/Reserved/Maintenance/Lost
- `Condition` - New/Good/Fair/Poor/Damaged
- `Location` - Physical location (e.g., "A-12-3")
- `AcquisitionDate` - Purchase date
- `PurchasePrice` - Acquisition cost
- `Notes` - Copy-specific notes
- `LastBorrowedDate` - Last checkout date

### Loan
Borrowing transactions:
- `LoanID` - Primary key
- `CopyID` - Foreign key to BookCopy
- `MemberID` - Foreign key to Member
- `LibrarianID` - Who processed the loan
- `LoanDate` - Checkout date
- `DueDate` - Return deadline (default: 14 days)
- `ReturnDate` - Actual return date
- `Status` - Active/Returned/Overdue/Lost
- `FineAmount` - Late fees
- `FinePaid` - Payment status
- `RenewalCount` - Number of renewals
- `Notes` - Transaction notes
- `IsOverdue` - Computed property
- `DaysOverdue` - Computed property

### Reservation
Book reservations:
- `ReservationID` - Primary key
- `BookID` - Foreign key to Book
- `MemberID` - Foreign key to Member
- `ReservationDate` - When reserved
- `Status` - Pending/Ready/Fulfilled/Cancelled/Expired
- `AvailableDate` - When book became available
- `ExpiryDate` - Reservation expiry
- `FulfilledDate` - When picked up
- `AssignedCopyID` - Assigned book copy
- `QueuePosition` - Position in queue
- `Notes` - Reservation notes

### LibrarySetting
System configuration:
- `SettingID` - Primary key
- `SettingKey` - Setting name
- `SettingValue` - Setting value
- `Description` - Setting description
- `DataType` - Value type
- `Category` - Setting category

Common settings defined as constants:
- `MAX_BOOKS_PER_MEMBER`
- `DEFAULT_LOAN_PERIOD_DAYS`
- `MAX_RENEWAL_COUNT`
- `FINE_PER_DAY`
- `RESERVATION_EXPIRY_DAYS`
- `LIBRARY_NAME`, `LIBRARY_EMAIL`, etc.

## Collection Classes

All collection classes inherit from `List<T>` and provide convenient search/filter methods:

- **UsersList**: FindByEmail, FindByRole, GetActiveUsers
- **MembersList**: FindByMemberID, FindByCardNumber, GetActiveMembers, GetMembersWithFines
- **LibrariansList**: FindByEmployeeID, GetAdmins, GetActiveLibrarians
- **AuthorsList**: FindByName, FindByNationality, GetActiveAuthors
- **PublishersList**: FindByName, FindByCountry, GetActivePublishers
- **CategoriesList**: FindByName, GetRootCategories, GetSubCategories
- **BooksList**: FindByISBN, FindByTitle, FindByCategory, FindByAuthor, GetAvailableBooks
- **BookCopiesList**: FindByBarcode, FindByBookID, GetAvailableCopies
- **LoansList**: FindByMemberID, GetActiveLoans, GetOverdueLoans, GetTotalFinesOwed
- **ReservationsList**: FindByMemberID, FindByBookID, GetPendingReservations
- **LibrarySettingsList**: FindByKey, GetValue, FindByCategory

## Database Mapping

These entities are designed to map directly to Access Database tables:
- Property names match database column names
- Primary keys use ID fields
- Foreign keys reference related entities
- Navigation properties can be added later in ViewModel layer

## Build Status

? Project created successfully
? All 23 Model files created
? Build successful (with nullable reference warnings only)
? Ready for Phase 2 (ViewModel/DAL layer)

## Next Steps

1. Create ViewModel folder
2. Add Access Database connection (BaseDB.cs)
3. Implement DB classes for each entity (UserDB, BookDB, LoanDB, etc.)
4. Add OleDb package for Access database connectivity

## Technologies Used

- C# 10+
- .NET 6/7/8 (WPF)
- Object-Oriented Programming
- Inheritance (User -> Member/Librarian)
- Collections and LINQ

## Notes

- All entities include proper XML documentation
- Collection classes provide LINQ-based filtering
- Computed properties (e.g., IsOverdue, FullName) for convenience
- Soft delete support via IsActive property
- Nullable reference types for optional fields
