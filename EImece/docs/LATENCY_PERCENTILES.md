# Latency percentiles (P90 / P95 / P99) — Controllers & Services

Production-ready instrumentation for EImece (ASP.NET MVC 5 / .NET Framework 4.8.1).

## 1. Recommended architecture

| Layer | Technique | Why |
|-------|-----------|-----|
| **Controllers** | Global `TelemetryActionFilter` (`IActionFilter`) | Non-invasive; covers every MVC action (sync + async). Framework awaits `Task<ActionResult>` before `OnActionExecuted`. |
| **Services** | `MeasuredServiceProxy` (`RealProxy` transparent proxy) applied in DI | Non-invasive; wraps all interface-based service registrations automatically. Times sync methods and awaits `Task` / `Task<T>` before recording. |
| **Storage** | `ApplicationMetrics` bounded ring buffer (2048 samples/key) | Thread-safe, fixed memory, correct under high concurrency. |
| **Export** | OpenTelemetry histogram `eimece.method.duration` + in-memory Admin UI | Histograms let backends compute true P90/P95/P99; Admin `/Admin/Metrics` shows nearest-rank percentiles immediately. |

**Why histograms / sample windows instead of “running average”?**  
Averages hide tail latency. P90/P95/P99 answer “how slow are the worst 10% / 5% / 1% of calls?” — the number users actually feel. Storing every sample forever is unsafe at scale; a ring buffer keeps memory bounded while still estimating tails from recent traffic. OTel histograms push raw bucket data to Prometheus / Azure Monitor / Grafana for longer-term percentile queries.

```
HTTP request
   └─ TelemetryActionFilter  →  request:* + controller:*  →  ApplicationMetrics + OTel
         └─ IProductService (proxy)
               └─ MeasuredServiceProxy  →  service:IProductService.Method  →  ApplicationMetrics + OTel
                     └─ ProductService (concrete)
```

## 2. Implementation (what shipped)

### Core types

- `EImece.Domain/Observability/Metrics/ApplicationMetrics.cs` — ring buffer + P90/P95/P99
- `EImece.Domain/Observability/Metrics/MeasuredServiceProxy.cs` — service interceptor
- `EImece.Domain/Observability/Metrics/OpenTelemetryMetrics.cs` — `eimece.method.duration` histogram
- `EImece/Filters/TelemetryActionFilter.cs` — controller timing
- DI wrap in `DependencyInjectionConfig` (`MaybeWrapWithMetricsProxy`)

### Percentile formula (nearest-rank)

For a sorted window of `n` samples and percentile `p` ∈ (0, 1]:

```
index = ceil(p × n) - 1   // clamped to [0, n-1]
P = sorted[index]
```

Example: samples `1..100` ms → P90=90, P95=95, P99=99.

`Count` = lifetime invocations; percentiles use the **most recent ≤ 2048** samples (`SampleWindowSize`).

## 3. Global registration / usage

Already wired. No per-controller or per-service attributes required.

### Controllers

Registered once in `Global.asax` / `Application_Start`:

```csharp
var metrics = DependencyResolver.Current.GetService<IApplicationMetrics>();
var options = DependencyResolver.Current.GetService<ObservabilityOptions>();
GlobalFilters.Filters.Add(new TelemetryActionFilter(metrics, options));
```

### Services

Every `AddScopedWithProps<TService, TImplementation>` / singleton / transient interface registration is wrapped when:

```xml
<add key="EnableMetrics" value="true" />
<add key="EnableServiceMethodMetrics" value="true" />
```

Disable service proxies without turning off request metrics:

```xml
<add key="EnableServiceMethodMetrics" value="false" />
```

### OpenTelemetry / Azure Monitor

Same as [OPENTELEMETRY.md](./OPENTELEMETRY.md):

```bash
export OTEL_EXPORTER_OTLP_ENDPOINT="http://localhost:4317"
# or
export APPLICATIONINSIGHTS_CONNECTION_STRING="..."
```

