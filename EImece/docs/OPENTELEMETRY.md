# OpenTelemetry (ASP.NET MVC 5 / .NET Framework 4.8.1)

Vendor-neutral traces and metrics for EImece. Existing `/health`, `/healthz`, and admin `/metrics` endpoints are unchanged.

## What is instrumented

| Signal | Source |
|--------|--------|
| Server spans | Global `TelemetryActionFilter` (controller.action) |
| Client spans | `ResilientHttpClient` + OpenTelemetry HttpClient instrumentation |
| Payment spans | `IyzicoService` authorize / callback |
| Metrics | `ApplicationMetrics` snapshots **and** OTel counters/histograms |
| Logs ↔ traces | Serilog + NLog enriched with `CorrelationId`, `TraceId`, `SpanId` |

ActivitySource / Meter name: **`EImece`**.

## Enable OTLP

Prefer environment variables in deployed environments (no secrets in git):

```bash
export OTEL_EXPORTER_OTLP_ENDPOINT="http://localhost:4317"
```

Or `Web.config` / appSettings (non-secret endpoint only):

```xml
<add key="EnableTracing" value="true" />
<add key="EnableMetrics" value="true" />
<add key="OtlpEndpoint" value="http://localhost:4317" />
<add key="SamplingRatio" value="0.1" />
```

Exporters stay idle until an endpoint (or Azure Monitor connection string) is configured. Providers still build when tracing/metrics are enabled so Activities enrich logs locally.

## Sampling

- Parent-based sampler + ratio for roots
- `SamplingRatio` in `[0.0, 1.0]`
- Defaults: `1.0` in non-live, `0.1` when `SiteStatus=live` (override via appSetting)

## Azure Monitor exporter (optional)

```bash
export APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=...;IngestionEndpoint=..."
```

```xml
<add key="EnableAzureMonitorExporter" value="true" />
```

The connection string is read **from environment only** (`APPLICATIONINSIGHTS_CONNECTION_STRING`, `APPINSIGHTS_CONNECTIONSTRING`, or `AZURE_MONITOR_CONNECTION_STRING`). Do not commit secrets.

Legacy `Microsoft.ApplicationInsights.Web` remains available separately; prefer OTel + Azure Monitor exporter for new pipelines.

## Config keys

| Key / env | Purpose | Default |
|-----------|---------|---------|
| `EnableTracing` | Build TracerProvider + server/client Activities | `true` in non-live |
| `EnableMetrics` | In-process snapshots + MeterProvider | `true` |
| `OtlpEndpoint` / `OTEL_EXPORTER_OTLP_ENDPOINT` | OTLP collector URL | empty (exporter off) |
| `SamplingRatio` | Root sample ratio | `1.0` / `0.1` live |
| `EnableAzureMonitorExporter` | Feature flag for Azure Monitor OTel exporter | `false` |
| `EnableEfSqlLogging` | Full SQL text to logs (masked) | on in non-live |
| `EnableEfTelemetry` | Light DB metrics/Activities without SQL text | `true` |
| `OtelServiceName` | Resource `service.name` | `EImece` |
| `OtelDeploymentEnvironment` | Resource `deployment.environment` | `SiteStatus` |

## Privacy and cost

- No card data, payment payloads, or connection strings in attributes/logs (`SensitiveDataMasker`)
- Metric labels use method + status + **normalized** route/operation (never raw full URLs)
- Batch exporters; request threads are not blocked on export I/O
- Shutdown/dispose runs on `Application_End` for AppDomain recycle

## Local collector example

```bash
# Jaeger all-in-one with OTLP
docker run --rm -p 16686:16686 -p 4317:4317 jaegertracing/all-in-one:latest
export OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
```

Then open http://localhost:16686 and exercise the site.
