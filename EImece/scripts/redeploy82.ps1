$src = "C:\Users\eminy\source\repos\eminyuce\EImece\publish\Eimece_Core"
$dst = "C:\inetpub\wwwroot\Eimece_Core"
$log = "C:\Users\eminy\source\repos\eminyuce\EImece\EImece\scripts\redeploy82.log"
"START $(Get-Date)" | Set-Content $log
& "$env:windir\system32\inetsrv\appcmd.exe" stop site /site.name:Eimece_Core 2>&1 | Add-Content $log
Start-Sleep 2
robocopy $src $dst /MIR /R:2 /W:1 /NFL /NDL /NP | Out-Null
"ROBO=$LASTEXITCODE" | Add-Content $log
# patch appsettings bypass + conn
$appsettings = Join-Path $dst "appsettings.json"
$j = Get-Content $appsettings -Raw | ConvertFrom-Json
$j.EImece.BypassAdminAuth = $true
$j.ConnectionStrings.EImeceDbConnection = "Data Source=YUCE\SQLEXPRESS;Initial Catalog=yuva8905_yuvadan;User ID=sqluser;Password=sqluser;Encrypt=True;TrustServerCertificate=True;"
($j | ConvertTo-Json -Depth 20) | Set-Content $appsettings -Encoding UTF8
& "$env:windir\system32\inetsrv\appcmd.exe" start site /site.name:Eimece_Core 2>&1 | Add-Content $log
& "$env:windir\system32\inetsrv\appcmd.exe" recycle apppool /apppool.name:Eimece_Core 2>&1 | Add-Content $log
"DONE" | Add-Content $log
