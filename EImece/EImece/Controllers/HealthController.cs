using EImece.Domain;
using EImece.Domain.Observability.HealthChecks;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

using EImece.Domain.Helpers;
using EImece.Web.Filters;

namespace EImece.Controllers
{
    [UnderConst]
    [AllowAnonymous]
    public class HealthController : Controller
    {
        private readonly IHealthCheckService _healthCheckService;

        public HealthController(IHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext != null && filterContext.HttpContext != null)
            {
                var settingService = DependencyResolver.Current?.GetService(typeof(EImece.Domain.Services.IServices.ISettingService)) as EImece.Domain.Services.IServices.ISettingService;
                var isUnderConstruction = settingService != null && settingService.GetSettingByKey(Constants.IsSiteUnderConstruction).ToBool(false);
                if (isUnderConstruction)
                {
                    var user = filterContext.HttpContext.User;
                    var isAuth = user != null && user.Identity != null && user.Identity.IsAuthenticated;
                    var isAdmin = isAuth && (user.IsInRole(Constants.AdministratorRole) || user.IsInRole(Constants.EditorRole));
                    if (!isAdmin)
                    {
                        filterContext.Result = new RedirectResult("/underconstruction");
                        return;
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }
        /*
         * One writable root for uploads + NLog files (media/images and media/logs).
         * Run elevated after publish:
         *
         *   mkdir "C:\inetpub\wwwroot\Eimece\media\images" 2>nul
         *   mkdir "C:\inetpub\wwwroot\Eimece\media\logs" 2>nul
         *   icacls "C:\inetpub\wwwroot\Eimece\media" /grant "IIS AppPool\Eimece":(OI)(CI)M /T
         *
         * See docs/IIS_APP_POOL_PERMISSIONS.md
         * 
         * if exist "C:\Users\eminy\source\repos\EImece\EImece\EImece\obj" rmdir /s /q "C:\Users\eminy\source\repos\EImece\EImece\EImece\obj" && "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_compiler.exe" -p "C:\Users\eminy\source\repos\EImece\EImece\EImece" -v / -f "C:\Publish\EImece"

         * "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_compiler.exe" -p "C:\Users\eminy\source\repos\EImece\EImece\EImece" -v / -f "C:\Publish\EImece"
         * PS C:\Users\eminy\source\repos\EImece\EImece> & "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_compiler.exe" -p "C:\Users\eminy\source\repos\EImece\EImece\EImece" -v / -f "C:\Publish\EImece"
         */

        [HttpGet]
        [Route("health")]
        [Route("healthz")]
        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
            var response = await _healthCheckService.GetHealthAsync(cancellationToken).ConfigureAwait(false);
            var statusCode = response.Status == "UP" ? HttpStatusCode.OK : HttpStatusCode.ServiceUnavailable;
            Response.StatusCode = (int)statusCode;
            Response.ContentType = "application/json";

            // Anonymous callers only get aggregate status (no dependency error details).
            var isAdmin = User?.Identity != null
                && User.Identity.IsAuthenticated
                && User.IsInRole(Constants.AdministratorRole);

            if (isAdmin)
            {
                return Content(JsonConvert.SerializeObject(response, Formatting.Indented), "application/json");
            }

            var publicPayload = new { status = response.Status };
            return Content(JsonConvert.SerializeObject(publicPayload), "application/json");
        }

        /*
         * 
         * 
         * 
         * Since SonarQube can return more than 500 issues, the best approach is to paginate the API and save everything to a JSON file.

PowerShell — download all issues

Run this from PowerShell:
         * 
         * $baseUrl = "http://localhost:9000"
$project = "Eimece"
$token = "YOUR_NEW_TOKEN"

$allIssues = @()
$page = 1
$pageSize = 500

do {
    Write-Host "Fetching page $page..."

    $headers = @{
        Authorization = "Bearer $token"
    }

    $url = "$baseUrl/api/issues/search?componentKeys=$project&ps=$pageSize&p=$page"

    $response = Invoke-RestMethod `
        -Uri $url `
        -Headers $headers `
        -Method Get

    if ($response.issues) {
        $allIssues += $response.issues
        Write-Host "Retrieved $($response.issues.Count) issues. Total: $($allIssues.Count)"
    }

    $page++
}
while ($allIssues.Count -lt $response.total)

$output = "sonarqube-issues.json"

$allIssues |
    ConvertTo-Json -Depth 20 |
    Set-Content -Path $output -Encoding UTF8

Write-Host ""
Write-Host "Downloaded $($allIssues.Count) issues."
Write-Host "Saved to: $((Get-Location).Path)\$output"
        */

        // ============================================================
        // ASP.NET MVC 5 - Precompiled Deployment / IIS Publish
        // ============================================================
        //
        // IMPORTANT:
        // This application uses ASP.NET MVC 5 / .NET Framework and
        // Microsoft.CodeDom.Providers.DotNetCompilerPlatform.
        //
        // Do NOT run aspnet_compiler directly after deleting bin/obj.
        // The application must first be successfully built in Visual Studio
        // so that all required assemblies are restored/recreated in bin.
        //
        // Recommended procedure:
        //
        // 1. Open EImece.sln in Visual Studio.
        //
        // 2. Select:
        //      Configuration: Release
        //
        // 3. Build/Rebuild the solution:
        //
        //      Build -> Rebuild Solution
        //
        //    This recreates the required bin files, including:
        //      - EImece.dll
        //      - EImece.Domain.dll
        //      - Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll
        //      - other NuGet/runtime dependencies
        //
        // 4. If necessary, clean old build artifacts before rebuilding:
        //
        //      Delete:
        //        EImece\bin
        //        EImece\obj
        //
        //    Then use Visual Studio:
        //
        //      Build -> Rebuild Solution
        //
        // 5. Create/clean the precompiled publish directory from CMD.
        //
        //    Run each command separately:
        //
        //      rmdir /s /q "C:\Publish\EImece"
        //      mkdir "C:\Publish\EImece"
        //
        //    IMPORTANT:
        //    Do NOT concatenate commands on the same line.
        //
        // 6. Run ASP.NET's precompiler from CMD:
        //
        //      "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_compiler.exe" -p "C:\Users\eminy\source\repos\EImece\EImece\EImece" -v / -f "C:\Publish\EImece"
        //
        // 7. IMPORTANT - aspnet_compiler.exe may appear to be stuck.
        //
        //    The compiler normally produces NO progress output while it is
        //    precompiling the application. After displaying:
        //
        //      Microsoft (R) ASP.NET Compilation Tool version 4.8.9221.0
        //
        //    it may remain silent for several minutes.
        //
        //    Do NOT immediately terminate the process.
        //
        //    Check from a SECOND CMD window:
        //
        //      tasklist | findstr /i "aspnet_compiler"
        //
        //    If aspnet_compiler.exe is listed, it is still running.
        //
        //    Also check whether the publish directory is being populated:
        //
        //      dir "C:\Publish\EImece" /s
        //
        //    You can repeat this command after 30-60 seconds to see whether
        //    files are being created.
        //
        //    Task Manager can also be used to check CPU/disk activity.
        //
        // 8. When aspnet_compiler.exe finishes, it may simply return to the
        //    command prompt without displaying a "success" message.
        //
        //    An empty final console output does NOT necessarily mean failure.
        //
        //    Verify the publish directory:
        //
        //      dir "C:\Publish\EImece" /s
        //
        //    Verify the application assembly:
        //
        //      dir "C:\Publish\EImece\EImece.dll"
        //
        //    Verify the dependencies:
        //
        //      dir "C:\Publish\EImece\bin"
        //
        //    If the publish directory contains the application files and
        //    assemblies, the precompilation completed successfully.
        //
        // 9. The precompiled application will be generated under:
        //
        //      C:\Publish\EImece
        //
        //    This directory can then be deployed to IIS.
        //
        // ============================================================
        // Known Issues / Troubleshooting
        // ============================================================
        //
        // ISSUE 1:
        //
        //      Unrecognized attribute 'xmlns:xdt'
        //
        // Example:
        //
        //      obj\Release\TransformWebConfig\Assist\web.config(11):
        //      error ASPCONFIG: Unrecognized attribute 'xmlns:xdt'
        //
        // Cause:
        // The ASP.NET compiler is processing a generated Web.config transform
        // file under the obj directory:
        //
        //      obj\Release\TransformWebConfig\Assist\web.config
        //
        // Solution:
        //
        //      rmdir /s /q "C:\Users\eminy\source\repos\EImece\EImece\EImece\obj"
        //
        // Then rebuild the application using Visual Studio:
        //
        //      Build -> Rebuild Solution
        //
        // IMPORTANT:
        // Do not delete obj and then immediately run aspnet_compiler.
        // Rebuild the application first.
        //
        // ============================================================
        //
        // ISSUE 2:
        //
        //      The CodeDom provider type
        //      "Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider..."
        //      could not be located.
        //
        //      Could not load type 'EImece.MvcApplication'
        //
        // Cause:
        // The required assemblies are missing from bin, usually because bin
        // was deleted and the application was not rebuilt.
        //
        // Solution:
        //
        // 1. Rebuild the application using Visual Studio:
        //
        //      Build -> Rebuild Solution
        //
        // 2. Verify:
        //
        //      EImece\bin\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll
        //
        // 3. Verify:
        //
        //      EImece\bin\EImece.dll
        //
        // 4. Run aspnet_compiler again.
        //
        // ============================================================
        //
        // ISSUE 3:
        //
        //      'msbuild' is not recognized as an internal or external command
        //
        // Cause:
        // MSBuild is not available in the current CMD PATH.
        //
        // Solution:
        // Use Visual Studio:
        //
        //      Build -> Rebuild Solution
        //
        // Alternatively, use the Developer Command Prompt for Visual Studio,
        // where MSBuild is normally available.
        //
        // ============================================================
        //
        // CMD vs PowerShell
        //
        // CMD:
        //
        //      "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_compiler.exe" -p ...
        //
        // PowerShell:
        //
        //      & "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_compiler.exe" -p ...
        //
        // IMPORTANT:
        // Do NOT paste a PowerShell prompt such as:
        //
        //      PS C:\Users\eminy\source\repos\EImece\EImece>
        //
        // into CMD.
        //
        // "PS C:\Users\..." is the PowerShell prompt, not part of the command.
        //
        // In CMD the prompt should look like:
        //
        //      C:\Users\eminy\source\repos\EImece\EImece>
        //
        // ============================================================
        //
        // VERIFIED WORKING PROCEDURE:
        //
        //      1. Delete obj if stale TransformWebConfig files exist.
        //      2. Delete bin if a completely clean rebuild is required.
        //      3. Open EImece.sln in Visual Studio.
        //      4. Select Release configuration.
        //      5. Build/Rebuild Solution.
        //      6. Verify EImece\bin contains required assemblies.
        //      7. Create/clean C:\Publish\EImece.
        //      8. Run aspnet_compiler.exe.
        //      9. Wait patiently if the compiler produces no progress output.
        //     10. Verify C:\Publish\EImece contains the compiled application.
        //     11. Deploy C:\Publish\EImece to IIS.
        //
        // ============================================================
    }
}
