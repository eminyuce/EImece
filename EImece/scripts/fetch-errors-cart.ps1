$urls = @(
    "http://localhost:81/Payment/ShoppingCart",
    "http://localhost:81/c/pc/qacrudmst5hlxmpcatedit-0j6g4h1b"
)

foreach ($url in $urls) {
    Write-Host "=== Fetching $url ==="
    $req = [System.Net.HttpWebRequest]::Create($url)
    try {
        $resp = $req.GetResponse()
        Write-Host "Success: $($resp.StatusCode)"
    } catch [System.Net.WebException] {
        $resp = $_.Exception.Response
        if ($resp) {
            $stream = $resp.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $html = $reader.ReadToEnd()
            Write-Host "Status: $($resp.StatusCode)"
            Write-Host "=== ERROR HTML ==="
            Write-Host $html
        } else {
            Write-Host "Error: $($_.Exception.Message)"
        }
    }
}
