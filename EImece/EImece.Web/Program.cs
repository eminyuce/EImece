using EImece.Web.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Microsoft.Extensions.DependencyInjection composition root (same DI stack as legacy after Ninject removal).
builder.Services.AddEImeceCore(builder.Configuration);
builder.Services.AddControllersWithViews();

// Connection string env override parity with legacy ConnectionStringProvider.
var envConnection = Environment.GetEnvironmentVariable("EIMECE_DB_CONNECTION_STRING");
if (!string.IsNullOrWhiteSpace(envConnection))
{
    builder.Configuration["ConnectionStrings:EImeceDbConnection"] = envConnection;
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Area route placeholders (Admin / Customers) — controllers migrate in Phase 6.
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Expose entry assembly for integration tests in later phases.
public partial class Program;
