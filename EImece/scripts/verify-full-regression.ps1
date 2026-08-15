# Full Regression Verification Suite: Sitemap, Storefront URLs, Admin Pages (BypassAuth)

$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:81"

Write-Host "=========================================================="
Write-Host " STARTING FULL REGRESSION TEST ON $baseUrl"
Write-Host "=========================================================="

# 1. Test Sitemap and All Contained URLs
Write-Host "`n--- 1. Testing Sitemap.xml and all contained URLs ---"
$sitemapUrl = "$baseUrl/sitemap.xml"
try {
    $sitemapRes = Invoke-WebRequest -Uri $sitemapUrl -UseBasicParsing -TimeoutSec 60
    Write-Host "Sitemap HTTP Status: $($sitemapRes.StatusCode)"
    [xml]$xml = $sitemapRes.Content
    $urls = $xml.urlset.url.loc
    $totalCount = $urls.Count
    Write-Host "Total URLs in sitemap: $totalCount"

    $failedUrls = @()
    $testedCount = 0
    
    foreach ($url in $urls) {
        $testedCount++
        try {
            $pageRes = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 30
            if ($pageRes.StatusCode -ne 200) {
                $failedUrls += "$url -> $($pageRes.StatusCode)"
            }
        } catch {
            $failedUrls += "$url -> $($_.Exception.Message)"
        }
        if ($testedCount % 50 -eq 0) {
            Write-Host "  ... validated $testedCount / $totalCount URLs"
        }
    }
    Write-Host "Completed testing all $testedCount sitemap URLs."
    if ($failedUrls.Count -eq 0) {
        Write-Host "SUCCESS: All $testedCount sitemap URLs returned HTTP 200 OK!" -ForegroundColor Green
    } else {
        Write-Host "WARNING: $($failedUrls.Count) URLs failed:" -ForegroundColor Red
        $failedUrls | ForEach-Object { Write-Host "  - $_" }
    }
} catch {
    Write-Host "Sitemap Error: $($_.Exception.Message)" -ForegroundColor Red
}

# 2. Test Admin Panel Pages (with BypassAdminAuth)
Write-Host "`n--- 2. Testing Admin Panel Pages (BypassAdminAuth) ---"
$adminPages = @(
    "/admin",
    "/admin/dashboard/index",
    "/admin/dashboard/oursitefeatures",
    "/admin/dashboard/systemhealth",
    "/admin/adminsettings",
    "/admin/adminsettings/systemsettings",
    "/admin/products",
    "/admin/productcategories",
    "/admin/brands",
    "/admin/orders",
    "/admin/customers",
    "/admin/coupons",
    "/admin/faq",
    "/admin/mainpageimages",
    "/admin/menus",
    "/admin/mailtemplates",
    "/admin/stories",
    "/admin/storycategories",
    "/admin/tagcategories",
    "/admin/tags",
    "/admin/lists",
    "/admin/subscribers",
    "/admin/templates",
    "/admin/users",
    "/admin/shoppingcarts",
    "/admin/report",
    "/admin/report/couponusage",
    "/admin/report/fraudanalysis",
    "/admin/report/paymentmethod",
    "/admin/report/paymentstatus",
    "/admin/report/getregionalsalesreport",
    "/admin/applogs",
    "/admin/metrics"
)

$adminPassed = 0
$adminFailed = @()

foreach ($page in $adminPages) {
    $fullUrl = "$baseUrl$page"
    try {
        $res = Invoke-WebRequest -Uri $fullUrl -UseBasicParsing -TimeoutSec 30
        if ($res.StatusCode -eq 200) {
            $adminPassed++
            Write-Host "  [OK 200] $page ($($res.Content.Length) bytes)"
        } else {
            $adminFailed += "$page -> HTTP $($res.StatusCode)"
            Write-Host "  [FAIL $($res.StatusCode)] $page" -ForegroundColor Red
        }
    } catch {
        $adminFailed += "$page -> $($_.Exception.Message)"
        Write-Host "  [ERROR] $page -> $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`nAdmin Pages Summary: $adminPassed / $($adminPages.Count) pages returned HTTP 200 OK."
if ($adminFailed.Count -gt 0) {
    Write-Host "Failed Admin Pages:" -ForegroundColor Red
    $adminFailed | ForEach-Object { Write-Host "  - $_" }
}

# 3. Test Storefront Key Scenarios
Write-Host "`n--- 3. Testing Storefront Key Scenarios ---"
$storefrontScenarios = @(
    @{ Name = "Homepage"; Url = "/" },
    @{ Name = "Search (Empty state UX)"; Url = "/p/arama?search=nonexistentkeyword123xyz" },
    @{ Name = "Shopping Cart (Empty state UX)"; Url = "/Payment/ShoppingCart" },
    @{ Name = "Robots.txt"; Url = "/robots.txt" },
    @{ Name = "Health Status"; Url = "/health" }
)

foreach ($sc in $storefrontScenarios) {
    try {
        $res = Invoke-WebRequest -Uri "$baseUrl$($sc.Url)" -UseBasicParsing -TimeoutSec 30
        Write-Host "  [OK $($res.StatusCode)] $($sc.Name): $($sc.Url)"
    } catch {
        Write-Host "  [FAIL] $($sc.Name): $($sc.Url) -> $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n=========================================================="
Write-Host " FULL REGRESSION SUITE COMPLETED"
Write-Host "=========================================================="
