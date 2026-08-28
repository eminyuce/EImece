# TimedInterceptor + ProxyFactory — Usage

## 1) Marker attribute (plain, NOT ActionFilter)
Located at `EImece.Domain/Observability/Telemetry/TimedAttribute.cs:1`

```csharp
using EImece.Domain.Observability.Telemetry;

public class ConversationService
{
    // Service → service.{entity}.{operation}
    [Timed("service.conversations.get_by_user", "Time taken to get conversations")]
    public virtual List<Conversation> GetConversations(int userId)
    {
        return _repo.GetByUser(userId);
    }

    // Async is also supported
    [Timed("service.conversations.get_by_user_async")]
    public virtual async Task<List<Conversation>> GetConversationsAsync(int userId)
    {
        return await _repo.GetByUserAsync(userId);
    }
}

public class ConversationRepository
{
    // Repository → repo.{entity}.{operation}
    [Timed("repo.conversations.get_by_user")]
    public virtual List<Conversation> GetByUser(int userId)
    {
        // EF6 code
        return Context.Conversations.Where(c => c.UserId == userId).ToList();
    }
}
```
> **Requirement:** Methods MUST be `virtual` for class proxies (DynamicProxy subclasses at runtime).
> For non-virtual/interface methods use `ProxyFactory.CreateInterface<T>()`.

## 2) Manual creation

```csharp
using EImece.Domain.Observability.Telemetry;

var repo = new ConversationRepository();
var timedRepo = ProxyFactory.Create(repo);              // or CreateInterface<IConversationRepository>(repo)

var svc = new ConversationService(timedRepo);
var timedSvc = ProxyFactory.Create(svc);

var list = timedSvc.GetConversations(42); // -> histogram service.conversations.get_by_user (ms) + Activity tags
```

## 3) DI registration

### Autofac
```csharp
using Autofac;
using Autofac.Extras.DynamicProxy; // or manual registration
using Castle.DynamicProxy;

builder.RegisterType<TimedInterceptor>().SingleInstance();

builder.RegisterType<ConversationService>()
       .AsSelf()
       .EnableClassInterceptors()
       .InterceptedBy<TimedInterceptor>();

builder.RegisterType<ConversationRepository>()
       .AsSelf()
       .EnableClassInterceptors()
       .InterceptedBy<TimedInterceptor>();

// Alternatively without EnableClassInterceptors (explicit proxy):
builder.Register(c => ProxyFactory.Create(new ConversationService(c.Resolve<ConversationRepository>())))
       .AsSelf().SingleInstance();
```

### Microsoft.Extensions.DependencyInjection (current EImece/App_Start/DependencyInjectionConfig.cs)

 Castle DynamicProxy does not have built-in MS.DI integration; decorate after build:

```csharp
using EImece.Domain.Observability.Telemetry;
using Microsoft.Extensions.DependencyInjection;

services.AddSingleton<TimedInterceptor>();

// Register concrete, then replace with proxied instance
services.AddTransient<ConversationRepository>();
services.AddTransient<ConversationService>();

// In a factory or after BuildServiceProvider():
var repo = new ConversationRepository(...);
services.AddSingleton<ConversationRepository>(ProxyFactory.Create(repo));
services.AddSingleton<ConversationService>(ProxyFactory.Create(new ConversationService(repo)));

// Or generic decorator extension:
public static void AddTimed<T>(this IServiceCollection s) where T : class
{
    s.AddTransient<T>(sp =>
    {
        var target = ActivatorUtilities.CreateInstance<T>(sp);
        return ProxyFactory.Create(target);
    });
}
```

### Simple custom resolver
```csharp
var interceptor = new TimedInterceptor();
var generator = new ProxyGenerator();
var proxy = generator.CreateClassProxyWithTarget<ConversationService>(new ConversationService(repo), interceptor);
```

## 4) Controller vs Service/Repo

* Controllers: `EImece.Filters.TimedActionFilterAttribute` (`[TimedActionFilter]` on `BaseController`) — MVC ActionFilter, auto `app.{controller}.{action}`
* Services/Repos: `EImece.Domain.Observability.Telemetry.TimedAttribute` (`[Timed]`) + `TimedInterceptor` via Castle
* Both record to `Telemetry.GetOrCreateHistogram(name, description)` (ms) and tag `Activity.Current` (`timed.metric`, `timed.duration_ms`). Never throws.
