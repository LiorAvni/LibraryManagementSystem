# ? LOAN & RESERVATION LIMITS - VALIDATION COMPLETE!

## ?? Summary

Successfully implemented validation for:
1. **MAX_LOANS_PER_MEMBER** - Maximum books a member can borrow at once
2. **MAX_RESERVATIONS_PER_MEMBER** - Maximum reservations a member can have

Both limits are read from the `library_settings` table in the database, making them configurable.

---

## ?? Database Setup

### Required Library Settings

Make sure these settings exist in your `library_settings` table:

```sql
-- Insert library settings if they don't exist
INSERT INTO library_settings (setting_key, setting_value, description)
VALUES 
    ('MAX_LOAN_DAYS', '14', 'Default loan period in days'),
    ('MAX_BOOKS_PER_MEMBER', '3', 'Maximum books a member can borrow at once'),
    ('MAX_RESERVATIONS_PER_MEMBER', '3', 'Maximum reservations a member can have'),
    ('FINE_PER_DAY', '0.50', 'Fine amount per day for overdue books'),
    ('LIBRARY_NAME', 'City Central Library', 'Name of the library'),
    ('LIBRARY_EMAIL', 'info@citylibrary.com', 'Library contact email');
```

---

## ?? Feature #1: Loan Limit Validation

### Implementation Location:
**File:** `LendBookPage.xaml.cs`

### How It Works:

When a librarian tries to lend a book to a member, the system:

1. **Reads MAX_BOOKS_PER_MEMBER from database:**
```csharp
string maxLoansStr = _loanDB.GetLibrarySetting("MAX_BOOKS_PER_MEMBER");
int maxLoans = 3; // Default fallback
if (!string.IsNullOrEmpty(maxLoansStr) && int.TryParse(maxLoansStr, out int max))
{
    maxLoans = max;
}
```

2. **Counts current active loans:**
```csharp
int currentLoans = _loanDB.GetActiveLoanCount(member.MemberId);
```

**SQL Query:**
```sql
SELECT COUNT(*) 
FROM loans 
WHERE member_id = ? 
AND return_date IS NULL
```

3. **Validates before lending:**
```csharp
if (currentLoans >= maxLoans)
{
    MessageBox.Show(
        $"This member has reached the maximum number of loans ({maxLoans}).{Environment.NewLine}" +
        "They must return a book before borrowing another.",
        "Loan Limit Reached",
        MessageBoxButton.OK,
        MessageBoxImage.Warning);
    return;
}
```

---

### Error Dialog:

```
????????????????????????????????????????????
? ?? Loan Limit Reached                    ?
????????????????????????????????????????????
? This member has reached the maximum     ?
? number of loans (3).                     ?
?                                          ?
? They must return a book before          ?
? borrowing another.                       ?
?                                          ?
?              [OK]                        ?
????????????????????????????????????????????
```

---

### Complete Lend Book Flow with Validation:

```
Step 1: Librarian clicks "?? Lend Book" for a member
  ?
Step 2: Get MAX_BOOKS_PER_MEMBER from library_settings
  Query: SELECT setting_value FROM library_settings WHERE setting_key = 'MAX_BOOKS_PER_MEMBER'
  Result: "3"
  ?
Step 3: Count member's active loans
  Query: SELECT COUNT(*) FROM loans WHERE member_id = ? AND return_date IS NULL
  Result: 3
  ?
Step 4: Validate: 3 >= 3 ?
  ? YES - Member has reached limit
  ?
Step 5: Show error message
  "This member has reached the maximum number of loans (3)."
  ?
Step 6: Stop (do not create loan)
```

**If member has fewer loans:**
```
Step 4: Validate: 2 >= 3 ?
  ? NO - Member can borrow more
  ?
Step 5: Continue with lending process
  - Get available copy
  - Create loan record
  - Update copy status to BORROWED
  - Show success message
```

---

## ?? Feature #2: Reservation Limit Validation

### Implementation Location:
**File:** `ReservationDB.cs` - `ReserveBook()` method

### How It Works:

When a member tries to reserve a book, the system:

