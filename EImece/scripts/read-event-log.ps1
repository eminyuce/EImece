$events = Get-EventLog -LogName Application -After (Get-Date).AddMinutes(-10) | Where-Object { $_.Source -like '*ASP.NET*' -or $_.Source -like '*IIS*' }
foreach ($e in $events) {
    Write-Host "Time: $($e.TimeGenerated)"
    Write-Host "Source: $($e.Source)"
    Write-Host "Message: $($e.Message)"
    Write-Host "----------------------------------"
}
