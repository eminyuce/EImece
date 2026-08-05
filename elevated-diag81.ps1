$log = "C:\Users\eminy\source\repos\eminyuce\EImece\elevated-diag81.log"
"START" | Set-Content $log
$wc = "C:\inetpub\wwwroot\Eimece\Web.config"
Copy-Item $wc "$wc.bak-diag" -Force
$raw = Get-Content $wc -Raw
# customErrors mode=Off
$raw2 = $raw -replace 'customErrors mode="[^"]*"','customErrors mode="Off"'
if ($raw2 -eq $raw) {
  # try insert
  $raw2 = $raw -replace '(<system.web>)',"`$1`r`n    <customErrors mode=`"Off`" />"
}
Set-Content $wc -Value $raw2 -Encoding UTF8
& "$env:windir\system32\inetsrv\appcmd.exe" recycle apppool /apppool.name:Eimece 2>&1 | Add-Content $log
"CONFIG PATCHED" | Add-Content $log
