@echo off
REM Run this as Administrator to deploy the pre-built legacy publish folder to IIS :82
REM Prerequisites: publish already created at ..\..\publish\Eimece (or run MSBuild first)

set SRC=%~dp0..\..\publish\Eimece
set DST=C:\inetpub\wwwroot\Eimece
set APPCMD=%windir%\system32\inetsrv\appcmd.exe

echo Deploying %SRC% -^> %DST%
net session >nul 2>&1
if errorlevel 1 (
  echo ERROR: Run this BAT as Administrator.
  pause
  exit /b 1
)

REM Unlock files
for /f "tokens=*" %%i in ('%APPCMD% list site /text:name') do (
  for /f "tokens=*" %%b in ('%APPCMD% list site "%%i" /text:bindings') do (
    echo %%b | findstr /C:":82:" >nul && (
      echo Stopping site %%i
      %APPCMD% stop site /site.name:"%%i"
    )
  )
)

robocopy "%SRC%" "%DST%" /MIR /R:2 /W:1
echo Robocopy exit %ERRORLEVEL%

REM Ensure BypassAdminAuth=true for smoke testing (set false before production!)
powershell -NoProfile -Command "(Get-Content '%DST%\Web.config' -Raw) -replace 'key=\"BypassAdminAuth\" value=\"false\"','key=\"BypassAdminAuth\" value=\"true\"' | Set-Content '%DST%\Web.config' -Encoding UTF8"

for /f "tokens=*" %%i in ('%APPCMD% list site /text:name') do (
  for /f "tokens=*" %%b in ('%APPCMD% list site "%%i" /text:bindings') do (
    echo %%b | findstr /C:":82:" >nul && (
      echo Pointing %%i to %DST%
      %APPCMD% set vdir "%%i/" /physicalPath:"%DST%"
      for /f "tokens=*" %%p in ('%APPCMD% list app "%%i/" /text:applicationPool') do (
        %APPCMD% set apppool /apppool.name:%%p /managedRuntimeVersion:v4.0 /managedPipelineMode:Integrated
        %APPCMD% recycle apppool /apppool.name:%%p
      )
      %APPCMD% start site /site.name:"%%i"
    )
  )
)

echo Done. Open http://localhost:82/Admin/
pause
