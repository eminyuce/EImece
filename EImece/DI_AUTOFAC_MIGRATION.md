# EImece MVC 5.2.3 — Ninject to Autofac Migration Guide

This guide is for **ASP.NET MVC 5 (System.Web)** + **EF6** + **SQL Server 2008** using Repository/Service layers.

## 1. Remove Ninject

1. Remove Ninject NuGet packages from the MVC web project (`EImece/EImece`):
   - `Ninject`
   - `Ninject.Web.Common`
   - `Ninject.Web.Common.WebHost`
   - `Ninject.MVC5`
   - `WebActivatorEx` (remove only if it was added exclusively by Ninject bootstrapper)

2. Delete Ninject bootstrapping artifacts if they exist:
   - `App_Start/NinjectWebCommon.cs`
   - Any custom `Ninject*` module classes.

3. Remove Ninject wiring from config if present:
   - In `Web.config` under `<appSettings>` remove entries like:
     - `owin:appStartup` entries that only point to Ninject startup.
     - Any key/value pair that references Ninject bootstrapper types.
   - Under `<runtime><assemblyBinding>` remove Ninject binding redirects if any.

4. Ensure `Global.asax` no longer references Ninject kernel bootstrapping.

---

## 2. Install Autofac

For MVC 5 on .NET Framework (not .NET Core), install:

```powershell
Install-Package Autofac -Version 6.5.0
Install-Package Autofac.Mvc5 -Version 6.1.0
```

Optional (only if you host Web API 2 with Autofac too):

```powershell
Install-Package Autofac.WebApi2
Install-Package Autofac.WebApi2.Owin
```

> You do **not** need `Microsoft.Extensions.DependencyInjection` for MVC 5 Autofac integration.

---

## 3. Configure Autofac

### 3.1 Web.config changes (if needed)

In a pure MVC 5 Autofac setup, no mandatory Autofac-specific `Web.config` section is required.

Only ensure old Ninject references are removed.

---

### 3.2 Add Autofac bootstrapper (`App_Start/AutofacConfig.cs`)

```csharp
using Autofac;
using Autofac.Integration.Mvc;
using EImece.Domain.DbContext;
using EImece.Domain.Repositories;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using System.Web.Mvc;

namespace EImece.App_Start
{
    public static class AutofacConfig
    {
        public static IContainer RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            // Controller activation
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterFilterProvider();
            builder.RegisterModelBinderProvider();

            // DbContext (EF6) - request scoped
            builder.RegisterType<EImeceContext>()
                   .As<IEImeceContext>()
                   .WithParameter("nameOrConnectionString", Domain.Constants.DbConnectionKey)
                   .InstancePerRequest();

            // Generic repository example
            builder.RegisterType<ProductRepository>()
                   .As<IProductRepository>()
                   .InstancePerRequest();

            // Service layer example
            builder.RegisterType<ProductService>()
                   .As<IProductService>()
                   .InstancePerRequest();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            return container;
        }
    }
}
```

---

### 3.3 Global.asax changes

Call the bootstrapper in `Application_Start` and dispose in `Application_End`.

```csharp
using Autofac;
using EImece.App_Start;

namespace EImece
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static IContainer Container;

        protected void Application_Start()
        {
            Container = AutofacConfig.RegisterDependencies();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_End()
        {
            Container?.Dispose();
        }
    }
}
```

---

### 3.4 Example controller constructor injection

```csharp
using EImece.Domain.Services.IServices;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        public ActionResult Index()
        {
            var items = _productService.GetProducts();
            return View(items);
        }
    }
}
```

---

### 3.5 Production registration pattern (recommended)

Keep architecture unchanged by isolating registration methods:

```csharp
private static void BindRepositories(ContainerBuilder builder)
{
    builder.RegisterType<ProductRepository>().As<IProductRepository>().InstancePerRequest();
    builder.RegisterType<OrderRepository>().As<IOrderRepository>().InstancePerRequest();
    // ...others
}

private static void BindServices(ContainerBuilder builder)
{
    builder.RegisterType<ProductService>().As<IProductService>().InstancePerRequest();
    builder.RegisterType<OrderService>().As<IOrderService>().InstancePerRequest();
    // ...others
}
```

This preserves your Generic Repository + Service Layer separation exactly as-is.

---

## 4. Lifetime mapping table

| Ninject lifetime | Autofac equivalent | Typical use in MVC 5 |
|---|---|---|
| `InRequestScope()` | `InstancePerRequest()` | `DbContext`, repositories, services that depend on request state |
| `InSingletonScope()` | `SingleInstance()` | stateless caches, app-wide factories, expensive shared components |
| `InTransientScope()` | `InstancePerDependency()` | lightweight stateless helpers created each resolve |

Notes:
- For EF6, `DbContext` should be `InstancePerRequest()` to avoid context sharing across requests.
- Services/repositories using that context should also be `InstancePerRequest()`.

---

## 5. Validation checklist

Use this checklist after migration:

- [ ] No Ninject packages remain in `packages.config`.
- [ ] No `NinjectWebCommon.cs` or Ninject module files remain.
- [ ] `Application_Start` calls `AutofacConfig.RegisterDependencies()`.
- [ ] `DependencyResolver` is set to `AutofacDependencyResolver`.
- [ ] Controllers resolve via constructor injection (no parameterless fallback needed).
- [ ] EF6 context is registered with `InstancePerRequest()`.
- [ ] Repository registrations use `InstancePerRequest()`.
- [ ] Service registrations use `InstancePerRequest()`.
- [ ] Application starts without DI activation exceptions.
- [ ] A sample request executes end-to-end (Controller -> Service -> Repository -> EF6).

