# ?? LIBRARIAN DASHBOARD - STRUCTURE DOCUMENTATION

## ?? Overview

The Librarian Dashboard serves as the main navigation hub for library staff, providing quick access to all administrative functions of the Library Management System.

---

## ??? Dashboard Structure

### **Page Information**
- **File Path (XAML):** `LibraryManagementSystem\View\Pages\LibrarianDashboard.xaml`
- **File Path (Code-Behind):** `LibraryManagementSystem\View\Pages\LibrarianDashboard.xaml.cs`
- **Namespace:** `LibraryManagementSystem.View.Pages`
- **Page Title:** "Librarian Dashboard"
- **Layout Type:** Card-based Grid Layout (3 columns ª 3 rows)

---

## ?? Visual Layout

### **Regular Librarian:**
```
???????????????????????????????????????????????????????????????????????
?  Welcome, [Librarian Name]!                                         ?
?  Employee ID: [EMP###]                                              ?
???????????????????????????????????????????????????????????????????????

???????????????????????????????????????????????????????
?  ?? Search Books ?  ?? Manage Books ?  ?? Manage      ?
?                  ?                  ?     Members     ?
?  Search the      ?  Add, edit, or   ?                 ?
?  library         ?  remove books    ?  View and       ?
?  catalog         ?  from library    ?  manage members ?
?                  ?                  ?                 ?
?  [Search]        ?  [Go to Books]   ?  [Go to Members]?
???????????????????????????????????????????????????????

?????????????????????????????????????
?  ?? Manage Loans ?  ?? Reservations ?
?                  ?                  ?
?  Process book    ?  View and manage ?
?  loans and       ?  book            ?
?  returns         ?  reservations    ?
?                  ?                  ?
?  [Go to Loans]   ?[Go to Reservations]?
?????????????????????????????????????

???????????????????????????????????????????????????????
?  ?? Manage       ?  ??? Categories  ?  ????? Publishers ?
?     Authors      ?                  ?                 ?
?                  ?  Manage book     ?  Manage         ?
?  Add, edit, or   ?  categories      ?  publishers     ?
?  remove authors  ?                  ?                 ?
?                  ?                  ?                 ?
?  [Go to Authors] ?[Go to Categories]?[Go to Publishers]?
???????????????????????????????????????????????????????

?????????????????????????????????????
?  ?? Reports      ?  ?? Settings     ?
?                  ?                  ?
?  View library    ?  Configure       ?
?  statistics and  ?  library         ?
?  reports         ?  settings        ?
?                  ?                  ?
?  [View Reports]  ?  [Go to Settings]?
?????????????????????????????????????
```

### **Admin Librarian:**
```
???????????????????????????????????????????????????????????????????????
?  Welcome, [Librarian Name]!                                         ?
?  Employee ID: [EMP###]                                              ?
???????????????????????????????????????????????????????????????????????

???????????????????????????????????????????????????????
?  ?? Search Books ?  ?? Manage Books ?  ?? Manage Users?
?                  ?                  ?                 ?
?  Search the      ?  Add, edit, or   ?  View and       ?
?  library         ?  remove books    ?  manage members ?
?  catalog         ?  from library    ?  & librarians   ?
?                  ?                  ?  & admins       ?
?  [Search]        ?  [Go to Books]   ?  [Go to Users]  ?
???????????????????????????????????????????????????????

?????????????????????????????????????
?  ?? Manage Loans ?  ?? Reservations ?
?                  ?                  ?
?  Process book    ?  View and manage ?
?  loans and       ?  book            ?
?  returns         ?  reservations    ?
?                  ?                  ?
?  [Go to Loans]   ?[Go to Reservations]?
?????????????????????????????????????

???????????????????????????????????????????????????????
?  ?? Manage       ?  ??? Categories  ?  ????? Publishers ?
?     Authors      ?                  ?                 ?
?                  ?  Manage book     ?  Manage         ?
?  Add, edit, or   ?  categories      ?  publishers     ?
?  remove authors  ?                  ?                 ?
?                  ?                  ?                 ?
?  [Go to Authors] ?[Go to Categories]?[Go to Publishers]?
???????????????????????????????????????????????????????

?????????????????????????????????????
?  ?? Reports      ?  ?? Settings     ?
?                  ?                  ?
?  View library    ?  Configure       ?
?  statistics and  ?  library         ?
?  reports         ?  settings        ?
?                  ?                  ?
?  [View Reports]  ?  [Go to Settings]?
?????????????????????????????????????
```

---

## ?? Components Breakdown

