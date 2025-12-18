# Database Setup Script for LibraryManagement.accdb

## ?? SQL Statements to Run in Microsoft Access

Open your database `C:\_Lior_\School\project\þLibraryManagmentCS_5\database\LibraryManagement.accdb` and run these queries:

### 1. Insert Test User (Admin/Librarian)

```sql
-- Insert admin user
INSERT INTO users (user_id, email, password_hash, first_name, last_name, phone, address, created_date, last_login, is_active)
VALUES (
    '{11111111-1111-1111-1111-111111111111}',
    'admin@library.com',
    '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',
    'Admin',
    'User',
    '123-456-7890',
    '123 Library St, Book City',
    Now(),
    NULL,
    True
);

-- Assign Librarian role to admin user
INSERT INTO user_roles (user_id, role)
VALUES ('{11111111-1111-1111-1111-111111111111}', 'Librarian');

-- Create librarian record
INSERT INTO librarians (librarian_id, user_id, employee_id, hire_date, is_admin)
VALUES (
    '{AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA}',
    '{11111111-1111-1111-1111-111111111111}',
    'EMP001',
    Now(),
    True
);
```

**Login Credentials:**
- Email: `admin@library.com`
- Password: `admin`

---

### 2. Insert Test Member User

```sql
-- Insert member user
INSERT INTO users (user_id, email, password_hash, first_name, last_name, phone, address, created_date, last_login, is_active)
VALUES (
    '{22222222-2222-2222-2222-222222222222}',
    'member@library.com',
    '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',
    'John',
    'Doe',
    '987-654-3210',
    '456 Reader Ave, Book Town',
    Now(),
    NULL,
    True
);

-- Assign Member role
INSERT INTO user_roles (user_id, role)
VALUES ('{22222222-2222-2222-2222-222222222222}', 'Member');

-- Create member record
INSERT INTO members (member_id, user_id, membership_date, membership_status)
VALUES (
    '{BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB}',
    '{22222222-2222-2222-2222-222222222222}',
    Now(),
    'Active'
);
```

**Login Credentials:**
- Email: `member@library.com`
- Password: `admin`

---

### 3. Insert Sample Publishers

```sql
INSERT INTO publishers (publisher_id, name, country, website, founded_year)
VALUES 
    ('{P0000001-0000-0000-0000-000000000001}', 'Penguin Books', 'USA', 'www.penguin.com', 1935),
    ('{P0000002-0000-0000-0000-000000000002}', 'HarperCollins', 'USA', 'www.harpercollins.com', 1989),
    ('{P0000003-0000-0000-0000-000000000003}', 'Random House', 'USA', 'www.randomhouse.com', 1927);
```

---

### 4. Insert Sample Categories

```sql
INSERT INTO categories (category_id, name, description)
VALUES 
    ('{C0000001-0000-0000-0000-000000000001}', 'Fiction', 'Fictional literature and novels'),
    ('{C0000002-0000-0000-0000-000000000002}', 'Non-Fiction', 'Factual and educational books'),
    ('{C0000003-0000-0000-0000-000000000003}', 'Science', 'Scientific literature'),
    ('{C0000004-0000-0000-0000-000000000004}', 'Biography', 'Life stories and biographies'),
    ('{C0000005-0000-0000-0000-000000000005}', 'Technology', 'Technology and computing books');
```

---

### 5. Insert Sample Authors

```sql
INSERT INTO authors (author_id, first_name, last_name, biography, birth_date)
VALUES 
    ('{A0000001-0000-0000-0000-000000000001}', 'George', 'Orwell', 'English novelist and journalist', #1903-06-25#),
    ('{A0000002-0000-0000-0000-000000000002}', 'J.K.', 'Rowling', 'British author of Harry Potter series', #1965-07-31#),
    ('{A0000003-0000-0000-0000-000000000003}', 'Stephen', 'King', 'American author of horror and suspense', #1947-09-21#);
```

---

### 6. Insert Sample Books

