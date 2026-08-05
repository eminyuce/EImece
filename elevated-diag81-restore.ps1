$wc = "C:\inetpub\wwwroot\Eimece\Web.config"
if (Test-Path "$wc.bak-diag") { Copy-Item "$wc.bak-diag" $wc -Force }
# keep BypassAdminAuth true
$raw = Get-Content $wc -Raw
$raw = $raw -replace 'key="BypassAdminAuth" value="false"','key="BypassAdminAuth" value="true"'
Set-Content $wc -Value $raw -Encoding UTF8
& "$env:windir\system32\inetsrv\appcmd.exe" recycle apppool /apppool.name:Eimece
"RESTORED" | Set-Content "C:\Users\eminy\source\repos\eminyuce\EImece\elevated-diag81-restore.log"
