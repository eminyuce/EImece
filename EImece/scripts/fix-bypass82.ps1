$dst = "C:\inetpub\wwwroot\Eimece_Core"
$log = "C:\Users\eminy\source\repos\eminyuce\EImece\EImece\scripts\fix-bypass82.log"
"START" | Set-Content $log
$webConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\EImece.Web.dll" stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Development" />
          <environmentVariable name="EImece__BypassAdminAuth" value="true" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
"@
Set-Content -Path (Join-Path $dst "web.config") -Value $webConfig -Encoding UTF8
# Also force Production json bypass true for this local host (local smoke); keep source Production false for real prod
$prod = Join-Path $dst "appsettings.Production.json"
if (Test-Path $prod) {
  $j = Get-Content $prod -Raw | ConvertFrom-Json
  $j.EImece.BypassAdminAuth = $true
  ($j | ConvertTo-Json -Depth 10) | Set-Content $prod -Encoding UTF8
}
Get-Content (Join-Path $dst "web.config") | Add-Content $log
& "$env:windir\system32\inetsrv\appcmd.exe" recycle apppool /apppool.name:Eimece_Core 2>&1 | Add-Content $log
"DONE" | Add-Content $log
