$ErrorActionPreference = "Stop"
$root = "C:\inetpub\wwwroot\Eimece"
$paths = @(
  "$root\App_Data",
  "$root\App_Data\logs"
)
foreach ($p in $paths) {
  if (-not (Test-Path $p)) { New-Item -ItemType Directory -Path $p -Force | Out-Null }
  $acl = Get-Acl $p
  $rule1 = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS APPPOOL\Eimece","Modify","ContainerInherit,ObjectInherit","None","Allow")
  $rule2 = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS","Modify","ContainerInherit,ObjectInherit","None","Allow")
  $acl.SetAccessRule($rule1)
  $acl.SetAccessRule($rule2)
  Set-Acl -Path $p -AclObject $acl
}
Import-Module WebAdministration
Restart-WebAppPool -Name Eimece
"ACL OK" | Set-Content "c:\Users\eminy\source\repos\eminyuce\EImece\EImece\_publish\acl.log"
