# New Arrivals - Simplified Query Fix ?

## Problem
Persistent error: **"No value given for one or more required parameters"**

Despite fixing column names, the error continued because:
1. Unknown which column was causing the issue
2. Subqueries might not be working correctly
3. Date filter column name unknown
4. Description column might not exist

## Solution: Simplified Query

I've created a **minimal working query** that:
- ? Removes date filtering (we'll add it back once working)
- ? Removes complex subqueries for copy counts
- ? Uses TOP 50 to limit results
- ? Uses IIF() to handle NULL values safely
- ? Returns hardcoded 0 for copy counts (temporary)

## New Simplified Query

```sql
SELECT TOP 50
    b.book_id,
    b.title,
    b.isbn,
    b.publication_year,
    IIF(a.first_name IS NULL, 'Unknown Author', 
        a.first_name & ' ' & a.last_name) AS Author,
    IIF(p.publisher_name IS NULL, 'Unknown Publisher', 
        p.publisher_name) AS Publisher,
    IIF(c.category_name IS NULL, 'Uncategorized', 
        c.category_name) AS Category,
    0 AS AvailableCopies,     -- Hardcoded for now
    0 AS TotalCopies          -- Hardcoded for now
FROM (((books b
LEFT JOIN book_authors ba ON b.book_id = ba.book_id)
LEFT JOIN authors a ON ba.author_id = a.author_id)
LEFT JOIN publishers p ON b.publisher_id = p.publisher_id)
LEFT JOIN categories c ON b.category_id = c.category_id
ORDER BY b.book_id DESC       -- Newest by ID instead of date
```

## What Changed

### Removed (Temporarily):
1. ? `b.description` - Column might not exist
2. ? `b.date_added` filter - Column name unknown
3. ? Subqueries for AvailableCopies - May have syntax issues
4. ? Subqueries for TotalCopies - May have syntax issues
5. ? Parameter-based filtering - Simplified to no filter

### Added:
1. ? `TOP 50` - Limits results to 50 books
2. ? `IIF()` for NULL handling - Prevents display errors
3. ? Hardcoded copy counts (0) - Shows all books as unavailable for now
4. ? `ORDER BY b.book_id DESC` - Shows newest books by ID

## What You'll See

### Books Display:
```
???????????????????????????????????
? Harry Potter and the Sorcerer's?
? Stone                           ?
?                                 ?
? Author: J.K. Rowling           ?
? Publisher: Scholastic          ?
? Year: 1997                     ?
? Category: Fantasy              ?
? Available: 0 / 0 ?            ?  ? Temporary!
?                                 ?
? [Borrow - Disabled] [Reserve]  ?
???????????????????????????????????
```

**Note:** All books will show as unavailable (0/0) for now. This is temporary until we fix the copy count queries.

## Next Steps (After This Works)

Once this query loads successfully:

### Step 1: Add Copy Counts
```sql
-- Test this query separately in Access first:
SELECT COUNT(*) FROM book_copies WHERE book_id = '{some-guid}'
```

### Step 2: Find Date Column
Check your books table for one of these:
- `date_added`
- `created_date`  
- `add_date`
- `date_created`
- `created_at`

### Step 3: Re-add Date Filter
```sql
WHERE b.[column_name] >= DateAdd('d', -90, Date())
```

### Step 4: Add Description (If Exists)
```sql
b.description,
-- or
b.book_description,
-- or remove if doesn't exist
```

## Testing Plan

### Test 1: Does it load?
```
? Navigate to New Arrivals
? Page loads without error
? Shows top 50 books
? Books have titles, authors, publishers, categories
```

### Test 2: Are JOINs working?
```
? Author names appear (not NULL)
? Publisher names appear (not "Unknown Publisher")
? Category names appear (not "Uncategorized")
? If any show "Unknown", that book has missing data
```

### Test 3: Search/Filter works?
```
? Type book title in search
? Select category from dropdown
? Check "Available only" (will show nothing since all are 0/0)
? Clear filters
```

## Build Status
? **Build Successful**

## To Test:

1. **Stop debugger** (Shift+F5)
2. **Restart app** (F5)
3. **Navigate to New Arrivals**
4. **Check if books load** (even with 0/0 copies)

## Expected Outcome

### If Successful:
- Page loads
- Shows 50 books
- Books have proper titles, authors, publishers, categories
- All books show "Available: 0 / 0"
- Borrow button disabled (no copies available)
- Reserve button enabled

### If Still Fails:
We need to check:
1. Are table names correct? (books, authors, publishers, categories)
2. Are JOIN column names correct? (book_id, author_id, publisher_id, category_id)
3. Does book_authors junction table exist?

## Debugging If It Still Fails

If you still get an error, run this simple query directly in Access:

```sql
SELECT TOP 10 
    book_id, 
    title, 
    isbn 
FROM books
```

Then try:
```sql
SELECT TOP 10 
    b.book_id, 
    b.title, 
    a.first_name 
FROM books b 
LEFT JOIN book_authors ba ON b.book_id = ba.book_id
LEFT JOIN authors a ON ba.author_id = a.author_id
```

This will help identify which table/column is causing the issue.

---

**Approach**: Start simple, add complexity gradually  
**Current Status**: Minimal working query  
**Next**: Add features back one at a time  
**Goal**: Get SOMETHING to load, then improve  

Let me know if this works! ??
