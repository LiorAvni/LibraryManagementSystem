# ?? MEMBER SUSPENSION SYSTEM - IMPLEMENTATION PLAN

## ?? Summary

Implement a system to check member suspension status and restrict access to loan and reservation features while still allowing:
- ? Returning books
- ? Canceling reservations  
- ? Paying fines
- ? Viewing account status

---

## ?? Requirements

### When Member is SUSPENDED:
? **Cannot:**
- Borrow new books
- Reserve books

? **Can Still:**
- Return currently borrowed books
- Cancel active reservations
- Pay outstanding fines
- View account information
- Browse books (read-only)

---

## ?? Database Check

### Query to Get Member Status:
```sql
SELECT membership_status
FROM members
WHERE user_id = ?
```

**Possible Values:**
- `ACTIVE` - Member can use all features
- `SUSPENDED` - Member restricted from loans/reservations

---

## ?? Implementation Strategy

### 1. **Add Database Method** ? DONE
**File:** `MemberDB.cs`

```csharp
/// <summary>
/// Checks if a member is suspended by user ID
/// </summary>
public bool IsMemberSuspended(string userId)
{
    string query = "SELECT membership_status FROM members WHERE user_id = ?";
    object result = ExecuteScalar(query, new OleDbParameter("@UserID", userId));
    return result?.ToString().Equals("SUSPENDED", StringComparison.OrdinalIgnoreCase) ?? false;
}

/// <summary>
/// Gets member status by user ID
/// </summary>
public string GetMemberStatusByUserId(string userId)
{
    string query = "SELECT membership_status FROM members WHERE user_id = ?";
    object result = ExecuteScalar(query, new OleDbParameter("@UserID", userId));
    return result?.ToString();
}
```

---

### 2. **Update Member Dashboard**
**File:** `MemberDashboard.xaml.cs`

#### On Page Load:
```csharp
private void Page_Loaded(object sender, RoutedEventArgs e)
{
    CheckMemberStatus();
    LoadDashboardData();
}

private void CheckMemberStatus()
{
    var currentUser = MainWindow.CurrentUser;
    if (currentUser != null && !string.IsNullOrEmpty(currentUser.UserIdString))
    {
        string status = _memberDB.GetMemberStatusByUserId(currentUser.UserIdString);
        
        if (status == "SUSPENDED")
        {
            ShowSuspensionWarning();
            DisableLoanReservationFeatures();
        }
    }
}

private void ShowSuspensionWarning()
{
    // Show warning banner at top of dashboard
    suspensionWarning.Visibility = Visibility.Visible;
}

private void DisableLoanReservationFeatures()
{
    // Disable relevant buttons/features
    // (implementation depends on existing dashboard structure)
}
```

---

### 3. **Update Search/Browse Pages**
**Files:** `SearchBooksPage.xaml.cs`, `NewArrivalsPage.xaml.cs`

#### Disable Reserve Button:
```csharp
private void LoadBooks()
{
    // Load books...
    
    var currentUser = MainWindow.CurrentUser;
    bool isSuspended = _memberDB.IsMemberSuspended(currentUser.UserIdString);
    
    foreach (var book in books)
    {
        book.CanReserve = !isSuspended && book.IsAvailable;
    }
}
```

---

### 4. **Update Reserve Book Logic**
**File:** `ReservationDB.cs` - `ReserveBook()` method

#### Add Suspension Check:
```csharp
public bool ReserveBook(string bookId, string userId, int expiryDays = 7)
{
    // Get member_id from user_id
    string getMemberIdQuery = "SELECT member_id FROM members WHERE user_id = ?";
    object memberIdObj = ExecuteScalar(getMemberIdQuery, 
        new OleDbParameter("@UserID", userId));
    
    if (memberIdObj == null)
    {
        throw new Exception("Member not found.");
    }
    
    string memberId = memberIdObj.ToString();
    
    // ? NEW: Check if member is suspended
    string statusQuery = "SELECT membership_status FROM members WHERE member_id = ?";
    object statusObj = ExecuteScalar(statusQuery, 
        new OleDbParameter("@MemberID", memberId));
    
    if (statusObj != null && statusObj.ToString().Equals("SUSPENDED", StringComparison.OrdinalIgnoreCase))
    {
        throw new Exception("Your account is suspended. You cannot make new reservations at this time.");
    }
    
    // Continue with existing logic...
    // Check reservation limit
    // Check for duplicates
    // Create reservation
}
```

