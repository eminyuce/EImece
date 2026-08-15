# Verification of Admin System Health Status Badge and Dashboard Tile

Write-Host "=== 1. Testing GET /health endpoint directly ==="
try {
    $healthRes = Invoke-WebRequest -Uri "http://localhost:81/health" -UseBasicParsing -TimeoutSec 30
    Write-Host "Health Endpoint Status: $($healthRes.StatusCode)"
    Write-Host "Health Response Content-Type: $($healthRes.Headers['Content-Type'])"
    Write-Host "Health Response Body:`n$($healthRes.Content)"
} catch {
    Write-Host "Health Endpoint Error: $($_.Exception.Message)"
}

Write-Host "`n=== 2. Testing Admin Dashboard (http://localhost:81/admin) ==="
try {
    $adminRes = Invoke-WebRequest -Uri "http://localhost:81/admin" -UseBasicParsing -TimeoutSec 60
    Write-Host "Admin Dashboard Status: $($adminRes.StatusCode) ($($adminRes.Content.Length) bytes)"
    
    if ($adminRes.Content -match 'adminSystemHealthContainer') {
        Write-Host "  -> Topbar System Health Container: FOUND"
    } else {
        Write-Host "  -> Topbar System Health Container: NOT FOUND"
    }

    if ($adminRes.Content -match 'healthStatusDot') {
        Write-Host "  -> Topbar Health Status Dot: FOUND"
    }

    if ($adminRes.Content -match 'systemHealthModal') {
        Write-Host "  -> System Health Details Modal: FOUND"
    }

    if ($adminRes.Content -match 'dashboardHealthDot') {
        Write-Host "  -> Dashboard Visual Health Tile: FOUND"
    }

    if ($adminRes.Content -match 'pollingIntervalMs') {
        Write-Host "  -> Health Poller JavaScript: FOUND"
    }
} catch {
    Write-Host "Admin Dashboard Error: $($_.Exception.Message)"
}

Write-Host "`n=== All Admin Health Checks Completed ==="
