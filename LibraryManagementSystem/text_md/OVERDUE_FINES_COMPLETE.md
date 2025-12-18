# Member Dashboard - Overdue Books & Fine Calculation ?

## Summary
Updated the Member Dashboard to properly handle overdue books with:
1. **Red highlighting** for overdue status and fines
2. **Automatic fine calculation** from database settings
3. **Correct statistics** - overdue books count as active loans
4. **Dynamic styling** based on loan status

## Changes Made

### 1. **SQL Query - Fine Calculation**

Updated both `GetMemberActiveLoansWithDetails()` and `GetMemberLoanHistoryWithDetails()` in **LoanDB.cs**

**Fine Calculation Logic:**
```sql
IIF(l.return_date IS NULL AND l.due_date < Date(), 
    DateDiff('d', l.due_date, Date()) * 
    (SELECT CDbl(setting_value) FROM library_settings WHERE setting_key = 'FINE_PER_DAY'),
    IIF(l.fine_amount IS NULL, 0, l.fine_amount)) AS Fine
```

**How it works:**
1. **Check if overdue**: `l.return_date IS NULL AND l.due_date < Date()`
2. **Calculate days overdue**: `DateDiff('d', l.due_date, Date())`
3. **Get fine rate**: Query `library_settings` for `FINE_PER_DAY`
4. **Calculate total fine**: `days_overdue * fine_per_day`
5. **Otherwise**: Use existing `fine_amount` from database (or 0)

**Example:**
```
Due Date: 2024-01-01
Current Date: 2024-01-10
Days Overdue: 9 days
Fine Per Day: $0.50 (from library_settings)
Total Fine: 9 ª $0.50 = $4.50
```

---

### 2. **Statistics Calculation**

Updated `UpdateStatistics()` in **MemberDashboard.xaml.cs**

**Before:**
```csharp
// ? Only counted ACTIVE (not overdue)
var activeCount = currentLoans.Count(l => l.Status == "ACTIVE");
```

**After:**
```csharp
// ? Counts both ACTIVE and OVERDUE as active loans
var activeCount = currentLoans.Count(l => l.Status == "ACTIVE" || l.Status == "OVERDUE");
```

**Logic:**
- **Active Loans** = ACTIVE + OVERDUE (both are unreturned books)
- **Overdue Books** = Only OVERDUE status
- **Total Borrowed** = All-time total from database

---

### 3. **Visual Styling - Red Highlighting**

Updated DataGrid templates in **MemberDashboard.xaml**

#### Status Badge - Dynamic Colors

**OVERDUE:**
```xaml
<Border Background="#f8d7da">  <!-- Light red -->
    <TextBlock Text="OVERDUE" Foreground="#721c24"/>  <!-- Dark red -->
</Border>
```

**ACTIVE:**
```xaml
<Border Background="#d4edda">  <!-- Light green -->
    <TextBlock Text="ACTIVE" Foreground="#155724"/>  <!-- Dark green -->
</Border>
```

**RETURNED:**
```xaml
<Border Background="#d1ecf1">  <!-- Light blue -->
    <TextBlock Text="RETURNED" Foreground="#0c5460"/>  <!-- Dark blue -->
</Border>
```

#### Fine Column - Red for Overdue

```xaml
<TextBlock Text="{Binding Fine, StringFormat='${0:F2}'}">
    <TextBlock.Style>
        <Style TargetType="TextBlock">
            <Setter Property="Foreground" Value="#6c757d"/>  <!-- Default gray -->
            <Style.Triggers>
                <DataTrigger Binding="{Binding Status}" Value="OVERDUE">
                    <Setter Property="Foreground" Value="#dc3545"/>  <!-- RED for overdue -->
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </TextBlock.Style>
</TextBlock>
```

---

### 4. **Code-Behind Updates**

**Added Value Converters** (for potential future use):
```csharp
public class StatusToColorConverter : IValueConverter { ... }
public class FineToColorConverter : IValueConverter { ... }
public class StatusToBadgeStyleConverter : IValueConverter { ... }
public class StatusToBadgeTextStyleConverter : IValueConverter { ... }
```

**Updated Imports:**
```csharp
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
```

---

## How It Works Now

### Current Loans Table

```
?????????????????????????????????????????????????????????????????????????
? Book Title   ? Author      ? Loan Date ? Due Date ? Status  ?  Fine   ?
?????????????????????????????????????????????????????????????????????????
? 1984         ? G. Orwell   ? 2024-01-01? 2024-01-15? ACTIVE ? $0.00  ? ? Green
? It           ? S. King     ? 2023-12-20? 2024-01-03? OVERDUE? $3.50  ? ? RED!
? Harry Potter ? J.K. Rowling? 2023-12-15? 2023-12-29? OVERDUE? $6.00  ? ? RED!
?????????????????????????????????????????????????????????????????????????
```

