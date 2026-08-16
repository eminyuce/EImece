$sitemapUrl = "http://localhost:81/sitemap.xml"
Write-Host "Fetching sitemap from $sitemapUrl..."

try {
    $sitemapResponse = Invoke-WebRequest -Uri $sitemapUrl -UseBasicParsing
    [xml]$xml = $sitemapResponse.Content
    $urls = $xml.urlset.url | ForEach-Object { $_.loc.Trim() }

    Write-Host "Found $($urls.Count) URLs in sitemap."
    Write-Host "--------------------------------------------------------"

    $results = @()
    $index = 1

    foreach ($rawUrl in $urls) {
        # Map remote host to localhost:81 for local IIS verification
        $uri = [System.Uri]$rawUrl
        $localUrl = "http://localhost:81" + $uri.PathAndQuery

        try {
            $response = Invoke-WebRequest -Uri $localUrl -UseBasicParsing -TimeoutSec 15
            $status = $response.StatusCode
            $results += [PSCustomObject]@{
                Index = $index
                Status = $status
                OriginalUrl = $rawUrl
                LocalUrl = $localUrl
                Result = "OK"
            }
            Write-Host "[$index/$($urls.Count)] $status OK -> $localUrl" -ForegroundColor Green
        }
        catch {
            $ex = $_.Exception
            $statusCode = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { 0 }
            $results += [PSCustomObject]@{
                Index = $index
                Status = $statusCode
                OriginalUrl = $rawUrl
                LocalUrl = $localUrl
                Result = "ERROR: " + $ex.Message
            }
            Write-Host "[$index/$($urls.Count)] $statusCode FAILED -> $localUrl ($($ex.Message))" -ForegroundColor Red
        }
        $index++
    }

    Write-Host "--------------------------------------------------------"
    $successCount = ($results | Where-Object { $_.Status -eq 200 }).Count
    $failedCount = ($results | Where-Object { $_.Status -ne 200 }).Count
    Write-Host "Sitemap Check Summary: Total: $($urls.Count), Succeeded (200 OK): $successCount, Failed: $failedCount"
    
    return $results
}
catch {
    Write-Host "Failed to retrieve or parse sitemap: $_" -ForegroundColor Red
}
