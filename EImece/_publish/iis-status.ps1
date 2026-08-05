$ErrorActionPreference = "Continue"
$log = "c:\Users\eminy\source\repos\eminyuce\EImece\EImece\_publish\iis-status.log"
Import-Module WebAdministration
$out = @()
$out += "=== Sites ==="
Get-Website | ForEach-Object {
  $binds = ($_.bindings.Collection | ForEach-Object { "$($_.protocol)://$($_.bindingInformation)" }) -join "; "
  $out += "$($_.Name) state=$($_.State) path=$($_.PhysicalPath) pool=$($_.applicationPool) binds=$binds"
}
$out += "=== AppPools ==="
Get-ChildItem IIS:\AppPools | ForEach-Object { $out += "$($_.Name) state=$((Get-WebAppPoolState -Name $_.Name).Value)" }
$out | Tee-Object -FilePath $log