### **1. Welcome Section**
```xaml
<Border Background="White" Padding="30" CornerRadius="10">
    <StackPanel>
        <TextBlock>Welcome, [Librarian Name]!</TextBlock>
        <TextBlock>Employee ID: [EMP###]</TextBlock>
    </StackPanel>
</Border>
```

**Data Binding:**
- `txtLibrarianName` ? Displays: `{FirstName} {LastName}` from `MainWindow.CurrentUser`
- `txtEmployeeId` ? Displays: `"EMP" + UserID` (formatted)

**Features:**
- ? Dynamic librarian name display
- ? Employee ID generation
- ? Styled welcome message

---

### **2. Menu Cards Grid**

#### **Layout Configuration:**
- **Grid:** 3 columns ª 4 rows
- **Total Cards:** 10 menu items (8 visible + 2 empty slots)
- **Card Styling:**
  - Background: White
  - Padding: 30px
  - Corner Radius: 10px
  - Shadow Effect: Drop shadow (opacity 0.1, depth 2px, blur 10px)
  - Cursor: Hand (pointer)

#### **Role-Based Display:**
- **Regular Librarian:** "Manage Members" card shows member management only
- **Admin Librarian:** "Manage Users" card shows full user management (members, librarians, admins)

---

## ?? Menu Cards Details

### **Row 1 - Core Management**

#### **Card 1: ?? Manage Books**
- **Icon:** ?? (U+1F4DA)
- **Title:** "Manage Books"
- **Description:** "Add, edit, or remove books from the library"
- **Button:** "Go to Books"
- **Navigation:** ? `ManageBooksPage`
- **Position:** Row 0, Column 0

**Functionality:**
```csharp
private void ManageBooks_Click(object sender, RoutedEventArgs e)
{
    NavigationService?.Navigate(new ManageBooksPage());
}
```

---

#### **Card 2: ?? Manage Authors**
- **Icon:** ?? (U+270D + U+FE0F)
- **Title:** "Manage Authors"
- **Description:** "Add, edit, or remove book authors"
- **Button:** "Go to Authors"
- **Navigation:** ? `ManageAuthorsPage`
- **Position:** Row 0, Column 1

**Functionality:**
```csharp
private void ManageAuthors_Click(object sender, RoutedEventArgs e)
{
    NavigationService?.Navigate(new ManageAuthorsPage());
}
```

---

#### **Card 3: ?? Manage Members**
- **Icon:** ?? (U+1F465)
- **Title:** "Manage Members"
- **Description:** "View and manage library members"
- **Button:** "Go to Members"
- **Navigation:** ? `ManageMembersPage`
- **Position:** Row 0, Column 2

**Functionality:**
```csharp
private void ManageMembers_Click(object sender, RoutedEventArgs e)
{
    NavigationService?.Navigate(new ManageMembersPage());
}
```

---

### **Row 2 - Operations**

#### **Card 4: ?? Manage Loans**
- **Icon:** ?? (U+1F4D6)
- **Title:** "Manage Loans"
- **Description:** "Process book loans and returns"
- **Button:** "Go to Loans"
- **Navigation:** ? `ManageLoansPage`
- **Position:** Row 1, Column 0

**Functionality:**
```csharp
private void ManageLoans_Click(object sender, RoutedEventArgs e)
{
    NavigationService?.Navigate(new ManageLoansPage());
}
```

**Features:**
- ? View all active loans
- ? Process book returns
- ? Calculate fines
- ? Filter and search loans

---

#### **Card 5: ?? Reservations**
- **Icon:** ?? (U+1F4C5)
- **Title:** "Reservations"
- **Description:** "View and manage book reservations"
- **Button:** "Go to Reservations"
- **Navigation:** ? `ManageReservationsPage`
- **Position:** Row 1, Column 1

**Functionality:**
```csharp
private void ManageReservations_Click(object sender, RoutedEventArgs e)
{
    NavigationService?.Navigate(new ManageReservationsPage());
}
```

**Features:**
- ? Approve PENDING reservations ? RESERVED
- ? Dis-approve RESERVED reservations ? PENDING
- ? Cancel reservations
- ? View reservation history

---

#### **Card 6: ?? Search Books**
- **Icon:** ?? (U+1F50D)
- **Title:** "Search Books"
- **Description:** "Search the library catalog"
- **Button:** "Search"
- **Navigation:** ? `SearchBooksPage`
- **Position:** Row 1, Column 2

**Functionality:**
```csharp
private void SearchBooks_Click(object sender, RoutedEventArgs e)
{
    NavigationService?.Navigate(new SearchBooksPage());
}

private void NavigateToSearch(object sender, MouseButtonEventArgs e)
{
    NavigationService?.Navigate(new SearchBooksPage());
}
```

