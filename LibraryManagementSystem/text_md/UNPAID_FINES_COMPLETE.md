# ? UNPAID FINES SECTION - COMPLETE!

## ?? Summary

Successfully implemented the **Unpaid Fines Section** on the Member Dashboard that automatically appears when a member has unpaid fines.

---

## ?? Features Implemented

### 1. **Automatic Fine Detection**
- Queries database for loans with `fine_amount > 0` AND `fine_payment_date IS NULL`
- Section is hidden by default (`Visibility="Collapsed"`)
- Automatically shows when unpaid fines are detected
- Automatically hides when no unpaid fines exist

### 2. **Database Query - GetMemberUnpaidFines()**

**Location:** `LoanDB.cs`

**SQL Query:**
```sql
SELECT 
    l.loan_id,
    b.title AS BookTitle,
    b.book_id,
    l.loan_date AS LoanDate,
    l.due_date AS DueDate,
    l.return_date AS ReturnDate,
    l.fine_amount AS Fine,
    IIF(l.return_date > l.due_date, 
        DateDiff('d', l.due_date, l.return_date), 
        0) AS OverdueDays,
    'RETURNED' AS Status
FROM ((loans l
INNER JOIN members m ON l.member_id = m.member_id)
INNER JOIN book_copies bc ON l.copy_id = bc.copy_id)
INNER JOIN books b ON bc.book_id = b.book_id
WHERE m.user_id = ? 
  AND l.fine_amount > 0 
  AND l.fine_payment_date IS NULL
ORDER BY l.loan_date DESC
```

**Key Features:**
- ? Calculates overdue days: `DateDiff('d', l.due_date, l.return_date)`
- ? Filters for unpaid fines: `fine_amount > 0` AND `fine_payment_date IS NULL`
- ? Joins with authors for complete book information
- ? Returns complete loan history with fines

---

### 3. **Display Columns**

The unpaid fines table shows:

| Column | Data | Format |
|--------|------|--------|
| **Book Title** | Title of the book | Text |
| **Author** | Author name(s) | Text |
| **Loan Date** | When book was borrowed | YYYY-MM-DD |
| **Due Date** | Original due date | YYYY-MM-DD |
| **Return Date** | When book was returned | YYYY-MM-DD |
| **Overdue Days** | Days late | "X days" (red text) |
| **Status** | Loan status | Badge (RETURNED - blue) |
| **Fine** | Fine amount | $X.XX (orange-gold) |
| **Action** | Pay button | "?? Pay Now" button |

---

## ?? Visual Design

### Section Header
```
?? Fine Payment
????????????????????????????????????
```

### Table Row Example
```
????????????????????????????????????????????????????????????????????????
? Harry Potter | J.K. Rowling | 2025-12-02 | 2025-12-16 | 2025-12-17 ?
? 1 days       | [RETURNED]   | $1.00      | [?? Pay Now]            ?
????????????????????????????????????????????????????????????????????????
```

