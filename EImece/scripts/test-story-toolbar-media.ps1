# Test Story SaveOrEdit toolbar contains Resimler button
$url1 = "http://localhost:81/admin/stories/saveoredit/215"
$url2 = "http://localhost:81/admin/stories/saveoredit?id=215"

foreach ($url in @($url1, $url2)) {
    Write-Host "`n--------------------------------------------------"
    Write-Host "Testing URL: $url"
    try {
        $res = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 30
        $html = $res.Content
        Write-Host "HTTP Status: $($res.StatusCode)"

        $hasGalleryBtn = $html.Contains("admin-edit-gallery-btn")
        $hasResimlerText = $html.Contains("Resimler")
        $hasExpectedUrl = $html.Contains("contentId=215") -and $html.Contains("mod=Stories") -and $html.Contains("imageType=StoryGallery")

        Write-Host "Has admin-edit-gallery-btn: $hasGalleryBtn"
        Write-Host "Has 'Resimler' text: $hasResimlerText"
        Write-Host "Has target media URL: $hasExpectedUrl"

        if ($hasGalleryBtn -and $hasExpectedUrl) {
            Write-Host "RESULT: PASS" -ForegroundColor Green
        } else {
            Write-Host "RESULT: FAIL" -ForegroundColor Red
        }
    } catch {
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $body = $reader.ReadToEnd()
            Write-Host "Response Body (first 500 chars):"
            Write-Host $body.Substring(0, [Math]::Min(500, $body.Length))
        }
    }
}
