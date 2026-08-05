# EImece.Web — Deployment (ASP.NET Core 8)

Parallel Core host (`EImece.Web`). Legacy MVC5 (`EImece`) remains on IIS/.NET Framework until cutover.

## Prerequisites

- .NET 8 runtime (or SDK for `dotnet publish`)
- SQL Server reachable from the host
- Reverse proxy recommended in production (nginx, Caddy, IIS ARR, Traefik)

## Configuration

| Setting | Source | Notes |
|---------|--------|--------|
| Connection string | `ConnectionStrings:EImeceDbConnection` or env `EIMECE_DB_CONNECTION_STRING` | Prefer env / user-secrets / Key Vault — never commit real passwords |
| Site options | `EImece:*` | Domain, under-construction, BypassAdminAuth (keep **false** in prod) |
| Iyzico | `Iyzico:*` | Sandbox vs live BaseUrl + keys |
| SMTP | `Smtp:*` | Empty `Host` → log-only sink (dev) |
| Media | `Media:RootRelativePath` | Default `wwwroot/media` |
| DataProtection | `App_Data/DataProtection-Keys` | Persist across instances; gitignored |

Local secrets:

```bash
cd EImece.Web
dotnet user-secrets set "ConnectionStrings:EImeceDbConnection" "Server=...;Database=EImece;..."
dotnet user-secrets set "Iyzico:ApiKey" "..."
dotnet user-secrets set "Iyzico:SecretKey" "..."
```

## Publish

```bash
# From a neutral cwd (empty stub *.csproj under EImece/ confuse the SDK)
cd /tmp
dotnet publish /path/to/EImece/EImece.Web/EImece.Web.csproj -c Release -o /var/www/eimece
```

Run:

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://127.0.0.1:5080
export EIMECE_DB_CONNECTION_STRING='Server=...;...'
dotnet /var/www/eimece/EImece.Web.dll
```

Health: `GET /health` (JSON; host stays UP if SQL is down — check `database` field).

## Linux + nginx (Kestrel)

Example upstream:

```nginx
server {
    listen 443 ssl http2;
    server_name shop.example.com;

    location / {
        proxy_pass         http://127.0.0.1:5080;
        proxy_http_version 1.1;
        proxy_set_header   Host $host;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
        proxy_set_header   Connection keep-alive;
    }
}
```

`EImece.Web` enables `UseForwardedHeaders` so cookies/`UseHttpsRedirection` see the public scheme.

systemd unit sketch:

```ini
[Service]
WorkingDirectory=/var/www/eimece
ExecStart=/usr/bin/dotnet /var/www/eimece/EImece.Web.dll
Restart=always
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5080
EnvironmentFile=/etc/eimece/env
```

## Windows + IIS

1. Install [.NET 8 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/8.0).
2. Publish to a folder; create an IIS site pointing at that folder (no managed pipeline module required — ASP.NET Core Module).
3. Set environment variables in `web.config` or IIS → Configuration Editor → `system.webServer/aspNetCore/environmentVariables`.
4. Ensure the app-pool identity can write `App_Data/DataProtection-Keys` and `wwwroot/media`.

## Docker (optional)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY EImece.Web/EImece.Web.csproj EImece.Web/
COPY EImece.Domain.Core/EImece.Domain.Core.csproj EImece.Domain.Core/
COPY Resources/Resources.csproj Resources/
RUN dotnet restore EImece.Web/EImece.Web.csproj
COPY EImece.Web/ EImece.Web/
COPY EImece.Domain.Core/ EImece.Domain.Core/
COPY Resources/ Resources/
# Theme assets: either mount EImece/Content+Scripts or copy into the image
RUN dotnet publish EImece.Web/EImece.Web.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "EImece.Web.dll"]
```

Mount or bake `../EImece/Content` and `../EImece/Scripts` beside the content root so `UseLegacyThemeStaticFiles` resolves theme CSS/JS.

## Performance notes

- Response compression (Brotli/Gzip) enabled for HTTPS.
- Image resize responses use memory cache + `ResponseCache` attributes.
- Storefront catalog queries use `AsNoTracking()` where shelled.
- Keep `BypassAdminAuth=false` and scheduler off unless intentionally enabled.

## Cutover reminder

Run Core in parallel until catalog → cart → checkout → admin are verified against production data (see `FUNCTIONAL_VERIFICATION.md`). Do not delete the MVC5 app until cutover is approved.
