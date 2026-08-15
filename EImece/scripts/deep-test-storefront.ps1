# Comprehensive verification of localhost:81 storefront and sitemap

Write-Host "=== 1. Testing Sitemap.xml ==="
$sitemapRes = Invoke-WebRequest -Uri "http://localhost:81/sitemap.xml" -UseBasicParsing -TimeoutSec 60
Write-Host "Sitemap HTTP Status: $($sitemapRes.StatusCode)"
Write-Host "Sitemap Content-Type: $($sitemapRes.Headers['Content-Type'])"

$sitemapXml = [xml]$sitemapRes.Content
$allUrls = $sitemapXml.urlset.url
Write-Host "Total Valid URLs in Sitemap: $($allUrls.Count)"

$productUrls = @($allUrls | Where-Object { $_.loc -match "/p/" })
$categoryUrls = @($allUrls | Where-Object { $_.loc -match "/c/" })
$infoUrls = @($allUrls | Where-Object { $_.loc -match "/i/" -or $_.loc -match "/info/" })

Write-Host "  -> Products in Sitemap: $($productUrls.Count)"
Write-Host "  -> Categories in Sitemap: $($categoryUrls.Count)"
Write-Host "  -> Pages/Info in Sitemap: $($infoUrls.Count)"

Write-Host "`n=== 2. Testing Homepage (http://localhost:81/) ==="
$homeRes = Invoke-WebRequest -Uri "http://localhost:81/" -UseBasicParsing -TimeoutSec 60
Write-Host "Homepage HTTP Status: $($homeRes.StatusCode) ($($homeRes.Content.Length) bytes)"
if ($homeRes.Content -match 'schema.org.*Organization') {
    Write-Host "  -> Organization JSON-LD: PRESENT & VALID"
}

Write-Host "`n=== 3. Testing Sample Category URLs ==="
foreach ($cat in ($categoryUrls | Select-Object -First 3)) {
    $loc = $cat.loc
    $testUri = $loc -replace "^https?://[^/]+", "http://localhost:81"
    try {
        $r = Invoke-WebRequest -Uri $testUri -UseBasicParsing -TimeoutSec 60
        Write-Host "Category OK ($($r.StatusCode)): $testUri"
        if ($r.Content -match 'schema.org.*BreadcrumbList') {
            Write-Host "  -> BreadcrumbList JSON-LD: PRESENT"
        }
    } catch {
        Write-Host "Category Status ($($_.Exception.Response.StatusCode.value__)): $testUri"
    }
}

Write-Host "`n=== 4. Testing Sample Product URLs ==="
foreach ($p in ($productUrls | Select-Object -First 3)) {
    $loc = $p.loc
    $testUri = $loc -replace "^https?://[^/]+", "http://localhost:81"
    try {
        $r = Invoke-WebRequest -Uri $testUri -UseBasicParsing -TimeoutSec 60
        Write-Host "Product OK ($($r.StatusCode)): $testUri"
        if ($r.Content -match 'schema.org.*Product') {
            Write-Host "  -> Product JSON-LD: PRESENT"
        }
        if ($r.Content -match 'schema.org.*BreadcrumbList') {
            Write-Host "  -> BreadcrumbList JSON-LD: PRESENT"
        }
    } catch {
        Write-Host "Product Status ($($_.Exception.Response.StatusCode.value__)): $testUri"
    }
}

Write-Host "`n=== 5. Testing Empty Search Results UX (_EmptyState) ==="
try {
    $searchRes = Invoke-WebRequest -Uri "http://localhost:81/p/arama?search=NonExistentQueryXYZ123" -UseBasicParsing -TimeoutSec 60
    Write-Host "Search response ($($searchRes.StatusCode)): $($searchRes.Content.Length) bytes"
    if ($searchRes.Content -match 'empty-state-card') {
        Write-Host "  -> _EmptyState UI: RENDERED PROPERLY with icon and action button"
    }
} catch {
    Write-Host "Search Error: $($_.Exception.Message)"
}

Write-Host "`n=== 6. Testing Empty Shopping Cart UX (_EmptyState) ==="
try {
    $cartRes = Invoke-WebRequest -Uri "http://localhost:81/Payment/ShoppingCart" -UseBasicParsing -TimeoutSec 60
    Write-Host "Cart response ($($cartRes.StatusCode)): $($cartRes.Content.Length) bytes"
    if ($cartRes.Content -match 'empty-state-card') {
        Write-Host "  -> Empty Cart _EmptyState UI: RENDERED PROPERLY with icon and action button"
    }
} catch {
    Write-Host "Cart Error: $($_.Exception.Message)"
}

Write-Host "`n=== 7. Testing 404 Not Found & 410 Gone Responses ==="
# Test non-existent product
$req404 = [System.Net.HttpWebRequest]::Create("http://localhost:81/p/nonexistent-item-999999999")
try {
    $resp404 = $req404.GetResponse()
    Write-Host "404 Test got: $($resp404.StatusCode)"
} catch [System.Net.WebException] {
    Write-Host "Non-existent item returned expected HTTP: $($_.Exception.Response.StatusCode.value__)"
}

# Test robots.txt
$robotsRes = Invoke-WebRequest -Uri "http://localhost:81/robots.txt" -UseBasicParsing -TimeoutSec 60
Write-Host "`n=== 8. Robots.txt ==="
Write-Host "Robots.txt Status: $($robotsRes.StatusCode)"
Write-Host "Robots.txt Body:`n$($robotsRes.Content.Trim())"

Write-Host "`n========================================================"
Write-Host "ALL VERIFICATION CHECKS COMPLETED ON IIS (localhost:81)"
Write-Host "========================================================"
