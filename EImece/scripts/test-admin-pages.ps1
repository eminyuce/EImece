$connStr = "Data Source=YUCE\SQLEXPRESS;Initial Catalog=yuva8905_yuvadan;User ID=sqluser;Password=sqluser;Encrypt=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT TOP 1 Id, Name FROM Products WHERE IsActive = 1 ORDER BY Id"
$reader = $cmd.ExecuteReader()
$prodId = 0
$prodName = ""
if ($reader.Read()) {
    $prodId = [int]$reader["Id"]
    $prodName = $reader["Name"].ToString()
}
$reader.Close()
$conn.Close()

Write-Host "Found Product: ID = $prodId, Name = '$prodName'"

$urls = @(
    "http://localhost:81/admin/stories/saveoredit/215",
    "http://localhost:81/admin/products/saveoredit/$prodId",
    "http://localhost:81/admin/menus/saveoredit/1"
)

foreach ($url in $urls) {
    Write-Host "--------------------------------------------------"
    Write-Host "Testing URL: $url"
    try {
        $res = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 30
        Write-Host "Status: $($res.StatusCode)"
        $hasGallery = $res.Content.Contains("admin-edit-gallery-btn")
        $hasResimler = $res.Content.Contains("Resimler")
        Write-Host "Has admin-edit-gallery-btn: $hasGallery"
        Write-Host "Has Resimler: $hasResimler"
    } catch {
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}
