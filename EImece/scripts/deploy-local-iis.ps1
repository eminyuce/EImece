Write-Host "Stopping AppPool Eimece..."
& "$env:SystemRoot\system32\inetsrv\appcmd.exe" stop apppool /apppool.name:Eimece
Start-Sleep -Seconds 2

$source = "c:\Users\eminy\source\repos\EImece\EImece\EImece"
$destination = "C:\inetpub\wwwroot\Eimece"

Write-Host "Copying binary and view files to IIS..."
robocopy "$source" "$destination" /MIR /XD .git .vs obj App_Data logs media\logs /XF *.cs *.csproj *.user *.sln *.log /R:1 /W:1 /NP /NDL /NFL

Write-Host "Starting AppPool Eimece..."
& "$env:SystemRoot\system32\inetsrv\appcmd.exe" start apppool /apppool.name:Eimece
Start-Sleep -Seconds 2

Write-Host "IIS Deployment finished."