1. **Reads MAX_RESERVATIONS_PER_MEMBER from database:**
```csharp
string maxReservationsQuery = "SELECT setting_value FROM library_settings WHERE setting_key = 'MAX_RESERVATIONS_PER_MEMBER'";
object maxReservationsObj = ExecuteScalar(maxReservationsQuery);
int maxReservations = 3; // Default fallback
if (maxReservationsObj != null && int.TryParse(maxReservationsObj.ToString(), out int max))
{
    maxReservations = max;
}
```

2. **Counts current active reservations:**
```csharp
int currentReservations = GetActiveReservationCount(memberId);
```

**SQL Query:**
```sql
SELECT COUNT(*) 
FROM reservations 
WHERE member_id = ? 
AND reservation_status IN ('PENDING', 'READY')
```

3. **Validates before reserving:**
```csharp
if (currentReservations >= maxReservations)
{
    throw new Exception($"You have reached the maximum number of reservations ({maxReservations}). " +
                       "Please cancel an existing reservation or wait for one to expire.");
}
```

---

### Error Message:

```
????????????????????????????????????????????
? ? Error                                  ?
????????????????????????????????????????????
? You have reached the maximum number of  ?
? reservations (3).                        ?
?                                          ?
? Please cancel an existing reservation   ?
? or wait for one to expire.               ?
?                                          ?
?              [OK]                        ?
????????????????????????????????????????????
```

---

### Complete Reserve Book Flow with Validation:

```
Step 1: Member clicks "Reserve" button on a book
  ?
Step 2: Get member_id from user_id
  Query: SELECT member_id FROM members WHERE user_id = ?
  Result: "mem-001"
  ?
Step 3: Get MAX_RESERVATIONS_PER_MEMBER from library_settings
  Query: SELECT setting_value FROM library_settings WHERE setting_key = 'MAX_RESERVATIONS_PER_MEMBER'
  Result: "3"
  ?
Step 4: Count member's active reservations
  Query: SELECT COUNT(*) FROM reservations WHERE member_id = ? AND reservation_status IN ('PENDING', 'READY')
  Result: 3
  ?
Step 5: Validate: 3 >= 3 ?
  ? YES - Member has reached limit
  ?
Step 6: Throw exception with error message
  "You have reached the maximum number of reservations (3)..."
  ?
Step 7: Stop (do not create reservation)
```

**If member has fewer reservations:**
```
Step 5: Validate: 2 >= 3 ?
  ? NO - Member can reserve more
  ?
Step 6: Check for duplicate reservation (same book)
  Query: SELECT COUNT(*) FROM reservations WHERE book_id = ? AND member_id = ? AND reservation_status IN ('PENDING', 'READY')
  Result: 0 (no duplicate)
  ?
Step 7: Create reservation
  - Generate reservation_id GUID
  - Set reservation_date = NOW()
  - Set expiry_date = NOW() + 7 days
  - Set reservation_status = 'PENDING'
  - Insert into reservations table
  ?
Step 8: Show success message
```

---

## ?? Database Methods

### LoanDB.cs - Loan-Related Methods:

#### 1. `GetLibrarySetting(string settingKey)`
```csharp
public string GetLibrarySetting(string settingKey)
{
    string query = "SELECT setting_value FROM library_settings WHERE setting_key = ?";
    OleDbParameter param = new OleDbParameter("@SettingKey", OleDbType.VarChar, 50) { Value = settingKey };
    object result = ExecuteScalar(query, param);
    return result?.ToString();
}
```

**Used for:** Getting MAX_BOOKS_PER_MEMBER, MAX_LOAN_DAYS

---

#### 2. `GetActiveLoanCount(string memberId)`
```csharp
public int GetActiveLoanCount(string memberId)
{
    string query = @"
        SELECT COUNT(*) 
        FROM loans 
        WHERE member_id = ? AND return_date IS NULL";
    
    OleDbParameter param = new OleDbParameter("@MemberID", OleDbType.VarChar, 36) { Value = memberId };
    object result = ExecuteScalar(query, param);
    return result != null ? Convert.ToInt32(result) : 0;
}
```

