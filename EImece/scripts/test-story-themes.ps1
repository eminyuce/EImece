$uri = "http://localhost:81/admin/storycategories/saveoredit/"
try {
    $r = Invoke-WebRequest -Uri $uri -UseBasicParsing -TimeoutSec 30
    Write-Host "Admin StoryCategories SaveOrEdit Status: $($r.StatusCode)"
    Write-Host "Has page-theme-picker: $($r.Content.Contains('page-theme-picker'))"
    Write-Host "Has story-tema-01.svg: $($r.Content.Contains('story-tema-01.svg'))"
    Write-Host "Has story-tema-02.svg: $($r.Content.Contains('story-tema-02.svg'))"
    Write-Host "Has story-tema-03.svg: $($r.Content.Contains('story-tema-03.svg'))"
    Write-Host "Has story-tema-04.svg: $($r.Content.Contains('story-tema-04.svg'))"
    Write-Host "Has story-tema-05.svg: $($r.Content.Contains('story-tema-05.svg'))"
    Write-Host "Has pageThemeLightbox: $($r.Content.Contains('pageThemeLightbox'))"
} catch {
    Write-Host "Error: $($_.Exception.Message)"
}
