# ?? Check User Roles in Database

## Issue: "Invalid user role" error

The login is succeeding (you see the user's name), but navigation fails because the role value doesn't match expectations.

---

## ? What I Fixed

### 1. **Case-Insensitive Role Comparison**
The code now compares roles case-insensitively, so `"member"`, `"Member"`, or `"MEMBER"` will all work.

### 2. **Better Error Messages**
The error message now shows the actual role value, so you can see what's in the database.

### 3. **Null/Empty Role Handling**
The code now handles cases where the role is NULL or empty.

---

## ?? To Diagnose the Issue

### Step 1: Run the app again
1. Stop the current debugging session (if running)
2. Press **F5** to restart with the new code
3. Try logging in again
4. The error message will now show: `"Invalid user role: '[actual_role_value]'"`

This will tell you exactly what role is stored in the database.

---

## ??? Check Your Database Directly

Open your database and run this query in Microsoft Access:

```sql
SELECT 
    u.user_id,
    u.email,
    u.first_name,
    u.last_name,
    ur.role,
    u.is_active
FROM users u
LEFT JOIN user_roles ur ON u.user_id = ur.user_id
WHERE u.is_active = True
ORDER BY u.user_id;
```

**Expected Results:**

| user_id | email | first_name | last_name | role | is_active |
|---------|-------|------------|-----------|------|-----------|
| user-001 | admin@library.com | Admin | User | Librarian | True |
| user-002 | librarian1@library.com | ... | ... | Librarian | True |
| user-003 | librarian2@library.com | ... | ... | Librarian | True |
| user-004 | john.doe@email.com | John | Doe | Member | True |
| user-005 | jane.smith@email.com | Jane | Smith | Member | True |
| user-006 | bob.wilson@email.com | Bob | Wilson | Member | True |
| user-007 | alice.brown@email.com | Alice | Brown | Member | True |

---

## ?? Expected Role Values

The code expects one of these role values (case-insensitive):
- `"Member"` ? Opens Member Dashboard
- `"Librarian"` ? Opens Librarian Dashboard  
- `"Admin"` ? Opens Librarian Dashboard

---

## ?? Common Issues

### Issue 1: Role is NULL
**Symptom:** Error shows "User role is not set"

**Cause:** The `user_roles` table doesn't have a record for this user

**Fix:** Run this SQL for each user:
```sql
-- For admin/librarian users
INSERT INTO user_roles (user_id, role)
VALUES ('user-001', 'Librarian');

-- For member users
INSERT INTO user_roles (user_id, role)
VALUES ('user-004', 'Member');
```

---

### Issue 2: Role has wrong spelling
**Symptom:** Error shows "Invalid user role: 'Membar'" or similar

**Cause:** Typo in the database

**Fix:** Update the role:
```sql
UPDATE user_roles 
SET role = 'Member' 
WHERE user_id = 'user-004';
```

---

### Issue 3: Role has extra spaces
**Symptom:** Error shows "Invalid user role: ' Member '" with spaces

**Fix:** The code now automatically trims spaces, but you can clean the database:
```sql
UPDATE user_roles 
SET role = TRIM(role);
```

---

## ?? Quick Fix for All Users

If you need to set up roles for all your users, run these queries:

```sql
-- Clear existing roles (optional)
DELETE FROM user_roles;

-- Add roles for all users
INSERT INTO user_roles (user_id, role) VALUES ('user-001', 'Librarian');
INSERT INTO user_roles (user_id, role) VALUES ('user-002', 'Librarian');
INSERT INTO user_roles (user_id, role) VALUES ('user-003', 'Librarian');
INSERT INTO user_roles (user_id, role) VALUES ('user-004', 'Member');
INSERT INTO user_roles (user_id, role) VALUES ('user-005', 'Member');
INSERT INTO user_roles (user_id, role) VALUES ('user-006', 'Member');
INSERT INTO user_roles (user_id, role) VALUES ('user-007', 'Member');
```

---

## ?? Files Modified

1. ? `View/Pages/LoginPage.xaml.cs` - Case-insensitive role comparison + better error messages
2. ? `ViewModel/UserDB.cs` - Better null handling for roles

---

## ?? Next Steps

1. **Stop the current app** (if running)
2. **Press F5** to rebuild and run
3. **Try logging in** with any user
4. **Read the error message** - it will show the actual role value
5. **Come back here** with that information, and we'll fix the database

---

## ?? Hot Reload Tip

Since you're debugging, you can try **Hot Reload**:
1. Keep the app running
2. Press **Ctrl+Shift+F5** or click the Hot Reload button
3. Try logging in again

If Hot Reload doesn't work, just restart the app with F5.

---

**Status:** ? Code Fixed - Waiting for database verification