---

### 5. **Update Lend Book Logic** (Librarian Side)
**File:** `LendBookPage.xaml.cs`

#### Add Suspension Check:
```csharp
private void LendBook_Click(object sender, RoutedEventArgs e)
{
    // Get selected member
    // Get selected copy
    
    // ? NEW: Check if member is suspended
    string status = _memberDB.GetMemberStatusByUserId(member.UserId);
    
    if (status == "SUSPENDED")
    {
        MessageBox.Show(
            $"Cannot lend book to {member.FullName}.\\n\\n" +
            "This member's account is SUSPENDED.\\n" +
            "Please contact an administrator to reactivate the account.",
            "Member Suspended",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
        return;
    }
    
    // Continue with existing logic...
    // Check loan limit
    // Create loan
}
```

---

## ?? UI Updates

### 1. **Suspension Warning Banner** (Member Dashboard)

```xaml
<!-- Add to MemberDashboard.xaml -->
<Border x:Name="suspensionWarning"
        Background="#f8d7da"
        BorderBrush="#f5c6cb"
        BorderThickness="1"
        CornerRadius="5"
        Padding="15"
        Margin="0,0,0,20"
        Visibility="Collapsed">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="?? " FontSize="20" VerticalAlignment="Center"/>
        <StackPanel>
            <TextBlock Text="Account Suspended" 
                      FontWeight="Bold" 
                      Foreground="#721c24"
                      FontSize="16"
                      Margin="0,0,0,5"/>
            <TextBlock TextWrapping="Wrap" Foreground="#721c24">
                Your account is currently suspended. You cannot borrow or reserve books at this time.
                You can still return books, cancel reservations, and pay fines.
                Please contact the library for more information.
            </TextBlock>
        </StackPanel>
    </StackPanel>
</Border>
```

### Visual Result:
```
???????????????????????????????????????????????????????
?  ??  Account Suspended                              ?
?                                                     ?
?  Your account is currently suspended. You cannot   ?
?  borrow or reserve books at this time.             ?
?  You can still return books, cancel reservations,  ?
?  and pay fines.                                     ?
?  Please contact the library for more information.  ?
???????????????????????????????????????????????????????
```

---

### 2. **Disabled Buttons**

#### For Suspended Members:
```xaml
<Button Content="Reserve Book" 
        IsEnabled="{Binding IsNotSuspended}"
        ToolTip="{Binding ReserveButtonTooltip}"/>
```

**Code-behind:**
```csharp
public class BookViewModel
{
    public bool IsSuspended { get; set; }
    public bool IsNotSuspended => !IsSuspended;
    
    public string ReserveButtonTooltip => 
        IsSuspended ? "Your account is suspended. Cannot reserve books." : "Reserve this book";
}
```

---

## ?? User Experience Flow

### Scenario 1: Active Member
```
1. Member logs in
2. Status check: ACTIVE ?
3. Dashboard loads normally
4. All features enabled
5. Can reserve books
6. Can borrow books (via librarian)
```

### Scenario 2: Suspended Member
```
1. Member logs in
2. Status check: SUSPENDED ??
3. Dashboard shows warning banner
4. "Reserve" buttons disabled
5. "Borrow" blocked (librarian side)
6. Can still:
   - Return books
   - Cancel reservations
   - Pay fines
   - Browse catalog (read-only)
```

### Scenario 3: Member Tries to Reserve While Suspended
```
Member clicks "Reserve" (if somehow enabled)
  ?
ReservationDB.ReserveBook() called
  ?
Check membership_status
  ?
Status = SUSPENDED
  ?
Throw exception with clear message
  ?
Show error dialog:
"Your account is suspended. You cannot make new reservations."
  ?
No reservation created
```

### Scenario 4: Librarian Tries to Lend to Suspended Member
```
Librarian selects suspended member
  ?
Clicks "Lend Book"
  ?
Check member status
  ?
Status = SUSPENDED
  ?
Show warning dialog:
"Cannot lend book to John Doe.
This member's account is SUSPENDED."
  ?
No loan created
```

---

## ?? Testing Scenarios

### Test 1: Suspend a Member
```sql
-- Suspend a test member
UPDATE members 
SET membership_status = 'SUSPENDED'
WHERE member_id = 'mem-001'
```

