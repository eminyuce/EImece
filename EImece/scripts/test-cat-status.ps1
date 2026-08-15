$req = [System.Net.HttpWebRequest]::Create("http://localhost:81/c/pc/qacrudmst5hlxmpcatedit-0j6g4h1b")
$req.Timeout = 15000
try {
    $resp = $req.GetResponse()
    Write-Host "Success: $($resp.StatusCode)"
} catch [System.Net.WebException] {
    $resp = $_.Exception.Response
    if ($resp) {
        Write-Host "Returned Expected HTTP Status: $($resp.StatusCode)"
    } else {
        Write-Host "Error: $($_.Exception.Message)"
    }
}
