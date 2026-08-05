Import-Module WebAdministration
Restart-WebAppPool -Name Eimece
Start-Sleep -Seconds 2
"recycled" | Set-Content "c:\Users\eminy\source\repos\eminyuce\EImece\EImece\_publish\recycle.log"
