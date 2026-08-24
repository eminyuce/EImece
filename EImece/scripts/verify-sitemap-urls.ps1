Add-Type -AssemblyName System.Net.Http

$ErrorActionPreference = "Continue"

Write-Host "Fetching sitemap from http://localhost:81/sitemap.xml..."
$sitemapResponse = Invoke-WebRequest -Uri "http://localhost:81/sitemap.xml" -UseBasicParsing -TimeoutSec 30
[xml]$xml = $sitemapResponse.Content

$urls = $xml.urlset.url.loc | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
$total = $urls.Count
Write-Host "Found $total URLs in sitemap.xml to verify."

$handler = New-Object System.Net.Http.HttpClientHandler
$handler.AllowAutoRedirect = $true
$client = New-Object System.Net.Http.HttpClient($handler)
$client.Timeout = [TimeSpan]::FromSeconds(15)

$successCount = 0
$failCount = 0
$failures = @()

$index = 0
foreach ($url in $urls) {
    $index++
    $retry = 0
    $success = $false
    $lastErr = ""
    $statusCode = 0

    while ($retry -lt 3 -and -not $success) {
        $retry++
        try {
            $respTask = $client.GetAsync($url)
            $resp = $respTask.Result
            $statusCode = [int]$resp.StatusCode
            if ($statusCode -ge 200 -and $statusCode -lt 400) {
                $success = $true
                $successCount++
            } else {
                $lastErr = "HTTP " + $statusCode + " " + $resp.ReasonPhrase
            }
        } catch {
            if ($_.Exception.InnerException) {
                $lastErr = $_.Exception.InnerException.Message
            } else {
                $lastErr = $_.Exception.Message
            }
            Start-Sleep -Milliseconds 100
        }
    }

    if (-not $success) {
        $failCount++
        $failures += [PSCustomObject]@{
            Index = $index
            Url = $url
            StatusCode = $statusCode
            Error = $lastErr
        }
        Write-Host "FAILED [$index / $total]: $url -> $lastErr" -ForegroundColor Red
    }

    if ($index % 50 -eq 0 -or $index -eq $total) {
        Write-Host "Progress: $index / $total (Passed: $successCount, Failed: $failCount)"
    }
}

Write-Host "`n================ SITEMAP VERIFICATION SUMMARY ================"
Write-Host "Total URLs Checked : $total"
Write-Host "Successful (2xx/3xx): $successCount"
Write-Host "Failed              : $failCount"

if ($failures.Count -gt 0) {
    Write-Host "`nFailed URLs details:"
    $failures | Format-Table -AutoSize
} else {
    Write-Host "`nSUCCESS: ALL $total URLs IN SITEMAP.XML ARE WORKING FINE (HTTP 200 OK)!" -ForegroundColor Green
}
