using EImece.Domain;
using EImece.Domain.Observability.HealthChecks;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    [AllowAnonymous]
    public class HealthController : Controller
    {
        private readonly IHealthCheckService _healthCheckService;

        public HealthController(IHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
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
// 3. Build the solution/project:
//      Build -> Rebuild Solution
//
//    This recreates the required bin files, including:
//      - EImece.dll
//      - EImece.Domain.dll
//      - Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll
//      - other NuGet/runtime dependencies
//
// 4. If necessary, clean old build artifacts before rebuilding:
//      Delete:
//        EImece\bin
//        EImece\obj
//
//    Then use Visual Studio:
//      Build -> Rebuild Solution
//
// 5. Create/clean the precompiled publish directory from CMD:
//
//      rmdir /s /q "C:\Publish\EImece"
//      mkdir "C:\Publish\EImece"
//
//    Run each command separately. Do NOT concatenate commands.
//
// 6. Run ASP.NET's precompiler from CMD:
//
//      "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_compiler.exe" -p "C:\Users\eminy\source\repos\EImece\EImece\EImece" -v / -f "C:\Publish\EImece"
//
// 7. Verify that the compilation completes without ASPCONFIG,
//    ASPPARSE, or CodeDom errors.
//
// 8. The precompiled application will be generated under:
//
//      C:\Publish\EImece
//
// IMPORTANT CMD/POWERSHELL NOTE:
//
// CMD:
//      "C:\...\aspnet_compiler.exe" -p ...
//
// PowerShell:
//      & "C:\...\aspnet_compiler.exe" -p ...
//
// Do NOT paste a PowerShell prompt such as:
//      PS C:\Users\eminy\...>
//
// into CMD. The "PS ..." text is the PowerShell prompt, not part
// of the command.
//
// ============================================================
// Known Issues / Troubleshooting
// ============================================================
//
// If aspnet_compiler reports:
//
//      Unrecognized attribute 'xmlns:xdt'
//
// the compiler is probably processing a generated file under:
//
//      obj\Release\TransformWebConfig\Assist\web.config
//
// Clean the obj directory and rebuild the project in Visual Studio:
//
//      rmdir /s /q "C:\Users\eminy\source\repos\EImece\EImece\EImece\obj"
//
// Then rebuild using Visual Studio.
//
// If aspnet_compiler reports:
//
//      The CodeDom provider type
//      Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider
//      could not be located
//
// rebuild the project in Visual Studio first. The required DLL must
// exist in:
//
//      EImece\bin\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll
//
// If:
//
//      Could not load type 'EImece.MvcApplication'
//
// is also reported, this is generally a consequence of the missing
// application assembly/dependencies. Verify that:
//
//      EImece\bin\EImece.dll
//
// exists and that the Visual Studio Release build completed successfully.
//
// NOTE:
// "msbuild" may not be available from a normal CMD prompt:
//
//      'msbuild' is not recognized as an internal or external command.
//
// In that situation, use Visual Studio's Build/Rebuild Solution instead,
// or use the Developer Command Prompt for Visual Studio.
//
// ============================================================


    }
}
