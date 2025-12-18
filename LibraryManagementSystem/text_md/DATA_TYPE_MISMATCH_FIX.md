# Return Book - Data Type Mismatch Fix ?

## Problem
Error when returning a book: **"אי-התאמה של סוג נתונים בביטוי קריטריונים"** (Hebrew for "Data type mismatch in criteria expression")

## Root Cause
The OleDbParameter types were not explicitly specified, causing Access database to misinterpret the data types:
- `return_date` expected Date type
- `fine_amount` expected Currency type
- `loan_id` expected VarChar(36) for TEXT GUID

## Solution

### Before (Caused Error):
```csharp
OleDbParameter[] parameters = {
    new OleDbParameter("@ReturnDate", returnDate),        // ? Auto-detected type
    new OleDbParameter("@FineAmount", fineAmount),        // ? Auto-detected type
    new OleDbParameter("@LoanID", loanId)                 // ? Auto-detected type
};
```

### After (Fixed):
```csharp
OleDbParameter[] parameters = {
    new OleDbParameter("@ReturnDate", OleDbType.Date) { Value = returnDate },
    new OleDbParameter("@FineAmount", OleDbType.Currency) { Value = fineAmount },
    new OleDbParameter("@LoanID", OleDbType.VarChar, 36) { Value = loanId }
};
```

## Explicit Type Mapping

| Parameter | Column Type | OleDbType | Size | Value |
|-----------|-------------|-----------|------|-------|
| @ReturnDate | Date/Time | Date | - | DateTime.Now |
| @FineAmount | Currency | Currency | - | decimal (e.g., 2.50) |
| @LoanID | TEXT(36) | VarChar | 36 | GUID string with braces |

## Why This Matters

### 1. **Date Parameter**
```csharp
OleDbType.Date  // Ensures Access recognizes it as Date/Time
```
Without explicit type, Access might treat it as a string and fail.

### 2. **Currency Parameter**
```csharp
OleDbType.Currency  // Access currency type (4 decimal places)
```
Ensures proper decimal handling in Access database.

### 3. **GUID Parameter** (Most Important)
```csharp
OleDbType.VarChar, 36  // TEXT(36) in Access
```
- Access stores GUIDs as TEXT(36): `{12345678-1234-1234-1234-123456789012}`
- Must specify VarChar with size 36
- Without this, Access might try to convert it to another type

## Updated SQL Query

```sql
UPDATE loans 
SET return_date = ?,      -- OleDbType.Date
    fine_amount = ?       -- OleDbType.Currency
WHERE loan_id = ?         -- OleDbType.VarChar(36)
```

## Complete Fixed Method

```csharp
public bool ReturnBook(string loanId, DateTime returnDate, decimal fineAmount = 0)
{
    try
    {
        string query = @"
            UPDATE loans 
            SET return_date = ?, 
                fine_amount = ?
            WHERE loan_id = ?";
        
        OleDbParameter[] parameters = {
            new OleDbParameter("@ReturnDate", OleDbType.Date) { Value = returnDate },
            new OleDbParameter("@FineAmount", OleDbType.Currency) { Value = fineAmount },
            new OleDbParameter("@LoanID", OleDbType.VarChar, 36) { Value = loanId }
        };

        return ExecuteNonQuery(query, parameters) > 0;
    }
    catch (Exception ex)
    {
        throw new Exception($"Failed to return book: {ex.Message}", ex);
    }
}
```

## Example Usage

```csharp
// From MemberDashboard.xaml.cs
bool success = _loanDB.ReturnBook(
    loan.LoanIdString,     // "{ABC-123-...}" as VarChar(36)
    DateTime.Now,          // Current date as Date
    loan.Fine              // 2.50 as Currency
);
```

## Database Update Example

### Before Update:
```
loan_id: {ABC-123-DEF-456}
return_date: NULL
fine_amount: NULL
```

### After Update:
```
loan_id: {ABC-123-DEF-456}
return_date: 2024-01-15 14:30:00  ?
fine_amount: $2.50                ?
```

## Common Access OleDbTypes

| Access Type | OleDbType | C# Type | Notes |
|-------------|-----------|---------|-------|
| Text | VarChar | string | Need to specify size |
| Memo | LongVarChar | string | Large text |
| Number (Long) | Integer | int | 32-bit integer |
| Number (Double) | Double | double | Floating point |
| Currency | Currency | decimal | 4 decimal places |
| Date/Time | Date | DateTime | Date and time |
| Yes/No | Boolean | bool | True/False |
| AutoNumber | Integer | int | Auto-increment |

## Why Hebrew Error Message?

The error message appeared in Hebrew because:
1. Your Windows or Access installation is configured for Hebrew locale
2. Access returns error messages in the system language
3. The actual error is: **"Data type mismatch in criteria expression"**

This happens when the WHERE clause uses a parameter that doesn't match the column type.

## Testing Checklist

- [x] Build successful
- [ ] Stop debugger and restart app
- [ ] Login as member
- [ ] Navigate to dashboard
- [ ] Click "Return Book" on a loan
- [ ] Verify book is returned in database
- [ ] Verify book removed from Current Loans
- [ ] Verify book appears in Loan History as RETURNED
- [ ] Verify fine amount is saved correctly

## Files Modified

1. **LibraryManagementSystem\ViewModel\LoanDB.cs**
   - ? Updated `ReturnBook()` method
   - ? Added explicit OleDbType for all parameters
   - ? Specified VarChar(36) for GUID parameter

## Build Status
? **Build Successful** - Ready to test!

---

**Issue**: Data type mismatch error when returning book  
**Cause**: OleDbParameters without explicit type specification  
**Solution**: Added explicit OleDbType for Date, Currency, and VarChar(36)  
**Status**: ? Fixed - Restart app to apply

---

## How to Apply the Fix

Since the app is running in debug mode:

1. **Stop the debugger** (Shift+F5 or Stop button)
2. **Restart the application** (F5 or Start button)
3. **Test the Return Book functionality**

The Return Book button should now work without errors! ??
