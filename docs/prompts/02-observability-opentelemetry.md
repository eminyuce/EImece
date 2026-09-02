# Observability with OpenTelemetry

- **Captured:** 2026-08-05 1:27:35 PM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

You are a senior observability architect and .NET engineer with deep expertise in:
- OpenTelemetry (traces, metrics, logs)
- ASP.NET MVC 5 / System.Web
- .NET Framework 4.8.1
- Microsoft.Extensions.DependencyInjection
- AOP patterns (ActionFilters, HttpModules, DI decorators)
- Diagnostics (ActivitySource, DiagnosticSource, Meter)
- Performance engineering and production-grade telemetry
- NLog, Serilog, Polly, Entity Framework 6

Work exclusively on the existing open-source e-commerce solution:
https://github.com/eminyuce/EImece (master branch)

Tech stack (do not change the platform):
- Runtime: .NET Framework 4.8.1
- Web: ASP.NET MVC 5.3, System.Web, OWIN, ASP.NET Identity
- Data: Entity Framework 6.5, SQL Server
- DI: Microsoft.Extensions.DependencyInjection (already migrated from Ninject)
- Logging: NLog + Serilog
- Resilience: Polly
- Payments: Iyzico
- Existing observability under: EImece.Domain/Observability/

Existing observability you MUST reuse and extend (do not rewrite from scratch):
- Observability/Configuration/ObservabilityOptions.cs
- Observability/Metrics/ApplicationMetrics.cs + IApplicationMetrics
- Observability/Logging/StructuredLoggingBootstrap.cs
- Observability/Logging/CorrelationIdContext.cs
- Observability/Logging/SensitiveDataMasker.cs
- Observability/Logging/EfSqlLogger.cs
- Observability/Http/ResilientHttpClient.cs + IResilientHttpClient
- Observability/HealthChecks/* (SqlServer, FileStorage, ExternalApi, BackgroundService, etc.)
- Health endpoints: GET /health, GET /healthz
- Admin metrics: GET /metrics

Goal
Implement production-grade OpenTelemetry instrumentation that is vendor-neutral, low-invasive, secure, and cost-aware, while keeping all existing health/metrics/logging behavior working.

Requirements

1. Packages (net481-compatible only)
   Add the minimum set of OpenTelemetry packages that support .NET Framework 4.8.1.
   Prefer OTLP as the primary exporter. Support optional Azure Monitor / Application Insights exporter.
   Keep existing System.Diagnostics.DiagnosticSource, Serilog, NLog, Polly.

2. Bootstrap
   Create EImece.Domain/Observability/OpenTelemetryBootstrap.cs that:
   - Creates a single ActivitySource named "EImece" (versioned)
   - Creates a Meter named "EImece"
   - Builds TracerProvider + MeterProvider with Resource (service.name=EImece, service.version, deployment.environment)
   - Adds source "EImece", HttpClient instrumentation
   - Configures OTLP exporter from config/env (no secrets in source)
   - Optional Azure Monitor exporter behind a feature flag
   - Safe no-op when tracing/metrics disabled
   - Clean Shutdown/Dispose for AppDomain recycle

3. Extend ObservabilityOptions
   Add flags and settings (with sensible defaults, loaded via existing AppConfig pattern):
   - EnableTracing
   - EnableMetrics (already exists)
   - OtlpEndpoint
   - SamplingRatio (0.0–1.0)
   - EnableAzureMonitorExporter
   - AzureMonitorConnectionString (from env only, never commit)
   Keep EnableRequestLogging, EnableEfSqlLogging, Http* resilience settings.

4. Correlation & request tracing (AOP)
   - Keep CorrelationIdContext; ensure X-Correlation-Id is accepted or generated and stored in HttpContext.Items
   - Propagate correlation into Activity tags and log enrichers
   - Prefer W3C traceparent when present
   - Implement a global ActionFilter (and/or HttpModule) that:
     - Starts a Server Activity for controller/action
     - Sets http.method, http.route (or action name), status code, duration
     - Records into existing IApplicationMetrics AND OpenTelemetry Meter
     - Stops Activity and sets status on success/error
   - Controllers and services must stay free of telemetry boilerplate where possible

5. Bridge existing ApplicationMetrics to OpenTelemetry
   - Keep ConcurrentDictionary snapshots and /metrics endpoint behavior
   - Additionally emit OTel Counter + Histogram for:
     - http.server.requests / http.server.duration
     - outbound http calls
     - db operations
   - Strict cardinality control: never use raw full URLs as metric labels; prefer method + status + normalized route/operation

6. Outbound HTTP (ResilientHttpClient)
   - Propagate W3C trace context + X-Correlation-Id
   - Create Client Activity around calls
   - Record duration, status, retry count into metrics and spans
   - Do not log response bodies or secrets

7. EF6
   - Keep EfSqlLogger + SensitiveDataMasker
   - EnableEfSqlLogging remains gated (default on in non-prod only)
   - Optionally add light Activity/metric recording for significant DB operations without flooding production with full SQL text

8. Logging ↔ traces
   - Enrich Serilog/NLog with TraceId, SpanId, CorrelationId when Activity.Current exists
   - Always run SensitiveDataMasker on messages that may contain credentials, tokens, or payment data
   - Never log card data, full payment payloads, or connection strings

9. DI registration
   - Register OpenTelemetry bootstrap, IApplicationMetrics, options, and any filters/modules via Microsoft.Extensions.DependencyInjection
   - Call bootstrap once from Application_Start (or equivalent) and dispose on shutdown
   - Use constructor injection; keep existing property-injection helpers if still required by legacy code

10. Health
    - Do not break /health or /healthz
    - Optionally expose a simple health gauge metric
    - Keep readiness (SQL + critical deps) separate from liveness

11. Security, privacy, cost
    - No secrets in source control or telemetry attributes
    - Sampling in production (parent-based + ratio)
    - Mask PII; drop high-cardinality attributes
    - Batch processors; never block request threads on export I/O

12. AOP style preferred on this stack
    - Global ActionFilter for MVC requests
    - DI decorator/wrapper for critical services (checkout, payment callback, search) only if filters are insufficient
    - Explicit ActivitySource usage only in high-value business paths (Iyzico payment authorize/callback, order save)
    - Avoid heavy runtime weaving libraries unless already present

Deliverables (implement, do not only describe)
1. OpenTelemetryBootstrap.cs and any supporting types under EImece.Domain/Observability/
2. Extended ObservabilityOptions
3. Telemetry ActionFilter (and HttpModule if needed) + registration
4. Updates to ApplicationMetrics / ResilientHttpClient / StructuredLoggingBootstrap / CorrelationIdContext as required
5. DI registration changes
6. Global.asax / startup wiring + clean shutdown
7. Minimal config keys documented (appSettings + env vars); no committed secrets
8. Short README section or docs note: how to enable OTLP, sampling, and Azure Monitor exporter
9. Keep existing tests green; add focused unit tests where practical for correlation, masking, and metric recording

Constraints
- Target net481 / ASP.NET MVC 5 only — no ASP.NET Core migration
- Prefer incremental, reviewable changes over a big rewrite
- Preserve Repository + Service layer architecture
- Production-safe defaults: tracing/metrics can be enabled via config; exporters off until endpoint is configured
- Code must compile on the existing solution structure (EImece, EImece.Domain, etc.)

Acceptance criteria
- Request path produces a Server span with correlation id and status
- Outbound HttpClient calls produce Client spans with context propagation
- Existing /health, /healthz, /metrics still work
- Logs contain CorrelationId and, when available, TraceId/SpanId
- Sensitive data remains masked
- OTLP (or Azure Monitor) receives traces/metrics when enabled and endpoint is set
- No secrets committed; cardinality controlled; no request-thread blocking on export

Start by inspecting the current Observability folder and DI registration, then implement the bootstrap + filter path first, then bridge metrics and ResilientHttpClient, then logging enrichment and config.