**Then verify:**
1. Login as that member
2. Dashboard shows warning banner
3. Reserve buttons are disabled
4. Attempting to reserve shows error
5. Can still view books

---

### Test 2: Librarian Attempts to Lend
```sql
-- Ensure member is suspended
SELECT membership_status FROM members WHERE member_id = 'mem-001'
-- Should return: SUSPENDED
```

**Then verify:**
1. Login as librarian
2. Try to lend book to suspended member
3. Error message appears
4. No loan is created

---

### Test 3: Reactivate Member
```sql
-- Reactivate the member
UPDATE members 
SET membership_status = 'ACTIVE'
WHERE member_id = 'mem-001'
```

**Then verify:**
1. Login as that member
2. No warning banner
3. Reserve buttons enabled
4. Can reserve books
5. Librarian can lend books

---

### Test 4: Suspended Member Returns Book
```sql
-- Member is suspended but has active loan
SELECT * FROM loans 
WHERE member_id = 'mem-001' AND return_date IS NULL
```

**Then verify:**
1. Login as librarian
2. Process return for suspended member
3. **Should succeed** - returns are always allowed
4. Fine calculation works normally

---

### Test 5: Suspended Member Cancels Reservation
```sql
-- Member is suspended but has active reservation
SELECT * FROM reservations 
WHERE member_id = 'mem-001' AND reservation_status = 'PENDING'
```

**Then verify:**
1. Login as that member
2. View "My Reservations"
3. Click "Cancel" on reservation
4. **Should succeed** - cancellations are always allowed

---

## ? Implementation Checklist

### Phase 1: Database Layer ?
- [x] Add `IsMemberSuspended(userId)` to MemberDB
- [x] Add `GetMemberStatusByUserId(userId)` to MemberDB

### Phase 2: Member Dashboard
- [ ] Add suspension check on page load
- [ ] Add warning banner to XAML
- [ ] Disable reserve/borrow UI elements
- [ ] Show tooltips explaining suspension

### Phase 3: Book Pages
- [ ] Update SearchBooksPage to check suspension
- [ ] Update NewArrivalsPage to check suspension
- [ ] Disable reserve buttons for suspended members
- [ ] Show suspension message on button hover

### Phase 4: Reservation System
- [ ] Add suspension check to ReserveBook()
- [ ] Show clear error message
- [ ] Prevent reservation creation

### Phase 5: Librarian Lend System
- [ ] Add suspension check to LendBookPage
- [ ] Show warning to librarian
- [ ] Prevent loan creation

### Phase 6: Testing
- [ ] Test with suspended member
- [ ] Test with active member
- [ ] Test reactivation
- [ ] Test edge cases (returns, cancellations, fines)

---

## ?? Security Considerations

1. **Server-Side Validation** ?
   - All checks done in database layer
   - UI controls are convenience, not security

2. **Consistent Checks** ?
   - Same logic in ReservationDB and LendBookPage
   - Member status from single source (database)

3. **Clear Messaging** ?
   - Users understand why features are disabled
   - Librarians know member is suspended

4. **Audit Trail** (Future Enhancement)
   - Log when suspended member attempts restricted action
   - Track who suspended/reactivated accounts

---

## ?? Database Queries Summary

### Check Suspension Status:
```sql
SELECT membership_status
FROM members
WHERE user_id = ?
```

### Suspend a Member (Admin):
```sql
UPDATE members
SET membership_status = 'SUSPENDED'
WHERE member_id = ?
```

### Reactivate a Member (Admin):
```sql
UPDATE members
SET membership_status = 'ACTIVE'
WHERE member_id = ?
```

### Get All Suspended Members:
```sql
SELECT m.member_id, u.first_name, u.last_name, u.email
FROM members m
INNER JOIN users u ON m.user_id = u.user_id
WHERE m.membership_status = 'SUSPENDED'
```

---

## ?? Success Criteria

? Suspended members cannot reserve books  
? Suspended members cannot borrow books  
? Suspended members can return books  
? Suspended members can cancel reservations  
? Suspended members can pay fines  
? Librarians see clear warning when attempting to lend to suspended member  
? Member dashboard shows suspension status prominently  
? All checks done server-side (database layer)  
? Clear, user-friendly error messages  

---

**Ready to implement! This will ensure proper access control while maintaining essential member functions.** ??
