# New Arrivals - Column Name Fix ?

## Problem
Error loading new arrivals: **"No value given for one or more required parameters"**

This means the SQL query referenced column names that don't exist in your database.

## Root Cause
The query used generic column names that didn't match your actual database schema:

### Wrong Column Names (Before):
- ? `p.name` (publishers table)
- ? `c.name` (categories table)
- ? `bc.status` (book_copies table)
- ? `b.created_date` (books table)
- ? `b.is_active` (books table)

### Correct Column Names (After):
- ? `p.publisher_name` (publishers table)
- ? `c.category_name` (categories table)
- ? `bc.copy_status` (book_copies table)
- ? `b.date_added` (books table)
- ? Removed `is_active` filter (column doesn't exist)

## Fixed SQL Query

```sql
SELECT 
    b.book_id,
    b.title,
    b.isbn,
    b.publication_year,
    b.description,
    a.first_name & ' ' & a.last_name AS Author,
    p.publisher_name AS Publisher,              -- ? Fixed
    c.category_name AS Category,                 -- ? Fixed
    (SELECT COUNT(*) 
     FROM book_copies bc 
     WHERE bc.book_id = b.book_id 
     AND bc.copy_status = 'AVAILABLE') AS AvailableCopies,  -- ? Fixed
    (SELECT COUNT(*) 
     FROM book_copies bc 
     WHERE bc.book_id = b.book_id) AS TotalCopies,
    b.date_added AS AddedDate                    -- ? Fixed
FROM (((books b
LEFT JOIN book_authors ba ON b.book_id = ba.book_id)
LEFT JOIN authors a ON ba.author_id = a.author_id)
LEFT JOIN publishers p ON b.publisher_id = p.publisher_id)
LEFT JOIN categories c ON b.category_id = c.category_id
WHERE b.date_added >= DateAdd('d', -?, Date())   -- ? Fixed
ORDER BY b.date_added DESC, b.title              -- ? Fixed
```

## Changes Made

| Table | Column (Before) | Column (After) | Status |
|-------|----------------|----------------|--------|
| publishers | name | publisher_name | ? Fixed |
| categories | name | category_name | ? Fixed |
| book_copies | status | copy_status | ? Fixed |
| books | created_date | date_added | ? Fixed |
| books | is_active | (removed) | ? Removed |

## Your Database Schema

Based on the fixes, your database schema is:

### publishers table
```
publisher_id (PK)
publisher_name  ? Used in query
```

### categories table
```
category_id (PK)
category_name   ? Used in query
```

### book_copies table
```
copy_id (PK)
book_id (FK)
copy_status     ? Used in query (AVAILABLE, BORROWED, etc.)
```

### books table
```
book_id (PK)
title
isbn
publication_year
description
publisher_id (FK)
category_id (FK)
date_added      ? Used for filtering last 90 days
```

## Files Modified

1. **LibraryManagementSystem\ViewModel\BookDB.cs**
   - ? Updated `GetNewArrivalsWithDetails()` method
   - ? Fixed all column names to match database schema
   - ? Removed `is_active` filter

## Build Status
? **Build Successful** - Ready to test!

## To Apply the Fix

Since the app is running in debug mode:

1. **Stop the debugger** (Shift+F5)
2. **Restart the application** (F5)
3. **Navigate to New Arrivals** page
4. Books from last 90 days should load correctly!

---

**Issue**: Column names in SQL query didn't match database schema  
**Cause**: Generic names used instead of actual schema column names  
**Solution**: Updated all column references to match your database  
**Status**: ? Fixed - Restart app to apply

The New Arrivals page should now load books correctly from your database! ??
