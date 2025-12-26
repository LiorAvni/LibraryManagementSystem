# Script to replace all ?? question marks with proper emojis in all XAML files

$xamlFiles = Get-ChildItem "LibraryManagementSystem\View\Pages\*.xaml" -Recurse

$replacements = @{
    # Common icons
    'Text="?? Search'           = 'Text="🔍 Search'
    'Content="?? Search'        = 'Content="🔍 Search'
    'Text="?? Category'         = 'Text="🏷️ Category'
    'Text="??? Category'        = 'Text="🏷️ Category'
    'Content="?? Filter'        = 'Content="🔍 Filter'
    'Content="?? Edit'          = 'Content="✏️ Edit'
    'Content="??? Delete'       = 'Content="🗑️ Delete'
    'Content="?? Borrow'        = 'Content="📚 Borrow'
    'Content="?? Reserve'       = 'Content="📌 Reserve'
    'Content="? Cancel'         = 'Content="❌ Cancel'
    'Content="?? Pay Now'       = 'Content="💳 Pay Now'
    
    # Page titles and headers
    'Text="?? New Arrivals'     = 'Text="🆕 New Arrivals'
    'Text="?? Current Loans'    = 'Text="📚 Current Loans'
    'Text="?? My Reservations'  = 'Text="📌 My Reservations'
    'Text="?? Fine Payment'     = 'Text="💵 Fine Payment'
    'Text="?? Loan History'     = 'Text="📜 Loan History'
    'Text="?? Search Results'   = 'Text="🔍 Search Results'
    
    # Dashboard icons
    'Text="??"([^"]*Search)'    = 'Text="🔍$1'
    'Text="??"([^"]*Book)'      = 'Text="📚$1'
    'Text="??"([^"]*User)'      = 'Text="👥$1'
    'Text="??"([^"]*Member)'    = 'Text="👥$1'
    'Text="??"([^"]*Loan)'      = 'Text="📖$1'
    'Text="??"([^"]*Reservation)' = 'Text="📅$1'
    'Text="??"([^"]*Author)'    = 'Text="✍️$1'
    'Text="??"([^"]*Report)'    = 'Text="📊$1'
    'Text="??"([^"]*Setting)'   = 'Text="⚙️$1'
    
    # Specific standalone icons
    'Text="??"'                 = 'Text="🔍"'
    'Text="???"'                = 'Text="🏷️"'
    'Text="?????"'              = 'Text="🏢"'
    'Text="? "'                 = 'Text="✅ "'
    'Text="? Availability'      = 'Text="✅ Availability'
}

foreach ($file in $xamlFiles) {
    Write-Host "Processing: $($file.Name)" -ForegroundColor Cyan
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $modified = $false
    
    foreach ($pattern in $replacements.Keys) {
        $replacement = $replacements[$pattern]
        if ($content -match [regex]::Escape($pattern)) {
            $content = $content -replace [regex]::Escape($pattern), $replacement
            $modified = $true
            Write-Host "  ✓ Replaced: $pattern" -ForegroundColor Green
        }
    }
    
    # Additional regex-based replacements for standalone ?? in Text attributes
    if ($content -match 'Text="(\?\?+)"') {
        # Replace any remaining ?? with search icon as default
        $content = $content -replace 'Text="(\?\?+)"', 'Text="🔍"'
        $modified = $true
        Write-Host "  ✓ Replaced standalone ??" -ForegroundColor Green
    }
    
    if ($modified) {
        $content | Set-Content $file.FullName -Encoding UTF8 -NoNewline
        Write-Host "  ✅ File updated" -ForegroundColor Yellow
    } else {
        Write-Host "  ⚪ No changes needed" -ForegroundColor Gray
    }
}

Write-Host "`n✨ All files processed!" -ForegroundColor Green