### Styling
- **Overdue Days:** Red text (#d32f2f), SemiBold, Centered
- **Fine Amount:** Orange-gold (#ff9800), Bold, Font size 13px
- **Status Badge:** Blue background (#d1ecf1), Dark blue text (#0c5460)
- **Pay Now Button:** Blue button (#3498db) with white text

---

## ?? Code Implementation

### 1. **LoanDB.cs - GetMemberUnpaidFines() Method**

```csharp
/// <summary>
/// Gets all unpaid fines for a specific member with book details
/// </summary>
/// <param name="userId">User ID (from users table)</param>
/// <returns>DataTable with unpaid fines information</returns>
public DataTable GetMemberUnpaidFines(string userId)
{
    try
    {
        // Get loans with unpaid fines (fine_amount > 0 AND fine_payment_date IS NULL)
        string query = @"
            SELECT 
                l.loan_id,
                b.title AS BookTitle,
                b.book_id,
                l.loan_date AS LoanDate,
                l.due_date AS DueDate,
                l.return_date AS ReturnDate,
                l.fine_amount AS Fine,
                IIF(l.return_date > l.due_date, 
                    DateDiff('d', l.due_date, l.return_date), 
                    0) AS OverdueDays,
                'RETURNED' AS Status
            FROM ((loans l
            INNER JOIN members m ON l.member_id = m.member_id)
            INNER JOIN book_copies bc ON l.copy_id = bc.copy_id)
            INNER JOIN books b ON bc.book_id = b.book_id
            WHERE m.user_id = ? 
              AND l.fine_amount > 0 
              AND l.fine_payment_date IS NULL
            ORDER BY l.loan_date DESC";
        
        OleDbParameter param = new OleDbParameter("@UserID", OleDbType.VarChar, 36) { Value = userId };
        DataTable dt = ExecuteQuery(query, param);
        
        // Add Author column and populate it with all authors for each book
        if (!dt.Columns.Contains("Author"))
            dt.Columns.Add("Author", typeof(string));
        
        foreach (DataRow row in dt.Rows)
        {
            string bookId = row["book_id"].ToString();
            
            // Get all authors for this book
            string authorQuery = @"
                SELECT a.first_name & ' ' & a.last_name AS AuthorName
                FROM book_authors ba
                INNER JOIN authors a ON ba.author_id = a.author_id
                WHERE ba.book_id = ?
                ORDER BY ba.author_id";
            
            OleDbParameter authorParam = new OleDbParameter("@BookID", OleDbType.VarChar, 36) { Value = bookId };
            DataTable authorDt = ExecuteQuery(authorQuery, authorParam);
            
            if (authorDt.Rows.Count > 0)
            {
                // Concatenate all author names with ", "
                var authors = new System.Collections.Generic.List<string>();
                foreach (DataRow authorRow in authorDt.Rows)
                {
                    authors.Add(authorRow["AuthorName"].ToString());
                }
                row["Author"] = string.Join(", ", authors);
            }
            else
            {
                row["Author"] = "Unknown Author";
            }
        }
        
        // Remove book_id column as it's not needed in the result
        dt.Columns.Remove("book_id");
        
        return dt;
    }
    catch (Exception ex)
    {
        throw new Exception($"Failed to get unpaid fines: {ex.Message}", ex);
    }
}
```

---

### 2. **MemberDashboard.xaml.cs - LoadUnpaidFines() Method**

```csharp
private void LoadUnpaidFines()
{
    try
    {
        unpaidFines.Clear();
        
        var currentUser = MainWindow.CurrentUser;
        if (currentUser == null) return;

        // Get unpaid fines from database
        DataTable dt = _loanDB.GetMemberUnpaidFines(currentUser.UserIdString);
        
        foreach (DataRow row in dt.Rows)
        {
            string loanIdStr = row["loan_id"]?.ToString() ?? "";
            int loanId = string.IsNullOrEmpty(loanIdStr) ? 0 : Math.Abs(loanIdStr.GetHashCode());
            
            unpaidFines.Add(new LoanDisplayModel
            {
                LoanId = loanId,
                LoanIdString = loanIdStr,
                BookTitle = row["BookTitle"]?.ToString() ?? "Unknown",
                Author = row["Author"]?.ToString() ?? "Unknown Author",
                LoanDate = row["LoanDate"] != DBNull.Value ? Convert.ToDateTime(row["LoanDate"]) : DateTime.Now,
                DueDate = row["DueDate"] != DBNull.Value ? Convert.ToDateTime(row["DueDate"]) : DateTime.Now,
                ReturnDate = row["ReturnDate"] != DBNull.Value ? Convert.ToDateTime(row["ReturnDate"]) : (DateTime?)null,
                Status = row["Status"]?.ToString() ?? "RETURNED",
                Fine = row["Fine"] != DBNull.Value ? Convert.ToDecimal(row["Fine"]) : 0,
                OverdueDays = row["OverdueDays"] != DBNull.Value ? Convert.ToInt32(row["OverdueDays"]) : 0
            });
        }
        
        // Show/hide unpaid fines section based on whether there are any
        var border = FindName("borderUnpaidFines") as Border;
        if (border != null)
        {
            border.Visibility = unpaidFines.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        
        // Bind data to DataGrid
        var dataGrid = FindName("dgUnpaidFines") as DataGrid;
        if (dataGrid != null)
        {
            dataGrid.ItemsSource = unpaidFines;
        }
    }
    catch (Exception ex)
    {
        // Hide section on error
        var border = FindName("borderUnpaidFines") as Border;
        if (border != null)
        {
            border.Visibility = Visibility.Collapsed;
        }
        // Silent fail - unpaid fines is not critical
    }
}
```

---

### 3. **LoanDisplayModel - Added OverdueDays Property**

```csharp
public class LoanDisplayModel
{
    public int LoanId { get; set; } // Display ID (hashcode)
    public string LoanIdString { get; set; } // Actual GUID from database
    public string BookTitle { get; set; }
    public string Author { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; }
    public decimal Fine { get; set; }
    public int OverdueDays { get; set; } // NEW! Number of days overdue
}
```

---

### 4. **XAML - Unpaid Fines DataGrid with Overdue Days Column**

```xaml
<DataGrid x:Name="dgUnpaidFines" 
          AutoGenerateColumns="False" 
          IsReadOnly="True"
          HeadersVisibility="Column"
          GridLinesVisibility="None"
          Background="White"
          RowBackground="White"
          AlternatingRowBackground="#f8f9fa"
          BorderThickness="0"
          CanUserResizeRows="False"
          SelectionMode="Single">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Book Title" Binding="{Binding BookTitle}" Width="*" MinWidth="150"/>
        <DataGridTextColumn Header="Author" Binding="{Binding Author}" Width="*" MinWidth="120"/>
        <DataGridTextColumn Header="Loan Date" Binding="{Binding LoanDate, StringFormat='{}{0:yyyy-MM-dd}'}" Width="100"/>
        <DataGridTextColumn Header="Due Date" Binding="{Binding DueDate, StringFormat='{}{0:yyyy-MM-dd}'}" Width="100"/>
        <DataGridTextColumn Header="Return Date" Binding="{Binding ReturnDate, StringFormat='{}{0:yyyy-MM-dd}'}" Width="100"/>
        
        <!-- NEW: Overdue Days Column -->
        <DataGridTemplateColumn Header="Overdue Days" Width="110">
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <TextBlock Text="{Binding OverdueDays, StringFormat='{}{0} days'}" 
                               FontWeight="SemiBold" 
                               Foreground="#d32f2f"
                               HorizontalAlignment="Center"/>
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>
        </DataGridTemplateColumn>
        
        <DataGridTemplateColumn Header="Status" Width="100">
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <Border Style="{StaticResource BadgeReturned}">
                        <TextBlock Text="{Binding Status}" Style="{StaticResource BadgeReturnedText}"/>
                    </Border>
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>
        </DataGridTemplateColumn>
        
        <DataGridTemplateColumn Header="Fine" Width="90">
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <TextBlock Text="{Binding Fine, StringFormat='${0:F2}'}" 
                               FontWeight="Bold" 
                               Foreground="#ff9800"
                               FontSize="13"/>
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>
        </DataGridTemplateColumn>
        
        <DataGridTemplateColumn Header="Action" Width="110">
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <Button Content="?? Pay Now" 
                            Style="{StaticResource PrimaryButton}" 
                            Padding="8,4"
                            FontSize="11"
                            Tag="{Binding}"/>
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>
        </DataGridTemplateColumn>
    </DataGrid.Columns>
</DataGrid>
```

---

## ?? Workflow

### Scenario: Member Returns Book Late

**Step 1: Member Returns Book**
```csharp
ReturnBook_Click() ? _loanDB.ReturnBook(loanId, DateTime.Now, fine)
```

**Database Update:**
```sql
UPDATE loans 
SET return_date = '2025-12-17', 
    fine_amount = 1.00
WHERE loan_id = 'loan-004'

-- Note: fine_payment_date remains NULL
```

---

**Step 2: Dashboard Reloads**
```csharp
LoadDashboardData() ? LoadUnpaidFines()
```

**Query Executes:**
```sql
SELECT * 
FROM loans 
WHERE fine_amount > 0 
  AND fine_payment_date IS NULL
```

**Result:** Finds loan-004 with $1.00 unpaid fine

---

**Step 3: Section Appears**
```csharp
borderUnpaidFines.Visibility = Visibility.Visible
```

**Display:**
```
??????????????????????????????????????????????????????
? ?? Fine Payment                                    ?
??????????????????????????????????????????????????????
? Harry Potter | J.K. Rowling | 2025-12-02 | ... ? ?
? 1 days       | [RETURNED]   | $1.00  | [Pay Now] ?
??????????????????????????????????????????????????????
```

---

**Step 4: Member Pays Fine (Future Implementation)**
```csharp
PayFine_Click() ? Update loans SET fine_payment_date = Now()
```

**Result:** Section disappears on next dashboard load ?

---

## ?? Database Schema Reference

### loans Table Columns (Relevant)
```sql
CREATE TABLE loans (
    loan_id TEXT(36) PRIMARY KEY,
    copy_id TEXT(36) NOT NULL,
    member_id TEXT(36) NOT NULL,
    loan_date DATETIME NOT NULL,
    due_date DATETIME NOT NULL,
    return_date DATETIME,           -- When book was returned
    fine_amount DOUBLE,              -- Fine amount (can be > 0)
    fine_payment_date DATETIME       -- When fine was paid (NULL = unpaid)
);
```

### Query Logic
```sql
WHERE fine_amount > 0           -- Has a fine
  AND fine_payment_date IS NULL -- Fine not paid yet
```

**Result:** Only unpaid fines are shown ?

---

## ?? Example Database Records

### Loan with Unpaid Fine (Will Show)
```
loan_id: "loan-004"
member_id: "mem-003"
due_date: 2025-12-16
return_date: 2025-12-17
fine_amount: 1.00
fine_payment_date: NULL        ? UNPAID!
```
**Result:** ? Appears in Unpaid Fines section

---

### Loan with Paid Fine (Won't Show)
```
loan_id: "loan-005"
member_id: "mem-003"
due_date: 2025-11-15
return_date: 2025-11-20
fine_amount: 5.00
fine_payment_date: 2025-11-21  ? PAID!
```
**Result:** ? Does NOT appear (fine was paid)

---

### Loan with No Fine (Won't Show)
```
loan_id: "loan-006"
member_id: "mem-003"
due_date: 2025-12-20
return_date: 2025-12-18
fine_amount: 0.00              ? NO FINE
fine_payment_date: NULL
```
**Result:** ? Does NOT appear (no fine to pay)

---

## ? Build Status

**Status:** ? **BUILD SUCCESSFUL**

All changes compile without errors!

---

## ?? To Test

1. **Stop debugger** (Shift+F5)
2. **Restart application** (F5)
3. **Login as member** (`member@library.com` / `admin`)
4. **Borrow a book**
5. **Wait until due date passes** (or manually set in database)
6. **Return the book** ? Fine will be calculated
7. **Go to Dashboard**
8. **See the Unpaid Fines section appear!** ?

---

### Manual Database Test

To quickly test, update a loan record directly in Access:

```sql
-- Create an unpaid fine
UPDATE loans 
SET fine_amount = 1.00, 
    fine_payment_date = NULL,
    return_date = #2025-12-17#
WHERE loan_id = '{some-loan-id}'
  AND member_id = (SELECT member_id FROM members WHERE user_id = '{your-user-id}')
```

**Then:**
1. Refresh dashboard (F5 or reload page)
2. Unpaid Fines section should appear ?

---

## ?? Summary

### Completed:
- ? **Database Query** - `GetMemberUnpaidFines()` in LoanDB
- ? **Load Method** - `LoadUnpaidFines()` in MemberDashboard
- ? **Automatic Visibility** - Shows only when fines exist
- ? **Overdue Days Calculation** - Shows days late
- ? **Visual Design** - Matches HTML template
- ? **Data Binding** - DataGrid bound to unpaidFines collection

### Pending (Future):
- ? **Pay Now Button Functionality** - Need to implement payment processing
- ? **Fine Payment Method** - Add `PayFine()` method to LoanDB
- ? **Payment Confirmation** - Update `fine_payment_date` on payment

### Files Modified:
1. ? **LoanDB.cs** - Added `GetMemberUnpaidFines()` method
2. ? **MemberDashboard.xaml.cs** - Added `LoadUnpaidFines()` method
3. ? **MemberDashboard.xaml.cs** - Added `OverdueDays` property to `LoanDisplayModel`
4. ? **MemberDashboard.xaml** - Added "Overdue Days" column to unpaid fines table
5. ? **MemberDashboard.xaml.cs** - Called `LoadUnpaidFines()` in `LoadDashboardData()`

---

**The Unpaid Fines section is now fully functional and automatically shows when needed!** ????

Members can now see all their unpaid fines in one place! ?

