import os
import sys
import json
import time
import subprocess
from config import BASE_URL, HOST_HEADER, CORE_ENDPOINTS, MAX_ERROR_RATE_PERCENT, MAX_LATENCY_P95_MS
from engine import LoadEngine
from monitor import SystemMonitor
from scenarios import UserScenarios
from db_profiler import get_sql_server_metrics

def main():
    print("=" * 80)
    print("      EIMECE ASP.NET MVC PRODUCTION READINESS STRESS TEST SUITE      ")
    print("=" * 80)
    print(f"Target:        {BASE_URL} (Host: {HOST_HEADER})")
    print(f"Timestamp:     {time.strftime('%Y-%m-%d %H:%M:%S')}")
    print("=" * 80)

    engine = LoadEngine(BASE_URL, HOST_HEADER)
    scenarios = UserScenarios(BASE_URL)
    results = {
        "timestamp": time.strftime("%Y-%m-%d %H:%M:%S"),
        "target": BASE_URL,
        "baseline_single": {},
        "baseline_10": {},
        "baseline_100": {},
        "progressive_load": {},
        "scenarios": {},
        "cache_profiling": {},
        "system_telemetry": {},
        "sql_metrics": {}
    }

    # ==========================================
    # PHASE 1: Baseline Measurements (1, 10, 100)
    # ==========================================
    print("\n>>> [PHASE 1] Establishing Baseline Performance (Single, 10, 100 sequential requests)...", flush=True)
    
    # 1 request baseline
    print("\n--- Single Request Latency & Payload ---", flush=True)
    for name, path in CORE_ENDPOINTS.items():
        res1 = engine.execute_sequential(path, count=1)
        results["baseline_single"][name] = res1
        print(f"  {name:<22} : {res1['avg_ms']:6.1f} ms | HTTP {list(res1['status_codes'].keys())[0]} | {res1['avg_size_kb']:5.1f} KB", flush=True)

    # 10 sequential baseline
    print("\n--- 10 Sequential Requests ---", flush=True)
    for name, path in CORE_ENDPOINTS.items():
        res10 = engine.execute_sequential(path, count=10)
        results["baseline_10"][name] = res10
        print(f"  {name:<22} : Avg: {res10['avg_ms']:6.1f} ms | p95: {res10['p95_ms']:6.1f} ms | RPS: {res10['rps']:5.1f}", flush=True)

    # 100 sequential baseline on primary storefront endpoints
    print("\n--- 100 Sequential Requests (Hot Paths) ---", flush=True)
    hot_paths = ["Homepage", "Category_Elektronik", "Product_1", "Search_Termos", "Info_AboutUs", "Bundle_CSS_Crizal"]
    for name in hot_paths:
        path = CORE_ENDPOINTS[name]
        res100 = engine.execute_sequential(path, count=100)
        results["baseline_100"][name] = res100
        print(f"  {name:<22} : Avg: {res100['avg_ms']:6.1f} ms | p50: {res100['p50_ms']:6.1f} ms | p95: {res100['p95_ms']:6.1f} ms | p99: {res100['p99_ms']:6.1f} ms | RPS: {res100['rps']:5.1f}", flush=True)

    # ==========================================
    # PHASE 2: Cache Efficacy & Stampede
    # ==========================================
    print("\n>>> [PHASE 2] Cache Efficacy & Stampede Profiling...", flush=True)
    # Measure cold response vs warm responses
    cold_url = "/p/kulaklik--ses/copilotmicrosoft-5i0j0j0j4h1b"
    print("Testing Cold vs Warm responses on Product Detail...", flush=True)
    cold_res = engine.execute_sequential(cold_url, count=1)
    warm_res = engine.execute_sequential(cold_url, count=10)
    results["cache_profiling"]["cold_ms"] = cold_res["avg_ms"]
    results["cache_profiling"]["warm_avg_ms"] = warm_res["avg_ms"]
    results["cache_profiling"]["warm_p95_ms"] = warm_res["p95_ms"]
    print(f"  Cold Request: {cold_res['avg_ms']} ms  ->  Warm 10x Avg: {warm_res['avg_ms']} ms (Improvement: {round((cold_res['avg_ms']-warm_res['avg_ms'])/cold_res['avg_ms']*100, 1)}%)", flush=True)

    # ==========================================
    # PHASE 3: Progressive Load Testing (Levels 1 - 6)
    # ==========================================
    print("\n>>> [PHASE 3] Executing Progressive Load Tests (Levels 1 - 6)...", flush=True)
    
    levels = [
        ("Level 1 - Smoke", 5, 30),
        ("Level 2 - Light", 10, 45),
        ("Level 3 - Normal", 25, 60),
        ("Level 4 - Heavy", 50, 60),
        ("Level 5 - High", 100, 60),
        ("Level 6 - Extreme", 250, 45)
    ]

    monitor = SystemMonitor(sample_interval_sec=2.0)
    monitor.start()

    for level_name, cu, duration in levels:
        print(f"\n--- Running {level_name} ({cu} Concurrent Users, Duration: {duration}s) ---", flush=True)
        
        step_counter = 0
        def anon_worker(session):
            nonlocal step_counter
            step_counter += 1
            return scenarios.scenario_anonymous_step(session, step_counter)

        load_res = engine.execute_concurrent(anon_worker, concurrency=cu, duration_sec=duration)
        results["progressive_load"][level_name] = load_res
        
        print(f"  Total Reqs: {load_res['total_requests']} | RPS: {load_res['rps']} | Avg: {load_res['avg_ms']} ms | p50: {load_res['p50_ms']} ms | p95: {load_res['p95_ms']} ms | p99: {load_res['p99_ms']} ms | Errors: {load_res['errors']} ({load_res['error_rate_pct']}%)", flush=True)
        
        # Check Circuit Breakers
        if load_res["error_rate_pct"] > MAX_ERROR_RATE_PERCENT:
            print(f"  [CIRCUIT BREAKER TRIGGERED] Error rate {load_res['error_rate_pct']}% exceeds threshold {MAX_ERROR_RATE_PERCENT}%. Halting further load escalation.", flush=True)
            break
        if load_res["p95_ms"] > MAX_LATENCY_P95_MS:
            print(f"  [CIRCUIT BREAKER TRIGGERED] p95 latency {load_res['p95_ms']} ms exceeds critical threshold {MAX_LATENCY_P95_MS} ms. Halting further load escalation.", flush=True)
            break

    monitor.stop()
    telemetry = monitor.get_summary()
    results["system_telemetry"] = telemetry
    print("\nSystem Telemetry Summary during Load:", flush=True)
    print(json.dumps(telemetry, indent=2), flush=True)

    # ==========================================
    # PHASE 4: Realistic Multi-Step Scenarios
    # ==========================================
    print("\n>>> [PHASE 4] Multi-Step Scenario Profiling (Anonymous vs Authenticated Customer)...", flush=True)
    
    print("Running Scenario A: Anonymous Browsing Journey (20 concurrent users, 30s)...", flush=True)
    anon_journey_res = engine.execute_concurrent(
        lambda s: scenarios.scenario_anonymous_step(s, 0),
        concurrency=20,
        duration_sec=30
    )
    results["scenarios"]["Anonymous_Journey"] = anon_journey_res
    print(f"  Anonymous RPS: {anon_journey_res['rps']} | Avg: {anon_journey_res['avg_ms']} ms | p95: {anon_journey_res['p95_ms']} ms", flush=True)

    print("Running Scenario B: Authenticated Customer Journey (5 concurrent users, 30s)...", flush=True)
    cust_journey_res = engine.execute_concurrent(
        lambda s: scenarios.scenario_customer_session(s),
        concurrency=5,
        duration_sec=30
    )
    results["scenarios"]["Customer_Journey"] = cust_journey_res
    print(f"  Customer Journey Total Cycles: {cust_journey_res['total_requests']} | Avg Cycle Latency: {cust_journey_res['avg_ms']} ms | p95: {cust_journey_res['p95_ms']} ms | Error %: {cust_journey_res['error_rate_pct']}%", flush=True)

    # ==========================================
    # PHASE 5: SQL Server Metrics & Final Export
    # ==========================================
    print("\n>>> [PHASE 5] Collecting SQL Server DMV Metrics...", flush=True)
    sql_metrics = get_sql_server_metrics()
    results["sql_metrics"] = sql_metrics
    print(f"SQL Server Active DB Sessions: {sql_metrics.get('active_db_sessions', 'N/A')}", flush=True)

    # Save to disk
    out_file = "stress_test_master_results.json"
    with open(out_file, "w", encoding="utf-8") as f:
        json.dump(results, f, indent=2)
    print(f"\n Master Stress Test Results exported to {out_file}", flush=True)

if __name__ == "__main__":
    main()