**Features:**
- ? Search by title, author, ISBN
- ? View book details
- ? Check availability
- ? Lend books directly

---

### **Row 3 - Configuration & Analysis**

#### **Card 7: ?? Reports**
- **Icon:** ?? (U+1F4CA)
- **Title:** "Reports"
- **Description:** "View library statistics and reports"
- **Button:** "View Reports"
- **Navigation:** ? ?? **TODO** (Not yet implemented)
- **Position:** Row 2, Column 0

**Functionality:**
```csharp
private void ViewReports_Click(object sender, RoutedEventArgs e)
{
    // TODO: Navigate to Reports page
    MessageBox.Show("Navigate to Reports page", "Info", 
        MessageBoxButton.OK, MessageBoxImage.Information);
}
```

**Planned Features:**
- ?? Loan statistics
- ?? Popular books report
- ?? Fine collection report
- ?? Monthly activity summary
- ?? Member statistics

---

#### **Card 8: ?? Settings**
- **Icon:** ?? (U+2699 + U+FE0F)
- **Title:** "Settings"
- **Description:** "Configure library settings"
- **Button:** "Go to Settings"
- **Navigation:** ? ?? **TODO** (Not yet implemented)
- **Position:** Row 2, Column 1

**Functionality:**
```csharp
private void Settings_Click(object sender, RoutedEventArgs e)
{
    // TODO: Navigate to Settings page
    MessageBox.Show("Navigate to Settings page", "Info", 
        MessageBoxButton.OK, MessageBoxImage.Information);
}
```

**Planned Features:**
- ?? Configure loan period (MAX_LOAN_DAYS)
- ?? Set borrowing limits (MAX_BOOKS_PER_MEMBER)
- ?? Set reservation limits (MAX_RESERVATIONS_PER_MEMBER)
- ?? Configure fine amounts (FINE_PER_DAY)
- ?? System preferences

---

#### **Card 9: ??? Categories**
- **Icon:** ??? (U+1F3F7 + U+FE0F)
- **Title:** "Categories"
- **Description:** "Manage book categories"
- **Button:** "Go to Categories"
- **Navigation:** ? ?? **TODO** (Not yet implemented)
- **Position:** Row 2, Column 2

**Functionality:**
```csharp
private void ManageCategories_Click(object sender, RoutedEventArgs e)
{
    // TODO: Navigate to Manage Categories page
    MessageBox.Show("Navigate to Manage Categories page", "Info", 
        MessageBoxButton.OK, MessageBoxImage.Information);
}
```

**Planned Features:**
- ??? Add/Edit/Delete categories
- ??? Assign categories to books
- ??? View category statistics

---

## ?? Styling System

### **Card Styles**

#### **MenuCard Style**
```xaml
<Style x:Key="MenuCard" TargetType="Border">
    <Setter Property="Background" Value="White"/>
    <Setter Property="Padding" Value="30"/>
    <Setter Property="CornerRadius" Value="10"/>
    <Setter Property="Margin" Value="10"/>
    <Setter Property="Cursor" Value="Hand"/>
    <Setter Property="Effect">
        <Setter.Value>
            <DropShadowEffect Color="Black" Opacity="0.1" 
                            ShadowDepth="2" BlurRadius="10"/>
        </Setter.Value>
    </Setter>
</Style>
```

**Properties:**
- Background: White (`#FFFFFF`)
- Padding: 30px (all sides)
- Corner Radius: 10px
- Margin: 10px
- Shadow: Black, 10% opacity, 2px depth, 10px blur

---

#### **MenuButton Style**
```xaml
<Style x:Key="MenuButton" TargetType="Button">
    <Setter Property="Background" Value="#3498db"/>  <!-- Blue -->
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="Padding" Value="20,10"/>
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="FontWeight" Value="SemiBold"/>
    <Style.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <Setter Property="Background" Value="#2980b9"/>  <!-- Darker Blue -->
        </Trigger>
    </Style.Triggers>
</Style>
```

**Color Palette:**
- Normal: `#3498db` (Blue)
- Hover: `#2980b9` (Darker Blue)
- Text: White

---

#### **IconText Style**
```xaml
<Style x:Key="IconText" TargetType="TextBlock">
    <Setter Property="FontSize" Value="48"/>
    <Setter Property="HorizontalAlignment" Value="Center"/>
    <Setter Property="Margin" Value="0,0,0,10"/>
</Style>
```

**Properties:**
- Font Size: 48px
- Alignment: Center
- Bottom Margin: 10px

---