```xml
<add key="EnableMetrics" value="true" />
<add key="EnableAzureMonitorExporter" value="true" />
```

## 4. How to view P90 / P95 / P99

### A) Admin UI (in-memory)

Browse to **`/Admin/Metrics`** (authenticated admin).

| Metric name | Meaning |
|-------------|---------|
| `request:Products.Index` | MVC action (legacy series) |
| `controller:Products.Index` | Same action in the percentile pipeline |
| `service:IProductService.GetMainPageProductsAsync` | Service method |
| `http:GET:200` | Outbound HTTP |
| `db:...` | EF/DB operations |

Example row after traffic:

| Metrik Adı | Adet | Hata | Ort. | P90 | P95 | P99 |
|------------|------|------|------|-----|-----|-----|
| `controller:Products.Index` | 1842 | 3 | 48.2 | 72 | 95 | 180 |
| `service:IProductService.GetMainPageProductsAsync` | 1842 | 0 | 31.0 | 45 | 60 | 120 |

### B) OpenTelemetry → Prometheus / Grafana

Histogram: `eimece.method.duration`  
Labels: `eimece.layer`, `eimece.type`, `eimece.method`, `eimece.success`

PromQL (metric name may be normalized by the collector):

```promql
histogram_quantile(0.90, sum by (le, eimece_type, eimece_method) (
  rate(eimece_method_duration_milliseconds_bucket[5m])))

histogram_quantile(0.95, sum by (le, eimece_type, eimece_method) (
  rate(eimece_method_duration_milliseconds_bucket[5m])))

histogram_quantile(0.99, sum by (le, eimece_type, eimece_method) (
  rate(eimece_method_duration_milliseconds_bucket[5m])))
```

### C) Azure Monitor / Application Insights

With Azure Monitor OTel exporter enabled, query histogram percentiles in Log Analytics / workbook metrics for `eimece.method.duration`, filtered by `eimece.layer` (`controller` | `service`).

## 5. Optional lighter alternative

If you only need **HTTP endpoint** percentiles (not every service method):

1. Set `EnableServiceMethodMetrics=false`
2. Keep `TelemetryActionFilter` + `http.server.duration` histogram
3. Or rely solely on Application Insights Server Request metrics / IIS Failed Request Tracing

That drops proxy overhead and cardinality from hundreds of service methods, at the cost of losing method-level attribution inside a slow action.

## Best practices & pitfalls

1. **Never put IDs, emails, or query strings in metric names/labels** — cardinality explosion. This stack normalizes routes and uses type+method only.
2. **Prefer histograms over “exact” global percentiles** for long-term storage; ring-buffer snapshots are for live ops, not forever.
3. **Async correctness** — always measure until the `Task` completes; never stop the stopwatch when the async method returns a hot task.
4. **Circular DI** — during `[Inject]` construction the bare concrete is returned (no proxy). Cross-calls made only during ctor/property inject are not measured; normal request-time calls are.
5. **Concrete-only services** (`IyzicoService`, `ReportService`, …) are not proxied unless resolved via an interface. Prefer interfaces for anything you need in the percentile table.
6. **Repositories are not proxied** — only `*.Services.IServices` interfaces (or names ending in `Service`) are wrapped.
7. **Filter vs middleware** — MVC action filters exclude result-execution time (view render). That is intentional for “action/service” latency; use server request metrics for full HTTP duration.
8. **Keep overhead tiny** — `Stopwatch` + one dictionary update under a short lock; no allocations on the hot path beyond the ring slot write.
9. **Alert on P99 or P95**, not averages. Pair with error rate (`ErrorCount` / `eimece.success=false`).

## Quick verification

```csharp
// Unit tests
// EImece.Tests/Helpers/ApplicationMetricsTests.cs
// - RecordMethod_ComputesP90P95P99
// - MeasuredServiceProxy_RecordsSyncAndAsyncDurations
```

```bash
cd EImece && ./scripts/build.sh
# Run MSTest for ApplicationMetricsTests on a Windows/CI agent with packages restored
```