**Returns:** Number of books currently borrowed (not yet returned)

---

### ReservationDB.cs - Reservation-Related Methods:

#### 1. `GetActiveReservationCount(string memberId)`
```csharp
public int GetActiveReservationCount(string memberId)
{
    string query = @"
        SELECT COUNT(*) 
        FROM reservations 
        WHERE member_id = ? 
        AND reservation_status IN ('PENDING', 'READY')";
    
    OleDbParameter param = new OleDbParameter("@MemberID", OleDbType.VarChar, 36) { Value = memberId };
    object result = ExecuteScalar(query, param);
    return result != null ? Convert.ToInt32(result) : 0;
}
```

**Returns:** Number of active reservations (PENDING or READY status)

---

#### 2. `ReserveBook(string bookId, string userId, int expiryDays = 7)`
**Updated to include reservation limit check:**
```csharp
public bool ReserveBook(string bookId, string userId, int expiryDays = 7)
{
    // 1. Get member_id from user_id
    // 2. Get MAX_RESERVATIONS_PER_MEMBER from settings ? NEW
    // 3. Count current active reservations ? NEW
    // 4. Validate against limit ? NEW
    // 5. Check for duplicate reservation
    // 6. Create reservation if all checks pass
}
```

---

## ?? Default Values & Fallbacks

Both validations have fallback defaults in case the database settings are missing:

### Loan Limit:
```csharp
int maxLoans = 3; // Default
if (!string.IsNullOrEmpty(maxLoansStr) && int.TryParse(maxLoansStr, out int max))
{
    maxLoans = max;
}
```

### Reservation Limit:
```csharp
int maxReservations = 3; // Default
if (maxReservationsObj != null && int.TryParse(maxReservationsObj.ToString(), out int max))
{
    maxReservations = max;
}
```

**This ensures the system works even if library_settings are not configured.**

---

## ?? Testing Scenarios

### Test 1: Loan Limit Validation

**Setup:**
1. Set MAX_BOOKS_PER_MEMBER to 3 in library_settings
2. Create a member with 3 active loans

**Test:**
1. Login as librarian
2. Navigate to Manage Books
3. Click "Lend" on any available book
4. Select the member with 3 loans
5. Click "?? Lend Book"
6. **Expected:** Error message "Member has reached maximum loan limit (3)"
7. **Database:** No new loan created, no copy status changed

---

### Test 2: Loan Limit - Member Can Borrow

**Setup:**
1. Set MAX_BOOKS_PER_MEMBER to 3
2. Create a member with 2 active loans

**Test:**
1. Same steps as Test 1
2. **Expected:** Confirmation dialog appears
3. Click "Yes"
4. **Expected:** Success message with loan ID
5. **Database:** New loan created, copy status = BORROWED, member now has 3 loans

---

### Test 3: Reservation Limit Validation

**Setup:**
1. Set MAX_RESERVATIONS_PER_MEMBER to 3 in library_settings
2. Create a member with 3 active reservations (PENDING or READY)

**Test:**
1. Login as member
2. Navigate to book catalog
3. Click "Reserve" on any book
4. **Expected:** Error message "You have reached the maximum number of reservations (3)..."
5. **Database:** No new reservation created

---

### Test 4: Reservation Limit - Member Can Reserve

**Setup:**
1. Set MAX_RESERVATIONS_PER_MEMBER to 3
2. Create a member with 2 active reservations

**Test:**
1. Same steps as Test 3
2. **Expected:** Confirmation dialog appears
3. Click "Yes"
4. **Expected:** Success message
5. **Database:** New reservation created with status PENDING, member now has 3 reservations

---

### Test 5: Custom Limits

**Setup:**
```sql
-- Change limits
UPDATE library_settings SET setting_value = '5' WHERE setting_key = 'MAX_BOOKS_PER_MEMBER';
UPDATE library_settings SET setting_value = '2' WHERE setting_key = 'MAX_RESERVATIONS_PER_MEMBER';
```

**Test:**
1. Try to borrow when member has 4 loans
2. **Expected:** Still allowed (limit is now 5)
3. Try to reserve when member has 2 reservations
4. **Expected:** Blocked (limit is now 2)