```sql
INSERT INTO books (book_id, title, isbn, publisher_id, publication_year, category_id, description, created_date)
VALUES 
    ('{B0000001-0000-0000-0000-000000000001}', 
     '1984', 
     '978-0-452-28423-4', 
     '{P0000001-0000-0000-0000-000000000001}',
     1949,
     '{C0000001-0000-0000-0000-000000000001}',
     'Dystopian novel by George Orwell',
     Now()),
    
    ('{B0000002-0000-0000-0000-000000000002}', 
     'Harry Potter and the Philosopher''s Stone', 
     '978-0-7475-3269-9', 
     '{P0000002-0000-0000-0000-000000000002}',
     1997,
     '{C0000001-0000-0000-0000-000000000001}',
     'First book in the Harry Potter series',
     Now()),
    
    ('{B0000003-0000-0000-0000-000000000003}', 
     'The Shining', 
     '978-0-385-12167-5', 
     '{P0000003-0000-0000-0000-000000000003}',
     1977,
     '{C0000001-0000-0000-0000-000000000001}',
     'Horror novel by Stephen King',
     Now());
```

---

### 7. Link Books to Authors

```sql
INSERT INTO book_authors (book_id, author_id)
VALUES 
    ('{B0000001-0000-0000-0000-000000000001}', '{A0000001-0000-0000-0000-000000000001}'),
    ('{B0000002-0000-0000-0000-000000000002}', '{A0000002-0000-0000-0000-000000000002}'),
    ('{B0000003-0000-0000-0000-000000000003}', '{A0000003-0000-0000-0000-000000000003}');
```

---

### 8. Insert Book Copies

```sql
INSERT INTO book_copies (copy_id, book_id, copy_number, status, condition, acquisition_date, location)
VALUES 
    ('{BC000001-0000-0000-0000-000000000001}', '{B0000001-0000-0000-0000-000000000001}', 1, 'AVAILABLE', 'GOOD', Now(), 'A-1-1'),
    ('{BC000002-0000-0000-0000-000000000002}', '{B0000001-0000-0000-0000-000000000001}', 2, 'AVAILABLE', 'GOOD', Now(), 'A-1-2'),
    ('{BC000003-0000-0000-0000-000000000003}', '{B0000002-0000-0000-0000-000000000002}', 1, 'AVAILABLE', 'NEW', Now(), 'B-2-1'),
    ('{BC000004-0000-0000-0000-000000000004}', '{B0000002-0000-0000-0000-000000000002}', 2, 'AVAILABLE', 'GOOD', Now(), 'B-2-2'),
    ('{BC000005-0000-0000-0000-000000000005}', '{B0000003-0000-0000-0000-000000000003}', 1, 'AVAILABLE', 'FAIR', Now(), 'C-3-1');
```

---

### 9. Insert Library Settings

```sql
INSERT INTO library_settings (setting_key, setting_value, description)
VALUES 
    ('MAX_LOAN_DAYS', '14', 'Default loan period in days'),
    ('MAX_RENEWALS', '2', 'Maximum number of times a book can be renewed'),
    ('FINE_PER_DAY', '0.50', 'Fine amount per day for overdue books'),
    ('MAX_BOOKS_PER_MEMBER', '3', 'Maximum books a member can borrow at once'),
    ('LIBRARY_NAME', 'City Central Library', 'Name of the library'),
    ('LIBRARY_EMAIL', 'info@citylibrary.com', 'Library contact email');
```

---

## ? How to Run These Queries

1. **Open Microsoft Access**
2. **Open your database**: `C:\_Lior_\School\project\þLibraryManagmentCS_5\database\LibraryManagement.accdb`
3. **Create a new query**:
   - Click "Create" ? "Query Design"
   - Close the "Show Table" dialog
   - Click "SQL View" button
4. **Paste each SQL block above** (one at a time)
5. **Click Run** (red exclamation mark button)
6. **Repeat for all sections**

---

## ?? Test Login Credentials

After running the setup:

**Librarian Account:**
- Email: `admin@library.com`
- Password: `admin`

**Member Account:**
- Email: `member@library.com`
- Password: `admin`

---

## ?? Notes

- Password hash `8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918` = "admin" (SHA256)
- All GUIDs use curly braces `{}` format for Access
- Date format uses `Now()` function or `#YYYY-MM-DD#` format
- Status codes must match those in `copy_statuses` table

---

## ?? Alternative: Import from Excel

If SQL queries don't work, you can:
1. Create data in Excel with same structure
2. Use Access "External Data" ? "Excel" to import
3. Map columns correctly during import

---

**After setup, run your application and login with the credentials above!** ??
