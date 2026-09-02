# Production-ready end-to-end QA

- **Captured:** 2026-08-12 11:12:44 PM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

You are a Senior QA Engineer and ASP.NET MVC Architect with 20 years of experience in finding real bugs, edge cases, and production issues.

Goal:
Make the EImece application fully production-ready by performing thorough end-to-end testing of both desktop and mobile views. Find every bug, functional issue, UI problem, and error. Produce a clear JSON report so AI coding agents can fix the issues.

Application Details:
- Running on IIS at: http://localhost:81/
- Admin login: http://localhost:81/account/adminlogin/
- Customer login: http://localhost:81/account/login/
- IIS folder: C:\inetpub\wwwroot\Eimece
- Source code: C:\Users\eminy\source\repos\EImece\EImece
- GitHub: https://github.com/eminyuce/EImece
- Runtime: .NET Framework 4.8.1
- Stack: ASP.NET MVC 5.3, ASP.NET Identity, OWIN, Entity Framework 6.5, SQL Server
- Payments: Iyzico (Strategy pattern)
- Storefront: Interchangeable Razor designs (Crizal, Modern)
- License: Apache License 2.0

Database connection (already configured):
Data Source=<SQL_SERVER>;Initial Catalog=<DATABASE>;User ID=<USER>;Password=<PASSWORD>;Encrypt=True;TrustServerCertificate=True;

Important paths:
- Uploads & logs root: C:\inetpub\wwwroot\Eimece\media\ (images + logs)
- Docs: read everything inside the docs/ folder
- Also read the web.config in the IIS folder and the full source code

Permissions:
You have full permission to do everything needed. You do not need to ask me for approval.
- You may write and run test scripts
- You may use Playwright / Chrome for automated browser testing
- You may check application logs
- You may temporarily change web.config values (including bypassing admin login if required for testing)
- You may create folders or adjust permissions only if necessary for testing

Testing Scope (must cover all):
1. Frontend / Customer pages (desktop + mobile)
2. Admin panel pages
3. Authentication flows (customer + admin)
4. Shopping cart, checkout, and payment flows
5. AJAX / jQuery-driven admin operations
6. File uploads and media handling
7. Reports and exports
8. Error pages and edge cases
9. Logs for any runtime errors or exceptions

Process:
1. Read the docs/ folder thoroughly.
2. Inspect web.config and key source code areas.
3. Test all important pages and user flows on both desktop and mobile viewports.
4. Actively look for:
   - Functional bugs
   - UI/UX issues
   - Broken links or missing resources
   - JavaScript / AJAX errors
   - Validation problems
   - Authorization issues
   - Performance or loading problems
   - Any errors in logs
5. After testing, produce a structured JSON report.

Required JSON Report Format:
{
  "summary": {
    "totalIssues": 0,
    "critical": 0,
    "high": 0,
    "medium": 0,
    "low": 0,
    "testedAreas": []
  },
  "issues": [
    {
      "id": "BUG-001",
      "severity": "Critical|High|Medium|Low",
      "area": "Frontend|Admin|Auth|Cart|Payment|Reports|Other",
      "page": "URL or page name",
      "title": "Short clear title",
      "description": "Detailed description of the problem",
      "stepsToReproduce": ["step 1", "step 2"],
      "expected": "What should happen",
      "actual": "What actually happens",
      "device": "Desktop|Mobile|Both",
      "screenshotOrLog": "optional reference",
      "suggestedFix": "Brief suggestion for the coding agent"
    }
  ]
}

Rules:
- Focus only on real, reproducible issues.
- Be precise and actionable so coding agents can fix the problems directly.
- Test both desktop and mobile versions.
- Check logs after important actions.
- Do not stop until the main user journeys and admin functions have been thoroughly tested.

Start by reading the docs/ folder and understanding the application structure, then begin systematic end-to-end testing.