---

## ?? Database Schema

### library_settings Table:
```sql
CREATE TABLE library_settings (
    setting_key VARCHAR(50) PRIMARY KEY,
    setting_value VARCHAR(100) NOT NULL,
    description MEMO
);
```

**Required Settings:**
| Setting Key | Default Value | Description |
|-------------|---------------|-------------|
| MAX_LOAN_DAYS | 14 | Default loan period in days |
| MAX_BOOKS_PER_MEMBER | 3 | Maximum books a member can borrow at once |
| MAX_RESERVATIONS_PER_MEMBER | 3 | Maximum reservations a member can have |
| FINE_PER_DAY | 0.50 | Fine amount per day for overdue books |

---

### loans Table:
```sql
CREATE TABLE loans (
    loan_id TEXT(36) PRIMARY KEY,
    copy_id TEXT(36) NOT NULL,
    member_id TEXT(36) NOT NULL,
    librarian_id TEXT(36),
    loan_date DATETIME NOT NULL,
    due_date DATETIME NOT NULL,
    return_date DATETIME,  -- NULL = active loan
    fine_amount DOUBLE,
    fine_payment_date DATETIME
);
```

**Active Loan Check:**
```sql
WHERE return_date IS NULL
```

---

### reservations Table:
```sql
CREATE TABLE reservations (
    reservation_id VARCHAR(36) PRIMARY KEY,
    book_id VARCHAR(36) NOT NULL,
    book_copy_id VARCHAR(36),
    member_id VARCHAR(36) NOT NULL,
    reservation_date DATETIME NOT NULL,
    expiry_date DATETIME,
    reservation_status VARCHAR(20) NOT NULL  -- PENDING, READY, FULFILLED, CANCELLED
);
```

**Active Reservation Check:**
```sql
WHERE reservation_status IN ('PENDING', 'READY')
```

---

## ? Summary

### Files Modified:
1. ? **ReservationDB.cs** - Added `GetActiveReservationCount()` method
2. ? **ReservationDB.cs** - Updated `ReserveBook()` to check MAX_RESERVATIONS_PER_MEMBER

### Files Already Implementing Validation:
3. ? **LendBookPage.xaml.cs** - Already checks MAX_BOOKS_PER_MEMBER
4. ? **LoanDB.cs** - Already has `GetLibrarySetting()` and `GetActiveLoanCount()`

---

## ?? Features Implemented:

### ? Loan Limit Validation:
- ? Reads MAX_BOOKS_PER_MEMBER from database
- ? Counts active loans (return_date IS NULL)
- ? Validates before creating loan
- ? Shows clear error message
- ? Prevents borrowing if limit reached
- ? Fallback default value (3)

### ? Reservation Limit Validation:
- ? Reads MAX_RESERVATIONS_PER_MEMBER from database
- ? Counts active reservations (PENDING or READY)
- ? Validates before creating reservation
- ? Shows clear error message
- ? Prevents reserving if limit reached
- ? Fallback default value (3)

### ? Configurable Limits:
- ? Both limits stored in database
- ? Can be changed via UPDATE query
- ? No code changes needed to adjust limits
- ? Centralized configuration

---

## ?? How Limits Work Together:

### Example: Member "John Doe"

**Current State:**
- Active Loans: 2 (out of 3 max)
- Active Reservations: 3 (out of 3 max)

**Scenario 1: Try to Borrow**
```
? Can borrow (2 < 3)
? Librarian lends book
? Active loans: 3
```

**Scenario 2: Try to Reserve**
```
? Cannot reserve (3 >= 3)
? Error message shown
? Active reservations: still 3
```

**Scenario 3: Return a Book**
```
? Active loans: 2
? Still can borrow more
```

**Scenario 4: Cancel a Reservation**
```
? Active reservations: 2
? Now can reserve more
```

---

## ? Build Status

**BUILD SUCCESSFUL!** ?

---

**Both loan and reservation limits are now fully validated using configurable database settings!** ????

**The system enforces proper limits while remaining flexible through database configuration!** ?

