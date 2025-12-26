# Comprehensive emoji replacement script

$files = @{
    "EditMemberPage.xaml" = @{
        'Content="? Dashboard"' = 'Content="🏠 Dashboard"'
    }
    "LendBookPage.xaml" = @{
        'Content="?? Save Changes"' = 'Content="💾 Save Changes"'
        'Content="?? Lend Book"' = 'Content="📚 Lend Book"'
        'Content="? Back to Manage Books"' = 'Content="🔙 Back to Manage Books"'
    }
    "LoginPage.xaml" = @{
        'Text="?? Library System"' = 'Text="📚 Library System"'
    }
    "ManageReservationsPage.xaml" = @{
        'Content="? Approve"' = 'Content="✅ Approve"'
        'Content="? Dis-Approve"' = 'Content="⏸️ Dis-Approve"'
    }
    "MemberDetailsPage.xaml" = @{
        'Content="? Dashboard"' = 'Content="🏠 Dashboard"'
        'Content="? Back to Members"' = 'Content="🔙 Back to Members"'
    }
    "SearchResultsPage.xaml" = @{
        'Content="? Back to Search"' = 'Content="🔙 Back to Search"'
        'Text="?? "' = 'Text="🔍 "'
    }
}

foreach ($fileName in $files.Keys) {
    $filePath = "LibraryManagementSystem\View\Pages\$fileName"
    
    if (Test-Path $filePath) {
        Write-Host "Processing: $fileName" -ForegroundColor Cyan
        $content = Get-Content $filePath -Raw -Encoding UTF8
        $modified = $false
        
        foreach ($pattern in $files[$fileName].Keys) {
            $replacement = $files[$fileName][$pattern]
            if ($content -match [regex]::Escape($pattern)) {
                $content = $content -replace [regex]::Escape($pattern), $replacement
                $modified = $true
                Write-Host "  ✓ Replaced: $pattern → $replacement" -ForegroundColor Green
            }
        }
        
        if ($modified) {
            $content | Set-Content $filePath -Encoding UTF8 -NoNewline
            Write-Host "  ✅ File updated" -ForegroundColor Yellow
        } else {
            Write-Host "  ⚪ No changes needed" -ForegroundColor Gray
        }
    }
}

Write-Host "`n✨ All emoji replacements completed!" -ForegroundColor Green