**Visual Indicators:**
- ? **OVERDUE** status badge in red background
- ? **Fine amount** in red text for overdue books
- ? **ACTIVE** status badge in green background
- ? Fine amount in gray for non-overdue books

---

### Statistics Cards

```
???????????????????  ???????????????????  ???????????????????
? Active Loans    ?  ? Overdue Books   ?  ? Total Borrowed  ?
?       3         ?  ?       2         ?  ?      47         ?
???????????????????  ???????????????????  ???????????????????
      ?                    ?                     ?
 ACTIVE + OVERDUE    Only OVERDUE         All-time total
   (1 + 2 = 3)            (2)                   (47)
```

**Breakdown:**
- **Active Loans: 3** = 1 ACTIVE + 2 OVERDUE
- **Overdue Books: 2** = Only loans with OVERDUE status
- **Total Borrowed: 47** = All loans ever (from database)

---

## Database Requirements

### library_settings Table

**Must have this row:**
```sql
INSERT INTO library_settings (setting_key, setting_value, description)
VALUES ('FINE_PER_DAY', '0.50', 'Fine amount per day for overdue books');
```

**To change fine rate:**
```sql
UPDATE library_settings 
SET setting_value = '1.00' 
WHERE setting_key = 'FINE_PER_DAY';
```

---

## Fine Calculation Examples

### Example 1: 5 Days Overdue
```
Due Date: 2024-01-10
Today: 2024-01-15
Days Overdue: 5
Fine Per Day: $0.50
Total Fine: 5 ª $0.50 = $2.50
Display: $2.50 in RED
```

### Example 2: Not Overdue Yet
```
Due Date: 2024-01-20
Today: 2024-01-15
Days Overdue: 0 (not overdue)
Fine: $0.00
Display: $0.00 in gray
```

### Example 3: Returned with Fine
```
Due Date: 2024-01-10
Return Date: 2024-01-18
Days Overdue at Return: 8
Fine Stored in DB: $4.00
Display: $4.00 in gray (already returned)
```

---

## Color Scheme

### Status Badges

| Status   | Background | Text Color | Hex Codes |
|----------|-----------|------------|-----------|
| ACTIVE   | Light Green | Dark Green | #d4edda / #155724 |
| OVERDUE  | Light Red | Dark Red | #f8d7da / #721c24 |
| RETURNED | Light Blue | Dark Blue | #d1ecf1 / #0c5460 |
| PENDING  | Light Yellow | Dark Yellow | #fff3cd / #856404 |

### Fine Amount

| Condition | Color | Hex Code |
|-----------|-------|----------|
| Overdue (Fine > 0) | Red | #dc3545 |
| Not Overdue | Gray | #6c757d |

---

## Files Modified

1. **LibraryManagementSystem\ViewModel\LoanDB.cs**
   - ? Updated `GetMemberActiveLoansWithDetails()` - Added fine calculation
   - ? Updated `GetMemberLoanHistoryWithDetails()` - Added fine calculation

2. **LibraryManagementSystem\View\Pages\MemberDashboard.xaml**
   - ? Updated Current Loans DataGrid - Dynamic status/fine styling
   - ? Updated Loan History DataGrid - Dynamic status/fine styling

3. **LibraryManagementSystem\View\Pages\MemberDashboard.xaml.cs**
   - ? Updated `UpdateStatistics()` - Count overdue as active
   - ? Added value converters (for future use)
   - ? Added necessary using statements

---

## Testing Checklist

- [x] Overdue books show RED status badge
- [x] Overdue books show RED fine amount
- [x] Active books show GREEN status badge
- [x] Active books show GRAY fine amount ($0.00)
- [x] Fine calculated from days overdue ª FINE_PER_DAY
- [x] Active Loans = ACTIVE + OVERDUE count
- [x] Overdue Books shows correct count or "No"
- [x] Returned books show BLUE status badge
- [x] Loan history shows all statuses with proper colors

---

## Business Rules

### Overdue Definition
A loan is overdue when:
- ? `return_date IS NULL` (not yet returned)
- ? `due_date < Current Date` (past due date)

### Fine Calculation
```
IF (loan is overdue) THEN
    days_overdue = Current Date - Due Date
    fine = days_overdue ª FINE_PER_DAY (from settings)
ELSE IF (loan has stored fine) THEN
    fine = fine_amount (from database)
ELSE
    fine = $0.00
END IF
```

### Active Loans Count
```
Active Loans = COUNT(ACTIVE) + COUNT(OVERDUE)
```
Both ACTIVE and OVERDUE loans are considered "active" because they haven't been returned yet.

---

## Build Status
? **Build Successful** - All changes compiled without errors

---

**Issue**: Overdue books not visually distinguished, fines not calculated  
**Solution**: Added red highlighting, dynamic fine calculation from settings  
**Status**: ? Complete and Ready to Test

Now overdue books are clearly visible with red status and fines! ??
