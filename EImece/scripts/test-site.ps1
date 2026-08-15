try {
    $res = Invoke-WebRequest -Uri "http://localhost:81/" -UseBasicParsing -TimeoutSec 60
    Write-Host "Home Page: $($res.StatusCode)"
} catch {
    Write-Host "Home Page Error: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        $body = $reader.ReadToEnd()
        Write-Host "=== 500 ERROR BODY ==="
        Write-Host $body
    }
}
