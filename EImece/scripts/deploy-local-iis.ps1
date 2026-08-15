Import-Module WebAdministration

Write-Host "Stopping AppPool Eimece..."
Stop-WebAppPool -Name "Eimece" -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

$source = "c:\Users\eminy\source\repos\EImece\EImece\EImece"
$destination = "C:\inetpub\wwwroot\Eimece"

Write-Host "Copying binary and view files to IIS..."
robocopy "$source" "$destination" /MIR /XD .git .vs obj App_Data /XF *.cs *.csproj *.user *.sln /R:2 /W:1 /NP /NDL /NFL

Write-Host "Starting AppPool Eimece..."
Start-WebAppPool -Name "Eimece" -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

Write-Host "IIS Deployment finished."
