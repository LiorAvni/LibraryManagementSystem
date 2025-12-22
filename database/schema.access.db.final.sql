CREATE TABLE users (
    user_id TEXT(36) PRIMARY KEY,
    email TEXT(100) NOT NULL,
    password_hash TEXT(255) NOT NULL,
    first_name TEXT(50),
    last_name TEXT(50),
    phone TEXT(20),
    address MEMO,
    created_date DATETIME,
    last_login DATETIME,
    is_active YESNO
);

CREATE UNIQUE INDEX idx_users_email ON users(email);


CREATE TABLE user_roles (
    user_id VARCHAR(36) NOT NULL,
    role VARCHAR(20) NOT NULL,
    PRIMARY KEY (user_id, role),
    CONSTRAINT fk_user_roles_user FOREIGN KEY (user_id) REFERENCES users(user_id)
);


CREATE TABLE authors (
    author_id VARCHAR(36) PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    biography MEMO,
    birth_date DATETIME,
    is_deleted YESNO DEFAULT 0
);

CREATE TABLE publishers (
    publisher_id VARCHAR(36) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    country VARCHAR(50),
    website VARCHAR(200),
    founded_year INTEGER,
    is_deleted YESNO DEFAULT 0
);

CREATE UNIQUE INDEX idx_publishers_name ON publishers(name);

CREATE TABLE categories (
    category_id VARCHAR(36) PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    description MEMO,
    is_deleted YESNO DEFAULT 0
);

CREATE UNIQUE INDEX idx_categories_name ON categories(name);

CREATE TABLE copy_statuses (
    status_code VARCHAR(20) PRIMARY KEY,
    status_name VARCHAR(50) NOT NULL,
    description MEMO
);

INSERT INTO copy_statuses (status_code, status_name, description) VALUES
('AVAILABLE', 'Available', 'Copy is available for borrowing'),
('BORROWED', 'Borrowed', 'Copy is currently borrowed'),
('DAMAGED', 'Damaged', 'Copy is damaged and needs repair'),
('LOST', 'Lost', 'Copy has been reported lost'),
('RETIRED', 'Retired', 'Copy has been retired from circulation'),
('RESERVED', 'Reserved', 'Copy is reserved for a member'),
('IN_REPAIR', 'In Repair', 'Copy is being repaired');

CREATE TABLE copy_conditions (
    condition_code VARCHAR(20) PRIMARY KEY,
    condition_name VARCHAR(50) NOT NULL,
    description MEMO
);

INSERT INTO copy_conditions (condition_code, condition_name, description) VALUES
('NEW', 'New', 'Brand new condition'),
('GOOD', 'Good', 'Good condition with minimal wear'),
('FAIR', 'Fair', 'Fair condition with some wear'),
('POOR', 'Poor', 'Poor condition, significant wear'),
('DAMAGED', 'Damaged', 'Damaged and needs attention');

CREATE TABLE books (
    book_id TEXT(36) PRIMARY KEY,
    title TEXT(200) NOT NULL,
    isbn TEXT(20),
    publisher_id TEXT(36),
    publication_year INTEGER,
    category_id TEXT(36),
    description MEMO,
    created_date DATETIME DEFAULT Now()
);


ALTER TABLE books
ADD CONSTRAINT fk_books_publisher
FOREIGN KEY (publisher_id) REFERENCES publishers(publisher_id);

ALTER TABLE books
ADD CONSTRAINT fk_books_category
FOREIGN KEY (category_id) REFERENCES categories(category_id);

CREATE TABLE book_copies (
    copy_id TEXT(36) PRIMARY KEY,
    book_id TEXT(36) NOT NULL,
    copy_number INTEGER NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'AVAILABLE',
    condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    acquisition_date DATETIME,
    location VARCHAR(100),
    CONSTRAINT fk_book_copies_book FOREIGN KEY (book_id) REFERENCES books(book_id),
    CONSTRAINT fk_book_copies_status FOREIGN KEY (status) REFERENCES copy_statuses(status_code),
    CONSTRAINT fk_book_copies_condition FOREIGN KEY (condition) REFERENCES copy_conditions(condition_code)
);

CREATE UNIQUE INDEX idx_book_copies_book_number ON book_copies(book_id, copy_number);
CREATE INDEX idx_book_copies_status ON book_copies(status);

