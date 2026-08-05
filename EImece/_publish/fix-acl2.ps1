$ErrorActionPreference = "Stop"
$root = "C:\inetpub\wwwroot\Eimece"
$paths = @(
  "$root\App_Data",
  "$root\App_Data\logs",
  "$root\media",
  "$root\media\images",
  "$root\media\files",
  "$root\media\uploads"
)
foreach ($p in $paths) {
  if (-not (Test-Path $p)) { New-Item -ItemType Directory -Path $p -Force | Out-Null }
  icacls $p /grant "IIS APPPOOL\Eimece:(OI)(CI)M" /T | Out-Null
  icacls $p /grant "IIS_IUSRS:(OI)(CI)M" /T | Out-Null
}
Import-Module WebAdministration
Restart-WebAppPool -Name Eimece
"MEDIA ACL OK" | Set-Content "c:\Users\eminy\source\repos\eminyuce\EImece\EImece\_publish\acl2.log"
