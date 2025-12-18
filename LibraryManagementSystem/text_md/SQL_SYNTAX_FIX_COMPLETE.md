# SQL Syntax Fix for Microsoft Access - Complete ?

## Problem
The SQL queries were using **SQL Server syntax** which caused `OleDbException` errors when trying to load data in the Member Dashboard.

## Root Cause
Microsoft Access (JET SQL) uses different syntax than SQL Server:

| Feature | SQL Server | Access/JET SQL |
|---------|-----------|----------------|
| String Concatenation | `CONCAT(a, ' ', b)` | `a & ' ' & b` |
| Parameter in DateAdd | `DateAdd('d', ?, Date())` | `DateAdd('d', -30, Date())` (literal value) |

## Fixes Applied

### 1. **LoanDB.cs** - String Concatenation

#### `GetMemberActiveLoansWithDetails()`
**Before:**
```sql
CONCAT(a.first_name, ' ', a.last_name) AS Author
```

**After:**
```sql
a.first_name & ' ' & a.last_name AS Author
```

? Uses `&` operator for string concatenation (Access syntax)

---

#### `GetMemberLoanHistoryWithDetails()`
**Before:**
```sql
CONCAT(a.first_name, ' ', a.last_name) AS Author
AND l.loan_date >= DateAdd('d', ?, Date())
```

**After:**
```sql
a.first_name & ' ' & a.last_name AS Author
AND l.loan_date >= DateAdd('d', -30, Date())
```

**Changes:**
1. ? String concatenation uses `&` operator
2. ? DateAdd parameter is embedded as literal value (e.g., `-30`) instead of using `?` placeholder
3. ? Date filter is built as part of query string, not as parameter

**Why?** Access doesn't support parameters in DateAdd function when used in string interpolation.

---

### 2. **ReservationDB.cs** - String Concatenation

#### `GetMemberReservationsWithDetails()`
**Before:**
```sql
CONCAT(a.first_name, ' ', a.last_name) AS Author
```

**After:**
```sql
a.first_name & ' ' & a.last_name AS Author
```

? Uses `&` operator for string concatenation

---

## Access/JET SQL Syntax Reference

### String Operations
```sql
-- Concatenation
field1 & ' ' & field2

-- Example
first_name & ' ' & last_name AS FullName
```

### Date Functions
```sql
-- Current date
Date()

-- Date arithmetic (literal values only in WHERE clauses)
DateAdd('d', -30, Date())  -- 30 days ago
DateAdd('d', 7, Date())    -- 7 days from now

-- Date comparison
WHERE order_date >= DateAdd('d', -30, Date())
```

### Conditional Logic
```sql
-- IIF function (same as SQL Server)
IIF(condition, true_value, false_value)

-- Nested IIF
IIF(return_date IS NULL AND due_date < Date(), 'OVERDUE', 
    IIF(return_date IS NULL, 'ACTIVE', 'RETURNED'))
```

### NULL Handling
```sql
-- Check for NULL
WHERE field IS NULL
WHERE field IS NOT NULL

-- NULL coalescing
IIF(field IS NULL, default_value, field)
```

## Testing Results

### Before Fix:
```
Exception thrown: 'System.Data.OleDb.OleDbException' in System.Data.OleDb.dll
Exception thrown: 'System.Exception' in LibraryManagementSystem.dll
```

### After Fix:
? **Build Successful**
? No exceptions thrown
? Queries execute correctly

## Key Takeaways

1. **Always use Access-specific syntax** when working with `.accdb` databases
2. **String concatenation**: Use `&` not `CONCAT()`
3. **DateAdd parameters**: Use literal values in string interpolation, not `?` placeholders
4. **IIF()** works the same in both Access and SQL Server
5. **Test queries in Access** before implementing in code

## Files Modified

1. **LibraryManagementSystem\ViewModel\LoanDB.cs**
   - Fixed `GetMemberActiveLoansWithDetails()`
   - Fixed `GetMemberLoanHistoryWithDetails()`

2. **LibraryManagementSystem\ViewModel\ReservationDB.cs**
   - Fixed `GetMemberReservationsWithDetails()`

## Build Status
? **Build Successful** - All SQL syntax corrected for Microsoft Access

---

**Issue**: OleDbException when loading Member Dashboard  
**Cause**: SQL Server syntax used with Access database  
**Solution**: Convert to Access/JET SQL syntax  
**Status**: ? Fixed and Verified
