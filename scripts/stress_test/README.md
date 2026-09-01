# EImece Stress Testing & Performance Telemetry Suite

A modular, high-performance load testing and telemetry toolkit for the EImece ASP.NET MVC application running under IIS with SQL Server.

---

## 📂 Architecture

```
scripts/
├── check_sitemap.py             # Verifies all 262 URLs in sitemap.xml for HTTP 200 & latency
└── stress_test/
    ├── config.py                # Target endpoints, base URL, credentials, circuit-breaker thresholds
    ├── engine.py                # Core HTTP load generator with connection pooling, keep-alive, and stats
    ├── scenarios.py             # User journeys: Anonymous Browsing (A) and Authenticated Customer (B)
    ├── monitor.py               # Telemetry sampler: w3wp CPU/RAM, GC cycles, thread count, SQL RAM
    ├── db_profiler.py           # SQL Server DMV query stats and active session analyzer
    ├── memory_soak.py           # 5–15 min sustained load memory soak & GC retention tester
    ├── theme_compare.py         # Latency & payload comparison across storefront themes
    └── run_all.py               # Master test orchestrator generating JSON & terminal summaries
```

---

## 🚀 How to Run

### Prerequisites
- Python 3.10+
- `requests` package (`pip install requests`)

### 1. Full Master Stress Test Suite
Runs single, 10x, and 100x baselines, cache tests, progressive load levels 1–6 (5 to 250 CU), multi-step user scenarios, and outputs `stress_test_master_results.json`:
```bash
python scripts/stress_test/run_all.py
```

### 2. Sitemap URL Audit
Audits all URLs declared in `http://localhost:81/sitemap.xml`:
```bash
python scripts/check_sitemap.py
```

### 3. Memory Soak Test (Leak Detection)
Runs a steady 15-user workload for 5 minutes and monitors `w3wp.exe` working set delta:
```bash
python scripts/stress_test/memory_soak.py
```

### 4. Theme Benchmark
Compares latency and response sizes across active themes:
```bash
python scripts/stress_test/theme_compare.py
```

---

## 🛡️ Safety Safeguards
- **Read-Heavy & Isolated:** Does not mass-create, alter, or delete real business data.
- **Payment Safe:** Payment checkout execution is excluded from automated load testing.
- **Circuit Breakers:** Automatically stops load escalation if the error rate exceeds `5.0%` or p95 latency exceeds `5,000 ms`.