#### **CardTitle Style**
```xaml
<Style x:Key="CardTitle" TargetType="TextBlock">
    <Setter Property="FontSize" Value="20"/>
    <Setter Property="FontWeight" Value="SemiBold"/>
    <Setter Property="Foreground" Value="#2c3e50"/>  <!-- Dark Gray -->
    <Setter Property="HorizontalAlignment" Value="Center"/>
    <Setter Property="Margin" Value="0,15,0,10"/>
</Style>
```

**Properties:**
- Font Size: 20px
- Font Weight: SemiBold
- Color: `#2c3e50` (Dark Gray)
- Top Margin: 15px, Bottom Margin: 10px

---

#### **CardDescription Style**
```xaml
<Style x:Key="CardDescription" TargetType="TextBlock">
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="Foreground" Value="#7f8c8d"/>  <!-- Medium Gray -->
    <Setter Property="HorizontalAlignment" Value="Center"/>
    <Setter Property="TextWrapping" Value="Wrap"/>
    <Setter Property="TextAlignment" Value="Center"/>
</Style>
```

**Properties:**
- Font Size: 14px
- Color: `#7f8c8d` (Medium Gray)
- Text Wrapping: Enabled
- Alignment: Center

---

## ?? Color Scheme

| Element | Color | Hex Code | Usage |
|---------|-------|----------|-------|
| Background | Light Gray | `#f5f5f5` | Page background |
| Card Background | White | `#FFFFFF` | Card backgrounds |
| Primary Button | Blue | `#3498db` | Button normal state |
| Primary Button Hover | Dark Blue | `#2980b9` | Button hover state |
| Title Text | Dark Gray | `#2c3e50` | Card titles |
| Description Text | Medium Gray | `#7f8c8d` | Card descriptions |
| Shadow | Black | `#000000` | Drop shadow (10% opacity) |

---

## ?? Navigation Flow

```
Librarian Dashboard (Hub)
    ?
    ??? Manage Books Page
    ?   ??? Add/Edit/Delete books
    ?       ??? Manage book copies
    ?
    ??? Manage Authors Page
    ?   ??? Add/Edit/Delete authors
    ?
    ??? Manage Members Page
    ?   ??? View member details
    ?       ??? Suspend/Activate members
    ?       ??? View member loan history
    ?
    ??? Manage Loans Page
    ?   ??? Process returns
    ?       ??? Calculate fines
    ?       ??? View active loans
    ?
    ??? Manage Reservations Page
    ?   ??? Approve reservations (PENDING ? RESERVED)
    ?   ??? Dis-approve reservations (RESERVED ? PENDING)
    ?   ??? Cancel reservations
    ?
    ??? Search Books Page
    ?   ??? Search and lend books
    ?
    ??? Reports Page [TODO]
    ?   ??? View statistics
    ?
    ??? Settings Page [TODO]
    ?   ??? Configure system settings
    ?
    ??? Categories Page [TODO]
        ??? Manage book categories
```

---

## ?? Implementation Status

| Menu Item | Status | Page File | Functionality |
|-----------|--------|-----------|---------------|
| ?? Manage Books | ? Implemented | `ManageBooksPage.xaml` | Add/Edit/Delete books, Manage copies, Lend books |
| ?? Manage Authors | ? Implemented | `ManageAuthorsPage.xaml` | Add/Edit/Delete authors |
| ?? Manage Members | ? Implemented | `ManageMembersPage.xaml` | View/Edit members, Suspend/Activate |
| ?? Manage Loans | ? Implemented | `ManageLoansPage.xaml` | Process returns, Calculate fines |
| ?? Reservations | ? Implemented | `ManageReservationsPage.xaml` | Approve/Dis-approve/Cancel reservations |
| ?? Search Books | ? Implemented | `SearchBooksPage.xaml` | Search catalog, View details |
| ?? Reports | ?? TODO | - | Not yet implemented |
| ?? Settings | ?? TODO | - | Not yet implemented |
| ??? Categories | ?? TODO | - | Not yet implemented |

---

## ?? Access Control

### **Authentication:**
- User must be logged in with **Librarian role**
- Session managed through `MainWindow.CurrentUser`

### **User Data Display:**
```csharp
private void LoadDashboardData()
{
    var currentUser = MainWindow.CurrentUser;
    if (currentUser != null)
    {
        txtLibrarianName.Text = $"{currentUser.FirstName} {currentUser.LastName}";
        txtEmployeeId.Text = "EMP" + currentUser.UserID.ToString("D3");
    }
}
```

### **Security Features:**
- ? Session validation on page load
- ? Navigation service validation
- ? Role-based access control

---

## ?? Responsive Design

