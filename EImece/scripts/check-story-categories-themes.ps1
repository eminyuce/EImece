# Test calling GetSeoUrl from EImece.Domain.dll
[System.Reflection.Assembly]::LoadFrom("C:\Users\eminy\source\repos\EImece\EImece\EImece.Domain\bin\Release\EImece.Domain.dll") | Out-Null

$connStr = "Data Source=YUCE\SQLEXPRESS;Initial Catalog=yuva8905_yuvadan;User ID=sqluser;Password=sqluser;Encrypt=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT Id, Name, PageTheme, IsActive FROM StoryCategories ORDER BY Id"
$reader = $cmd.ExecuteReader()

$categories = @()
while ($reader.Read()) {
    $sc = New-Object EImece.Domain.Entities.StoryCategory
    $sc.Id = [int]$reader["Id"]
    $sc.Name = $reader["Name"].ToString()
    $sc.PageTheme = if ([string]::IsNullOrWhiteSpace($reader["PageTheme"].ToString())) { "T1" } else { $reader["PageTheme"].ToString() }
    $sc.IsActive = [bool]$reader["IsActive"]
    $categories += $sc
}
$reader.Close()
$conn.Close()

Write-Host "=========================================================================="
Write-Host "CHECKING ALL STORY CATEGORIES AND VERIFYING MATCHING PAGE THEMES"
Write-Host "=========================================================================="

foreach ($c in $categories) {
    $seoUrl = [EImece.Domain.Helpers.Extensions.EntityExtension]::GetSeoUrl($c)
    $url = "http://localhost:81/s/sc/$seoUrl"
    
    Write-Host "`nCategory ID: $($c.Id) | Name: '$($c.Name)' | Theme: $($c.PageTheme)"
    Write-Host "  URL: $url"

    try {
        $res = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 30
        $html = $res.Content
        $expectedTheme = $c.PageTheme
        $expectedClass = "crizal-story-page--" + $expectedTheme.ToLower()

        $matchesTheme = $html.Contains($expectedClass)
        $hasSidebar = $html.Contains("crizal-story-sidebar")
        $hasGrid = $html.Contains("crizal-story-grid")
        $hasHero = $html.Contains("crizal-story-card--hero")

        Write-Host "  HTTP Status: $($res.StatusCode)"
        Write-Host "  Expected CSS Class ($expectedClass): $matchesTheme"
        
        switch ($expectedTheme) {
            "T1" {
                Write-Host "  Layout Elements: Standard List + Right Sidebar (Sidebar: $hasSidebar)"
            }
            "T2" {
                Write-Host "  Layout Elements: Standard List + Left Sidebar (Sidebar: $hasSidebar, order-lg-1: $($html.Contains('order-lg-1')))"
            }
            "T3" {
                Write-Host "  Layout Elements: 2-Col Grid + Right Sidebar (Grid: $hasGrid, Sidebar: $hasSidebar)"
            }
            "T4" {
                Write-Host "  Layout Elements: 3-Col Wide Grid (Grid: $hasGrid, Filter Bar: $($html.Contains('crizal-story-filter-bar')))"
            }
            "T5" {
                Write-Host "  Layout Elements: Hero Headline + Grid + Sidebar (Hero: $hasHero, Grid: $hasGrid, Sidebar: $hasSidebar)"
            }
        }

        if ($matchesTheme) {
            Write-Host "  -> VERIFICATION: PASS (Matches $($c.PageTheme))" -ForegroundColor Green
        } else {
            Write-Host "  -> VERIFICATION: FAIL" -ForegroundColor Red
        }
    } catch {
        Write-Host "  -> ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }
}
