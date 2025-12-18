# Library Management System - Project Overview

## Project Information
- **Name**: LibraryManagementSystem
- **Type**: WPF Application (.NET)
- **Architecture**: Multi-Tier Monolithic
- **Database**: Microsoft Access (.accdb)

## Project Structure

```
LibraryManagementSystem/
??? Model/                          ? COMPLETED (Phase 1)
?   ??? Entities (12 files)
?   ?   ??? BaseEntity.cs
?   ?   ??? User.cs
?   ?   ??? Member.cs
?   ?   ??? Librarian.cs
?   ?   ??? Author.cs
?   ?   ??? Publisher.cs
?   ?   ??? Category.cs
?   ?   ??? Book.cs
?   ?   ??? BookCopy.cs
?   ?   ??? Loan.cs
?   ?   ??? Reservation.cs
?   ?   ??? LibrarySetting.cs
?   ?
?   ??? Collections (11 files)
?   ?   ??? UsersList.cs
?   ?   ??? MembersList.cs
?   ?   ??? LibrariansList.cs
?   ?   ??? AuthorsList.cs
?   ?   ??? PublishersList.cs
?   ?   ??? CategoriesList.cs
?   ?   ??? BooksList.cs
?   ?   ??? BookCopiesList.cs
?   ?   ??? LoansList.cs
?   ?   ??? ReservationsList.cs
?   ?   ??? LibrarySettingsList.cs
?   ?
?   ??? README.md
?
??? ViewModel/                      ? TODO (Phase 2 - Data Access Layer)
?   ??? Database1.accdb
?   ??? BaseDB.cs
?   ??? UserDB.cs
?   ??? BookDB.cs
?   ??? LoanDB.cs
?   ??? ReservationDB.cs
?   ??? ReportsDB.cs
?
??? Service/                        ? TODO (Phase 3 - Business Logic Layer)
?   ??? ILibraryService.cs
?   ??? LibraryService.cs
?
??? View/                          ? TODO (Phase 4 - UI Layer)
    ??? Pages/
    ?   ??? LoginPage.xaml
    ?   ??? MemberDashboard.xaml
    ?   ??? SearchBooksPage.xaml
    ?   ??? LibrarianDashboard.xaml
    ?   ??? ...
    ?
    ??? Windows/
        ??? BookDetailsWindow.xaml
        ??? ...
```

## Phase 1: Model Layer - ? COMPLETED

### Entity Classes (12)
All entity classes inherit from `BaseEntity` and map to database tables:

1. **BaseEntity** - Abstract base with common properties (ID, CreatedAt, UpdatedAt, IsActive)
2. **User** - Base user class (Email, PasswordHash, Role, etc.)
3. **Member** - Library member (inherits User) - can borrow books
4. **Librarian** - Library staff (inherits User) - manages library
5. **Author** - Book authors
6. **Publisher** - Publishing companies
7. **Category** - Book categories/genres (hierarchical)
8. **Book** - Book information (title, ISBN, etc.)
9. **BookCopy** - Physical book copies (status, condition, location)
10. **Loan** - Borrowing transactions (with fine calculation)
11. **Reservation** - Book reservations (queue system)
12. **LibrarySetting** - System configuration

### Collection Classes (11)
Each entity has a corresponding collection class with helper methods:
- Search and filter capabilities
- LINQ-based queries
- Type-safe operations

### Key Features
- ? Inheritance hierarchy (User ? Member/Librarian)
- ? Computed properties (IsOverdue, DaysOverdue, FullName)
- ? Soft delete support (IsActive flag)
- ? Comprehensive XML documentation
- ? Rich domain model with business logic
- ? Collection helper methods for common queries

### Build Status
```
? WPF Project Created
? 23 Model files created
? Build successful (warnings only)
? All entities properly documented
```

## Phase 2: ViewModel Layer (Data Access) - TODO

### Database Connection
- BaseDB.cs - OleDbConnection management
- Connection string configuration

### DB Classes
Each entity will have a corresponding DB class with CRUD operations:
- UserDB.cs - Login, GetUserByEmail, etc.
- BookDB.cs - SelectAllBooks, SelectBooksByFilter, InsertBook, etc.
- LoanDB.cs - SelectActiveLoansByMember, InsertLoan, UpdateReturnDate
- ReservationDB.cs - Reservation management
- ReportsDB.cs - Complex queries and statistics

### Required Package
```bash
dotnet add package System.Data.OleDb
```

## Phase 3: Service Layer (Business Logic) - TODO

### Services
- ILibraryService.cs - Interface definition
- LibraryService.cs - Implementation

### Business Rules
- Authentication and authorization
- Loan limits (max 3 books per member)
- Fine calculation for overdue books
- Reservation queue management
- Member status validation
- Book availability checks

## Phase 4: View Layer (UI) - TODO

### Member Features
- Login
- Search and browse books
- Borrow books
- View loan history
- Reserve books
- View new arrivals

### Librarian Features
- Manage books (Add, Edit, Retire)
- Manage members
- Process loans and returns
- Approve reservations
- Generate reports
- Manage authors/publishers/categories

### Admin Features
- Manage librarians
- System settings
- Advanced reports

## Database Schema (Reference)

The entities map to these Access Database tables:
- Users
- Members
- Librarians
- Books
- BookCopies
- Authors
- Publishers
- Categories
- Loans
- Reservations
- LibrarySettings

## Technologies

- **Framework**: .NET 6/7/8
- **UI**: WPF (Windows Presentation Foundation)
- **Language**: C# 10+
- **Database**: Microsoft Access (.accdb)
- **Data Access**: ADO.NET (OleDb)
- **Patterns**: Repository Pattern, Service Layer Pattern

## Development Progress

| Phase | Component | Status | Files |
|-------|-----------|--------|-------|
| 1 | Model Layer | ? Complete | 23/23 |
| 2 | ViewModel/DAL | ? Pending | 0/7+ |
| 3 | Service/BLL | ? Pending | 0/2+ |
| 4 | View/UI | ? Pending | 0/15+ |

## Next Steps

1. Create ViewModel folder and BaseDB.cs
2. Add System.Data.OleDb package
3. Implement database connection
4. Create DB classes for each entity
5. Test CRUD operations
6. Move to Service layer

## Notes

- All model classes include proper XML documentation
- Nullable reference warnings are non-critical
- Architecture follows separation of concerns
- Ready for database integration

---
**Generated**: December 16, 2025
**Status**: Phase 1 Complete - Ready for Phase 2
