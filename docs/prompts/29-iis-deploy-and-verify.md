# Deploy and verify the app on IIS

- **Captured:** 2026-08-21 7:51:05 AM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

Deploy and fully verify the ASP.NET MVC application on IIS.

*Environment*
- Application is running on http://localhost:81
- Review web.config for any configuration issues


Permissions:
You have full permission to do everything needed. You do not need to ask me for approval.
- You may write and run test scripts
- You may use Playwright / Chrome for automated browser testing
- You may check application logs
- You may temporarily change web.config values (including bypassing admin login if required for testing)
- You may create folders or adjust permissions only if necessary for testing


*Tasks*
1. Deploy the application to IIS (if not already deployed).
2. Check all Admin pages for correct functionality.
3. Validate every URL listed in sitemap.xml.
4. Thoroughly test all Store Front pages and Customer pages to ensure nothing was broken by recent changes (especially the ViewModel / DTO refactoring).
5. Confirm that no pages return errors, 404s, or broken functionality.
6. Use Playwright with Chrome to run regression tests on all pages.
7. After fixing any broken pages, create a Git commit and push the changes to the repository.
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
   
   

*Test credentials (Customer account)*
- Email: <REDACTED>
- Password: <REDACTED>

Report any broken pages, configuration issues, or errors found during the checks. Fix them before committing and pushing.


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
- Also read the web.config in the IIS folder and the full source code