CREATE TABLE book_authors (
    book_id VARCHAR(36) NOT NULL,
    author_id VARCHAR(36) NOT NULL,
    PRIMARY KEY (book_id, author_id),
    CONSTRAINT fk_book_authors_book FOREIGN KEY (book_id) REFERENCES books(book_id),
    CONSTRAINT fk_book_authors_author FOREIGN KEY (author_id) REFERENCES authors(author_id)
);

CREATE TABLE account_statuses (
    status_code VARCHAR(20) PRIMARY KEY,
    status_name VARCHAR(50) NOT NULL,
    description MEMO
);

INSERT INTO account_statuses (status_code, status_name, description) VALUES
('ACTIVE', 'Active', 'User is active'),
('SUSPENDED', 'Suspended', 'User is suspended'),
('DELETED', 'Deleted', 'User is deleted');

CREATE TABLE members (
    member_id VARCHAR(36) PRIMARY KEY,
    user_id VARCHAR(36) NOT NULL,
    membership_date DATETIME NOT NULL,
    membership_status VARCHAR(20) NOT NULL,
    CONSTRAINT fk_members_user FOREIGN KEY (user_id) REFERENCES users(user_id)
);

CREATE UNIQUE INDEX idx_members_user ON members(user_id);

CREATE TABLE librarians (
    librarian_id TEXT(36) PRIMARY KEY,
    user_id TEXT(36) NOT NULL,
    employee_id TEXT(20),
    hire_date DATETIME NOT NULL,
    librarian_status VARCHAR(20) NOT NULL
);

ALTER TABLE librarians
ADD CONSTRAINT fk_librarians_user
FOREIGN KEY (user_id) REFERENCES users(user_id);

ALTER TABLE members
ADD CONSTRAINT fk_members_account_status
FOREIGN KEY (membership_status) REFERENCES account_statuses(status_code);
ALTER TABLE members
ALTER COLUMN membership_status SET DEFAULT 'ACTIVE';

ALTER TABLE librarians
ADD CONSTRAINT fk_librarians_account_status
FOREIGN KEY (librarian_status) REFERENCES account_statuses(status_code);
ALTER TABLE librarians
ALTER COLUMN librarian_status SET DEFAULT 'ACTIVE';

CREATE UNIQUE INDEX idx_librarians_user ON librarians(user_id);

CREATE UNIQUE INDEX idx_librarians_employee ON librarians(employee_id);

CREATE TABLE loans (
    loan_id TEXT(36) PRIMARY KEY,
    copy_id TEXT(36) NOT NULL,
    member_id TEXT(36) NOT NULL,
    librarian_id TEXT(36),
    loan_date DATETIME NOT NULL,
    due_date DATETIME NOT NULL,
    return_date DATETIME,
    fine_amount DOUBLE,
    fine_payment_date DATETIME
);


ALTER TABLE loans
ADD CONSTRAINT fk_loans_book_copy
FOREIGN KEY (copy_id) REFERENCES book_copies(copy_id);

ALTER TABLE loans
ADD CONSTRAINT fk_loans_member
FOREIGN KEY (member_id) REFERENCES members(member_id);

ALTER TABLE loans
ADD CONSTRAINT fk_loans_librarian
FOREIGN KEY (librarian_id) REFERENCES librarians(librarian_id);

CREATE TABLE reservations (
    reservation_id VARCHAR(36) PRIMARY KEY,
    book_id VARCHAR(36) NOT NULL,
    book_copy_id VARCHAR(36),
    member_id VARCHAR(36) NOT NULL,
    reservation_date DATETIME NOT NULL,
    expiry_date DATETIME,
    reservation_status VARCHAR(20) NOT NULL,
    CONSTRAINT fk_reservations_book FOREIGN KEY (book_id) REFERENCES books(book_id),
    CONSTRAINT fk_reservations_copy FOREIGN KEY (book_copy_id) REFERENCES book_copies(copy_id),
    CONSTRAINT fk_reservations_member FOREIGN KEY (member_id) REFERENCES members(member_id)
);

-- Library Settings Table
-- This table stores configurable system settings for the library
CREATE TABLE library_settings (
    setting_key VARCHAR(50) PRIMARY KEY,
    setting_value VARCHAR(100) NOT NULL,
    description MEMO
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_books_isbn ON books(isbn);
CREATE INDEX idx_books_title ON books(title);
CREATE INDEX idx_books_publisher ON books(publisher_id);
CREATE INDEX idx_books_category ON books(category_id);
CREATE INDEX idx_loans_member ON loans(member_id);
CREATE INDEX idx_loans_copy ON loans(copy_id);