### **Layout Features:**
- ? Scrollable viewport (`ScrollViewer`)
- ? Maximum width constraint (1200px)
- ? Flexible card grid (adapts to content)
- ? Consistent margins (30px)

### **Grid Configuration:**
```xaml
<Grid MaxWidth="1200" Margin="30,30,30,30">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>  <!-- Welcome Section -->
        <RowDefinition Height="Auto"/>  <!-- Menu Grid -->
    </Grid.RowDefinitions>
</Grid>
```

---

## ?? User Experience

### **Interaction Design:**
- **Hover Effects:** Buttons change color on hover (`#3498db` ? `#2980b9`)
- **Cursor Feedback:** Hand cursor on clickable cards
- **Visual Hierarchy:** Icon ? Title ? Description ? Button
- **Consistent Spacing:** 30px padding, 10px margins

### **Navigation Pattern:**
- **Single-Click Navigation:** Click button to navigate
- **Card Click:** Some cards support click on entire card area
- **Back Navigation:** Browser-style back navigation supported

---

## ??? Technical Details

### **XAML Structure:**
```
Page (LibrarianDashboard)
??? Page.Resources (Styles)
    ??? ScrollViewer (Viewport)
        ??? Grid (MaxWidth 1200px)
            ??? Border (Welcome Card)
            ?   ??? StackPanel
            ?       ??? TextBlock (Librarian Name)
            ?       ??? TextBlock (Employee ID)
            ??? Grid (Menu Cards 3ª3)
                ??? Border (Card 1: Manage Books)
                ??? Border (Card 2: Manage Authors)
                ??? Border (Card 3: Manage Members)
                ??? Border (Card 4: Manage Loans)
                ??? Border (Card 5: Reservations)
                ??? Border (Card 6: Search Books)
                ??? Border (Card 7: Reports)
                ??? Border (Card 8: Settings)
                ??? Border (Card 9: Categories)
```

### **Code-Behind Structure:**
```csharp
public partial class LibrarianDashboard : Page
{
    // Constructor
    public LibrarianDashboard()
    
    // Page Load Event
    private void Page_Loaded(object sender, RoutedEventArgs e)
    
    // Data Loading
    private void LoadDashboardData()
    
    // Navigation Methods (9 total)
    private void ManageBooks_Click(...)
    private void ManageAuthors_Click(...)
    private void ManageMembers_Click(...)
    private void ManageLoans_Click(...)
    private void ManageReservations_Click(...)
    private void SearchBooks_Click(...)
    private void ViewReports_Click(...)
    private void Settings_Click(...)
    private void ManageCategories_Click(...)
    private void NavigateToSearch(...)
}
```

---

## ?? Future Enhancements

### **Reports Page (TODO):**
- ?? Loan statistics dashboard
- ?? Popular books chart
- ?? Fine collection report
- ?? Monthly activity summary
- ?? Member statistics
- ?? Book circulation report

### **Settings Page (TODO):**
- ?? Library configuration
  - Loan period settings
  - Borrowing limits
  - Reservation limits
  - Fine amounts
- ?? UI theme settings
- ?? Notification preferences
- ?? Security settings

### **Categories Page (TODO):**
- ??? CRUD operations for categories
- ??? Category hierarchy management
- ??? Bulk category assignment
- ??? Category statistics

### **Additional Features:**
- ?? Real-time notifications
- ?? Quick stats on dashboard (total books, active loans, etc.)
- ?? Global search bar in header
- ?? Dark mode toggle
- ?? Mobile-responsive design

---

## ?? Notes

### **Design Philosophy:**
- **Simplicity:** Clean, minimalist card-based design
- **Clarity:** Clear iconography and descriptive text
- **Consistency:** Uniform styling across all cards
- **Efficiency:** Quick access to common tasks

### **Performance:**
- Lazy loading of pages (only load when navigated to)
- Lightweight dashboard (no heavy data operations)
- Fast navigation with `NavigationService`

### **Maintenance:**
- Modular design (easy to add/remove cards)
- Consistent naming conventions
- Well-documented code
- Separated concerns (XAML for UI, C# for logic)

---

## ? Summary

The Librarian Dashboard serves as a **centralized navigation hub** with:
- ? **9 menu cards** organized in a 3ª3 grid
- ? **6 fully implemented** pages
- ? **3 planned features** (Reports, Settings, Categories)
- ? **Card-based design** with icons, titles, descriptions, and action buttons
- ? **Consistent styling** and color scheme
- ? **User-friendly navigation** with hover effects
- ? **Session management** with user info display
- ? **Modular architecture** for easy expansion

**The dashboard provides librarians with efficient access to all core library management functions in a visually appealing and intuitive interface.** ???
