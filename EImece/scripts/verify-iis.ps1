# Verification script for IIS deployment

Write-Host "=== 1. Checking Home Page (http://localhost:81/) ==="
try {
    $res = Invoke-WebRequest -Uri "http://localhost:81/" -UseBasicParsing -TimeoutSec 15
    Write-Host "Home Page Status: $($res.StatusCode)"
    Write-Host "Home Page Size: $($res.Content.Length) bytes"
    if ($res.Content -match 'schema.org') {
        Write-Host "Organization JSON-LD: FOUND in homepage"
    } else {
        Write-Host "Organization JSON-LD: NOT FOUND in homepage"
    }
} catch {
    Write-Host "Home Page Error: $($_.Exception.Message)"
}

Write-Host "`n=== 2. Checking Sitemap XML (http://localhost:81/sitemap.xml) ==="
$sitemapXml = $null
try {
    $res = Invoke-WebRequest -Uri "http://localhost:81/sitemap.xml" -UseBasicParsing -TimeoutSec 15
    Write-Host "Sitemap Status: $($res.StatusCode)"
    Write-Host "Sitemap Content-Type: $($res.Headers['Content-Type'])"
    $sitemapXml = [xml]$res.Content
    $urls = $sitemapXml.urlset.url
    Write-Host "Total URLs in sitemap.xml: $($urls.Count)"
} catch {
    Write-Host "Sitemap Error: $($_.Exception.Message)"
}

Write-Host "`n=== 3. Checking Sitemap URLs ==="
if ($urls) {
    $sampleUrls = $urls | Select-Object -First 10
    foreach ($u in $sampleUrls) {
        $loc = $u.loc
        # Replace domain/host if necessary to point to localhost:81
        $testUri = $loc
        if ($testUri -match "^https?://[^/]+(/?.*)$") {
            $testUri = "http://localhost:81" + $matches[1]
        }
        try {
            $urlRes = Invoke-WebRequest -Uri $testUri -UseBasicParsing -TimeoutSec 10
            Write-Host "OK ($($urlRes.StatusCode)): $testUri (from $loc)"
        } catch {
            Write-Host "FAIL ($($_.Exception.Response.StatusCode.value__)): $testUri - $($_.Exception.Message)"
        }
    }
}

Write-Host "`n=== 4. Checking Robots.txt (http://localhost:81/robots.txt) ==="
try {
    $res = Invoke-WebRequest -Uri "http://localhost:81/robots.txt" -UseBasicParsing -TimeoutSec 10
    Write-Host "Robots.txt Status: $($res.StatusCode)"
    Write-Host "Robots.txt Content:`n$($res.Content)"
} catch {
    Write-Host "Robots.txt Error: $($_.Exception.Message)"
}

Write-Host "`n=== 5. Checking 404 / 410 Handling ==="
try {
    $res = Invoke-WebRequest -Uri "http://localhost:81/p/nonexistent-item-999999999" -UseBasicParsing -TimeoutSec 10
    Write-Host "Nonexistent product returned: $($res.StatusCode)"
} catch {
    Write-Host "Nonexistent product returned expected error status: $($_.Exception.Response.StatusCode.value__)"
}

Write-Host "`n=== Verification Completed ==="
