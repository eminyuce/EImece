# Application-wide unused-code cleanup

- **Captured:** 2026-08-14 6:26:17 PM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

Perform a *full application-wide unused-code cleanup*.

### Objective

Identify and remove any *unused methods* from:

* Controllers
* Services
* Repositories
* Service interfaces
* Repository interfaces
* Helper/utility classes where applicable

A method should be considered unused only after verifying that there are *no valid references to it anywhere in the application*.

### Requirements

1. *Controller methods*

   * Find controller action methods that are not referenced by:

     * Routes
     * Views
     * Html.Action
     * Html.RenderAction
     * AJAX calls
     * JavaScript
     * Forms
     * Redirects
     * Other controllers/services
     * Any configured/custom routing mechanism
   * Remove truly unused controller actions.

2. *Service methods*

   * Identify service methods that have no callers.
   * Check all controllers, services, background jobs, scheduled tasks, dependency-injection registrations, and other application code before removing them.
   * Remove the method from both the service implementation and its interface when appropriate.

3. *Repository methods*

   * Identify repository methods that have no callers.
   * Check all services and other repository consumers before removing them.
   * Remove the method from both the repository implementation and its interface when appropriate.

4. *Interface cleanup*

   * If a removed method exists in an interface, remove the corresponding interface declaration.
   * Verify that no implementation or consumer still depends on it.

5. *Do not remove methods based solely on static analysis*

   * ASP.NET MVC 5 applications may use reflection, routing conventions, Razor helpers, dependency injection, configuration, or string-based references.
   * Before removing a method, perform a comprehensive reference search.
   * Treat framework entry points such as controller actions and convention-based methods carefully.

6. *Preserve required public APIs*

   * Do not remove methods that may be consumed externally through:

     * Public APIs
     * Webhooks
     * Reflection
     * Configuration
     * External integrations
     * JavaScript/AJAX endpoints
     * Scheduled/background execution
   * If usage cannot be conclusively determined, do not remove the method. Report it as a candidate for manual review.

7. *Maintain behavior*

   * Do not change application functionality.
   * Do not refactor unrelated code.
   * Do not change business logic.
   * Do not rename methods unless required as part of removing unused code.

8. *Clean up cascading unused code*

   * After removing unused methods, perform another analysis pass.
   * Removing one method may make another method, interface member, dependency, or helper unused.
   * Continue until no confidently unused methods remain.

### Validation

After the cleanup:

* Build the entire solution.
* Run all available automated tests.
* Run the application's existing E2E/integration tests if available.
* Verify that there are no compilation errors.
* Verify that no broken MVC routes or Razor references were introduced.
* Verify that dependency injection registrations remain valid.

### Final Report

Provide a summary containing:

* Removed controller methods
* Removed service methods
* Removed repository methods
* Removed interface members
* Any additional unused code removed
* Items that were identified as potentially unused but intentionally retained because usage could not be proven
* Build/test results
* Any risks or areas requiring manual verification

*Principle:* Follow the *Clean Code / YAGNI principle*: code that is genuinely unused and has no required external/framework entry point should be removed rather than retained unnecessarily.


deploy to IIS and run check all pages and check each URL of sitemap.xml 




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

Lots of jquery ajax call exists in admin panel pages so pay attentions them
