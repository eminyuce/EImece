$req = [System.Net.HttpWebRequest]::Create("http://localhost:81/")
$req.Timeout = 90000
try {
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $resp = $req.GetResponse()
    $sw.Stop()
    Write-Host "Success: $($resp.StatusCode) in $($sw.ElapsedMilliseconds) ms"
    $stream = $resp.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    $html = $reader.ReadToEnd()
    Write-Host "HTML length: $($html.Length)"
    if ($html -match "application/ld\+json") {
        Write-Host "JSON-LD: PRESENT"
    } else {
        Write-Host "JSON-LD: NOT FOUND"
    }
} catch [System.Net.WebException] {
    $resp = $_.Exception.Response
    if ($resp) {
        $stream = $resp.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        $html = $reader.ReadToEnd()
        Write-Host "HTTP Status: $($resp.StatusCode)"
        Write-Host "=== ERROR HTML ==="
        Write-Host $html
    } else {
        Write-Host "No response: $($_.Exception.Message)"
    }
}
