# Comprehensive Verification script for local IIS deployment and Admin pages

Write-Host "=== 1. Testing GET /health & /healthz ==="
try {
    $healthRes = Invoke-WebRequest -Uri "http://localhost:81/health" -UseBasicParsing -TimeoutSec 30
    Write-Host "  -> [OK $($healthRes.StatusCode)] /health"
    Write-Host "Response Body:`n$($healthRes.Content)"
} catch {
    Write-Host "  -> [FAIL] /health - $($_.Exception.Message)"
}

try {
    $healthzRes = Invoke-WebRequest -Uri "http://localhost:81/healthz" -UseBasicParsing -TimeoutSec 30
    Write-Host "  -> [OK $($healthzRes.StatusCode)] /healthz"
} catch {
    Write-Host "  -> [FAIL] /healthz - $($_.Exception.Message)"
}

Write-Host "`n=== 2. Testing Storefront (http://localhost:81/) ==="
try {
    $storeRes = Invoke-WebRequest -Uri "http://localhost:81/" -UseBasicParsing -TimeoutSec 30
    Write-Host "  -> [OK $($storeRes.StatusCode)] Storefront Homepage (Bytes: $($storeRes.Content.Length))"
} catch {
    Write-Host "  -> [FAIL] Storefront Homepage - $($_.Exception.Message)"
}

Write-Host "`n=== 3. Testing Admin Panel Pages ==="
$adminPages = @(
    "http://localhost:81/admin",
    "http://localhost:81/admin/shoppingcarts",
    "http://localhost:81/admin/applogs",
    "http://localhost:81/admin/orders",
    "http://localhost:81/admin/products",
    "http://localhost:81/admin/productcategories",
    "http://localhost:81/admin/customers",
    "http://localhost:81/admin/coupons",
    "http://localhost:81/admin/brands",
    "http://localhost:81/admin/settings",
    "http://localhost:81/admin/users"
)

foreach ($page in $adminPages) {
    try {
        $res = Invoke-WebRequest -Uri $page -UseBasicParsing -TimeoutSec 30
        Write-Host "  -> [OK $($res.StatusCode)] $page (Bytes: $($res.Content.Length))"
    } catch {
        $statusCode = if ($_.Exception.Response) { $_.Exception.Response.StatusCode.value__ } else { "ERR" }
        Write-Host "  -> [FAIL $statusCode] $page - $($_.Exception.Message)"
    }
}

Write-Host "`n=== Verification Finished ==="
