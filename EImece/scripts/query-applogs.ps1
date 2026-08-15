$connStr = "Data Source=YUCE\SQLEXPRESS;Initial Catalog=yuva8905_yuvadan;User ID=sqluser;Password=sqluser;Encrypt=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
try {
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT TOP 10 * FROM AppLogs ORDER BY EventDateTime DESC;"
    $reader = $cmd.ExecuteReader()
    while ($reader.Read()) {
        Write-Host "=== Log Entry ==="
        Write-Host "Time: $($reader['EventDateTime']) | Level: $($reader['EventLevel'])"
        Write-Host "Message: $($reader['EventMessage'])"
        Write-Host "Error: $($reader['ErrorMessage'])"
        Write-Host "StackTrace: $($reader['InnerErrorMessage'])"
        Write-Host "-----------------"
    }
    $reader.Close()
} catch {
    Write-Host "Query Error: $($_.Exception.Message)"
} finally {
    $conn.Close()
}
